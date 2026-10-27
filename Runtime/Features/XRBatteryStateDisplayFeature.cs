// <copyright file="XRBatteryStateDisplayFeature.cs" company="Google LLC">
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

namespace Google.XR.Extensions
{
    using System;
    using System.Runtime.InteropServices;
    using Google.XR.Extensions.Internal;
    using UnityEngine;
    using UnityEngine.XR.OpenXR;
    using UnityEngine.XR.OpenXR.Features;
    using UnityEngine.XR.OpenXR.NativeTypes;

#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.XR.OpenXR.Features;
#endif // UNITY_EDITOR

    /// <summary>
    /// This <see cref="XRBatteryStateDisplayFeature"/> provides a function to query the battery
    /// state of input devices.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = UiName,
        BuildTargetGroups = new[] {
            BuildTargetGroup.Android,
        },
        Company = "Google",
        OpenxrExtensionStrings = ExtensionString,
        Desc = "Get input controller battery state information for display purposes.",
        Version = "1.0.0",
        Category = FeatureCategory.Feature,
        FeatureId = FeatureId)]
#endif
    public class XRBatteryStateDisplayFeature : OpenXRFeature
    {
        /// <summary>
        /// The UI name shows on the XR Plug-in Management panel, to help users understand
        /// validation errors and expected fixes.
        /// </summary>
        public const string UiName = "Battery State Display";

        /// <summary>
        /// The feature ID string.
        /// </summary>
        public const string FeatureId = "com.google.xr.extensions.battery_state_display";

        /// <summary>
        /// The OpenXR Extension string. Used to check if this extension is available or enabled.
        /// </summary>
        public const string ExtensionString = "XR_EXT_interaction_profile_battery_state_display";

        internal static bool? _extensionEnabled = null;

        /// <summary>
        /// Gets if the required OpenXR extension is enabled.
        /// When OpenXR runtime is waiting, it returns <c>null</c>. Otherwise, it indicates
        /// whether the XR_EXT_interaction_profile_battery_state_display extension is available on
        /// the current device.
        /// </summary>
        public static bool? IsExtensionEnabled => _extensionEnabled;

        /// <summary>
        /// Gets the battery state for a given top level user input path.
        /// </summary>
        /// </param>
        /// <param name="topLevelUserPath">
        /// Top level user input path from OpenXRInteractionFeature.UserPaths.
        /// </param>
        /// <param name="batteryStatus">
        /// Battery state information.
        /// </param>
        public static bool TryGetBatteryStateDisplay(
            string topLevelUserPath, out BatteryStatus status,
            out float level)
        {
            if (!IsExtensionEnabled.Value)
            {
                status = BatteryStatus.Unknown;
                level = 1.0f;
                return false;
            }

            return XRBatteryStateDisplayApi.TryGetBatteryStateDisplay(
                topLevelUserPath, out status, out level);
        }

        /// <inheritdoc/>
        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            if (!base.OnInstanceCreate(xrInstance))
            {
                return false;
            }

            _extensionEnabled = OpenXRRuntime.IsExtensionEnabled(ExtensionString);
            if (!_extensionEnabled.Value)
            {
                return false;
            }

            return XRInstanceManagerApi.Register(ApiXrFeature.BatteryStateDisplay);
        }

        /// <inheritdoc/>
        protected override void OnInstanceDestroy(ulong xrInstance)
        {
            XRInstanceManagerApi.Unregister(ApiXrFeature.BatteryStateDisplay);
        }
    }
}
