// <copyright file="TrackpadGesturesVisualizer.cs" company="Google LLC">
//
// Copyright 2026 Google LLC
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

namespace UnityEngine.XR.OpenXR.Samples.TrackpadGestures
{
    using System;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.UI;
    using UnityEngine.XR.OpenXR.Input;

    /// <summary>
    /// Visualizer for Trackpad gestures.
    /// </summary>
    public class TrackpadGesturesVisualizer : MonoBehaviour
    {
        [SerializeField]
        private InputActionAsset _trackpadGesturesActions = null;

        [SerializeField]
        private InputVisualizer _scrollActive = null;

        [SerializeField]
        private Vector2Visualizer _scrollDelta = null;

        [SerializeField]
        private InputVisualizer _pinchInActive = null;

        [SerializeField]
        private AxisVisualizer _pinchInValue = null;

        [SerializeField]
        private InputVisualizer _pinchOutActive = null;

        [SerializeField]
        private AxisVisualizer _pinchOutValue = null;

        [SerializeField]
        private InputVisualizer _rotateActive = null;

        [SerializeField]
        private AxisVisualizer _rotateValue = null;

        void Start()
        {
            if (_trackpadGesturesActions != null)
            {
                _trackpadGesturesActions.Enable();
            }

            InputVisualizer[] inputVisualizers = new InputVisualizer[]
            {
                _scrollActive,
                _scrollDelta,
                _pinchInActive,
                _pinchInValue,
                _pinchOutActive,
                _pinchOutValue,
                _rotateActive,
                _rotateValue
            };

            foreach (InputVisualizer inputVisualizer in inputVisualizers)
            {
                if (inputVisualizer == null || !inputVisualizer.Initialized())
                {
                    Debug.LogError("Cannot setup uninitialized InputVisualizer");
                    continue;
                }

                SetupAction(inputVisualizer);
            }
        }

        void SetupAction(InputVisualizer inputVisualizer)
        {
            InputAction action = inputVisualizer.ActionReference.action;

            action.started += ctx => OnActionStarted(inputVisualizer, ctx);
            action.performed += ctx => OnActionPerformed(inputVisualizer, ctx);
            action.canceled += ctx => OnActionCanceled(inputVisualizer, ctx);

            string actionName = action.name;

            if (action.controls.Count > 0)
            {
                OpenXRInput.TryGetInputSourceName(
                    inputAction: action,
                    index: 0,
                    name: out actionName,
                    flags: OpenXRInput.InputSourceNameFlags.Component,
                    inputDevice: action.controls[0].device);
            }

            inputVisualizer.Name.text = actionName;
        }

        void OnActionStarted(InputVisualizer visualizer, InputAction.CallbackContext ctx)
        {
            visualizer.StateImage.color = Color.green;
            OnActionPerformed(visualizer, ctx);
        }

        void OnActionPerformed(InputVisualizer visualizer, InputAction.CallbackContext ctx)
        {
            if (visualizer is AxisVisualizer axisVisualizer)
            {
                axisVisualizer.IntensitySlider.value = ctx.ReadValue<float>();
            }
            else if (visualizer is Vector2Visualizer vec2visualizer)
            {
                Vector2 point = (ctx.ReadValue<Vector2>() + Vector2.one) * 0.5f;
                RectTransform joystick = vec2visualizer.Joystick;

                joystick.anchorMin = joystick.anchorMax = point;
            }
        }

        void OnActionCanceled(InputVisualizer visualizer, InputAction.CallbackContext ctx)
        {
            visualizer.StateImage.color = Color.red;
            OnActionPerformed(visualizer, ctx);
        }

        [Serializable]
        class InputVisualizer
        {
            [Tooltip("Action Reference that represents the control")]
            public InputActionReference ActionReference = null;

            [Tooltip("Text element that will be set to the name of the action")]
            public Text Name = null;

            [Tooltip("Image that will represent the state of the action")]
            public Image StateImage = null;

            public virtual bool Initialized()
            {
                return ActionReference != null && Name != null && StateImage != null;
            }
        }

        [Serializable]
        class AxisVisualizer : InputVisualizer
        {
            [Tooltip("Slider which will sync with action value")]
            public Slider IntensitySlider = null;

            public override bool Initialized()
            {
                return base.Initialized() && IntensitySlider != null;
            }
        }

        [Serializable]
        class Vector2Visualizer : InputVisualizer
        {
            [Tooltip("The 2D UI element to which will sync with action value")]
            public RectTransform Joystick = null;

            public override bool Initialized()
            {
                return base.Initialized() && Joystick != null;
            }
        }
    }
}
