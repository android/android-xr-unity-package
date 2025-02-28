// <copyright file="XRControllerDisplay.cs" company="Samsung Electronics Co.">
//
// Copyright 2025 Samsung Electronics Co.
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

namespace Google.XR.Extensions.Samples.Controller.Script
{
    using UnityEngine;
    using UnityEngine.InputSystem;
    
    public class XRControllerDisplay : MonoBehaviour
    {
        [Header("Cached")]
        [SerializeField] private Transform thumbStick;
        [SerializeField] private Transform upperButton;
        [SerializeField] private Transform lowerButton;
        [SerializeField] private Transform systemButton;
        [SerializeField] private Transform trigger;
        [SerializeField] private Transform grip;

        [Header("Parameter")]
        [SerializeField] private Vector2 maxThumbStickRot;
        [SerializeField] private float pressedThumbStickOffset;
        [SerializeField] private float pressedUpperBtnOffset;
        [SerializeField] private float pressedLowerBtnOffset;
        [SerializeField] private float pressedSystemBtnOffset;
        [SerializeField] private float maxTriggerRot;
        [SerializeField] private float maxGripRot;

        [Header("Input")]
        [SerializeField] private InputAction thumbStickInput;
        [SerializeField] private InputAction thumbStickPressedInput;
        [SerializeField] private InputAction upperButtonPressedInput;
        [SerializeField] private InputAction lowerButtonPressedInput;
        [SerializeField] private InputAction systemButtonPressedInput;
        [SerializeField] private InputAction triggerInput;
        [SerializeField] private InputAction gripInput;
        [SerializeField] private bool inverseThumbStickX;
        [SerializeField] private bool inverseThumbStickY;
        [SerializeField] private Vector3 gripRotationPivot;
        [SerializeField] private Vector3 triggerRotationPivot;
    

        private Quaternion _initThumbStickRot;
        private Vector3 _initThumbStickPos;
        private Vector3 _initUpperButtonPos;
        private Vector3 _initLowerButtonPos;
        private Vector3 _initSystemButtonPos;
        private Quaternion _initTriggerRot;
        private Quaternion _initGripRot;


        private void OnEnable()
        {
            thumbStickInput.Enable();
            thumbStickPressedInput.Enable();
            upperButtonPressedInput.Enable();
            lowerButtonPressedInput.Enable();
            systemButtonPressedInput.Enable();
            triggerInput.Enable();
            gripInput.Enable();
        
            thumbStickInput.performed += ThumbStickInputPerformed;
            thumbStickInput.canceled += ThumbStickInputCanceled;
            thumbStickPressedInput.started += ThumbStickPressedInputStarted;
            thumbStickPressedInput.canceled += ThumbStickPressedInputCanceled;
            upperButtonPressedInput.started += UpperButtonPressedInputStarted;
            upperButtonPressedInput.canceled += UpperButtonPressedInputCanceled;
            lowerButtonPressedInput.started += LowerButtonPressedInputStarted;
            lowerButtonPressedInput.canceled += LowerButtonPressedInputCanceled;
            systemButtonPressedInput.started += SystemButtonPressedInputStarted;
            systemButtonPressedInput.canceled += SystemButtonPressedInputCanceled;
            triggerInput.performed += TriggerInputPerformed;
            triggerInput.canceled += TriggerInputCanceled;
            gripInput.performed += GripInputPerformed;
            gripInput.canceled += GripInputCanceled;
        }
    
    
        private void OnDisable()
        {
            thumbStickInput.Disable();
            thumbStickPressedInput.Disable();
            upperButtonPressedInput.Disable();
            lowerButtonPressedInput.Disable();
            triggerInput.Disable();
            gripInput.Disable();
        
            thumbStickInput.performed -= ThumbStickInputPerformed;
            thumbStickInput.canceled -= ThumbStickInputCanceled;
            thumbStickPressedInput.started -= ThumbStickPressedInputStarted;
            thumbStickPressedInput.canceled -= ThumbStickPressedInputCanceled;
            upperButtonPressedInput.started -= UpperButtonPressedInputStarted;
            upperButtonPressedInput.started -= UpperButtonPressedInputCanceled;
            lowerButtonPressedInput.started -= LowerButtonPressedInputStarted;
            lowerButtonPressedInput.started -= LowerButtonPressedInputCanceled;
            systemButtonPressedInput.started -= SystemButtonPressedInputStarted;
            systemButtonPressedInput.started -= SystemButtonPressedInputCanceled;
            triggerInput.performed -= TriggerInputPerformed;
            triggerInput.canceled -= TriggerInputCanceled;
            gripInput.performed -= GripInputPerformed;
            gripInput.canceled -= GripInputCanceled;
        }
    
    
        private void Start()
        {
            _initThumbStickRot = thumbStick.localRotation;
            _initThumbStickPos = thumbStick.localPosition;
            _initUpperButtonPos = upperButton.localPosition;
            _initLowerButtonPos = lowerButton.localPosition;
            _initSystemButtonPos = systemButton.localPosition;
            _initTriggerRot = trigger.localRotation;
            _initGripRot = grip.localRotation;
        }


        private void ThumbStickInputPerformed(InputAction.CallbackContext obj)
        {
            Vector2 value = obj.ReadValue<Vector2>();
            float axisX = Mathf.Lerp(0f, maxThumbStickRot.x, Mathf.Abs(value.y)) * -Mathf.Sign(value.y) *
                          (inverseThumbStickX ? -1f : 1f);
            float axisY = Mathf.Lerp(0f, maxThumbStickRot.y, Mathf.Abs(value.x)) * -Mathf.Sign(value.x) *
                          (inverseThumbStickY ? -1f : 1f);
            thumbStick.localRotation = Quaternion.Euler(_initThumbStickRot.eulerAngles + new Vector3(axisX, axisY, 0f));
        }
    
    
        private void ThumbStickInputCanceled(InputAction.CallbackContext obj)
        {
            thumbStick.localRotation = _initThumbStickRot;
        }
    

        private void ThumbStickPressedInputStarted(InputAction.CallbackContext obj)
        {
            Vector3 targetPos = transform.TransformPoint(_initThumbStickPos) - (thumbStick.up * pressedThumbStickOffset);
            thumbStick.position = targetPos;
        }
    
    
        private void ThumbStickPressedInputCanceled(InputAction.CallbackContext obj)
        {
            thumbStick.position = transform.TransformPoint(_initThumbStickPos);
        }
    
    
        private void UpperButtonPressedInputStarted(InputAction.CallbackContext obj)
        {
            Vector3 targetPos = transform.TransformPoint(_initUpperButtonPos) - (upperButton.up * pressedUpperBtnOffset);
            upperButton.position = targetPos;
        }

    
        private void UpperButtonPressedInputCanceled(InputAction.CallbackContext obj)
        {
            upperButton.position = transform.TransformPoint(_initUpperButtonPos);
        }

    
        private void LowerButtonPressedInputStarted(InputAction.CallbackContext obj)
        {
            Vector3 targetPos = transform.TransformPoint(_initLowerButtonPos) - (lowerButton.up * pressedLowerBtnOffset);
            lowerButton.position = targetPos;
        }

    
        private void LowerButtonPressedInputCanceled(InputAction.CallbackContext obj)
        {
            lowerButton.position = transform.TransformPoint(_initLowerButtonPos);
        }

    
        private void SystemButtonPressedInputStarted(InputAction.CallbackContext obj)
        {
            Vector3 targetPos = transform.TransformPoint(_initSystemButtonPos) - (systemButton.up * pressedSystemBtnOffset);
            systemButton.position = targetPos;
        }

    
        private void SystemButtonPressedInputCanceled(InputAction.CallbackContext obj)
        {
            systemButton.position = transform.TransformPoint(_initSystemButtonPos);
        }
    
    
        private void TriggerInputPerformed(InputAction.CallbackContext obj)
        {
            float value = obj.ReadValue<float>();
            float rot = Mathf.Lerp(0f, maxTriggerRot, Mathf.Abs(value));
            trigger.localRotation = Quaternion.Euler(_initTriggerRot.eulerAngles + triggerRotationPivot * rot);
        }
    

        private void TriggerInputCanceled(InputAction.CallbackContext obj)
        {
            trigger.localRotation = _initTriggerRot;
        }
    

        private void GripInputPerformed(InputAction.CallbackContext obj)
        {
            float value = obj.ReadValue<float>();
            float rot = Mathf.Lerp(0f, maxGripRot, Mathf.Abs(value));
            grip.localRotation = Quaternion.Euler(_initGripRot.eulerAngles + gripRotationPivot * rot);
        }
    

        private void GripInputCanceled(InputAction.CallbackContext obj)
        {
            grip.localRotation = _initGripRot;
        }
    }
}
