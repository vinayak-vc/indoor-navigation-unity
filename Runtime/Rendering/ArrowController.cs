using System.Collections.Generic;

using IndoorNavigation.Core.Interfaces;
using IndoorNavigation.Core.Models;

using UnityEngine;

namespace IndoorNavigation.Rendering {
    public sealed class ArrowController : MonoBehaviour, IArrowController {
        [SerializeField]
        private GameObject arrowPrefab;

        [SerializeField]
        private Transform arrowContainer;

        [SerializeField]
        [Min(1)]
        private int maxArrows = 16;

        [SerializeField]
        [Min(0.2f)]
        private float arrowSpacingMeters = 1.25f;

        private readonly Queue<Transform> _pooledArrows = new Queue<Transform>();
        private readonly List<Transform> _activeArrows = new List<Transform>();

        public void RenderArrows(NavigationPath path, int startCornerIndex) {
            Clear();

            if (arrowPrefab == null || path == null || !path.IsValid) {
                return;
            }

            int safeStart = Mathf.Clamp(startCornerIndex, 0, path.Corners.Count - 2);
            int arrowsPlaced = 0;

            for (int segmentIndex = safeStart; segmentIndex < path.Corners.Count - 1 && arrowsPlaced < maxArrows; segmentIndex++) {
                Vector3 from = path.Corners[segmentIndex];
                Vector3 to = path.Corners[segmentIndex + 1];

                float segmentLength = Vector3.Distance(from, to);
                if (segmentLength < 0.01f) {
                    continue;
                }

                Vector3 direction = (to - from).normalized;
                float cursor = arrowSpacingMeters * 0.5f;

                while (cursor < segmentLength && arrowsPlaced < maxArrows) {
                    Vector3 position = from + direction * cursor;
                    Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
                    Transform arrow = GetArrow();
                    arrow.SetPositionAndRotation(position, rotation);
                    arrow.gameObject.SetActive(true);
                    _activeArrows.Add(arrow);

                    arrowsPlaced++;
                    cursor += arrowSpacingMeters;
                }
            }
        }

        public void Clear() {
            for (int i = 0; i < _activeArrows.Count; i++) {
                Transform arrow = _activeArrows[i];
                arrow.gameObject.SetActive(false);
                _pooledArrows.Enqueue(arrow);
            }

            _activeArrows.Clear();
        }

        private Transform GetArrow() {
            if (_pooledArrows.Count > 0) {
                return _pooledArrows.Dequeue();
            }

            Transform parent = arrowContainer == null ? transform : arrowContainer;
            GameObject instance = Instantiate(arrowPrefab, parent);
            return instance.transform;
        }
    }
}
