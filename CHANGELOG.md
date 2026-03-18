# Changelog

All notable changes to this module are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [0.1.0] - 2026-03-17
### Added
- Core contracts and models:
  - `LocalizationStatus`
  - `LocalizationPose`
  - `NavigationPath`
  - `ILocalizationProvider`, `IAlignmentService`, `IPathfindingService`, `IPathRenderer`, `IArrowController`
- Localization layer:
  - `ImmersalLocalizationConfig`
  - `ImmersalLocalizationBridge`
  - `ImmersalLocalizationProvider`
  - `ImmersalSdkCallbackAdapter`
- Alignment layer:
  - `AlignmentService` with instant and smooth realignment modes.
- Navigation layer:
  - `FloorMapRegistry` (multi-floor/map-ready bindings).
  - `NavMeshPathfindingService`.
  - Optional debug node component `NavigationNode`.
- Rendering layer:
  - `PathRenderer` with target marker support.
  - `ArrowController` with object pooling.
- Controllers:
  - `AppController` orchestration of localization, tracking, alignment, and relocalization.
  - `NavigationController` route management and waypoint progression.
- Diagnostics/tooling:
  - `TrackingStateMonitor`.
  - `DebugOverlay`.
  - Editor utility `NavigationNodeTools`.
  - Editor utility `IndoorNavigationSceneBootstrapper` for one-click starter scene generation.
- Documentation:
  - Complete architecture + setup guide in `README.md`.

### Changed
- Replaced placeholder module README with full production setup and architecture documentation.
- Added fast bootstrap scene creation workflow to README.

### Fixed
- Corrected file placement to ensure all module scripts are inside `Assets/Games/indoor-navigation-unity`.
- Added navigation null checks to prevent runtime reference exceptions.

### Removed
- No removals in this release.
