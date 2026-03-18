using System;
using System.Collections.Generic;

using UnityEngine;

namespace IndoorNavigation.Core.Models {
    [Serializable]
    public sealed class NavigationPath {
        [SerializeField]
        private List<Vector3> corners = new List<Vector3>();

        public IReadOnlyList<Vector3> Corners {
            get {
                return corners;
            }
        }

        public bool IsValid {
            get {
                return corners.Count >= 2;
            }
        }
        public float TotalLength { get; private set; }

        public NavigationPath() {
        }

        public NavigationPath(IEnumerable<Vector3> points) {
            corners.AddRange(points);
            RecalculateLength();
        }

        public void SetCorners(IEnumerable<Vector3> points) {
            corners.Clear();
            corners.AddRange(points);
            RecalculateLength();
        }

        private void RecalculateLength() {
            TotalLength = 0f;
            for (int i = 1; i < corners.Count; i++) {
                TotalLength += Vector3.Distance(corners[i - 1], corners[i]);
            }
        }
    }
}
