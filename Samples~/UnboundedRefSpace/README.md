# Unbounded Reference Space Sample

Demonstrates Android XR Unbounded Reference Space feature and general
XRInputSubsystem usage at OpenXR runtime targeting Android Platform with Android
XR Provider.

## Enable Android XR Unbounded Reference Space

To enable the sample:

*   Navigate to **Edit** > **Project Settings** > **XR Plug-in Management** >
    **OpenXR**.
*   Switch to the **Android** tab.
*   Select **Android XR (Extensions): Session Management**.
*   Select **Android XR: Unbounded Reference Space**.
*   Under **XR Plug-in Management > Project Validation**, fix all **OpenXR**
    related issues. This will help to configure your **Player Settings**.

Configure reference space in Editor:

*   Add **XR > XR Origin (VR)** or **XR > XR Origin (Mobile AR)** gameobject to
    the scene and replace the default camera.
*   Under **XR Origin** gameobject > **XR Origin** component, select desired
    **Tracking Origin Mode**, such as **Unbounded**.

Configure reference space in script:

*   Refer to sample source `UnboundedRefSpaceController.cs`.
