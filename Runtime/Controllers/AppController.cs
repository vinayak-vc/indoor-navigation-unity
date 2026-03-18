using System.Threading;

using IndoorNavigation.Alignment;
using IndoorNavigation.Core.Enums;
using IndoorNavigation.Core.Models;
using IndoorNavigation.Localization;
using IndoorNavigation.Navigation;
using IndoorNavigation.Tracking;

using UnityEngine;

namespace IndoorNavigation.Controllers {
    public sealed class AppController : MonoBehaviour {
        [Header("Core Dependencies")]
        [SerializeField]
        private ImmersalLocalizationProvider localizationProvider;

        [SerializeField]
        private AlignmentService alignmentService;

        [SerializeField]
        private TrackingStateMonitor trackingStateMonitor;

        [SerializeField]
        private FloorMapRegistry floorMapRegistry;

        [SerializeField]
        private Transform arCameraTransform;

        [Header("Alignment Behavior")]
        [SerializeField]
        private bool instantAlignmentOnFirstLocalization = true;

        [SerializeField]
        [Min(0.05f)]
        private float smoothRealignmentDurationSeconds = 0.6f;

        [SerializeField]
        [Min(0f)]
        private float periodicRelocalizationSeconds = 10f;

        private CancellationTokenSource _appCts;
        private bool _hasLocalizedOnce;
        private LocalizationPose _lastLocalizationPose;
        private float _relocalizationTimer;

        public LocalizationStatus Status {
            get {
                return localizationProvider == null ? LocalizationStatus.Idle : localizationProvider.Status;
            }
        }

        public LocalizationPose LastLocalizationPose {
            get {
                return _lastLocalizationPose;
            }
        }

        private void Awake() {
            if (alignmentService != null && floorMapRegistry != null && floorMapRegistry.ActiveBinding != null) {
                alignmentService.Initialize(floorMapRegistry.ActiveBinding.NavigationRoot, arCameraTransform);
            }
        }

        private async void Start() {
            if (localizationProvider == null) {
                Debug.LogError("[AppController] LocalizationProvider is missing.");
                return;
            }

            _appCts = new CancellationTokenSource();
            await localizationProvider.StartLocalizationAsync(_appCts.Token);
        }

        private void OnEnable() {
            if (localizationProvider != null) {
                localizationProvider.LocalizationSucceeded += OnLocalizationSucceeded;
                localizationProvider.LocalizationFailed += OnLocalizationFailed;
            }

            if (trackingStateMonitor != null) {
                trackingStateMonitor.TrackingLost += OnTrackingLost;
                trackingStateMonitor.TrackingRecovered += OnTrackingRecovered;
            }
        }

        private void OnDisable() {
            if (localizationProvider != null) {
                localizationProvider.LocalizationSucceeded -= OnLocalizationSucceeded;
                localizationProvider.LocalizationFailed -= OnLocalizationFailed;
            }

            if (trackingStateMonitor != null) {
                trackingStateMonitor.TrackingLost -= OnTrackingLost;
                trackingStateMonitor.TrackingRecovered -= OnTrackingRecovered;
            }

            _appCts?.Cancel();
            _appCts?.Dispose();
            _appCts = null;
        }

        private void Update() {
            if (alignmentService != null && alignmentService.IsSmoothing) {
                alignmentService.Tick(Time.deltaTime);
            }

            if (!_hasLocalizedOnce || localizationProvider == null || periodicRelocalizationSeconds <= 0f) {
                return;
            }

            if (localizationProvider.Status != LocalizationStatus.Localized) {
                return;
            }

            if (trackingStateMonitor != null && !trackingStateMonitor.IsTrackingReliable) {
                return;
            }

            _relocalizationTimer += Time.deltaTime;
            if (_relocalizationTimer >= periodicRelocalizationSeconds) {
                _relocalizationTimer = 0f;
                localizationProvider.RequestRelocalization();
            }
        }

        private void OnLocalizationSucceeded(LocalizationPose pose) {
            _lastLocalizationPose = pose;

            if (floorMapRegistry != null && floorMapRegistry.ActivateByMapId(pose.MapId)) {
                alignmentService.Initialize(floorMapRegistry.ActiveBinding.NavigationRoot, arCameraTransform);
            }

            bool shouldAlignInstant = instantAlignmentOnFirstLocalization && !_hasLocalizedOnce;
            if (shouldAlignInstant) {
                alignmentService.ApplyInstant(pose);
            } else {
                alignmentService.ApplySmooth(pose, smoothRealignmentDurationSeconds);
            }

            _hasLocalizedOnce = true;
        }

        private void OnLocalizationFailed(string reason) {
            Debug.LogWarning($"[AppController] Localization failed: {reason}");
        }

        private void OnTrackingLost() {
            Debug.LogWarning("[AppController] AR tracking lost. Requesting relocalization.");
            localizationProvider?.RequestRelocalization();
        }

        private void OnTrackingRecovered() {
            if (_hasLocalizedOnce) {
                localizationProvider?.RequestRelocalization();
            }
        }
    }
}
