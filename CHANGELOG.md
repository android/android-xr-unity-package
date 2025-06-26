# Changelog

All notable changes to the Android XR Extensions for Unity package will be added
here. It includes changes that affect public APIs, samples and runtime
behaviors.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this package adheres to
[Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.1.0] - 2025-06-27

### Editor version and package compatibility

  * This version of the package requires minimal Unity Editor version `6000.1.0b12`. You can install the official **Unity 6.1** (e.g. `6000.1.0f1` or newer) from the Unity Hub.
  * Verified compatible packages:
    * Unity OpenXR Android XR (`com.unity.xr.androidxr-openxr`) 1.0.0-pre.3

### Known issues
  * None

### Added
  * None

### Changed
  * **Subsampling (Vulkan)** is now enabled by default in **Android XR (Extensions): Session Management**. To use it at runtime:
    * Foveation feature is required for **Subsampling (Vulkan)**. Otherwise, the application may result in rendering faulty.
    * For projects with Universal Render Pipeline (URP), select **Foveated Rendering** featurem. Check [Foveated rendering in OpenXR](https://docs.unity3d.com/Packages/com.unity.xr.openxr@1.12/manual/features/foveatedrendering.html) for more details.
    * For projects with Built-in Render Pipeline (BiRP), select **Foveation (Legacy)** feature for legacy support.
  * **Scene Meshing** is promoted from experimental feature to release feature.

### Deprecated
  * None

### Removed
  * None

### Fixed
  * Fixed [issue#7](https://github.com/android/android-xr-unity-package/issues/7): Compile Errors with Google.XR.Extensions on Non-Android Platforms.
  * Fixed enum value and comments in `XRFaceParameterIndices`.

## [1.0.0] - 2025-05-07

### Editor version and package compatibility

  * This version of the package requires minimal Unity Editor version `6000.1.0b12`. You can install the official **Unity 6.1** (e.g. `6000.1.0f1` or newer) from the Unity Hub.
  * Package dependencies updated in this version:
    * OpenXR Plugin (`com.unity.xr.openxr`) 1.14.2
    * AR Foundation (`com.unity.xr.arfoundation`) 6.1.0
  * Verified compatible packages:
    * Unity OpenXR Android XR (`com.unity.xr.androidxr-openxr`) 1.0.0-pre.2
    * Composition Layer (`com.unity.xr.compositionlayers`) 2.0.0

### Known issues
  * **Vulkan Subsampling** now causes rendering issues in multiple samples, including **ImageTracking**, **HandMesh**, **SceneMeshing**, and **XRController**.
    * It's confirmed as a regression issue from **OpenXR Plugin** package, expect to be fixed by future releases.
    * **Workaround**: To disable it, under the setting menu of **Android XR (Extensions) Session Management**, unselect **Subsampling (Vulkan)**.

### Added
  * Marker Tracking and QR Code Tracking:
    * New OpenXR Feature `XRMarkerTrackingFeature` which provides marker tracking via `ARTrackedImageManager`. Including APIs:
      * Extension method `ARTrackedImage.IsMarker()` to check if the image instance is a marker.
      * Extension method `ARTrackedImage.TryGetMarkerData(out XRMarkerDictionary, out int)` to get marker data from a marker instance.
      * New ScriptableObject `XRMarkerDatabase`, a container of `XRMarkerDatabaseEntry`, used to manage marker references in an `XRReferenceImageLibrary`.
    * New OpenXR Feature `XRQrCodeTrackingFeature` which provides QR Code tracking via `ARTrackedImageManager`. Including APIs:
      * Extension method `ARTrackedImage.IsQrCode()` to check if the image instance is a QR Code.
      * Extension method `ARTrackedImage.TryGetQrCodeData(out string)` to get the QR code data from a QR Code instance.
    * New implementation of `XRImageTrackingSubsystem`. To use it with AR Foundation's `ARTrackedImageManager`, create an `XRReferenceImageLibrary` with a QR Code reference named **QrCode** and/or updates marker references from `XRMarkerDatabase`.
    * New sample **ImageTracking** which demonstrates the usage of QR Code tracking and marker tracking.
  * Added experimental features support which use OpenXR extensions `XR_ANDROIDX_*`. **Noted**:
    * Applications can then access `ANDROIDX` extensions at runtime but **cannot be published in Google Play Store**.
    * Experimental features can take several iterations towards formal releases which **may include breaking changes, deprecation, and removal**.
  * Body Tracking **(Experimental*)**:
    * New Experimental OpenXR Feature `XRBodyTrackingFeature` which provides body tracking via `ARHumanBodyManager`.
    * New sample **BodyTracking** which demonstrates the usage of body tracking.
  * System State **(Experimental*)**:
    * New Experimental OpenXR Feature `XRSystemStateFeature` which provides system state via `TryGetSystemState(out systemState)`.
    * Added system state API usage to **BlendMode** sample.
  * Scene Meshing **(Experimental*)**:
    * New Experimental OpenXR Feature `XRSceneMeshingFeature` which provides scene mesh data via `XRMeshSubsystem`.
    * Added new sample **SceneMeshing** which demonstrates the usage of scene meshing API.

### Changed
  * **Subsampling (Vulkan)** is now disabled by default in **Android XR (Extensions) Session Management**.
  * Face Tracking:
    * Updated `XRFaceParameterIndices` with new enum values for tongue blendshapes.
    * Updated `XRFaceState` with new property `ConfidenceRegions`, indexed by new enum `XRFaceConfidenceRegion`.

### Deprecated
  * N/A

### Removed
  * Removed `XRTrackableFeature`, `XRAnchorFeature`, and **Trackables** sample. Please use `ARPlaneFeature` and `ARAnchorFeature` from **Unity OpenXR Android XR** instead.
  * Removed `XRDisplayRefreshRateFeature` and **DisplayRefreshRate** sample. Please use `DisplayUtilitiesFeature` from **Unity OpenXR Android XR** instead.

### Fixed
  * Fixed typo in `XRHandMeshFeature` which referred to depth texture permission rather than hand tracking permission.
  * Fixed bug in `XRHandMeshFeature` which started a subsystem in the `OnSubsystemStop()` callback.

## [0.9.1] - 2025-04-03

### Added
  * New Sample **XRController** which syncs and displays Samsung Controller.

### Changed
  * N/A

### Deprecated
  * N/A

### Removed
  * N/A

### Fixed
  * Updated Android XR permissions.

## [0.9.0] - 2024-12-12

This is the first release of **Android XR Extensions for Unity
<com.google.xr.extensions> v0.9.0** developer preview package. This version
supports the following features:

-   Device Tracking via OpenXR Plug-in Provider
-   Session Management via Session Subsystem Provider
-   Planes via Plane Subsystem Provider
-   Anchors via Anchor Subsystem Provider
-   Tracked Objects via Object Tracking Subsystem Provider
-   Hand Meshes via Mesh Subsystem Provider.
-   Passthrough Composition Layer via OpenXR Composition Layer Provider
-   Face Tracking via MonoBehaviour `XRFaceTrackingManager`
-   Unbounded Reference Space
-   Display Refresh Rate
-   Environment Blend Mode
-   Foveation (Legacy)
-   Android XR Mouse Interaction Profile
