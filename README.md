# Indoor Navigation Unity (AR + VPS)

Production-ready modular indoor AR navigation foundation built with Unity, AR Foundation, NavMesh, and Immersal VPS integration points.

## Project Overview
This module implements an indoor AR navigation pipeline:
1. Localize device camera with Immersal VPS.
2. Align virtual floor/navigation content to real-world pose by moving `NavigationRoot`.
3. Compute path on NavMesh from user location to destination.
4. Render path line, directional arrows, and destination marker in AR.
5. Handle tracking loss and relocalization.
6. Support multi-floor map architecture with floor/map switching.

## Folder Structure
```text
Assets/Games/indoor-navigation-unity
├── Editor
│   └── NavigationNodeTools.cs
├── Runtime
│   ├── Alignment
│   │   └── AlignmentService.cs
│   ├── Controllers
│   │   ├── AppController.cs
│   │   └── NavigationController.cs
│   ├── Core
│   │   ├── Enums
│   │   │   └── LocalizationStatus.cs
│   │   ├── Interfaces
│   │   │   ├── IAlignmentService.cs
│   │   │   ├── IArrowController.cs
│   │   │   ├── ILocalizationProvider.cs
│   │   │   ├── IPathfindingService.cs
│   │   │   └── IPathRenderer.cs
│   │   └── Models
│   │       ├── LocalizationPose.cs
│   │       └── NavigationPath.cs
│   ├── Diagnostics
│   │   └── DebugOverlay.cs
│   ├── Localization
│   │   ├── ImmersalLocalizationBridge.cs
│   │   ├── ImmersalLocalizationConfig.cs
│   │   ├── ImmersalLocalizationProvider.cs
│   │   └── ImmersalSdkCallbackAdapter.cs
│   ├── Navigation
│   │   ├── FloorMapRegistry.cs
│   │   ├── NavigationNode.cs
│   │   └── NavMeshPathfindingService.cs
│   ├── Rendering
│   │   ├── ArrowController.cs
│   │   └── PathRenderer.cs
│   └── Tracking
│       └── TrackingStateMonitor.cs
├── CHANGELOG.md
└── README.md
```

## Architecture
Layered architecture with clear separation of concerns:

- Localization Layer
  - `ImmersalLocalizationProvider`: localization orchestration (retry/relocalization/status).
  - `ImmersalLocalizationBridge`: adapter point between app code and SDK callbacks.
  - `ImmersalSdkCallbackAdapter`: helper to forward SDK callbacks into the bridge.
- Alignment Layer
  - `AlignmentService`: computes `NavigationRoot` transform from localized camera pose and applies instant/smoothed correction.
- Navigation Layer
  - `FloorMapRegistry`: floor/map bindings and active floor switching.
  - `NavMeshPathfindingService`: NavMesh-based path generation.
- Rendering Layer
  - `PathRenderer`: line + destination marker rendering (limited future corners).
  - `ArrowController`: pooled directional arrow rendering.
- Core Controllers
  - `AppController`: app lifecycle, localization->alignment pipeline, relocalization trigger flow.
  - `NavigationController`: destination set, repathing, waypoint progression, renderer updates.
- Diagnostics/Tooling
  - `DebugOverlay`: runtime state HUD.
  - `NavigationNode` + `NavigationNodeTools`: optional editor graph placement utilities.

## Setup Instructions
### 1) Prerequisites
1. Unity LTS (recommended: 2022.3+).
2. AR Foundation + ARCore XR Plugin + ARKit XR Plugin.
3. URP configured for target project.
4. Immersal Unity SDK imported into project.

### 2) Scene Setup
1. Add AR scene essentials:
   - `AR Session`
   - `AR Session Origin` with AR Camera
2. Add `NavigationRoot` empty GameObject in scene.
3. Parent your floor model + NavMesh surfaces under `NavigationRoot`.
4. Bake NavMesh on walkable geometry (per floor model).
5. Add manager GameObject `IndoorNavigationSystem` and attach:
   - `AppController`
   - `ImmersalLocalizationProvider`
   - `AlignmentService`
   - `TrackingStateMonitor`
   - `FloorMapRegistry`
   - `NavigationController`
   - `NavMeshPathfindingService`
   - `PathRenderer`
   - `ArrowController`
   - `DebugOverlay` (optional)

### 3) Wire References
1. In `FloorMapRegistry`, add one or more floor map entries:
   - `FloorId`
   - `ImmersalMapId`
   - `NavigationRoot`
   - `FloorContentRoot`
2. In `AppController`:
   - Assign localization/alignment/tracking/floor refs.
   - Assign AR camera transform.
3. In `NavigationController`:
   - Assign path service, path renderer, arrow controller, and user transform.
4. In `PathRenderer`:
   - Assign `LineRenderer` and destination marker transform.
5. In `ArrowController`:
   - Assign arrow prefab and optional container.

### Fast Scene Bootstrap (Recommended)
Use the included editor bootstrapper to auto-create a ready-to-run starter scene with all core objects/components wired:
1. In Unity menu, click `Indoor Navigation -> Create Starter Scene`.
2. Generated scene path:
   - `Assets/Games/indoor-navigation-unity/Scenes/IndoorNavigationStarter.unity`
3. Generated assets:
   - `Assets/Games/indoor-navigation-unity/Prefabs/ArrowPlaceholder.prefab`
   - `Assets/Games/indoor-navigation-unity/Configs/ImmersalLocalizationConfig.asset`
4. After generation, set your real:
   - Immersal map IDs in `FloorMapRegistry`
   - floor model under `NavigationRoot`
   - NavMesh bake on walkable geometry
   - Immersal SDK callbacks to `ImmersalSdkCallbackAdapter`

## Immersal Setup Guide (Mapping + Map ID)
1. Create map(s) in Immersal Mapper/Portal for each floor.
2. Publish maps and note each `Map ID`.
3. Add each map ID into `FloorMapRegistry` entries.
4. Integrate Immersal localization callback into this module:
   - Add `ImmersalLocalizationBridge`.
   - Add `ImmersalSdkCallbackAdapter`.
   - Connect your Immersal SDK success/fail callback handlers to:
     - `OnLocalizationSuccessWithPose(position, rotation, confidence, mapId)` (preferred),
     - or `OnLocalizationSuccess(Transform localizedCameraInMapSpace)`.
5. Trigger localize requests by subscribing Immersal localize call to `ImmersalLocalizationBridge.LocalizationRequested`.

## Alignment Logic
- AR Camera is never moved.
- Localization gives camera pose in map coordinates.
- `AlignmentService` computes root transform:
  - `rootRotation = cameraRotation * inverse(localizedRotation)`
  - `rootPosition = cameraPosition - rootRotation * localizedPosition`
- Applies either:
  - Instant alignment (first lock), or
  - Smooth correction (`ApplySmooth`) for relocalization/drift correction.

## Navigation Logic
1. Start position sampled from current user transform.
2. Destination sampled from selected target.
3. `NavMesh.CalculatePath` generates corners.
4. `PathRenderer` draws limited corner subset for performance.
5. `ArrowController` places pooled arrows along near path segments.
6. Waypoints advance when user reaches threshold; path recalculates on interval.

## Build Instructions
### Android
1. Install Android Build Support + SDK/NDK in Unity Hub.
2. Switch platform to Android.
3. Enable ARCore support in XR Plug-in Management.
4. Set camera permissions and required AR settings in Player Settings.
5. Build APK/AAB and test on ARCore-supported device.

### iOS
1. Install iOS Build Support in Unity Hub + Xcode on macOS.
2. Switch platform to iOS.
3. Enable ARKit support in XR Plug-in Management.
4. Ensure camera usage description is set in Player Settings.
5. Build Xcode project, sign, and deploy to ARKit-compatible iPhone/iPad.

## Error Handling
- Localization failure:
  - `ImmersalLocalizationProvider.LocalizationFailed` event + warning logs.
- Tracking loss:
  - `TrackingStateMonitor` triggers relocalization request.
- Invalid path:
  - `NavigationController` clears renderers and logs failure.

## Performance Notes
- Repathing is interval-based (`repathIntervalSeconds`), not every frame.
- Smooth alignment only ticks while active.
- Path rendering limits visible corners.
- Arrow rendering uses pooling and max-arrow cap.

## Limitations and Known Issues
- Immersal SDK APIs vary by version; callback wiring is done via bridge/adapter, not hardcoded SDK classes.
- Multi-floor transitions are map-based; cross-floor routing graph/elevator logic is not included yet.
- Localization confidence threshold tuning is environment-dependent.
- Path quality depends on NavMesh bake quality and floor geometry correctness.

## Future Improvements
- Cross-floor navigation planner (stairs/elevators/transfer nodes).
- Cloud Anchors/VPS fallback provider implementing `ILocalizationProvider`.
- Robust localization quality scoring (pose covariance/temporal filtering).
- Route instruction UI (turn-by-turn cards + voice prompts).
- Analytics, telemetry, and remote config for localization thresholds.
