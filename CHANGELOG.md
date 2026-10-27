# Changelog

All notable changes to the Android XR Extensions for Unity package will be added
here. It includes changes that affect public APIs, samples and runtime
behaviors.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this package adheres to
[Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.5.0] - 2026-09-30

### Editor version and package compatibility
  * Package dependencies updated in this version:
    * OpenXR Plugin (`com.unity.xr.openxr`) 1.18.0
    * XR Plugin Management (`com.unity.xr.management`) 4.7.0
  * Verified compatible packages:
    * Unity OpenXR Android XR (`com.unity.xr.androidxr-openxr`) 1.4.1
    * OpenXR Plugin (`com.unity.xr.openxr`) 1.18.0
    * AR Foundation (`com.unity.xr.arfoundation`) 6.6.2
    * Unity OpenXR: Spatial Entities (`com.unity.xr.spatialentities-openxr`) 6.6.2

### Known issues
  <!-- disableFinding(LINE_OVER_80) -->
  * Regression issue found in **OpenXR Plugin 1.18.0** where deserializing null features in `OpenXR Package Settings.asset` causes `NullReferenceException`, thrown by `UnityEditor.XR.OpenXR.Features.FeatureHelpersInternal.RefreshAllFeatureInfo`.
    * Reproducible in **OpenXR Plugin 1.19.0-pre.1** as well.
    * Expected to be formally fixed in a newer version of **OpenXR Plugin** package.
    * Workaround: manually edit `OpenXR Package Settings.asset` or delete the file and re-generate the asset.

### Added
  <!-- disableFinding(LINE_OVER_80) -->
  * New feature set **Android XR Spatial Entities**:
    * Groups additional spatial entity features which are using spatial entity framework.
    * It can be enabled alone or work together with [Unity OpenXR: Spatial Entities](https://docs.unity3d.com/Packages/com.unity.xr.spatialentities-openxr@6.6/manual/index.html) package.
    * New **SpatialEntities** sample to demonstrate the basic usage of spatial entity framework.
  * New **Spatial Entities (Extensions): Bounding Box** feature:
    * Provides [Bounding box detection](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@6.6/manual/features/bounding-box-detection.html) support using spaital entity framework.
  * New **Spatial Entities (Extensions): Shareable Anchors** feature:
    * Extends [anchor capabilities](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@6.6/manual/features/anchors.html) via a custom `XRShareableAnchorSubsystem` interface, which can work alone or together with other [`XRAnchorSubsystem`](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@6.6/api/UnityEngine.XR.ARSubsystems.XRAnchorSubsystem.html) providers.
    * `SpatialShareableAnchorFeature.EnableGeospatialHost`: editor setting to enable geospatial anchor usage at runtime. You can then check `SpatialShareableAnchorFeature.IsGeospatialHostSupported` to confirm if it's supported by the given runtime.
      * NOTE: **Android XR: Geospatial** feature is required for this setting.
    * `ARShareableAnchorManager.TryAttachGeospatialAnchor(XRGeospatialPose)`: attach a geospatial anchor at the given `XRGeospatialPose`.
    * `ARShareableAnchorManager.TryAttachSurfaceAnchorAsync(XRGeospatialSurfaceType, XRGeospatialPose, CancellationToken)`: attach a surface anchor (a specialized type of geospatial anchors) at the given `XRGeospatialPose` where `XRGeospatialPose.Altitude` represents the altitude in meters relative to the surface.
  * New APIs for **Android XR: Geospatial** feature:
    * `XRGeospatialManager.RequestedPlaneTracking`: editor setting to request to use plane tracking for surface anchors if available.
    * `XRGeospatialManager.PlaneTrackingInUse`: indicates whether plane tracking is currently in use for surface anchors.
    * `XRGeospatialManager.MaxNumberOfSurfaceAnchors`: gets the maximum number of surface anchors supported by the system.
  * New **Spatial Container** feature:
    * This feature allows the application content to be rendered within a single spatial container.
    * New APIs to configure the spatial container:
      * `XRSpatialContainerFeature.TryGetState(out XRSpatialContainerStateFlags)`
      * `XRSpatialContainerFeature.TryGetBounds(out Vector3)`
      * `XRSpatialContainerFeature.TryRequestBoundsMode(bool)`
      * `XRSpatialContainerFeature.TryRequestVisibility(bool)`
    * New callback events to monitor the spatial container changes:
      * `XRSpatialContainerFeature.OnSpatialContainerClosed`
      * `XRSpatialContainerFeature.OnSpatialContainerBoundsChanged`
      * `XRSpatialContainerFeature.OnSpatialContainerStateChanged`
      * `XRSpatialContainerFeature.OnSpatialContainerVisibilityRequestDenied`
      * `XRSpatialContainerFeature.OnSpatialContainerBoundsModeRequestDenied`
  * Added `XRSpatialSdkVersions.XRSpatialApiLevel4` to represent the latest Android XR OTA releases.

### Changed
  * Updated **Geospatial** sample with geospatial anchor usage.

### Deprecated
  * N/A

### Removed
  * N/A

### Fixed
  * Fixed an issue where `System.Runtime.CompilerServices.Unsafe` may not be available from the project, replaced by `Unity.Collections.LowLevel.Unsafe`.

## [1.4.0] - 2026-07-07

### Added
  * Google Cloud:
    * New **Android XR: Google Cloud** feature, providing authentication strategies: API Key, Keyless, and Authentication Token.
      * You can use feature settings to automatically configure authentication at session start up. Or use `TrySetAuthenticationAsync(XRAuthenticationStrategy, string)` for manual setup.
      * For strategy support status at runtime, check [Android XR devsite](https://developer.android.com/develop/xr/unity) for the latest update.
  * Geospatial:
    * New **Android XR: Geospatial** feature, enabling `XRGeospatialManager` to access geospatial capabilities. Including APIs:
      * `XRGeospatialManager.OnStateChanged`: the event invoked whenever the state changes.
      * `XRGeospatialManager.TryCheckVPSAvailabilityAsync(double, double)`: attempts to check the availability of VPS near a given location asynchronously.
      * `XRGeospatialManager.TryLocate(Pose, out XRGeospatialPoseResult)`: attempts to locate the geospatial pose for the given reference frame.
      * `XRGeospatialManager.TryLocate(XRGeospatialPose, out XRPoseResult)`: attempts to locate the pose in Unity world space from a geospatial pose.
    * Added new sample **Geospatial** to demonstrate the usage around Geospatial features.
  * Multimodal Face Tracking:
    * New **Android XR (Extensions): Multimodal Face** feature, which extends Unity's ARFace feature and exposes multimodal face tracking capabilities:
      * `Requested Face Tracking Mode` settings for this feature is used to configure the requested face tracking modality.
      * `ARFaceManager.TryGetFaceTrackerMode(out XRFaceTrackerMode)`: extension method to retrieve the current tracking modality.
      * See more details in [Unity OpenXR Android XR | Face Tracking](https://docs.unity3d.com/Packages/com.unity.xr.androidxr-openxr@1.3/manual/features/faces.html)
  * Battery State Display:
    * New **Battery State Display** feature, to query the battery status and level of input devices:
      * `XRBatteryStateDisplayFeature.TryGetBatteryStateDisplay(string, out BatteryStatus, out float)`: retrieves the battery status and level for a given input path.
    * Added new sample **Controller Battery State** to demonstrate how to query the battery state of the controllers.

### Changed
  * Samples updated to use URP material.
  * Removed **Android XR (Extensions): Image Tracking** feature usage from **ImageTracking** sample.

### Deprecated
  * None.

### Removed
  * Removed **Foveation (Legacy)** feature and Foveation Sample. Please use Unity OpenXR [Foveated Rendering](https://docs.unity3d.com/Packages/com.unity.xr.openxr@1.17/manual/features/foveatedrendering.html) from **OpenXR Plugin 1.17.0** instead with [XRDisplaySubsystem.foveatedRenderingLevel](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/XR.XRDisplaySubsystem-foveatedRenderingLevel.html).
  * Removed **Android XR (Extensions): Image Tracking** feature. For replacement, follow [Unity OpenXR: Spatial Entities](https://docs.unity3d.com/Packages/com.unity.xr.spatialentities-openxr@0.1/manual/index.html) for the latest spatial entity equivalence.

### Fixed
  * None.

## [1.3.2] - 2026-06-23

### Editor version and package compatibility
  * Verified compatible packages:
    * Unity OpenXR Android XR (`com.unity.xr.androidxr-openxr`) 1.3.0-pre.1
    * AR Foundation (`com.unity.xr.arfoundation`) 6.4.0

### Known issues
  * None.

### Added
  * None.

### Changed
  * None.

### Deprecated
  * None.

### Removed
  * None.

### Fixed
  * XR Spatial API support:
    * Typo fix `android.software.xr.api.SPATIAL` to `android.software.xr.api.spatial`.

## [1.3.1] - 2026-04-29

### Editor version and package compatibility
  * Package dependencies updated in this version:
    * OpenXR Plugin (`com.unity.xr.openxr`) 1.17.0-pre.2
    * AR Foundation (`com.unity.xr.arfoundation`) 6.4.0
  * Verified compatible packages:
    * Unity OpenXR Android XR (`com.unity.xr.androidxr-openxr`) 1.3.0-pre.1
    * AR Foundation (`com.unity.xr.arfoundation`) 6.4.0

### Known issues
  * None.

### Added
  * Image Tracking:
    * New OpenXR Feature **Android XR (Extensions): Image Tracking**, which provides marker tracking via `ARTrackedImageManager`. Including APIs:
      * `AndroidXRImageTrackingSubsystem.OnImageTrackingConfigured`: the event indicating when asynchronous image tracking configuration completes.
      * `AndroidXRImageTrackingSubsystem.OnImageTrackingLost`: the event raised if image tracking encounters an internal failure.
    * Added image reference in **ImageTracking** sample.
  * Added `XRUnboundedRefSpaceFeature.IsExtensionEnabled` to indicate if the required extension is available at runtime.
  * Expanded **Android XR Streaming** support with DirectX graphics APIs. You can now select Vulkan, Direct3D12, and / or Direct3D11 for Play Mode.
  * Trackpad Gestures:
    * New additive OpenXR Interaction **Android XR Trackpad Gestures Interaction**, which provides ability to listen for trackpad gesture inputs.

### Changed
  * Promoted `XRSystemStateFeature` from experimental to public as `XRRecommendedSettings`.
  * Changed `XrInputModality` to be flags, allowing multiple modalities to be combined.

### Deprecated
  * None.

### Removed
  * Removed feature **Android XR Mouse Interaction Profile**. Please use [Android Mouse Interaction Profile](https://docs.unity3d.com/Packages/com.unity.xr.openxr@1.17/manual/features/androidmouseinteraction.html) from **OpenXR Plugin 1.17.0-pre.2** instead.

### Fixed
  * Fixed typo around `IsExtensionEnabled` in `XRPassthroughFeature`, `XRFineEyeFeature`, and `XRUnboundedRefSpaceFeature`.
  * Fixed various other typos in comments and code.

## [1.3.0] - 2026-02-19

### Editor version and package compatibility
  * This version of the package requires minimal Unity Editor version `6000.3.6f1`. You can install the official **Unity 6.3** (e.g. `6000.3.5f2` or newer) from the Unity Hub.
  * Package dependencies updated in this version:
    * OpenXR Plugin (`com.unity.xr.openxr`) 1.17.0-pre.1
    * AR Foundation (`com.unity.xr.arfoundation`) 6.3.3
    * XR Plugin Management (`com.unity.xr.management`): 4.5.4
  * Verified compatible packages:
    * Unity OpenXR Android XR (`com.unity.xr.androidxr-openxr`) 1.2.0
    * AR Foundation (`com.unity.xr.arfoundation`) 6.4.0
    * Composition Layer (`com.unity.xr.compositionlayers`) 2.3.0
    * XR Hands (`com.unity.xr.hands`) 1.7.3
    * Universal Render Pipeline (`com.unity.render-pipelines.universal`) 17.3.0

### Known issues
  * Regression issue found in **OpenXR Plugin 1.16.1** where projects hit validation issues with an error icon on the OpenXR Feature but no message showing under **XR Plug-in Management > Project Validation** panel.
    * Workaround: check individual samples' `README.md` on how to set up the sample or [Android XR devsite](https://developer.android.com/develop/xr/unity) for more instructions.
    * To include a formal fix, please upgrade to **OpenXR Plugin 1.17.0-pre.1** or newer versions.
  * Regression issue found in Unity Editors where **Package Manager** shows invalid signature errors under public packages.
    * Workaround: In case it blocks package importing, navigate to the problematic package, click **Manage** dropdown and select **customize** to include a customized copy of the package source into your project. You can then dismiss the Editor error and use it as before.
    * To include a formal fix, please upgrade Editor to `6000.3.5f2` or newer. See more details in [Community Discussions](https://discussions.unity.com/t/package-manager-invalid-signatures-issue/1705385/103).
    * Also check out [Export and sign your UPM package](https://docs.unity3d.com/6000.3/Documentation/Manual/cus-export.html) about the package signature.

### Added
  * XR Spatial API support:
    * Provide public API under `XRSessionFeature` to configure `uses-feature` element for `android.software.xr.api.SPATIAL` on the application's manifest file.
    * Implement `IXRSpatialSdk` interface for OpenXR features with XR Spatial API requirements.
    * For more details, check examples from [PackageManager features for XR apps](https://developer.android.com/develop/xr/jetpack-xr-sdk/build-immersive#packagemanager-features).
  * Fine Eye:
    * New OpenXR Feature **Android XR (Extensions): Fine Eye** which is a supplement to **Unity OpenXR Android XR**'s **Android XR: AR Face** feature and provides access to fine eye poses.
    * Provide extension method `TryGetFineEyePoses(this ARFace, out AndroidOpenXREyeTrackingStates,out Pose, out Pose)`.
    * See more details in [Unity OpenXR Android XR | Face Tracking](https://docs.unity3d.com/Packages/com.unity.xr.androidxr-openxr@1.2/manual/features/faces.html)
  * Android XR Streaming:
    * New OpenXR Feature **Android XR Streaming** which provides instructions on how to prepare projects ready for **Android XR Direct Preview**.
    * Added a Dynamic-link library (DLL) build of the native plugin to support **PlayMode** on **Windows** Editor.
    * For more information about **Android XR Direct Preview**, refer to [Android XR devsite](https://www.android.com/xr/) updates.
  * Light Estimation Cubemap:
    * New OpenXR Feature `XRCubemapLightEstimation` which provides lighting environment probes via `AREnvironmentProbeManager`.

### Changed
  * `IsExtensionEnabled` in each OpenXR Feature now reflects both extension availability and system supportness to better report runtime capabilities. Applications should check `IsExtensionEnabled.HasValue` and `IsExtensionEnabled.Value` before accessing other feature APIs, and handle unsupported features accordingly.
  * Updated samples **ObjectTracking**, **ImageTracking**, **XRControllerSample**, **SceneMeshing** with **AR Camera Manager** which prepare for passthrough usage from [AR Camera](https://docs.unity3d.com/Packages/com.unity.xr.androidxr-openxr@1.2/manual/features/camera.html#passthrough).
  * Added new settings **Joint Set** under `XRBodyTrackingFeature`. It defaults to `XRBodyJointSet.UpperBody` to match previous behavior.
    * Introduces two `XRBodyJointSet` types. `XRAvatarSkeletonJointID` is replaced by corresponding enum types `XRUpperBodyJointID` and `XRFullBodyJointID`.
    * `XRAvatarSkeletonJointIDUtility` is replaced by `XRBodyJointSetUtility` to support multiple joint sets.

### Deprecated
  * Deprecated **Subsampling (Vulkan)** option in **Android XR (Extensions): Session Management** feature. Please use `FoveatedRenderingFeature.TrySetSubsampledLayoutEnabled(bool)` from **OpenXR Plugin** package instead. See details in [Subsampled layout](https://docs.unity3d.com/Packages/com.unity.xr.openxr@1.16/manual/features/subsampledlayout.html).

### Removed
  * Removed feature **Android XR (Extensions): Hand Mesh**. Please use [Hand Mesh Data](https://docs.unity3d.com/Packages/com.unity.xr.androidxr-openxr@1.2/manual/features/hand-mesh-data.html) from **Unity OpenXR Android XR** instead.
  * Removed feature **Environment Blend Mode** and `TransparentBackgroundRendererFeature`. Please use [AR Camera](https://docs.unity3d.com/Packages/com.unity.xr.androidxr-openxr@1.2/manual/features/camera.html#passthrough) from **Unity OpenXR Android XR** to enable passthrough background.
  * Remove feature **Android XR: Face Tracking**. Please use [AR Face](https://docs.unity3d.com/Packages/com.unity.xr.androidxr-openxr@1.2/manual/features/faces.html) from **Unity OpenXR Android XR** instead.

### Fixed
  * Fixed a typo in public API `TryGetMarkerData` within `AndroidXRImageTrackingSubsystem`.

## [1.2.0] - 2025-09-30

### Editor version and package compatibility

  * This version of the package requires minimal Unity Editor version `6000.1.17f1`. You can install the official **Unity 6.1** (e.g. `6000.1.17f1` or newer) from the Unity Hub.
  * Package dependencies updated in this version:
    * OpenXR Plugin (`com.unity.xr.openxr`) 1.15.1
    * AR Foundation (`com.unity.xr.arfoundation`) 6.2.0
    * XR Plugin Management (`com.unity.xr.management`): 4.5.1
  * Verified compatible packages:
    * Unity OpenXR Android XR (`com.unity.xr.androidxr-openxr`) 1.1.0-pre.1
    * OpenXR Plugin (`com.unity.xr.openxr`) 1.15.1
    * AR Foundation (`com.unity.xr.arfoundation`) 6.3.0-pre.1
    * Composition Layer (`com.unity.xr.compositionlayers`) 2.1.0
    * XR Hands (`com.unity.xr.hands`) 1.6.1
    * Universal Render Pipeline (`com.unity.render-pipelines.universal`) 17.1.0

### Known issues
  * `SpaceWarpFeature` generates motion vectors in the wrong NDC space.
    * Unity has released the fix in **6000.1.13f1** and OpenXR Plugin **1.5.1**. Please upgrade Unity Editor and OpenXR packages to include the change.
    * Under **Application SpaceWarp** setting menu, check **Use Right Handed NDC** to apply the correct space on Android XR.
    * Also noted, on Android XR headsets, SpaceWarp does not need to be updated with the main camera's current position or rotation.
    * See more details in [Application SpaceWarp in OpenXR](https://docs.unity3d.com/Packages/com.unity.xr.openxr@1.15/manual/features/spacewarp.html).

### Added
  * Added extension function `XRMeshSubsystem.IsSceneMeshId(TrackableId)`.

### Changed
  * None.

### Deprecated
  * None.

### Removed
  * Removed editor settings `SpaceWarp` from `XRSessionFeature`. Please use `SpaceWarpFeature` from **OpenXR Plugins** instead.

### Fixed
  * Fixed `ARMeshManager` compatibility issue with `XRSceneMeshingFeature`.
  * Fixed alpha environment blend mode compatibility issue with URP.

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
    * Foveation feature is required for **Subsampling (Vulkan)**. Otherwise, the application may result in faulty rendering.
    * For projects with Universal Render Pipeline (URP), select **Foveated Rendering** feature. Check [Foveated rendering in OpenXR](https://docs.unity3d.com/Packages/com.unity.xr.openxr@1.12/manual/features/foveatedrendering.html) for more details.
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
    * It's confirmed as a regression issue from **OpenXR Plugin** package, expected to be fixed by future releases.
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
  * Added experimental features support which use OpenXR extensions `XR_ANDROIDX_*`. **Note**:
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
