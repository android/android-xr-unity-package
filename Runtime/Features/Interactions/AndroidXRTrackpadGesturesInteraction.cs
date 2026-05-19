// <copyright file="AndroidXRTrackpadGesturesInteraction.cs" company="Google LLC">
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
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Google.XR.Extensions.Internal;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.Controls;
    using UnityEngine.InputSystem.Layouts;
    using UnityEngine.InputSystem.XR;
    using UnityEngine.Scripting;
    using UnityEngine.XR;
    using UnityEngine.XR.OpenXR;
    using UnityEngine.XR.OpenXR.Features;
    using UnityEngine.XR.OpenXR.Features.Interactions;
    using UnityEngine.XR.OpenXR.Input;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    /// <summary>
    /// This <see cref="OpenXRInteractionFeature"/> enables the use of Android XR Trackpad Gestures
    /// interaction in OpenXR.
    /// </summary>
#if UNITY_EDITOR
    [UnityEditor.XR.OpenXR.Features.OpenXRFeature(
        UiName = UiName,
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        Company = "Google",
        Desc = "Add Android XR Trackpad Gestures interaction for supported input device.",
        OpenxrExtensionStrings = ExtensionString,
        Version = "0.0.1",
        FeatureId = FeatureId)]
#endif
    public class AndroidXRTrackpadGesturesInteraction : OpenXRInteractionFeature, IXRSpatialSdk
    {
        /// <summary>
        /// The UI name shows on the XR Plug-in Management panel, help users to understand
        /// validation errors and expected fixes.
        /// </summary>
        public const string UiName = "Android XR Trackpad Gestures Interaction";

        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string FeatureId = "com.google.xr.extensions.input.trackpadgestures";

        /// <summary>
        /// The OpenXR Extension string.
        /// This is used by OpenXR to check if this extension is available or enabled.
        /// </summary>
        public const string ExtensionString = "XR_ANDROIDX1_trackpad_gestures";

        // Available bindings.

        /// <summary>
        /// Constant for a boolean interaction binding
        /// '.../input/scroll_gesture_android/activate_android' OpenXR Input Binding. Used by input
        /// subsystem to bind actions to physical inputs.
        /// </summary>
        private const string _scrollActivate = "/input/scroll_gesture_android/activate_android";

        /// <summary>
        /// Constant for a Vector2 interaction binding '.../input/scroll_gesture_android' OpenXR
        /// Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        private const string _scroll = "/input/scroll_gesture_android";

        /// <summary>
        /// Constant for a boolean interaction binding
        /// '.../input/pinch_in_android/activate_android' OpenXR Input Binding. Used by input
        /// subsystem to bind actions to physical inputs.
        /// </summary>
        private const string _pinchInActivate = "/input/pinch_in_android/activate_android";

        /// <summary>
        /// Constant for a float interaction binding '.../input/pinch_in_android/value' OpenXR Input
        /// Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        private const string _pinchInValue = "/input/pinch_in_android/value";

        /// <summary>
        /// Constant for a boolean interaction binding
        /// '.../input/pinch_out_android/activate_android' OpenXR Input Binding. Used by input
        /// subsystem to bind actions to physical inputs.
        /// </summary>
        private const string _pinchOutActivate = "/input/pinch_out_android/activate_android";

        /// <summary>
        /// Constant for a float interaction binding '.../input/pinch_out_android/value' OpenXR
        /// Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        private const string _pinchOutValue = "/input/pinch_out_android/value";

        /// <summary>
        /// Constant for a boolean interaction binding
        /// '.../input/rotate_android/activate_android' OpenXR Input Binding. Used by input
        /// subsystem to bind actions to physical inputs.
        /// </summary>
        private const string _rotateActivate = "/input/rotate_android/activate_android";

        /// <summary>
        /// Constant for a float interaction binding '.../input/rotate_android/value' OpenXR Input
        /// Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        private const string _rotateValue = "/input/rotate_android/value";

        private const string _deviceLocalizedName = "Android XR Trackpad Gestures";

        private const string _layoutName = "AndroidXRTrackpadGestures";

        /// <inheritdoc/>
        [SuppressMessage(
            "UnityRules.UnityStyleRules", "US1108:NonPublicPropertiesMustBeLowerCamelCase",
            Justification = "Override Unity interface.")]
        protected override bool IsAdditive => true;

        /// <inheritdoc/>
        public XRSpatialSdkVersions GetTargetVersion()
        {
            return XRSpatialSdkVersions.XRSpatialApiLevel3;
        }

        /// <inheritdoc/>
        protected override bool OnInstanceCreate(ulong instance)
        {
            // Requires the trackpad gestures extension
            if (!OpenXRRuntime.IsExtensionEnabled(ExtensionString))
            {
                return false;
            }

            return base.OnInstanceCreate(instance);
        }

        /// <summary>
        /// Registers the <see cref="AndroidXRTrackpadGestures"/> layout with the Input System.
        /// </summary>
        protected override void RegisterDeviceLayout()
        {
            InputDeviceMatcher matcher = new InputDeviceMatcher()
                    .WithInterface(XRUtilities.InterfaceMatchAnyVersion)
                    .WithProduct(_deviceLocalizedName);
            InputSystem.RegisterLayout(
                typeof(AndroidXRTrackpadGestures), _layoutName, matches: matcher);
        }

        /// <summary>
        /// Removes the <see cref="AndroidXRTrackpadGestures"/> layout from the Input System.
        /// </summary>
        protected override void UnregisterDeviceLayout()
        {
            InputSystem.RemoveLayout(_layoutName);
        }

        /// <summary>
        /// Return interaction profile type. <see cref="AndroidXRTrackpadGestures"/> profile is
        /// Device type.
        /// </summary>
        /// <returns>Interaction profile type.</returns>
        protected override InteractionProfileType GetInteractionProfileType()
        {
            return typeof(AndroidXRTrackpadGestures).IsSubclassOf(typeof(XRController)) ?
                InteractionProfileType.XRController : InteractionProfileType.Device;
        }

        /// <summary>
        /// Return device layout string used for registering device
        /// <see cref="AndroidXRTrackpadGestures"/> in InputSystem.
        /// </summary>
        /// <returns>Device layout string.</returns>
        protected override string GetDeviceLayoutName()
        {
            return _layoutName;
        }

        /// <inheritdoc/>
        protected override void RegisterActionMapsWithRuntime()
        {
            var settings = OpenXRSettings.Instance;

            var mouseFeature = settings.GetFeature<AndroidMouseInteractionProfile>();
            bool mouseEnabled = mouseFeature != null && mouseFeature.enabled;

            List<(string profilePath, string userPath)> profileUserPaths =
                new List<(string profilePath, string userPath)>();

            if (mouseEnabled)
            {
                profileUserPaths.Add((
                    profilePath: AndroidMouseInteractionProfile.profile, userPath: "/user/mouse"));
            }

            var actions = new List<ActionConfig>()
            {
                //// Scroll Activate
                new ActionConfig()
                {
                    name = "scrollActivate",
                    localizedName = "Scroll Activate",
                    type = ActionType.Binary,
                    bindings = CreateActionBindings(_scrollActivate, profileUserPaths),
                    isAdditive = true
                },
                //// Scroll
                new ActionConfig()
                {
                    name = "gestureScroll",
                    localizedName = "Gesture Scroll",
                    type = ActionType.Axis2D,
                    bindings = CreateActionBindings(_scroll, profileUserPaths),
                    isAdditive = true
                },
                //// Pinch In Activate
                new ActionConfig()
                {
                    name = "pinchInActivate",
                    localizedName = "Pinch In Activate",
                    type = ActionType.Binary,
                    bindings = CreateActionBindings(_pinchInActivate, profileUserPaths),
                    isAdditive = true
                },
                //// Pinch In Value
                new ActionConfig()
                {
                    name = "pinchInValue",
                    localizedName = "Pinch In Value",
                    type = ActionType.Axis1D,
                    bindings = CreateActionBindings(_pinchInValue, profileUserPaths),
                    isAdditive = true
                },
                //// Pinch Out Activate
                new ActionConfig()
                {
                    name = "pinchOutActivate",
                    localizedName = "Pinch Out Activate",
                    type = ActionType.Binary,
                    bindings =
                        CreateActionBindings(_pinchOutActivate, profileUserPaths),
                    isAdditive = true
                },
                //// Pinch Out Value
                new ActionConfig()
                {
                    name = "pinchOutValue",
                    localizedName = "Pinch Out Value",
                    type = ActionType.Axis1D,
                    bindings = CreateActionBindings(_pinchOutValue, profileUserPaths),
                    isAdditive = true,
                },
                //// Rotate Activate
                new ActionConfig()
                {
                    name = "rotateActivate",
                    localizedName = "Rotate Activate",
                    type = ActionType.Binary,
                    bindings = CreateActionBindings(_rotateActivate, profileUserPaths),
                    isAdditive = true
                },
                //// Rotate Value
                new ActionConfig()
                {
                    name = "rotateValue",
                    localizedName = "Rotate Value",
                    type = ActionType.Axis1D,
                    bindings = CreateActionBindings(_rotateValue, profileUserPaths),
                    isAdditive = true
                }
            };

            string profilePath = AndroidMouseInteractionProfile.profile;
            if (profileUserPaths.Count > 0)
            {
                profilePath = profileUserPaths[0].profilePath;
            }

            ActionMapConfig actionMap = new ActionMapConfig()
            {
                name = "androidxrtrackpadgestures",
                localizedName = _deviceLocalizedName,
                desiredInteractionProfile = profilePath,
                manufacturer = "Google",
                serialNumber = string.Empty,
                deviceInfos = CreateDeviceConfigs(profileUserPaths),
                actions = actions
            };

            AddActionMap(actionMap);
        }

        /// <inheritdoc/>
        protected override void AddAdditiveActions(
            List<ActionMapConfig> actionMaps, ActionMapConfig additiveMap)
        {
            foreach (var actionMap in actionMaps)
            {
                string desiredProfile = actionMap.desiredInteractionProfile;
                bool validProfile = false;

                bool isMouseProfile = desiredProfile == AndroidMouseInteractionProfile.profile;
                validProfile |= isMouseProfile;

                if (!validProfile)
                {
                    continue;
                }

                bool validUserPath = false;
                foreach (var deviceInfo in actionMap.deviceInfos)
                {
                    if (isMouseProfile && deviceInfo.userPath == "/user/mouse")
                    {
                        validUserPath = true;
                        break;
                    }

                }

                if (!validUserPath)
                {
                    continue;
                }

                foreach (var additiveAction in additiveMap.actions)
                {
                    if (additiveAction.isAdditive)
                    {
                        actionMap.actions.Add(additiveAction);
                    }
                }
            }
        }

        private List<DeviceConfig> CreateDeviceConfigs(
            List<(string profilePath, string userPath)> profileUserPaths)
        {
            List<DeviceConfig> deviceConfigs = new List<DeviceConfig>();

            foreach (var pair in profileUserPaths)
            {
                deviceConfigs.Add(new DeviceConfig
                {
                    characteristics = InputDeviceCharacteristics.TrackedDevice,
                    userPath = pair.userPath
                });
            }

            return deviceConfigs;
        }

        private List<ActionBinding> CreateActionBindings(
            string interactionPath,
            List<(string profilePath, string userPath)> profileUserPaths)
        {
            List<ActionBinding> actionBindings = new List<ActionBinding>();

            foreach (var pair in profileUserPaths)
            {
                actionBindings.Add(new ActionBinding
                {
                    interactionPath = interactionPath,
                    interactionProfileName = pair.profilePath,
                    userPaths = new List<string>() { pair.userPath },
                });
            }

            return actionBindings;
        }

        /// <summary>
        /// An Input device based on Android XR Trackpad Gestures interaction profile. It's derived
        /// from <see cref="OpenXRDevice"/> to align with the general usage of other OpenXR
        /// interactions.
        /// </summary>
        [SuppressMessage(
            "UnityRules.UnityStyleRules", "US1109:PublicPropertiesMustBeUpperCamelCase",
            Justification = "Override Unity interface.")]
        [Preserve, InputControlLayout(
            displayName = "Android XR Trackpad Gestures (OpenXR)", isGenericTypeOfDevice = true)]
        public class AndroidXRTrackpadGestures : OpenXRDevice
        {
            /// <summary>
            /// Gets the <see cref="ButtonControl"/> that represents the
            /// <see cref="_scrollActivate"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(alias = "scrollActivate")]
            public ButtonControl scrollActivate { get; private set; }

            /// <summary>
            /// Gets the <see cref="Vector2Control"/> that represents the <see cref="_scroll"/>
            /// OpenXR binding.
            /// </summary>
            [Preserve, InputControl(name = "gestureScroll", alias = "scroll")]
            public Vector2Control scroll { get; private set; }

            /// <summary>
            /// Gets the <see cref="ButtonControl"/> that represents the
            /// <see cref="_pinchInActivate"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(alias = "pinchInActivate")]
            public ButtonControl pinchInActivate { get; private set; }

            /// <summary>
            /// Gets the <see cref="AxisControl"/> that represents the <see cref="_pinchInValue"/>
            /// OpenXR binding.
            /// </summary>
            [Preserve, InputControl(alias = "pinchInValue")]
            public AxisControl pinchInValue { get; private set; }

            /// <summary>
            /// Gets the <see cref="ButtonControl"/> that represents the
            /// <see cref="_pinchOutActivate"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(alias = "pinchOutActivate")]
            public ButtonControl pinchOutActivate { get; private set; }

            /// <summary>
            /// Gets the <see cref="AxisControl"/> that represents the <see cref="_pinchOutValue"/>
            /// OpenXR binding.
            /// </summary>
            [Preserve, InputControl(alias = "pinchOutValue")]
            public AxisControl pinchOutValue { get; private set; }

            /// <summary>
            /// Gets the <see cref="ButtonControl"/> that represents the
            /// <see cref="_rotateActivate"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(alias = "rotateActivate")]
            public ButtonControl rotateActivate { get; private set; }

            /// <summary>
            /// Gets the <see cref="AxisControl"/> that represents the <see cref="_rotateValue"/>
            /// OpenXR binding.
            /// </summary>
            [Preserve, InputControl(alias = "rotateValue")]
            public AxisControl rotateValue { get; private set; }

            /// <inheritdoc/>
            protected override void FinishSetup()
            {
                base.FinishSetup();
                scrollActivate = GetChildControl<ButtonControl>("scrollActivate");
                scroll = GetChildControl<Vector2Control>("gestureScroll");
                pinchInActivate = GetChildControl<ButtonControl>("pinchInActivate");
                pinchInValue = GetChildControl<AxisControl>("pinchInValue");
                pinchOutActivate = GetChildControl<ButtonControl>("pinchOutActivate");
                pinchOutValue = GetChildControl<AxisControl>("pinchOutValue");
                rotateActivate = GetChildControl<ButtonControl>("rotateActivate");
                rotateValue = GetChildControl<AxisControl>("rotateValue");
            }
        }

#if UNITY_EDITOR
        /// <inheritdoc/>
        protected override void GetValidationChecks(
            List<ValidationRule> results, BuildTargetGroup targetGroup)
        {
            if (targetGroup != BuildTargetGroup.Android)
            {
                return;
            }

            var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);

            results.Add(AndroidXRFeatureUtils.GetExperimentalFeatureValidationCheck(
                 this, UiName, targetGroup));

            var mouseFeature = settings.GetFeature<AndroidMouseInteractionProfile>();

            results.Add(new ValidationRule(this)
            {
                message = "<b>" + UiName + "</b> requires <b>Android Mouse Interaction Profile</b> "
                    + "to be enabled.",
                checkPredicate = () =>
                {
                    bool parentEnabled = mouseFeature != null && mouseFeature.enabled;

                    return parentEnabled;
                },
                fixItMessage = "Enable the required interaction profile.",
                fixIt = () =>
                {
                    if (mouseFeature != null)
                    {
                        mouseFeature.enabled = true;
                    }
                },
                error = true,
            });
        }
#endif // UNITY_EDITOR
    }
}
