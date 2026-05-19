// <copyright file="XRFineEyeFeature.cs" company="Google LLC">
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
    using System.Collections.Generic;
    using Google.XR.Extensions.Internal;
    using UnityEngine;
    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.ARSubsystems;
    using UnityEngine.XR.OpenXR;
    using UnityEngine.XR.OpenXR.Features;

#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.XR.OpenXR.Features;
#endif  // UNITY_EDITOR

#if UNITY_OPEN_XR_ANDROID_XR
    using ARFaceFeature = UnityEngine.XR.OpenXR.Features.Android.ARFaceFeature;
#endif

    /// <summary>
    /// This feature exposes fine eye info provided by <c>XR_ANDROID_eye_tracking</c> extension.
    /// It's a supplement to <b>Unity OpenXR Android XR</b>'s <b>Android XR: AR Face</b> feature
    /// which integrates <see cref="XRFaceSubsystem"/> and exposes coarse eye info via
    /// <see cref="ARFace.leftEye"/> and <see cref="ARFace.rightEye"/>.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(
        UiName = UiName,
        BuildTargetGroups = new[] {
            BuildTargetGroup.Android,
            BuildTargetGroup.Standalone,
        },
        Company = "Google",
        Desc = "Fine eye poses.",
        Version = "1.0.0",
        Category = FeatureCategory.Feature,
        FeatureId = FeatureId,
        OpenxrExtensionStrings = ExtensionStrings,
        Priority = 90)]
#endif  // UNITY_EDITOR
    public class XRFineEyeFeature : OpenXRFeature, IXRSpatialSdk
    {
        /// <summary>
        /// The UI name shows on the XR Plug-in Management panel, help users to understand
        /// validation errors and expected fixes.
        /// </summary>
        public const string UiName = "Android XR (Extensions): Fine Eye";

        /// <summary>
        /// The feature ID string.
        /// </summary>
        public const string FeatureId = "com.google.xr.extensions.fine_eye";

        /// <summary>
        /// The required OpenXR extension.
        /// </summary>
        public const string ExtensionStrings = "XR_ANDROID_eye_tracking";

        /// <summary>
        /// Runtime permissions required to enable fine eye.
        /// </summary>
        public static readonly AndroidXRPermission RequiredPermission =
            AndroidXRPermission.EyeTrackingFine;

        internal static bool? _extensionEnabled = null;
        private static bool _extensionAvailable = false;
        private static bool _extensionSupported = false;

        /// <summary>
        /// Gets if the required OpenXR extension is enabled.
        /// </summary>
        public static bool? IsExtensionEnabled => _extensionEnabled;

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

            _extensionAvailable = false;
            if (!OpenXRRuntime.IsExtensionEnabled(ExtensionStrings))
            {
                _extensionEnabled = false;
                return false;
            }

            _extensionAvailable = true;
            return XRInstanceManagerApi.Register(ApiXrFeature.EyeTracking);
        }

        /// <inheritdoc/>
        protected override void OnInstanceDestroy(ulong xrInstance)
        {
            XRInstanceManagerApi.Unregister(ApiXrFeature.EyeTracking);
        }

        /// <inheritdoc/>
        protected override void OnSystemChange(ulong xrSystem)
        {
            if (_extensionAvailable && XRInstanceManagerApi.CheckSystemSupport(
                ApiXrFeature.EyeTracking, ref _extensionSupported))
            {
                _extensionEnabled = _extensionSupported;
                if (!_extensionSupported)
                {
                    Debug.LogWarningFormat(
                        "System {0} does not support feature {1}.", xrSystem, UiName);
                }
            }
        }

#if UNITY_OPEN_XR_ANDROID_XR
        /// <inheritdoc/>
        protected override void OnSubsystemStart()
        {
            if (_extensionEnabled.HasValue && _extensionEnabled.Value)
            {
                XREyeTrackingApi.SetEnable(true);
            }
        }

        /// <inheritdoc/>
        protected override void OnSubsystemStop()
        {
            if (_extensionEnabled.HasValue && _extensionEnabled.Value)
            {
                XREyeTrackingApi.SetEnable(false);
            }
        }
#endif // UNITY_OPEN_XR_ANDROID_XR
#if UNITY_EDITOR
        /// <inheritdoc/>
        protected override void GetValidationChecks(
            List<ValidationRule> results, BuildTargetGroup targetGroup)
        {
            if (targetGroup != BuildTargetGroup.Android)
            {
                return;
            }
#if UNITY_OPEN_XR_ANDROID_XR
            results.Add(AndroidXRFeatureUtils.GetFeatureDependencyRule(
                feature: this,
                requiredFeatureType: typeof(ARFaceFeature),
                requiredFeatureName: ApiConstants.UnityARFaceFeature,
                buildTarget: targetGroup));
#else
            results.Add(AndroidXRFeatureUtils.GetPackageDependencyRule(
                feature: this,
                featureName: UiName,
                package: ApiConstants.UnityAXRPackage,
                displayName: ApiConstants.UnityAXRPackageDisplayName,
                version: ApiConstants.UnityAXRPackageVersion));
#endif // UNITY_OPEN_XR_ANDROID_XR
        }
#endif
    }
}
