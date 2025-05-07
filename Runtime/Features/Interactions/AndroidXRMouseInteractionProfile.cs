// <copyright file="AndroidXRMouseInteractionProfile.cs" company="Google LLC">
//
// Copyright 2024 Google LLC
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
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.Controls;
    using UnityEngine.InputSystem.Layouts;
    using UnityEngine.InputSystem.XR;
    using UnityEngine.Scripting;
    using UnityEngine.XR;
    using UnityEngine.XR.OpenXR;
    using UnityEngine.XR.OpenXR.Features;
    using UnityEngine.XR.OpenXR.Input;

#if UNITY_EDITOR
    using UnityEditor;
#endif

#if USE_INPUT_SYSTEM_POSE_CONTROL
    using PoseControl = UnityEngine.InputSystem.XR.PoseControl;
#else
    using PoseControl = UnityEngine.XR.OpenXR.Input.PoseControl;
#endif

    /// <summary>
    /// This <see cref="OpenXRInteractionFeature"/> enables the use of Android XR Mouse interaction
    /// profile in OpenXR.
    /// </summary>
#if UNITY_EDITOR
    [UnityEditor.XR.OpenXR.Features.OpenXRFeature(
        UiName = UiName,
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        Company = "Google",
        Desc = "Add Android XR Mouse interaction profile for mouse input device.",
        OpenxrExtensionStrings = ExtensionString,
        Version = "0.0.1",
        Category = UnityEditor.XR.OpenXR.Features.FeatureCategory.Interaction,
        FeatureId = FeatureId)]
#endif
    public class AndroidXRMouseInteractionProfile : OpenXRInteractionFeature
    {
        /// <summary>
        /// The UI name shows on the XR Plug-in Management panel, help users to understand
        /// validation errors and expected fixes.
        /// </summary>
        public const string UiName = "Android XR Mouse Interaction Profile";

        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string FeatureId = "com.google.xr.extensions.input.mouse";

        /// <summary>
        /// The OpenXR Extension string.
        /// This is used by OpenXR to check if this extension is available or enabled.
        /// </summary>
        public const string ExtensionString = "XR_ANDROID_mouse_interaction";

        /// <summary>
        /// The interaction profile string used to reference the mouse input device.
        /// </summary>
        public const string Profile = "/interaction_profiles/android/mouse_interaction_android";

        /// <summary>
        /// The OpenXR constant that is used to reference a mouse supported input device.
        /// </summary>
        public const string UserPath = "/user/mouse";

        // Available bindings.

        /// <summary>
        /// Constant for a pose interaction binding '.../input/aim/pose' OpenXR Input Binding.
        /// Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        private const string _pose = "/input/aim/pose";

        /// <summary>
        /// Constant for a boolean interaction binding '.../input/select/click' OpenXR Input
        /// Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        private const string _select = "/input/select/click";

        /// <summary>
        /// Constant for a boolean interaction binding '.../input/secondary_android/click' OpenXR
        /// Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        private const string _secondary = "/input/secondary_android/click";

        /// <summary>
        /// Constant for a boolean interaction binding '.../input/tertiary_android/click' OpenXR
        /// Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        private const string _tertiary = "/input/tertiary_android/click";

        /// <summary>
        /// Constant for a Vector2 interaction binding '.../input/scroll_android/value' OpenXR
        /// Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        private const string _scroll = "/input/scroll_android/value";

        private const string _deviceLocalizedName = "Android XR Mouse";

        private const string _layoutName = "AndroidXRMouse";

        /// <inheritdoc/>
        protected override bool OnInstanceCreate(ulong instance)
        {
            // Requires the mouse extension
            if (!OpenXRRuntime.IsExtensionEnabled(ExtensionString))
            {
                return false;
            }

            return base.OnInstanceCreate(instance);
        }

        /// <summary>
        /// Registers the <see cref="AndroidXRMouse"/> layout with the Input System.
        /// </summary>
        protected override void RegisterDeviceLayout()
        {
            InputDeviceMatcher matcher = new InputDeviceMatcher()
                    .WithInterface(XRUtilities.InterfaceMatchAnyVersion)
                    .WithProduct(_deviceLocalizedName);
            InputSystem.RegisterLayout(typeof(AndroidXRMouse), _layoutName, matches: matcher);
        }

        /// <summary>
        /// Removes the <see cref="AndroidXRMouse"/> layout from the Input System.
        /// </summary>
        protected override void UnregisterDeviceLayout()
        {
            InputSystem.RemoveLayout(_layoutName);
        }

        /// <summary>
        /// Return interaction profile type. <see cref="AndroidXRMouse"/> profile is Device type.
        /// </summary>
        /// <returns>Interaction profile type.</returns>
        protected override InteractionProfileType GetInteractionProfileType()
        {
            return typeof(AndroidXRMouse).IsSubclassOf(typeof(XRController)) ?
                InteractionProfileType.XRController : InteractionProfileType.Device;
        }

        /// <summary>
        /// Return device layer out string used for registering device <see cref="AndroidXRMouse"/>
        /// in InputSystem.
        /// </summary>
        /// <returns>Device layout string.</returns>
        protected override string GetDeviceLayoutName()
        {
            return _layoutName;
        }

        /// <inheritdoc/>
        protected override void RegisterActionMapsWithRuntime()
        {
            ActionMapConfig actionMap = new ActionMapConfig()
            {
                name = "androidxrmouse",
                localizedName = _deviceLocalizedName,
                desiredInteractionProfile = Profile,
                manufacturer = "Google",
                serialNumber = string.Empty,
                deviceInfos = new List<DeviceConfig>()
                {
                    new DeviceConfig()
                    {
                        characteristics =
                            InputDeviceCharacteristics.TrackedDevice |
                            InputDeviceCharacteristics.HeldInHand,
                        userPath = UserPath
                    }
                },
                actions = new List<ActionConfig>()
                {
                    //// Pointer/Aim Pose
                    new ActionConfig()
                    {
                        name = "pose",
                        localizedName = "Pose",
                        type = ActionType.Pose,
                        usages = new List<string>() { "Pointer" },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = _pose,
                                interactionProfileName = Profile,
                            }
                        }
                    },
                    //// Primary/Select Click
                    new ActionConfig()
                    {
                        name = "primaryButton",
                        localizedName = "Primary Button",
                        type = ActionType.Binary,
                        usages = new List<string>() { "PrimaryButton" },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = _select,
                                interactionProfileName = Profile,
                            }
                        }
                    },
                    //// Secondary Click
                    new ActionConfig()
                    {
                        name = "secondaryButton",
                        localizedName = "Secondary Button",
                        type = ActionType.Binary,
                        usages = new List<string>() { "SecondaryButton" },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = _secondary,
                                interactionProfileName = Profile,
                            }
                        }
                    },
                    //// Tertiary Click
                    new ActionConfig()
                    {
                        name = "tertiaryButton",
                        localizedName = "Tertiary Button",
                        type = ActionType.Binary,
                        usages = new List<string>() { "TertiaryButton" },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = _tertiary,
                                interactionProfileName = Profile,
                            }
                        }
                    },
                    //// Scroll Value
                    new ActionConfig()
                    {
                        name = "scroll",
                        localizedName = "Scroll",
                        type = ActionType.Axis2D,
                        usages = new List<string>() { "Scroll" },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = _scroll,
                                interactionProfileName = Profile,
                            }
                        }
                    },
                }
            };

            AddActionMap(actionMap);
        }

        /// <summary>
        /// Tags that can be used with <see cref="InputDevice.TryGetFeatureValue"/> to get mouse
        /// related input features.  See <seealso cref="CommonUsages"/> for additional usages.
        /// </summary>
        [SuppressMessage(
            "UnityRules.UnityStyleRules", "US1107:PublicFieldsMustBeUpperCamelCase",
            Justification = "Override Unity interface.")]
        public static class AndroidXRMouseUsages
        {
            /// <summary>
            /// The pointer position for the mouse.
            /// </summary>
            public static InputFeatureUsage<Vector3> pointerPosition =
                new InputFeatureUsage<Vector3>("pointerPosition");

            /// <summary>
            /// The pointer orientation of the mouse.
            /// </summary>
            public static InputFeatureUsage<Quaternion> pointerRotation =
                new InputFeatureUsage<Quaternion>("pointerRotation");
        }

        /// <summary>
        /// An Input device based on Android XR Mouse interaction profile. It's derived from
        /// <see cref="OpenXRDevice"/> instead of <see cref="Mouse"/> to align with the general
        /// usage of other OpenXR interactions.
        /// </summary>
        [SuppressMessage(
            "UnityRules.UnityStyleRules", "US1109:PublicPropertiesMustBeUpperCamelCase",
            Justification = "Override Unity interface.")]
        [Preserve, InputControlLayout(
            displayName = "Android XR Mouse (OpenXR)", isGenericTypeOfDevice = true)]
        public class AndroidXRMouse : OpenXRDevice
        {
            /// <summary>
            /// Gets the [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that
            /// represents the <see cref="_select"/> OpenXR bindings.
            /// </summary>
            [Preserve, InputControl(
                aliases = new[] { "select", "buttonSelect", "primary" }, usage = "PrimaryButton")]
            public ButtonControl primaryButton { get; private set; }

            /// <summary>
            /// Gets the [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that
            /// represents the <see cref="_secondary"/> OpenXR bindings.
            /// </summary>
            [Preserve, InputControl(
                aliases = new[] { "secondary", "buttonSecondary" }, usage = "SecondaryButton")]
            public ButtonControl secondaryButton { get; private set; }

            /// <summary>
            /// Gets the [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that
            /// represents the <see cref="_tertiary"/> OpenXR bindings.
            /// </summary>
            [Preserve, InputControl(
                aliases = new[] { "tertiary", "buttonTertiary" }, usage = "TertiaryButton")]
            public ButtonControl tertiaryButton { get; private set; }

            /// <summary>
            /// Gets the [Vector2Control](xref:UnityEngine.InputSystem.Controls.Vector2Control) that
            /// represents the <see cref="_scroll"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(alias = "scroll", usage = "Scroll")]
            public Vector2Control scroll { get; private set; }

            /// <summary>
            /// Gets the <see cref="PoseControl"/> that represents the <see cref="_pose"/> OpenXR
            /// binding.
            /// </summary>
            [Preserve, InputControl(offset = 0, alias = "aimPose", usage = "Pointer")]
            public PoseControl pose { get; private set; }

            /// <inheritdoc/>
            protected override void FinishSetup()
            {
                base.FinishSetup();
                primaryButton = GetChildControl<ButtonControl>("primaryButton");
                secondaryButton = GetChildControl<ButtonControl>("secondaryButton");
                tertiaryButton = GetChildControl<ButtonControl>("tertiaryButton");
                scroll = GetChildControl<Vector2Control>("scroll");
                pose = GetChildControl<PoseControl>("pose");
            }
        }
    }
}
