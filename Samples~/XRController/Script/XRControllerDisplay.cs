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
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.InputSystem;

    /// <summary>
    /// Used to display the controller asset.
    /// </summary>
    public class XRControllerDisplay : MonoBehaviour
    {
        private readonly List<(InputAction action,
            Action<InputAction.CallbackContext> onStarted,
            Action<InputAction.CallbackContext> onPerformed,
            Action<InputAction.CallbackContext> onCanceled)> _bindings = new();

#pragma warning disable CS0649 // Serialized fields don't need assignment.
        [Header("Buttons")]
        [SerializeField] private XrControllerButtonInfo _thumbstick;
        [SerializeField] private XrControllerButtonInfo _upperButton;
        [SerializeField] private XrControllerButtonInfo _lowerButton;
        [SerializeField] private XrControllerButtonInfo _systemButton;
        [SerializeField] private XrControllerButtonInfo _triggerButton;
        [SerializeField] private XrControllerButtonInfo _gripButton;

        [Header("Input Actions")]
        [SerializeField] private InputAction _thumbstickPressAction;
        [SerializeField] private InputAction _upperButtonPressAction;
        [SerializeField] private InputAction _lowerButtonPressAction;
        [SerializeField] private InputAction _systemButtonPressAction;
        [SerializeField] private InputAction _triggerAxisAction;
        [SerializeField] private InputAction _gripAxisAction;

        [Header("Thumbstick")]
        [SerializeField] private Transform _thumbstickTransform;
        [SerializeField] private Vector2 _maxThumbstickRot;
        [SerializeField] private bool _inverseThumbstickX;
        [SerializeField] private bool _inverseThumbstickY;
        [SerializeField] private InputAction _thumbstickAxisAction;
#pragma warning restore CS0649

        private Quaternion _thumbstickInitialRotation;

        private void OnEnable()
        {
            if (_thumbstickTransform != null)
            {
                _thumbstickInitialRotation = _thumbstickTransform.localRotation;
            }

            EnableActions(
                _thumbstickPressAction,
                _upperButtonPressAction,
                _lowerButtonPressAction,
                _systemButtonPressAction,
                _triggerAxisAction,
                _gripAxisAction,
                _thumbstickAxisAction);

            BindPress(_thumbstickPressAction, _thumbstick);
            BindPress(_upperButtonPressAction, _upperButton);
            BindPress(_lowerButtonPressAction, _lowerButton);
            BindPress(_systemButtonPressAction, _systemButton);

            BindAxis(_triggerAxisAction, _triggerButton);
            BindAxis(_gripAxisAction, _gripButton);

            BindThumbstickVector2(_thumbstickAxisAction, _thumbstickTransform);
        }

        private void OnDisable()
        {
            foreach (var (action, onStarted, onPerformed, onCanceled) in _bindings)
            {
                if (action == null)
                {
                    continue;
                }

                if (onStarted != null)
                {
                    action.started -= onStarted;
                }

                if (onPerformed != null)
                {
                    action.performed -= onPerformed;
                }

                if (onCanceled != null)
                {
                    action.canceled -= onCanceled;
                }
            }

            _bindings.Clear();

            DisableActions(
                _thumbstickPressAction,
                _upperButtonPressAction,
                _lowerButtonPressAction,
                _systemButtonPressAction,
                _triggerAxisAction,
                _gripAxisAction,
                _thumbstickAxisAction);

            ResetAll();
        }

        private void BindPress(InputAction action, XrControllerButtonInfo target)
        {
            if (action == null || target == null)
            {
                return;
            }

            Action<InputAction.CallbackContext> started = _ => target.SetStatus(1f);
            Action<InputAction.CallbackContext> canceled = _ => target.SetStatus(0f);

            action.started += started;
            action.canceled += canceled;

            _bindings.Add((action, started, null, canceled));
        }

        private void BindAxis(InputAction action, XrControllerButtonInfo target)
        {
            if (action == null || target == null)
            {
                return;
            }

            Action<InputAction.CallbackContext> performed = ctx =>
                target.SetStatus(ctx.ReadValue<float>());
            Action<InputAction.CallbackContext> canceled = _ => target.SetStatus(0f);

            action.performed += performed;
            action.canceled += canceled;

            _bindings.Add((action, null, performed, canceled));
        }

        private void BindThumbstickVector2(InputAction action, Transform stickTransform)
        {
            if (action == null || stickTransform == null)
            {
                return;
            }

            var initial = _thumbstickInitialRotation;

            Action<InputAction.CallbackContext> performed = ctx =>
            {
                var value = ctx.ReadValue<Vector2>();
                float axisX = Mathf.Lerp(0f, _maxThumbstickRot.x, Mathf.Abs(value.y))
                              * -Mathf.Sign(value.y)
                              * (_inverseThumbstickX ? -1f : 1f);
                float axisY = Mathf.Lerp(0f, _maxThumbstickRot.y, Mathf.Abs(value.x))
                              * -Mathf.Sign(value.x)
                              * (_inverseThumbstickY ? -1f : 1f);

                stickTransform.localRotation = Quaternion.Euler(
                    initial.eulerAngles + new Vector3(axisX, 0f, axisY));
            };

            Action<InputAction.CallbackContext> canceled = _ =>
            {
                stickTransform.localRotation = initial;
            };

            action.performed += performed;
            action.canceled += canceled;

            _bindings.Add((action, null, performed, canceled));
        }

        private void EnableActions(params InputAction[] actions)
        {
            foreach (var a in actions)
            {
                a?.Enable();
            }
        }

        private void DisableActions(params InputAction[] actions)
        {
            foreach (var a in actions)
            {
                a?.Disable();
            }
        }

        private void ResetAll()
        {
            _thumbstick?.SetStatus(0f);
            _upperButton?.SetStatus(0f);
            _lowerButton?.SetStatus(0f);
            _systemButton?.SetStatus(0f);
            _triggerButton?.SetStatus(0f);
            _gripButton?.SetStatus(0f);

            if (_thumbstickTransform != null)
            {
                _thumbstickTransform.localRotation = _thumbstickInitialRotation;
            }
        }
    }

    /// <summary>
    /// Holds released/pressed poses for a controller button and applies
    /// the interpolated transform to a target object.
    /// </summary>
    [Serializable]
    public class XrControllerButtonInfo
    {
#pragma warning disable CS0649 // Serialized fields don't need assignment.
        [SerializeField] private Transform _targetObject;
        [SerializeField] private Vector3 _releasedPosition;
        [SerializeField] private Vector3 _pressedPosition;
        [SerializeField] private Vector3 _releasedRotation;
        [SerializeField] private Vector3 _pressedRotation;
#pragma warning restore CS0649

        /// <summary>
        /// Applies the pressed/released interpolation to the target object's local transform.
        /// </summary>
        public void SetStatus(float t)
        {
            if (_targetObject == null)
            {
                return;
            }

            t = Mathf.Clamp01(t);

            _targetObject.localPosition = Vector3.Lerp(_releasedPosition, _pressedPosition, t);
            _targetObject.localRotation = Quaternion.Lerp(
                Quaternion.Euler(_releasedRotation),
                Quaternion.Euler(_pressedRotation),
                t);
        }
    }
}