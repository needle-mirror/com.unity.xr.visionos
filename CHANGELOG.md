---
uid: vosxr-changelog
---
# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [0.6.2] - 2023-11-13

### Added

### Changed

### Deprecated

### Removed

### Fixed

### Security

## [0.6.1] - 2023-11-09

### Added

### Changed

### Deprecated

### Removed

### Fixed

### Security

## [0.6.0] - 2023-11-08

### Added
- Added additional input controls on `VisionOSSpatialPointerDevice` which are needed to drive an XR Ray Interactor.
- Added VR samples for both Built-in and Universal Render Pipelines.

### Changed

### Deprecated

### Removed

### Fixed
- Fixed compile errors when the project has `com.unity.render-pipelines.core` but not `com.unity.render-pipelines.universal`.
- Fixed issue with over releasing material references for canvas items.
 
### Security

## [0.5.0] - 2023-10-26

### Added
- `VisionOSSpatialPointerDevice` for pinch/gaze input support in VR mode.

### Changed

### Deprecated

### Removed

### Fixed

### Security

## [0.4.3] - 2023-10-13

## [0.4.2] - 2023-10-12

## Fixed
- Fixed an issue where VR builds would only render to the left eye in device builds when using the built-in pipeline.

## [0.4.1] - 2023-10-06

### Added
- PolySpatial now supports Xcode 15.1 beta 1 and visionOS 1.0 beta 4
- Project Validation rules for Linear Color Space, ARSession + ARInputManager components, and cameras generating depth textures inside of the VisionOS XR package

### Removed

- Removed `VisionOSSettings.renderMode`, `VisionOSSettings.deviceTarget`, and related `visionos_config.h` file that was generated during builds. The XR plugin will automatically switch between single-pass and multi-pass rendering depending on whether the app was built for the visionOS simulator or a device.

### Fixed

- Fixed an issue where VR builds would only render to the left eye in device builds when using the built-in pipeline.

## [0.3.3] - 2023-09-28

### Changed

- Revert changes that were mistakenly included in 0.3.2

## [0.3.2] - 2023-09-27

### Changed

- Use renamed `ar_skeleton_get_anchor_from_joint_transform_for_joint` API. This fixes an issue where builds are rejected on TestFlight for using deprecated `ar_skeleton_get_skeleton_root_transform_for_joint` API.

## [0.3.1] - 2023-09-13

### Fixed

- Fixed linker errors in Xcode when building without visionOS loader enabled.

## [0.3.0] - 2023-09-12

### Added

- VisionOSSessionSubsystem now returns a structure including the native session pointer from the `nativePtr` property.

### Changed

- Xcode beta 8 and visionOS beta 3 compatibility.
- Static libraries were rebuilt with Xcode Version 15.0 beta 8 (15A5229m).

### Fixed

- Fixed an issue where plane detection would be disabled if meshing was not enabled.

## [0.2.0] - 2023-08-21

### Changed

- Xcode beta 5 and visionOS beta 2 compatibility
- Static libraries were rebuilt with Xcode Version 15.0 beta 2 (15A5161b).

### Fixed

- Fixed issues with AR mesh alignment.
- Fixed issues with AR anchor position.
- Fixed issues with AR authorization and session startup.
- Fixed an issue where Plane alignment values would not match the values expected by AR Foundation.

## [0.1.3] - 2023-07-19

### This is the first release of *Unity Package Apple visionOS XR Plugin*.

*Provides XR support for visionOS*
