# Blend Mode Sample

Demonstrates Environment Blend Mode Feature at OpenXR runtime targeting Android
Platform.

## Turn on Environment Blend Mode feature

To enable this sample:

*   Navigate to **Edit** > **Project Settings** > **XR Plug-in Management** >
    **OpenXR**.
*   Switch to **Android** platform tab.
*   Select **Android XR (Extensions): Session Management**.
*   Select **Environment Blend Mode**: Enable Editor and runtime configurations
    of blend modes.
*   (Optional) Select **Android XR: System State (Experimental*)**.
    *   NOTE: Applications with experimental features cannot be published in
        Google Play Store.
*   Under **XR Plug-in Management > Project Validation**, fix all **OpenXR**
    related issues. This will help to configure your **Player Settings**.

## Configure blend mode

To change the default mode:

*   Click the setting icon at **Environment Blend Mode** feature from **OpenXR**
    Android feature group.
*   Choose the desired mode under **Request Mode** dropdown menu, such as
    **Alpha Blend**.

The sample automatically switches among all supported modes.
