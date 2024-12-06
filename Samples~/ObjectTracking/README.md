# Object Tracking Sample

Demonstrates Android XR Object Tracking feature and general AR Foundation usage
at OpenXR runtime targeting Android Platform with Android XR Provider.

## Enable Android XR Subsystems

To enable the sample:

*   Navigate to **Edit** > **Project Settings** > **XR Plug-in Management** >
    **OpenXR**.
*   Switch to the **Android** tab.
*   Select **Android XR (Extensions) Session Management**.
*   Select **Android XR (Extensions) Plane**.
*   Select **Android XR (Extensions) Object Tracking**.
*   Under **XR Plug-in Management > Project Validation**, fix all **OpenXR**
    related issues. This will help to configure your **Player Settings**.

NOTE: Although `AndroidXRObjectTrackingSubsystem` doesn't use reference
libraries on object detection, a default `XRReferenceObjectLibrary` is required
by `XRObjectTrackingSubsystem` and `ARTrackedObjectManager` to work around
exceptions and spamming errors. Expect future update in AR Foundation package to
formally support none-library use case.
