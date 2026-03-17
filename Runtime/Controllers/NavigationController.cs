using IndoorNavigation.Core.Models;
using IndoorNavigation.Navigation;
using IndoorNavigation.Rendering;
using UnityEngine;

namespace IndoorNavigation.Controllers
{
    public sealed class NavigationController : MonoBehaviour
    {
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

        private NavigationPath currentPath;
        private Vector3 currentDestination;
        private int currentWaypointIndex;
        private float repathTimer;
        private bool hasDestination;

        public bool IsNavigating => currentPath != null && currentPath.IsValid;

        private void Update()
        {
            if (!hasDestination || userTransform == null)
            {
                return;
            }

            repathTimer += Time.deltaTime;
            if (repathTimer >= repathIntervalSeconds)
            {
                repathTimer = 0f;
                TryRebuildPath();
            }

            if (currentPath == null || !currentPath.IsValid)
            {
                return;
            }

            AdvanceWaypointIfNeeded();
            pathRenderer?.RenderPath(currentPath, currentWaypointIndex);
            arrowController?.RenderArrows(currentPath, currentWaypointIndex);
        }

        public void SetDestination(Transform destination)
        {
            if (destination == null)
            {
                Debug.LogWarning("[NavigationController] Destination transform is null.");
                return;
            }

            SetDestination(destination.position);
        }

        public void SetDestination(Vector3 destinationWorldPosition)
        {
            currentDestination = destinationWorldPosition;
            hasDestination = true;
            currentWaypointIndex = 0;
            repathTimer = 0f;
            TryRebuildPath();
        }

        public void StopNavigation()
        {
            hasDestination = false;
            currentWaypointIndex = 0;
            currentPath = null;
            pathRenderer?.Clear();
            arrowController?.Clear();
        }

        private void TryRebuildPath()
        {
            if (pathfindingService == null)
            {
                Debug.LogError("[NavigationController] PathfindingService reference is missing.");
                return;
            }

            if (userTransform == null)
            {
                Debug.LogError("[NavigationController] User transform reference is missing.");
                return;
            }

            if (!pathfindingService.TryBuildPath(userTransform.position, currentDestination, out NavigationPath newPath))
            {
                Debug.LogWarning("[NavigationController] Failed to build navigation path.");
                pathRenderer?.Clear();
                arrowController?.Clear();
                currentPath = null;
                return;
            }

            currentPath = newPath;
            currentWaypointIndex = Mathf.Clamp(currentWaypointIndex, 0, currentPath.Corners.Count - 1);
            pathRenderer?.RenderPath(currentPath, currentWaypointIndex);
            arrowController?.RenderArrows(currentPath, currentWaypointIndex);
        }

        private void AdvanceWaypointIfNeeded()
        {
            if (currentPath == null || currentWaypointIndex >= currentPath.Corners.Count)
            {
                return;
            }

            Vector3 targetWaypoint = currentPath.Corners[currentWaypointIndex];
            float distance = Vector3.Distance(userTransform.position, targetWaypoint);
            if (distance > waypointReachDistanceMeters)
            {
                return;
            }

            currentWaypointIndex++;
            if (currentWaypointIndex >= currentPath.Corners.Count - 1)
            {
                StopNavigation();
            }
        }
    }
}
