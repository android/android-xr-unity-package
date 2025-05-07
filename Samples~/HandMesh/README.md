# Hand Mesh

Sample for testing the XR_ANDROID_hand_mesh OpenXR extension.

## Turn on Hand Mesh feature

To enable this sample:

*   Navigate to **Edit** > **Project Settings** > **XR Plug-in Management** >
    **OpenXR**.
*   Switch to the **Android** platform tab.
*   Select **Android XR (Extensions): Session Management**.
    *   Known issue: Vulkan Subsampling now causes faulty rendering, and will
        cause the hand mesh to disappear when runtime permission is handled.
        To disable it, under the setting menu of **Android XR (Extensions)
        Session Management**, unselect **Subsampling (Vulkan)**.
*   Select **Android XR (Extensions): Hand Mesh**.
*   Select **Environment Blend Mode**, open the settings editor for the feature
    and select **Override Blend Mode** and change the **Request Mode** to
    **Alpha Blend**.
*   Under **XR Plug-in Management > Project Validation**, fix all **OpenXR**
    related issues. This will help to configure your **Player Settings**.
