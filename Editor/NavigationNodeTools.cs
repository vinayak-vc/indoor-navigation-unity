using IndoorNavigation.Navigation;

using UnityEditor;

using UnityEngine;

namespace IndoorNavigation.EditorTools {
    public static class NavigationNodeTools {
        [MenuItem("Indoor Navigation/Create Navigation Node %#n")]
        public static void CreateNodeAtSceneViewPivot() {
            SceneView sceneView = SceneView.lastActiveSceneView;
            Vector3 position = sceneView != null ? sceneView.pivot : Vector3.zero;

            GameObject node = new GameObject("NavigationNode");
            Undo.RegisterCreatedObjectUndo(node, "Create Navigation Node");
            node.transform.position = position;
            node.AddComponent<NavigationNode>();

            Selection.activeGameObject = node;
        }
    }
}