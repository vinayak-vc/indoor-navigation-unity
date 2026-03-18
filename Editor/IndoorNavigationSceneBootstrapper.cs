using IndoorNavigation.Alignment;
using IndoorNavigation.Controllers;
using IndoorNavigation.Diagnostics;
using IndoorNavigation.Localization;
using IndoorNavigation.Navigation;
using IndoorNavigation.Rendering;
using IndoorNavigation.Tracking;

using UnityEditor;
using UnityEditor.SceneManagement;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;

namespace IndoorNavigation.EditorTools {
    public static class IndoorNavigationSceneBootstrapper {
        private const string RootPath = "Assets/Games/indoor-navigation-unity";
        private const string SceneFolder = RootPath + "/Scenes";
        private const string PrefabFolder = RootPath + "/Prefabs";
        private const string ConfigFolder = RootPath + "/Configs";
        private const string ScenePath = SceneFolder + "/IndoorNavigationStarter.unity";
        private const string ArrowPrefabPath = PrefabFolder + "/ArrowPlaceholder.prefab";
        private const string ConfigPath = ConfigFolder + "/ImmersalLocalizationConfig.asset";

        [MenuItem("Indoor Navigation/Create Starter Scene", priority = 1)]
        public static void CreateStarterScene() {
            CreateStarterSceneInternal(showDialog: true);
        }

        public static void CreateStarterSceneBatch() {
            CreateStarterSceneInternal(showDialog: false);
        }

        private static void CreateStarterSceneInternal(bool showDialog) {
            EnsureFolders();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "IndoorNavigationStarter";

            GameObject arSession = new GameObject("AR Session");
            arSession.AddComponent<ARSession>();

            GameObject sessionOrigin = new GameObject("AR Session Origin");
            GameObject arCamera = new GameObject("AR Camera");
            arCamera.tag = "MainCamera";
            arCamera.transform.SetParent(sessionOrigin.transform, false);
            Camera camera = arCamera.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            arCamera.AddComponent<AudioListener>();
            arCamera.AddComponent<ARCameraManager>();
            arCamera.AddComponent<ARCameraBackground>();

            GameObject navigationRoot = new GameObject("NavigationRoot");
            GameObject floorF1 = new GameObject("Floor_F1");
            floorF1.transform.SetParent(navigationRoot.transform, false);

            GameObject destinationMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            destinationMarker.name = "DestinationMarker";
            destinationMarker.transform.localScale = Vector3.one * 0.2f;
            destinationMarker.transform.position = new Vector3(0f, 0.1f, 2f);
            destinationMarker.SetActive(false);

            GameObject arrowContainer = new GameObject("ArrowContainer");
            GameObject pathVisuals = new GameObject("PathVisuals");
            pathVisuals.transform.SetParent(navigationRoot.transform, false);
            LineRenderer lineRenderer = pathVisuals.AddComponent<LineRenderer>();
            lineRenderer.widthMultiplier = 0.05f;
            lineRenderer.positionCount = 0;
            lineRenderer.useWorldSpace = true;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = new Color(0f, 0.9f, 1f, 1f);
            lineRenderer.endColor = new Color(0f, 0.9f, 1f, 1f);

            GameObject systems = new GameObject("IndoorNavigationSystem");
            AppController appController = systems.AddComponent<AppController>();
            NavigationController navigationController = systems.AddComponent<NavigationController>();
            ImmersalLocalizationProvider localizationProvider = systems.AddComponent<ImmersalLocalizationProvider>();
            ImmersalLocalizationBridge localizationBridge = systems.AddComponent<ImmersalLocalizationBridge>();
            ImmersalSdkCallbackAdapter callbackAdapter = systems.AddComponent<ImmersalSdkCallbackAdapter>();
            AlignmentService alignmentService = systems.AddComponent<AlignmentService>();
            FloorMapRegistry floorMapRegistry = systems.AddComponent<FloorMapRegistry>();
            NavMeshPathfindingService navMeshPathfindingService = systems.AddComponent<NavMeshPathfindingService>();
            TrackingStateMonitor trackingStateMonitor = systems.AddComponent<TrackingStateMonitor>();
            DebugOverlay debugOverlay = systems.AddComponent<DebugOverlay>();

            PathRenderer pathRenderer = pathVisuals.AddComponent<PathRenderer>();
            ArrowController arrowController = arrowContainer.AddComponent<ArrowController>();

            ImmersalLocalizationConfig config = GetOrCreateConfigAsset();
            GameObject arrowPrefab = GetOrCreateArrowPrefab();

            WireFloorMapRegistry(floorMapRegistry, navigationRoot.transform, floorF1);
            WirePathRenderer(pathRenderer, lineRenderer, destinationMarker.transform);
            WireArrowController(arrowController, arrowPrefab, arrowContainer.transform);
            WireLocalizationProvider(localizationProvider, localizationBridge, config, floorMapRegistry);
            WireCallbackAdapter(callbackAdapter, localizationBridge);
            WireAppController(appController, localizationProvider, alignmentService, trackingStateMonitor, floorMapRegistry, arCamera.transform);
            WireNavigationController(navigationController, navMeshPathfindingService, pathRenderer, arrowController, arCamera.transform);
            WireDebugOverlay(debugOverlay, appController, navigationController);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (showDialog) {
                EditorUtility.DisplayDialog(
                    "Indoor Navigation",
                    "Starter scene created at Assets/Games/indoor-navigation-unity/Scenes/IndoorNavigationStarter.unity",
                    "OK");
            }
        }

        private static void EnsureFolders() {
            if (!AssetDatabase.IsValidFolder(SceneFolder)) {
                AssetDatabase.CreateFolder(RootPath, "Scenes");
            }

            if (!AssetDatabase.IsValidFolder(PrefabFolder)) {
                AssetDatabase.CreateFolder(RootPath, "Prefabs");
            }

            if (!AssetDatabase.IsValidFolder(ConfigFolder)) {
                AssetDatabase.CreateFolder(RootPath, "Configs");
            }
        }

        private static GameObject GetOrCreateArrowPrefab() {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(ArrowPrefabPath);
            if (existing != null) {
                return existing;
            }

            GameObject tempArrow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tempArrow.name = "ArrowPlaceholder";
            tempArrow.transform.localScale = new Vector3(0.08f, 0.02f, 0.2f);
            tempArrow.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(tempArrow, ArrowPrefabPath);
            Object.DestroyImmediate(tempArrow);
            return prefab;
        }

        private static ImmersalLocalizationConfig GetOrCreateConfigAsset() {
            ImmersalLocalizationConfig config = AssetDatabase.LoadAssetAtPath<ImmersalLocalizationConfig>(ConfigPath);
            if (config != null) {
                return config;
            }

            config = ScriptableObject.CreateInstance<ImmersalLocalizationConfig>();
            AssetDatabase.CreateAsset(config, ConfigPath);
            return config;
        }

        private static void WireFloorMapRegistry(FloorMapRegistry registry, Transform navigationRoot, GameObject floorContent) {
            SerializedObject so = new SerializedObject(registry);
            SerializedProperty maps = so.FindProperty("maps");
            maps.ClearArray();
            maps.InsertArrayElementAtIndex(0);

            SerializedProperty binding = maps.GetArrayElementAtIndex(0);
            binding.FindPropertyRelative("FloorId").stringValue = "F1";
            binding.FindPropertyRelative("ImmersalMapId").intValue = 0;
            binding.FindPropertyRelative("NavigationRoot").objectReferenceValue = navigationRoot;
            binding.FindPropertyRelative("FloorContentRoot").objectReferenceValue = floorContent;
            binding.FindPropertyRelative("IsDefault").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WirePathRenderer(PathRenderer pathRenderer, LineRenderer lineRenderer, Transform destinationMarker) {
            SerializedObject so = new SerializedObject(pathRenderer);
            so.FindProperty("lineRenderer").objectReferenceValue = lineRenderer;
            so.FindProperty("targetMarker").objectReferenceValue = destinationMarker;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireArrowController(ArrowController arrowController, GameObject arrowPrefab, Transform arrowContainer) {
            SerializedObject so = new SerializedObject(arrowController);
            so.FindProperty("arrowPrefab").objectReferenceValue = arrowPrefab;
            so.FindProperty("arrowContainer").objectReferenceValue = arrowContainer;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireLocalizationProvider(
            ImmersalLocalizationProvider provider,
            ImmersalLocalizationBridge bridge,
            ImmersalLocalizationConfig config,
            FloorMapRegistry registry) {
            SerializedObject so = new SerializedObject(provider);
            so.FindProperty("bridge").objectReferenceValue = bridge;
            so.FindProperty("config").objectReferenceValue = config;
            so.FindProperty("floorMapRegistry").objectReferenceValue = registry;
            so.FindProperty("fallbackMapId").intValue = 0;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireCallbackAdapter(ImmersalSdkCallbackAdapter adapter, ImmersalLocalizationBridge bridge) {
            SerializedObject so = new SerializedObject(adapter);
            so.FindProperty("bridge").objectReferenceValue = bridge;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireAppController(
            AppController appController,
            ImmersalLocalizationProvider localizationProvider,
            AlignmentService alignmentService,
            TrackingStateMonitor trackingMonitor,
            FloorMapRegistry floorMapRegistry,
            Transform arCameraTransform) {
            SerializedObject so = new SerializedObject(appController);
            so.FindProperty("localizationProvider").objectReferenceValue = localizationProvider;
            so.FindProperty("alignmentService").objectReferenceValue = alignmentService;
            so.FindProperty("trackingStateMonitor").objectReferenceValue = trackingMonitor;
            so.FindProperty("floorMapRegistry").objectReferenceValue = floorMapRegistry;
            so.FindProperty("arCameraTransform").objectReferenceValue = arCameraTransform;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireNavigationController(
            NavigationController navigationController,
            NavMeshPathfindingService pathfindingService,
            PathRenderer pathRenderer,
            ArrowController arrowController,
            Transform userTransform) {
            SerializedObject so = new SerializedObject(navigationController);
            so.FindProperty("pathfindingService").objectReferenceValue = pathfindingService;
            so.FindProperty("pathRenderer").objectReferenceValue = pathRenderer;
            so.FindProperty("arrowController").objectReferenceValue = arrowController;
            so.FindProperty("userTransform").objectReferenceValue = userTransform;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireDebugOverlay(DebugOverlay overlay, AppController appController, NavigationController navigationController) {
            SerializedObject so = new SerializedObject(overlay);
            so.FindProperty("appController").objectReferenceValue = appController;
            so.FindProperty("navigationController").objectReferenceValue = navigationController;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}