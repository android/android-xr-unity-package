# Android XR Trackpad Gestures Interaction Profile

Enables the OpenXR interaction for Android XR Trackpad Gestures and exposes
the `<AndroidXRTrackpadGestures>` device layout within the
[Unity Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/).

## Available controls

| OpenXR Path                                     | Unity Control Name | Type       |
| ----------------------------------------------- | ------------------ | ---------- |
| `/input/scroll_gesture_android/activate_android`| scrollActivate     | Boolean    |
| `/input/scroll_gesture_android`                 | gestureScroll      | Vector2    |
| `/input/pinch_in_android/activate_android`      | pinchInActivate    | Boolean    |
| `/input/pinch_in_android/value`                 | pinchInValue       | Axis       |
| `/input/pinch_out_android/activate_android`     | pinchOutActivate   | Boolean    |
| `/input/pinch_out_android/value`                | pinchOutValue      | Axis       |
| `/input/rotate_android/activate_android`        | rotateActivate     | Boolean    |
| `/input/rotate_android/value`                   | rotateValue        | Axis       |
