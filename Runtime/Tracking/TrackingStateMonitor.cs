using System;

using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace IndoorNavigation.Tracking {
    public sealed class TrackingStateMonitor : MonoBehaviour {
        public event Action TrackingLost;
        public event Action TrackingRecovered;

        public bool IsTrackingReliable { get; private set; } = true;

        private void OnEnable() {
            ARSession.stateChanged += OnArSessionStateChanged;
        }

        private void OnDisable() {
            ARSession.stateChanged -= OnArSessionStateChanged;
        }

        private void OnArSessionStateChanged(ARSessionStateChangedEventArgs eventArgs) {
            bool nowReliable = eventArgs.state == ARSessionState.SessionTracking;
            if (nowReliable == IsTrackingReliable) {
                return;
            }

            IsTrackingReliable = nowReliable;
            if (IsTrackingReliable) {
                TrackingRecovered?.Invoke();
            } else {
                TrackingLost?.Invoke();
            }
        }
    }
}