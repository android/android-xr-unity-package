// <copyright file="XRControllerDisplay.cs" company="Google LLC">
//
// Copyright 2025 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

namespace Google.XR.Extensions.Samples.XRController
{
    using UnityEngine;
    using UnityEngine.InputSystem;

    /// <summary>
    /// Used to display the controller asset.
    /// </summary>
    public class XRControllerDisplay : MonoBehaviour
    {
#pragma warning disable CS0649 // Serialized fields don't need assignment
        [Header("Cached")]
        [SerializeField] private Transform _thumbStick;
        [SerializeField] private Transform _upperButton;
        [SerializeField] private Transform _lowerButton;
        [SerializeField] private Transform _systemButton;
        [SerializeField] private Transform _trigger;
        [SerializeField] private Transform _grip;

        [Header("Parameter")]
        [SerializeField] private Vector2 _maxThumbStickRot;
        [SerializeField] private float _pressedThumbStickOffset;
        [SerializeField] private float _pressedUpperBtnOffset;
        [SerializeField] private float _pressedLowerBtnOffset;
        [SerializeField] private float _pressedSystemBtnOffset;
        [SerializeField] private float _maxTriggerRot;
        [SerializeField] private float _maxGripRot;

        [Header("Input")]
        [SerializeField] private InputAction _thumbStickInput;
        [SerializeField] private InputAction _thumbStickPressedInput;
        [SerializeField] private InputAction _upperButtonPressedInput;
        [SerializeField] private InputAction _lowerButtonPressedInput;
        [SerializeField] private InputAction _systemButtonPressedInput;
        [SerializeField] private InputAction _triggerInput;
        [SerializeField] private InputAction _gripInput;
        [SerializeField] private bool _inverseThumbStickX;
        [SerializeField] private bool _inverseThumbStickY;
        [SerializeField] private Vector3 _gripRotationPivot;
        [SerializeField] private Vector3 _triggerRotationPivot;
#pragma warning restore CS0649

        private Quaternion _initThumbStickRot;
        private Vector3 _initThumbStickPos;
        private Vector3 _initUpperButtonPos;
        private Vector3 _initLowerButtonPos;
        private Vector3 _initSystemButtonPos;
        private Quaternion _initTriggerRot;
        private Quaternion _initGripRot;

        private void OnEnable()
        {
            _thumbStickInput.Enable();
            _thumbStickPressedInput.Enable();
            _upperButtonPressedInput.Enable();
            _lowerButtonPressedInput.Enable();
            _systemButtonPressedInput.Enable();
            _triggerInput.Enable();
            _gripInput.Enable();

            _thumbStickInput.performed += ThumbStickInputPerformed;
            _thumbStickInput.canceled += ThumbStickInputCanceled;
            _thumbStickPressedInput.started += ThumbStickPressedInputStarted;
            _thumbStickPressedInput.canceled += ThumbStickPressedInputCanceled;
            _upperButtonPressedInput.started += UpperButtonPressedInputStarted;
            _upperButtonPressedInput.canceled += UpperButtonPressedInputCanceled;
            _lowerButtonPressedInput.started += LowerButtonPressedInputStarted;
            _lowerButtonPressedInput.canceled += LowerButtonPressedInputCanceled;
            _systemButtonPressedInput.started += SystemButtonPressedInputStarted;
            _systemButtonPressedInput.canceled += SystemButtonPressedInputCanceled;
            _triggerInput.performed += TriggerInputPerformed;
            _triggerInput.canceled += TriggerInputCanceled;
            _gripInput.performed += GripInputPerformed;
            _gripInput.canceled += GripInputCanceled;
        }

        private void OnDisable()
        {
            _thumbStickInput.Disable();
            _thumbStickPressedInput.Disable();
            _upperButtonPressedInput.Disable();
            _lowerButtonPressedInput.Disable();
            _triggerInput.Disable();
            _gripInput.Disable();

            _thumbStickInput.performed -= ThumbStickInputPerformed;
            _thumbStickInput.canceled -= ThumbStickInputCanceled;
            _thumbStickPressedInput.started -= ThumbStickPressedInputStarted;
            _thumbStickPressedInput.canceled -= ThumbStickPressedInputCanceled;
            _upperButtonPressedInput.started -= UpperButtonPressedInputStarted;
            _upperButtonPressedInput.started -= UpperButtonPressedInputCanceled;
            _lowerButtonPressedInput.started -= LowerButtonPressedInputStarted;
            _lowerButtonPressedInput.started -= LowerButtonPressedInputCanceled;
            _systemButtonPressedInput.started -= SystemButtonPressedInputStarted;
            _systemButtonPressedInput.started -= SystemButtonPressedInputCanceled;
            _triggerInput.performed -= TriggerInputPerformed;
            _triggerInput.canceled -= TriggerInputCanceled;
            _gripInput.performed -= GripInputPerformed;
            _gripInput.canceled -= GripInputCanceled;
        }

        private void Start()
        {
            _initThumbStickRot = _thumbStick.localRotation;
            _initThumbStickPos = _thumbStick.localPosition;
            _initUpperButtonPos = _upperButton.localPosition;
            _initLowerButtonPos = _lowerButton.localPosition;
            _initSystemButtonPos = _systemButton.localPosition;
            _initTriggerRot = _trigger.localRotation;
            _initGripRot = _grip.localRotation;
        }

        private void ThumbStickInputPerformed(InputAction.CallbackContext obj)
        {
            Vector2 value = obj.ReadValue<Vector2>();
            float axisX = Mathf.Lerp(0f, _maxThumbStickRot.x, Mathf.Abs(value.y))
                          * -Mathf.Sign(value.y) * (_inverseThumbStickX ? -1f : 1f);
            float axisY = Mathf.Lerp(0f, _maxThumbStickRot.y, Mathf.Abs(value.x))
                          * -Mathf.Sign(value.x) * (_inverseThumbStickY ? -1f : 1f);
            _thumbStick.localRotation = Quaternion.Euler(
                _initThumbStickRot.eulerAngles + new Vector3(axisX, axisY, 0f));
        }

        private void ThumbStickInputCanceled(InputAction.CallbackContext obj)
        {
            _thumbStick.localRotation = _initThumbStickRot;
        }

        private void ThumbStickPressedInputStarted(InputAction.CallbackContext obj)
        {
            Vector3 targetPos = transform.TransformPoint(_initThumbStickPos) -
                                (_thumbStick.up * _pressedThumbStickOffset);
            _thumbStick.position = targetPos;
        }

        private void ThumbStickPressedInputCanceled(InputAction.CallbackContext obj)
        {
            _thumbStick.position = transform.TransformPoint(_initThumbStickPos);
        }

        private void UpperButtonPressedInputStarted(InputAction.CallbackContext obj)
        {
            Vector3 targetPos = transform.TransformPoint(_initUpperButtonPos) -
                                (_upperButton.up * _pressedUpperBtnOffset);
            _upperButton.position = targetPos;
        }

        private void UpperButtonPressedInputCanceled(InputAction.CallbackContext obj)
        {
            _upperButton.position = transform.TransformPoint(_initUpperButtonPos);
        }

        private void LowerButtonPressedInputStarted(InputAction.CallbackContext obj)
        {
            Vector3 targetPos = transform.TransformPoint(_initLowerButtonPos) -
                                (_lowerButton.up * _pressedLowerBtnOffset);
            _lowerButton.position = targetPos;
        }

        private void LowerButtonPressedInputCanceled(InputAction.CallbackContext obj)
        {
            _lowerButton.position = transform.TransformPoint(_initLowerButtonPos);
        }

        private void SystemButtonPressedInputStarted(InputAction.CallbackContext obj)
        {
            Vector3 targetPos = transform.TransformPoint(_initSystemButtonPos) -
                                (_systemButton.up * _pressedSystemBtnOffset);
            _systemButton.position = targetPos;
        }

        private void SystemButtonPressedInputCanceled(InputAction.CallbackContext obj)
        {
            _systemButton.position = transform.TransformPoint(_initSystemButtonPos);
        }

        private void TriggerInputPerformed(InputAction.CallbackContext obj)
        {
            float value = obj.ReadValue<float>();
            float rot = Mathf.Lerp(0f, _maxTriggerRot, Mathf.Abs(value));
            _trigger.localRotation = Quaternion.Euler(_initTriggerRot.eulerAngles +
                                                      _triggerRotationPivot * rot);
        }

        private void TriggerInputCanceled(InputAction.CallbackContext obj)
        {
            _trigger.localRotation = _initTriggerRot;
        }

        private void GripInputPerformed(InputAction.CallbackContext obj)
        {
            float value = obj.ReadValue<float>();
            float rot = Mathf.Lerp(0f, _maxGripRot, Mathf.Abs(value));
            _grip.localRotation = Quaternion.Euler(_initGripRot.eulerAngles +
                                                  _gripRotationPivot * rot);
        }

        private void GripInputCanceled(InputAction.CallbackContext obj)
        {
            _grip.localRotation = _initGripRot;
        }
    }
}
