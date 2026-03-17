using IndoorNavigation.Core.Models;
using UnityEngine;

namespace IndoorNavigation.Core.Interfaces
{
    public interface IPathfindingService
    {
        bool TryBuildPath(Vector3 startWorldPosition, Vector3 destinationWorldPosition, out NavigationPath path);
    }
}
