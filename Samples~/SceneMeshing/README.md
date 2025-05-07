# Scene Meshing

Demonstrates scene meshing feature at OpenXR runtime.

## Turn on Scene Meshing feature

To enable this sample:

*   Navigate to **Edit** > **Project Settings** > **XR Plug-in Management** >
    **OpenXR**.
*   Switch to the **Android** platform tab.
*   Select **Android XR (Extensions): Session Management**.
    *   Known issue: Vulkan Subsampling now causes faulty rendering.
        To disable it, under the setting menu of **Android XR (Extensions)
        Session Management**, unselect **Subsampling (Vulkan)**.
*   Select **Android XR (Extensions): Scene Meshing (Experimental*)**.
    *   NOTE: Applications with experimental features cannot be published in
        Google Play Store.
*   Select **Environment Blend Mode**, open the settings editor for the feature
    and select **Override Blend Mode** and change the **Request Mode** to
    **Alpha Blend**.
*   Under **XR Plug-in Management > Project Validation**, fix all **OpenXR**
    related issues. This will help to configure your **Player Settings**.
