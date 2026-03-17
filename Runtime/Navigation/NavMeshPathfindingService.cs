using IndoorNavigation.Core.Interfaces;
using IndoorNavigation.Core.Models;
using UnityEngine;
using UnityEngine.AI;

namespace IndoorNavigation.Navigation
{
    public sealed class NavMeshPathfindingService : MonoBehaviour, IPathfindingService
    {
        [SerializeField]
        private int areaMask = NavMesh.AllAreas;

        [SerializeField]
        private float sampleDistance = 1.5f;

        public bool TryBuildPath(Vector3 startWorldPosition, Vector3 destinationWorldPosition, out NavigationPath path)
        {
            path = new NavigationPath();

            if (!TrySampleNavMeshPosition(startWorldPosition, out Vector3 sampledStart))
            {
                Debug.LogWarning("[NavMeshPathfindingService] Could not sample start position on NavMesh.");
                return false;
            }

            if (!TrySampleNavMeshPosition(destinationWorldPosition, out Vector3 sampledDestination))
            {
                Debug.LogWarning("[NavMeshPathfindingService] Could not sample destination position on NavMesh.");
                return false;
            }

            NavMeshPath navMeshPath = new NavMeshPath();
            bool pathFound = NavMesh.CalculatePath(sampledStart, sampledDestination, areaMask, navMeshPath);
            if (!pathFound || navMeshPath.status != NavMeshPathStatus.PathComplete || navMeshPath.corners.Length < 2)
            {
                Debug.LogWarning("[NavMeshPathfindingService] Invalid or incomplete path.");
                return false;
            }

            path.SetCorners(navMeshPath.corners);
            return true;
        }

        private bool TrySampleNavMeshPosition(Vector3 input, out Vector3 sampledPosition)
        {
            sampledPosition = input;
            if (NavMesh.SamplePosition(input, out NavMeshHit hit, sampleDistance, areaMask))
            {
                sampledPosition = hit.position;
                return true;
            }

            return false;
        }
    }
}
