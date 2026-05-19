// <copyright file="XRRecommendedSettingsFeature.cs" company="Google LLC">
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
    using UnityEngine.XR.OpenXR;
    using UnityEngine.XR.OpenXR.Features;
    using UnityEngine.XR.OpenXR.NativeTypes;

#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.XR.OpenXR.Features;
    using System.Collections.Generic;
#endif // UNITY_EDITOR

    /// <summary>
    /// This <see cref="XRRecommendedSettingsFeature"/> provides a function to query the recommended
    /// settings information at runtime.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = UiName,
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        Company = "Google",
        OpenxrExtensionStrings = ExtensionString,
        Desc = "Get recommended settings at runtime.",
        Version = "1.0.0",
        Category = FeatureCategory.Feature,
        FeatureId = FeatureId)]
#endif
    public class XRRecommendedSettingsFeature : OpenXRFeature, IXRSpatialSdk
    {
        /// <summary>
        /// The UI name shows on the XR Plug-in Management panel, to help users understand
        /// validation errors and expected fixes.
        /// </summary>
        public const string UiName = "Android XR: Recommended Settings";

        /// <summary>
        /// The feature ID string.
        /// </summary>
        public const string FeatureId = "com.google.xr.extensions.recommended_settings";

        /// <summary>
        /// The OpenXR Extension string. Used to check if this extension is available or enabled.
        /// </summary>
        public const string ExtensionString = "XR_ANDROID_recommended_settings";

        internal static bool? _extensionEnabled = null;

        /// <summary>
        /// Gets if the required OpenXR extension is enabled.
        /// When OpenXR runtime is waiting, it returns <c>null</c>. Otherwise, it indicates
        /// whether the XR_ANDROID_recommended_settings extension is available on the current device.
        /// </summary>
        public static bool? IsExtensionEnabled => _extensionEnabled;

        /// <summary>
        /// Gets the current recommended settings.
        /// </summary>
        /// </param>
        /// <param name="recommendedSettings">
        /// Recommended settings info obtained from the system.
        /// </param>
        public static bool TryGetRecommendedSettings(out XrRecommendedSettings recommendedSettings)
        {
            XrRecommendedSettings settings = default;
            bool result = OpenXRAndroidApi.TryGetRecommendedSettings(ref settings);
            recommendedSettings = settings;
            return result;
        }

        /// <inheritdoc/>
        public XRSpatialSdkVersions GetTargetVersion()
        {
            return XRSpatialSdkVersions.XRSpatialApiLevel1;
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

            return XRInstanceManagerApi.Register(ApiXrFeature.RecommendedSettings);
        }

        /// <inheritdoc/>
        protected override void OnInstanceDestroy(ulong xrInstance)
        {
            XRInstanceManagerApi.Unregister(ApiXrFeature.RecommendedSettings);
        }
    }
}
