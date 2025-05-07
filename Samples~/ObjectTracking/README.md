# Object Tracking Sample

Demonstrates Android XR Object Tracking feature and general AR Foundation usage
at OpenXR runtime targeting Android Platform with Android XR Provider.

## Enable Android XR Subsystems

To enable the sample:

*   Navigate to **Edit** > **Project Settings** > **XR Plug-in Management** >
    **OpenXR**.
*   Switch to the **Android** tab.
*   Select **Android XR (Extensions): Session Management**.
*   Select **Android XR (Extensions): Object Tracking**.
*   Select **Environment Blend Mode** and open **Setting** menu, set **Request
    Mode** to **Alpha Blend** which gives a better visual result.
*   Under **XR Plug-in Management > Project Validation**, fix all **OpenXR**
    related issues. This will help to configure your **Player Settings**.

If you have **Unity OpenXR Android XR** imported, you can also enable plane
tracking with following steps to render planes as the baseline:

*   Select **Android XR Support**, required by all Android XR features.
*   Select **Android XR: AR Session**, required by AR Foundation features.
*   Select **Android XR: Plane**, providing plane tracking on Android XR.
*   Under **XR Plug-in Management > Project Validation**, fix all **OpenXR**
    related issues.

NOTE: Although `AndroidXRObjectTrackingSubsystem` doesn't use reference
libraries on object detection, a default `XRReferenceObjectLibrary` is required
by `XRObjectTrackingSubsystem` and `ARTrackedObjectManager` to work around
exceptions and spamming errors. Expect future update in AR Foundation package to
formally support none-library use case.
