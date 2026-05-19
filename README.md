# Android XR Extensions for Unity

The Android XR Extensions for Unity provide APIs for the following:

1.  Session Management: required for all Android XR Extensions' features.
2.  Object Tracking.
3.  Image Tracking.
4.  Marker Tracking.
5.  QR Code Tracking.
6.  Scene Meshing.
7.  Unbounded Reference Space.
8.  Passthrough Composition Layer.
9.  Body Tracking *(Experimental)*.
10. Recommended Settings.
11. Fine Eye.
12. XR Streaming.
13. Cubemap Light Estimation.
14. Trackpad Gestures Interaction.

## How to use

1.  In **Window > Package Manager**, click the + button, and choose the **Add
    package from tarball...** option to import **Android XR Extensions for
    Unity** package.
2.  In **Project Settings > XR Plug-in Management**, switch to **Android** tab,
    then check **OpenXR** as the plug-in provider at OpenXR runtime.
3.  Under **XR Plug-in Management > OpenXR**, switch to **Android** tab, select
    **Android XR (Extensions)** feature group, or select individual features
    that your want to use at OpenXR runtime.

    NOTE: Some of them might have additional requirements or configuration,
    refer to the *README.md* from each sample for more details.

4.  Under **XR Plug-in Management > Project Validation**, fix all **OpenXR**
    related issues. This will help to configure your **Player Settings**.
5.  In **Player > Android > Identification > Minimal API Level**, select 24 or
    higher level. This is required by OpenXR Loader. Otherwise, it can fail the
    build.
6.  Build your project targeting **Android** platform.
