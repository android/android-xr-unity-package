// <copyright file="XRMultimodalFaceFeature.cs" company="Google LLC">
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
    /// This feature exposes multimodal face tracking capabilities provided by
    /// <c>XR_ANDROID_face_tracking_data_source</c> extension.
    /// It's a supplement to <b>Unity OpenXR Android XR</b>'s <b>Android XR: AR Face</b> feature
    /// which integrates <see cref="XRFaceSubsystem"/> and exposes current tracking modality via
    /// <see cref="ARFaceManagerExtensions.TryGetFaceTrackerMode(ARFaceManager, out XRFaceTrackerMode)"/>.
    ///
    /// If this feature is not enabled, ARFace will default to image-only face tracking, which may
    /// not be supported on all devices.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(
        UiName = UiName,
        BuildTargetGroups = new[] {
            BuildTargetGroup.Android,
        },
        Company = "Google",
        Desc = "Multimodal face tracking.",
        Version = "1.0.0",
        Category = FeatureCategory.Feature,
        FeatureId = FeatureId,
        OpenxrExtensionStrings = ExtensionStrings,
        Priority = 90)]
#endif  // UNITY_EDITOR
    public class XRMultimodalFaceFeature : OpenXRFeature
    {
        /// <summary>
        /// The UI name shows on the XR Plug-in Management panel, help users to understand
        /// validation errors and expected fixes.
        /// </summary>
        public const string UiName = "Android XR (Extensions): Multimodal Face";

        /// <summary>
        /// The feature ID string.
        /// </summary>
        public const string FeatureId = "com.google.xr.extensions.face_tracking_data_source";

        /// <summary>
        /// The required OpenXR extension.
        /// </summary>
        public const string ExtensionStrings = "XR_ANDROID_face_tracking_data_source";

        /// <summary>
        /// Runtime permissions required to enable extra face tracking modalities.
        /// </summary>
        public static readonly AndroidXRPermission RequiredPermission =
            AndroidXRPermission.RecordAudio;

        internal static bool? _extensionEnabled = null;

        [SerializeField]
        [Tooltip("The face tracking modality to request the provider to use. If not supported, " +
            "the provider will fall back to the first supported modality.")]
        private XRFaceTrackerMode _requestedFaceTrackingMode = XRFaceTrackerMode.Multimodal;

        /// <summary>
        /// Gets if the required OpenXR extension is enabled.
        /// </summary>
        public static bool? IsExtensionEnabled => _extensionEnabled;

        /// <summary>
        /// Gets the requested face tracking modality for the provider to try to use. If not
        /// supported, the provider will fall back to use the supported modalities. This can only
        /// be set in project settings before runtime.
        /// </summary>
        public XRFaceTrackerMode RequestedFaceTrackingMode => _requestedFaceTrackingMode;

        /// <inheritdoc/>
        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            if (!base.OnInstanceCreate(xrInstance))
            {
                return false;
            }

            if (!OpenXRRuntime.IsExtensionEnabled(ExtensionStrings))
            {
                Debug.LogWarningFormat("Extension {0} is missing. Not enabling multimodal face " +
                    "tracking.", ExtensionStrings);
                return true;
            }

            if (!XRInstanceManagerApi.Register(ApiXrFeature.MultimodalFaceTracking))
            {
                return false;
            }

            if (!XRMultimodalFaceTrackerApi.Enable(RequestedFaceTrackingMode))
            {
                return false;
            }

            _extensionEnabled = true;
            return true;
        }

        /// <inheritdoc/>
        protected override void OnInstanceDestroy(ulong xrInstance)
        {
            if (_extensionEnabled.HasValue && _extensionEnabled.Value)
            {
                XRInstanceManagerApi.Unregister(ApiXrFeature.MultimodalFaceTracking);
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
