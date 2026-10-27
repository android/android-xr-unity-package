# Spatial Entities

Demonstrates spatial entity features at OpenXR runtime.

## Turn on Spatial Entity features

To enable this sample:

*   Navigate to **Edit** > **Project Settings** > **XR Plug-in Management** >
    **OpenXR**.
*   Switch to the **Android** platform tab.
*   Under **Android XR (Extensions)** feature group:
    *   Select **Android XR (Extensions): Session Management**, open the
        settings editor for the feature and uncheck **Subsampling (Vulkan)** if
        you have enabled before.
*   Under **Android XR Spatial Entities** feature group:
    *   Select **Spatial Entities (Extensions): Bounding Box**.
*   Under **XR Plug-in Management > Project Validation**, fix all **OpenXR**
    related issues. This will help to configure your **Player Settings**.

If you have **Unity OpenXR: Spatial Entities** package imported, you can also
enable following spatial entity features:

*   Navigate to **Edit** > **Project Settings** > **XR Plug-in Management** >
    **OpenXR**, switch to **Spatial Entities** feature group.
*   Select **Spatial Entities: Anchor**.
*   Select **Spatial Entities: Plane**.
*   Refer to
    [Unity OpenXR: Spatial Entities](https://docs.unity3d.com/Packages/com.unity.xr.spatialentities-openxr@6.6/manual/index.html)
    manual for the latest updates.

If you have **Unity OpenXR Android XR** package imported, you can also enable
passthrough by:

*   Navigate to **Edit** > **Project Settings** > **XR Plug-in Management** >
    **OpenXR**, switch to **Android XR** feature group.
*   Select **Android XR Support**.
*   Select **Android XR: AR Session**.
*   Select **Android XR: AR Camera**.
*   Refer to manual
    [AR Camera](https://docs.unity3d.com/Packages/com.unity.xr.androidxr-openxr@1.2/manual/features/camera.html#passthrough)
    for more details.
