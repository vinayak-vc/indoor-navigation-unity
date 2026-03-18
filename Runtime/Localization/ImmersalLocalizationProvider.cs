using System;
using System.Threading;
using System.Threading.Tasks;

using IndoorNavigation.Core.Enums;
using IndoorNavigation.Core.Interfaces;
using IndoorNavigation.Core.Models;
using IndoorNavigation.Navigation;

using UnityEngine;

namespace IndoorNavigation.Localization {
    public sealed class ImmersalLocalizationProvider : MonoBehaviour, ILocalizationProvider {
        [SerializeField]
        private ImmersalLocalizationBridge bridge;

        [SerializeField]
        private ImmersalLocalizationConfig config;

        [SerializeField]
        private FloorMapRegistry floorMapRegistry;

        [SerializeField]
        [Tooltip("Fallback map ID used when there is no active floor binding.")]
        private int fallbackMapId;

        private CancellationTokenSource _localizationLoopCancellation;
        private float _lastRelocalizationRequestTime = -999f;

        public event Action<LocalizationPose> LocalizationSucceeded;
        public event Action<string> LocalizationFailed;
        public event Action<LocalizationStatus> StatusChanged;

        public LocalizationStatus Status { get; private set; } = LocalizationStatus.Idle;

        private void OnEnable() {
            if (bridge == null) {
                Debug.LogError("[ImmersalLocalizationProvider] Bridge reference is missing.");
                return;
            }

            bridge.PoseReceived += OnPoseReceived;
            bridge.LocalizationFailed += OnBridgeLocalizationFailed;
        }

        private void OnDisable() {
            if (bridge != null) {
                bridge.PoseReceived -= OnPoseReceived;
                bridge.LocalizationFailed -= OnBridgeLocalizationFailed;
            }

            StopLocalization();
        }

        public async Task StartLocalizationAsync(CancellationToken cancellationToken) {
            if (config == null) {
                Debug.LogError("[ImmersalLocalizationProvider] Config is missing.");
                return;
            }

            StopLocalization();
            _localizationLoopCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            UpdateStatus(LocalizationStatus.Localizing);

            try {
                while (!_localizationLoopCancellation.Token.IsCancellationRequested) {
                    if (!bridge.IsRequestInFlight) {
                        bridge.RequestLocalization(GetCurrentMapId());
                    }

                    await Task.Delay(TimeSpan.FromSeconds(config.LocalizationRetryIntervalSeconds), _localizationLoopCancellation.Token);
                }
            } catch (TaskCanceledException) {
            }
        }

        public void RequestRelocalization() {
            if (config == null || bridge == null) {
                return;
            }

            if (Time.time - _lastRelocalizationRequestTime < config.RelocalizationCooldownSeconds) {
                return;
            }

            _lastRelocalizationRequestTime = Time.time;
            UpdateStatus(LocalizationStatus.Localizing);
            if (!bridge.IsRequestInFlight) {
                bridge.RequestLocalization(GetCurrentMapId());
            }
        }

        public void StopLocalization() {
            if (_localizationLoopCancellation == null) {
                return;
            }

            _localizationLoopCancellation.Cancel();
            _localizationLoopCancellation.Dispose();
            _localizationLoopCancellation = null;

            UpdateStatus(LocalizationStatus.Idle);
        }

        private void OnPoseReceived(LocalizationPose pose) {
            if (config != null && pose.Confidence < config.MinimumAcceptedConfidence) {
                string reason = $"Localization confidence too low: {pose.Confidence:0.00}";
                OnBridgeLocalizationFailed(reason);
                return;
            }

            UpdateStatus(LocalizationStatus.Localized);
            LocalizationSucceeded?.Invoke(pose);
        }

        private void OnBridgeLocalizationFailed(string reason) {
            UpdateStatus(LocalizationStatus.Failed);
            LocalizationFailed?.Invoke(reason);
        }

        private int GetCurrentMapId() {
            if (floorMapRegistry != null && floorMapRegistry.ActiveBinding != null) {
                return floorMapRegistry.ActiveBinding.ImmersalMapId;
            }

            return fallbackMapId;
        }

        private void UpdateStatus(LocalizationStatus newStatus) {
            if (Status == newStatus) {
                return;
            }

            Status = newStatus;
            StatusChanged?.Invoke(Status);
        }
    }
}
