using IndoorNavigation.Core.Interfaces;
using IndoorNavigation.Core.Models;
using UnityEngine;

namespace IndoorNavigation.Alignment
{
    public sealed class AlignmentService : MonoBehaviour, IAlignmentService
    {
        [SerializeField]
        private Transform navigationRoot;

        [SerializeField]
        private Transform arCameraTransform;

        [SerializeField]
        [Range(0.1f, 20f)]
        private float smoothGain = 6f;

        [SerializeField]
        [Tooltip("Ignore very small correction jitter under this threshold (meters).")]
        private float positionEpsilon = 0.005f;

        [SerializeField]
        [Tooltip("Ignore very small correction jitter under this threshold (degrees).")]
        private float rotationEpsilonDegrees = 0.4f;

        private Vector3 targetPosition;
        private Quaternion targetRotation;
        private float requestedDuration;
        private float elapsed;

        public Transform NavigationRoot => navigationRoot;
        public bool IsSmoothing { get; private set; }

        public void Initialize(Transform navigationRootTransform, Transform arCamera)
        {
            navigationRoot = navigationRootTransform;
            arCameraTransform = arCamera;
        }

        public void ApplyInstant(LocalizationPose localizationPose)
        {
            if (!TryComputeRootTransform(localizationPose, out Vector3 rootPosition, out Quaternion rootRotation))
            {
                return;
            }

            IsSmoothing = false;
            navigationRoot.SetPositionAndRotation(rootPosition, rootRotation);
        }

        public void ApplySmooth(LocalizationPose localizationPose, float smoothDurationSeconds)
        {
            if (!TryComputeRootTransform(localizationPose, out Vector3 rootPosition, out Quaternion rootRotation))
            {
                return;
            }

            targetPosition = rootPosition;
            targetRotation = rootRotation;
            requestedDuration = Mathf.Max(0.05f, smoothDurationSeconds);
            elapsed = 0f;
            IsSmoothing = true;
        }

        public void Tick(float deltaTime)
        {
            if (!IsSmoothing || navigationRoot == null)
            {
                return;
            }

            elapsed += deltaTime;
            float durationFactor = Mathf.Clamp01(elapsed / requestedDuration);
            float t = 1f - Mathf.Exp(-smoothGain * durationFactor);

            Vector3 nextPosition = Vector3.Lerp(navigationRoot.position, targetPosition, t);
            Quaternion nextRotation = Quaternion.Slerp(navigationRoot.rotation, targetRotation, t);
            navigationRoot.SetPositionAndRotation(nextPosition, nextRotation);

            float remainingDistance = Vector3.Distance(navigationRoot.position, targetPosition);
            float remainingAngle = Quaternion.Angle(navigationRoot.rotation, targetRotation);
            if (remainingDistance <= positionEpsilon && remainingAngle <= rotationEpsilonDegrees)
            {
                navigationRoot.SetPositionAndRotation(targetPosition, targetRotation);
                IsSmoothing = false;
            }
        }

        private bool TryComputeRootTransform(LocalizationPose localizationPose, out Vector3 rootPosition, out Quaternion rootRotation)
        {
            rootPosition = Vector3.zero;
            rootRotation = Quaternion.identity;

            if (navigationRoot == null || arCameraTransform == null)
            {
                Debug.LogError("[AlignmentService] NavigationRoot or AR camera reference is missing.");
                return false;
            }

            Quaternion inverseLocalizedRotation = Quaternion.Inverse(localizationPose.Rotation);
            rootRotation = arCameraTransform.rotation * inverseLocalizedRotation;
            rootPosition = arCameraTransform.position - (rootRotation * localizationPose.Position);
            return true;
        }
    }
}
