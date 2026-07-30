// <copyright file="XRGeospatialFeature.cs" company="Google LLC">
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
    using Google.XR.Extensions.Internal;
    using UnityEngine;
    using UnityEngine.Android;
    using UnityEngine.XR.OpenXR.Features;

#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.XR.OpenXR.Features;
#endif

    /// <summary>
    /// This <c><see cref="OpenXRInteractionFeature"/></c> configures Android XR
    /// extensions <c>XR_ANDROID_geospatial</c> at runtime and provides
    /// <see cref="XRGeospatialManager"/> to access geospatial capabilities.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = UiName,
        BuildTargetGroups = new[] {
            BuildTargetGroup.Android,
        },
        Company = "Google",
        OpenxrExtensionStrings = ExtensionStrings,
        Desc = "Provides geospatial access at OpenXR runtimes.",
        Version = "1.0.0",
        Category = FeatureCategory.Feature,
        FeatureId = FeatureId,
        Priority = 95)]
#endif
    public sealed class XRGeospatialFeature : OpenXRFeature, IXRSpatialSdk
    {
        /// <summary>
        /// The UI name shows on the XR Plug-in Management panel, it helps users
        /// to understand validation errors and expected fixes.
        /// </summary>
        public const string UiName = "Android XR: Geospatial";

        /// <summary>
        /// The feature ID string.
        /// </summary>
        public const string FeatureId = "com.google.xr.extensions.geospatial";

        /// <summary>
        /// The OpenXR Extension string. Used to check if this extensions is
        /// available or enabled.
        /// </summary>
        public const string ExtensionStrings =
            "XR_EXT_future " +
            "XR_ANDROID_geospatial";

        /// <summary>
        /// Android runtime permission <see cref="Permission.FineLocation"/> is
        /// required for geospatial APIs.
        /// </summary>
        public static readonly string RequiredPermission = Permission.FineLocation;

        internal static bool? _extensionEnabled = null;

        /// <summary>
        /// Gets if the required OpenXR extension is enabled.
        /// When OpenXR runtime is waiting, it returns <c>null</c>.
        /// Otherwise, it indicates whether the <c>XR_ANDROID_geospatial</c>
        /// extension is available on current device.
        /// </summary>
        public static bool? IsExtensionEnabled => _extensionEnabled;

        /// <inheritdoc/>
        public XRSpatialSdkVersions GetTargetVersion()
        {
            return XRSpatialSdkVersions.XRSpatialApiLevel3;
        }

        /// <inheritdoc/>
        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            bool initialized = AndroidXRFeatureUtils.CheckExtensions(ExtensionStrings) &&
                XRInstanceManagerApi.Register(ApiXrFeature.Geospatial);
            if (!initialized)
            {
                _extensionEnabled = false;
                XRGeospatialManager.State = XRGeospatialState.Unsupported;
            }

            return initialized;
        }

        /// <inheritdoc/>
        protected override void OnInstanceDestroy(ulong xrInstance)
        {
            XRInstanceManagerApi.Unregister(ApiXrFeature.Geospatial);
        }

        /// <inheritdoc/>
        protected override void OnSystemChange(ulong xrSystem)
        {
            bool supported = false;
            if (!XRInstanceManagerApi.CheckSystemSupport(ApiXrFeature.Geospatial, ref supported))
            {
                return;
            }

            _extensionEnabled = supported;
            if (!supported)
            {
                XRGeospatialManager.State = XRGeospatialState.Unsupported;
                Debug.LogWarningFormat(
                    "System {0} does not support feature {1}.", xrSystem, UiName);
            }
            else
            {
                // Waiting for session creation.
                XRGeospatialManager.State = XRGeospatialState.Initializing;
            }
        }

        /// <inheritdoc/>
        protected override void OnSessionCreate(ulong xrSession)
        {
            XRGeospatialManager.SetSession(xrSession);
        }

        /// <inheritdoc/>
        protected override void OnSessionDestroy(ulong xrSession)
        {
            XRGeospatialManager.SetSession(0);
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

            // A valid authentication is required, currently only Cloud Auth is supported.
            results.Add(AndroidXRFeatureUtils.GetFeatureDependencyRule(
                feature: this,
                requiredFeatureType: typeof(XRGoogleCloudFeature),
                requiredFeatureName: XRGoogleCloudFeature.UiName,
                buildTarget: targetGroup));
        }
#endif // UNITY_EDITOR
    }
}
