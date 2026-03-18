using IndoorNavigation.Core.Models;
using IndoorNavigation.Navigation;
using IndoorNavigation.Rendering;

using UnityEngine;

namespace IndoorNavigation.Controllers {
    public sealed class NavigationController : MonoBehaviour {
        [SerializeField]
        private NavMeshPathfindingService pathfindingService;

        [SerializeField]
        private PathRenderer pathRenderer;

        [SerializeField]
        private ArrowController arrowController;

        [SerializeField]
        private Transform userTransform;

        [SerializeField]
        [Min(0.05f)]
        private float waypointReachDistanceMeters = 0.7f;

        [SerializeField]
        [Min(0.05f)]
        private float repathIntervalSeconds = 0.75f;

        private NavigationPath _currentPath;
        private Vector3 _currentDestination;
        private int _currentWaypointIndex;
        private float _repathTimer;
        private bool _hasDestination;

        public bool IsNavigating {
            get {
                return _currentPath != null && _currentPath.IsValid;
            }
        }

        private void Update() {
            if (!_hasDestination || userTransform == null) {
                return;
            }

            _repathTimer += Time.deltaTime;
            if (_repathTimer >= repathIntervalSeconds) {
                _repathTimer = 0f;
                TryRebuildPath();
            }

            if (_currentPath == null || !_currentPath.IsValid) {
                return;
            }

            AdvanceWaypointIfNeeded();
            pathRenderer?.RenderPath(_currentPath, _currentWaypointIndex);
            arrowController?.RenderArrows(_currentPath, _currentWaypointIndex);
        }

        public void SetDestination(Transform destination) {
            if (destination == null) {
                Debug.LogWarning("[NavigationController] Destination transform is null.");
                return;
            }

            SetDestination(destination.position);
        }

        public void SetDestination(Vector3 destinationWorldPosition) {
            _currentDestination = destinationWorldPosition;
            _hasDestination = true;
            _currentWaypointIndex = 0;
            _repathTimer = 0f;
            TryRebuildPath();
        }

        public void StopNavigation() {
            _hasDestination = false;
            _currentWaypointIndex = 0;
            _currentPath = null;
            pathRenderer?.Clear();
            arrowController?.Clear();
        }

        private void TryRebuildPath() {
            if (pathfindingService == null) {
                Debug.LogError("[NavigationController] PathfindingService reference is missing.");
                return;
            }

            if (userTransform == null) {
                Debug.LogError("[NavigationController] User transform reference is missing.");
                return;
            }

            if (!pathfindingService.TryBuildPath(userTransform.position, _currentDestination, out NavigationPath newPath)) {
                Debug.LogWarning("[NavigationController] Failed to build navigation path.");
                pathRenderer?.Clear();
                arrowController?.Clear();
                _currentPath = null;
                return;
            }

            _currentPath = newPath;
            _currentWaypointIndex = Mathf.Clamp(_currentWaypointIndex, 0, _currentPath.Corners.Count - 1);
            pathRenderer?.RenderPath(_currentPath, _currentWaypointIndex);
            arrowController?.RenderArrows(_currentPath, _currentWaypointIndex);
        }

        private void AdvanceWaypointIfNeeded() {
            if (_currentPath == null || _currentWaypointIndex >= _currentPath.Corners.Count) {
                return;
            }

            Vector3 targetWaypoint = _currentPath.Corners[_currentWaypointIndex];
            float distance = Vector3.Distance(userTransform.position, targetWaypoint);
            if (distance > waypointReachDistanceMeters) {
                return;
            }

            _currentWaypointIndex++;
            if (_currentWaypointIndex >= _currentPath.Corners.Count - 1) {
                StopNavigation();
            }
        }
    }
}
