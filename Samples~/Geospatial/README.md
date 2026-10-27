# Geospatial

Demonstrates geospatial features at OpenXR runtime.

## Prerequisites

*   Please ensure your device has connected to the internet for GPS services.
*   Please ensure you have create a Google Cloud project for the application on
    the [Google Cloud Console](https://console.cloud.google.com/) and set up
    credentials for Geospatial API access.

## Turn on Geospatial features

To enable this sample:

*   Navigate to **Edit** > **Project Settings** > **XR Plug-in Management** >
    **OpenXR**.
*   Switch to the **Android** platform tab.
*   Under **Android XR (Extensions)** feature group:
    *   Select **Android XR (Extensions): Session Management**.
    *   Select **Android XR: Google Cloud**.
    *   Select **Android XR: Geospatial**.
*   Under **Android XR Spatial Entities** feature group:
    *   Select **Spatial Entities (Extensions): Shareable Anchors**, open the
        settings editor for the feature and select **Enable Geospatial Host**.
*   Under **XR Plug-in Management > Project Validation**, fix all **OpenXR**
    related issues. This will help to configure your **Player Settings**.

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

Configure Google Cloud Authentication:

*   Depending your Google Cloud credentials, you may configure Keyless or API
    Key through Editor:
    *   Navigate to OpenXR Feature **Android XR: Google Cloud**.
        *   Click gear icon to open feature setting window.
        *   Under **Authentication Strategy**:
            *   Select **Keyless** if you have set it up for the application.
            *   Select **API Key** and provide credential key if you have set it
                up for the application.
    *   With a valid strategy, Google Cloud will attempt to set authentication
        automatically at session start up.
    *   Navigate to sample scene `Geospatial.unity`.
        *   In Hierarchy panel, click **XR Origin** game object.
        *   Under component **Geospatial Controller**:
            *   Set **Max Attempt Count** to `0`. So it's fully relying on
                auto-setting.
*   Alternatively, you can rely on manually setting, and update Authentication
    Token based on its expiration time.
    *   Navigate to OpenXR Feature **Android XR: Google Cloud**.
        *   Click gear icon to open feature setting window.
        *   Under **Authentication Strategy**:
            *   Select **None**, so the auto-setting is disabled.
    *   Navigate to sample scene `Geospatial.unity`.
        *   In Hierarchy panel, click **XR Origin** game object.
        *   Under component **Geospatial Controller**:
            *   Set **Max Attempt Count** to a positive number.
    *   In script `GeospatialController.TrySetAuthWithRetry()`,
        *   Modify the `strategy` and `credential` based on your Google Cloud
            project.

Attach geospatial anchors:

*   When geospatial anchor is supported at the runtime, you can perform right
    hand pinch to attach geospatial anchors. It circles through
    `XRGeospatialSurfaceType` and places different surface anchors with
    different colors.
