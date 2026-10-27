// <copyright file="XRInputActionMap.cs" company="Google LLC">
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

namespace Google.XR.Extensions
{
    using UnityEngine;
    using UnityEngine.InputSystem;

    /// <summary>
    /// Manages general supported XR input actions.
    /// </summary>
    [CreateAssetMenu(menuName = "XR/Input Action Map", fileName = "InputActionMap")]
    public class XRInputActionMap : ScriptableObject
    {
        [Header("Left Hand")]
        [SerializeField]
        private InputActionProperty _leftHandButton;

        [SerializeField]
        private InputActionProperty _leftHandPosition;

        [SerializeField]
        private InputActionProperty _leftHandRotation;

        [Header("Right Hand")]
        [SerializeField]
        private InputActionProperty _rightHandButton;

        [SerializeField]
        private InputActionProperty _rightHandPosition;

        [SerializeField]
        private InputActionProperty _rightHandRotation;

        /// <summary>
        /// Gets or sets the <see cref="InputActionProperty"/> for left hand button.
        /// </summary>
        public InputActionProperty LeftHandButton
        {
            get => _leftHandButton;
            set => _leftHandButton = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="InputActionProperty"/> for left hand position.
        /// </summary>
        public InputActionProperty LeftHandPosition
        {
            get => _leftHandPosition;
            set => _leftHandPosition = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="InputActionProperty"/> for left hand rotation.
        /// </summary>
        public InputActionProperty LeftHandRotation
        {
            get => _leftHandRotation;
            set => _leftHandRotation = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="InputActionProperty"/> for right hand button.
        /// </summary>
        public InputActionProperty RightHandButton
        {
            get => _rightHandButton;
            set => _rightHandButton = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="InputActionProperty"/> for right hand position.
        /// </summary>
        public InputActionProperty RightHandPosition
        {
            get => _rightHandPosition;
            set => _rightHandPosition = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="InputActionProperty"/> for right hand rotation.
        /// </summary>
        public InputActionProperty RightHandRotation
        {
            get => _rightHandRotation;
            set => _rightHandRotation = value;
        }

        /// <summary>
        /// Checks if the given <see cref="InputActionProperty"/> has a valid action.
        /// </summary>
        /// <param name="actionProperty">The target action property.</param>
        /// <returns>
        /// Returns <see langword="true"/> if it has a valid action.
        /// </returns>
        public static bool CheckAction(InputActionProperty actionProperty)
        {
            return actionProperty != null && actionProperty.action != null;
        }

        /// <summary>
        /// Toggles the given input action if it's assigned.
        /// </summary>
        /// <param name="actionProperty">
        /// The target action property.</param>
        /// <param name="enable">
        /// Whether to enable or disable the action.</param>
        public static void ToggleAction(InputActionProperty actionProperty, bool enable)
        {
            if (!CheckAction(actionProperty) || actionProperty.action.enabled == enable)
            {
                return;
            }

            if (enable)
            {
                actionProperty.action.Enable();
            }
            else
            {
                actionProperty.action.Disable();
            }
        }

        /// <summary>
        /// Toggles all actions from this map if they are assigned.
        /// </summary>
        /// <param name="enable">
        /// Whether to enable or disable the actions.
        /// </param>
        public void ToggleAllActions(bool enable)
        {
            ToggleAction(_leftHandButton, enable);
            ToggleAction(_leftHandPosition, enable);
            ToggleAction(_leftHandRotation, enable);

            ToggleAction(_rightHandButton, enable);
            ToggleAction(_rightHandPosition, enable);
            ToggleAction(_rightHandRotation, enable);
        }
    }
}
