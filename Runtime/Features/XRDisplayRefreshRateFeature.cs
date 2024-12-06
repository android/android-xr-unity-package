// <copyright file="XRDisplayRefreshRateFeature.cs" company="Google LLC">
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
    using Google.XR.Extensions.Internal;
    using UnityEngine.XR.OpenXR;
    using UnityEngine.XR.OpenXR.Features;

#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.XR.OpenXR.Features;
#endif

    /// <summary>
    /// This <see cref="OpenXRInteractionFeature"/> provides access
    /// to the XR_FB_display_refresh_rate extension.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = UiName,
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        Company = "Google",
        OpenxrExtensionStrings = ExtensionString,
        Desc = "Enable Android XR Performance Metrics at runtime.",
        Version = "1.0.0",
        Category = FeatureCategory.Feature,
        FeatureId = FeatureId)]
#endif
    public class XRDisplayRefreshRateFeature : OpenXRFeature
    {
        /// <summary>
        /// The UI name shows on the XR Plug-in Management panel, to help users understand
        /// validation errors and expected fixes.
        /// </summary>
        public const string UiName = "Display Refresh Rate";

        /// <summary>
        /// The feature ID string.
        /// </summary>
        public const string FeatureId = "com.google.xr.extensions.display_refresh_rate";

        /// <summary>
        /// The OpenXR Extension string. Used to check if this extension is available or enabled.
        /// </summary>
        public const string ExtensionString = "XR_FB_display_refresh_rate";

        internal static bool? _extensionEnabled = null;

        /// <summary>
        /// Gets if the required OpenXR extension is enabled.
        /// When OpenXR runtime is waiting, it returns <c>null</c>. Otherwise, it indicates
        /// whether the XR_FB_display_refresh_rate extension is available on the current device.
        /// </summary>
        public static bool? IsExtensionEnabled => _extensionEnabled;

        /// <summary>
        /// Gets the refresh rates supported by the device and
        /// the device's current refresh rate.
        /// </summary>
        /// </param>
        /// <param name="info">
        /// Refresh rate info obtained from the system.
        /// </param>
        public static bool GetDisplayRefreshRateInfo(
             out XRDisplayRefreshRateInfo info)
        {
            return XRDisplayRefreshRateApi.GetDisplayRefreshRateInfo(out info);
        }

        /// <summary>
        /// Request the system to dynamically change the display refresh rate.
        /// Note that this is only a request and does not guarantee the system
        /// will switch to the requested display refresh rate.
        /// </summary>
        /// </param>
        /// <param name="displayRefreshRate">
        /// The value must be 0.0f or one of the supported refresh rates
        /// returned by GetDisplayRefreshRateInfo. A value 0.0f indicates the
        /// application has no preference.
        /// </param>
        public static void RequestDisplayRefreshRate(
            float displayRefreshRate)
        {
            XRDisplayRefreshRateApi.RequestDisplayRefreshRate(displayRefreshRate);
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

            return XRInstanceManagerApi.Register(ApiXrFeature.DisplayRefreshRate);
        }

        /// <inheritdoc/>
        protected override void OnInstanceDestroy(ulong xrInstance)
        {
            XRInstanceManagerApi.Unregister(ApiXrFeature.DisplayRefreshRate);
        }
    }
}
