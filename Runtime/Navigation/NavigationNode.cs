using System.Collections.Generic;

using UnityEngine;

namespace IndoorNavigation.Navigation {
    public sealed class NavigationNode : MonoBehaviour {
        [SerializeField]
        private string nodeId = "Node";

        [SerializeField]
        private List<NavigationNode> neighbors = new List<NavigationNode>();

        [SerializeField]
        private Color nodeColor = Color.cyan;

        [SerializeField]
        private Color edgeColor = Color.yellow;

        [SerializeField]
        [Min(0.05f)]
        private float nodeRadius = 0.12f;

        public string NodeId {
            get {
                return nodeId;
            }
        }

        public IReadOnlyList<NavigationNode> Neighbors {
            get {
                return neighbors;
            }
        }

        private void OnDrawGizmos() {
            Gizmos.color = nodeColor;
            Gizmos.DrawSphere(transform.position, nodeRadius);

            Gizmos.color = edgeColor;
            for (int i = 0; i < neighbors.Count; i++) {
                if (neighbors[i] != null) {
                    Gizmos.DrawLine(transform.position, neighbors[i].transform.position);
                }
            }
        }
    }
}