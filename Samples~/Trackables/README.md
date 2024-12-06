# Trackables Sample

Demonstrates Android XR Trackable feature and general AR Foundation usage at
OpenXR runtime targeting Android Platform with Android XR Provider.

## Enable Android XR Subsystems

To enable the sample:

*   Navigate to **Edit** > **Project Settings** > **XR Plug-in Management** >
    **OpenXR**.
*   Switch to the **Android** tab.
*   Select **Android XR (Extensions)** feature group.
*   Enable **Android XR (Extensions): Session Management**.
*   Enable **Android XR (Extensions): Plane**.
*   Enable **Android XR (Extensions): Anchor**.
*   Under **XR Plug-in Management > Project Validation**, fix all **OpenXR**
    related issues. This will help to configure your **Player Settings**.

You can also use **Raycast** provided by **Unity OpenXR AndroidXR** package when v0.2.0+ is
available from this project:
*   Navigate to **Edit** > **Project Settings** > **XR Plug-in Management** >
    **OpenXR**.
*   Switch to the **Android** tab.
*   Select **Android XR** feature group.
*   Enable **Android XR: AR Session** feature.
*   Enable **Android XR: AR Raycast** feature.

## Configuration of Anchor Persistence

To use Anchor Persistence:

*   Click on the gear icon of **Android XR (Extensions): Anchor** feature.
*   Select **Use Persistence**.

The Trackables sample will try to place persisted Anchor and resolve the anchor from previous
sessions.
