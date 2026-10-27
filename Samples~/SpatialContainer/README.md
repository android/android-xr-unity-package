# Spatial Container

Demonstrates spatial container feature (multi-app pixel submission) at OpenXR
runtime.

## Turn on Spatial Container feature

To enable this sample:

*   Navigate to **Edit** > **Project Settings** > **XR Plug-in Management** >
    **OpenXR**.
*   Switch to the **Android** platform tab.
*   Select **Android XR (Extensions): Session Management**, open the settings
    editor for the feature and uncheck **Subsampling (Vulkan)**
*   Select **Spatial Container**.
*   Under **XR Plug-in Management > Project Validation**, fix all **OpenXR**
    related issues. This will help to configure your **Player Settings**.

If you have **Unity OpenXR Android XR** imported, you can also enable
passthrough by:

*   Navigate to **Edit** > **Project Settings** > **XR Plug-in Management** >
    **OpenXR**, switch to **Android XR** feature group.
*   Select **Android XR Support**.
*   Select **Android XR: AR Session**.
*   Select **Android XR: AR Camera**.
*   Refer to manual
    [AR Camera](https://docs.unity3d.com/Packages/com.unity.xr.androidxr-openxr@1.2/manual/features/camera.html#passthrough)
    for more details.
