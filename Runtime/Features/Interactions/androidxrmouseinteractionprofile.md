# Android XR Mouse Interaction Profile

Enables the OpenXR interaction profile for Android XR Mouse and exposes the
`<AndroidXRMouse>` device layout within the
[Unity Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/).

## Available controls

| OpenXR Path              | Unity Control Name | Type       |
| ------------------------ | ------------------ | ---------- |
| `/input/aim/pose`        | pose               | Pose       |
| `/input/select/click`    | primaryButton      | Boolean    |
| `/input/secondary/click` | secondaryButton    | Boolean    |
| `/input/tertiary/click`  | tertiaryButton     | Boolean    |
| `/input/scroll/value`    | scroll             | Vector2    |
| Unity Layout Only        | isTracked          | Flag Data  |
| Unity Layout Only        | trackingState      | Flag Data  |
| Unity Layout Only        | pointerPosition    | Vector3    |
| Unity Layout Only        | pointerRotation    | Quaternion |
