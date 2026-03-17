using IndoorNavigation.Core.Interfaces;
using IndoorNavigation.Core.Models;
using UnityEngine;

namespace IndoorNavigation.Rendering
{
    [RequireComponent(typeof(LineRenderer))]
    public sealed class PathRenderer : MonoBehaviour, IPathRenderer
    {
        [SerializeField]
        private LineRenderer lineRenderer;

        [SerializeField]
        [Min(2)]
        private int maxVisibleCorners = 8;

        [SerializeField]
        private Transform targetMarker;

        public void RenderPath(NavigationPath path, int startCornerIndex)
        {
            if (lineRenderer == null)
            {
                lineRenderer = GetComponent<LineRenderer>();
            }

            if (path == null || !path.IsValid)
            {
                Clear();
                return;
            }

            int safeStart = Mathf.Clamp(startCornerIndex, 0, path.Corners.Count - 1);
            int cornerCount = Mathf.Min(maxVisibleCorners, path.Corners.Count - safeStart);
            if (cornerCount < 2)
            {
                Clear();
                return;
            }

            lineRenderer.positionCount = cornerCount;
            for (int i = 0; i < cornerCount; i++)
            {
                lineRenderer.SetPosition(i, path.Corners[safeStart + i]);
            }

            if (targetMarker != null)
            {
                targetMarker.position = path.Corners[path.Corners.Count - 1];
                if (!targetMarker.gameObject.activeSelf)
                {
                    targetMarker.gameObject.SetActive(true);
                }
            }
        }

        public void Clear()
        {
            if (lineRenderer != null)
            {
                lineRenderer.positionCount = 0;
            }

            if (targetMarker != null && targetMarker.gameObject.activeSelf)
            {
                targetMarker.gameObject.SetActive(false);
            }
        }
    }
}
