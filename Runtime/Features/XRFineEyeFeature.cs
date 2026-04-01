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

#if !UNITY_OPEN_XR_ANDROID_XR && UNITY_EDITOR
        static private UnityEditor.PackageManager.Requests.AddRequest _addRequest = null;
        static private bool _packageInstalled = false;
#endif

        /// <summary>
        /// Gets if the required OpenXR extension is enabled.
        /// </summary>
        public static bool? IsExensionEnabled => _extensionEnabled;

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
            const string arFaceFeatureName = "Android XR: AR Face";
            results.Add(new ValidationRule(this)
            {
                message = string.Format("{0} is required for this feature.", arFaceFeatureName),
                checkPredicate = () =>
                {
                    var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);
                    if (settings == null)
                    {
                        return false;
                    }

                    var arFaceFeature = settings.GetFeature<ARFaceFeature>();
                    return arFaceFeature != null && arFaceFeature.enabled;
                },
                fixIt = () =>
                {
                    var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);
                    if (settings == null)
                    {
                        Debug.LogWarningFormat(
                            "Autofix failed with missing OpenXRSettings on {0} platform.",
                            targetGroup);
                        return;
                    }

                    var feature = settings.GetFeature<ARFaceFeature>();
                    if (feature == null)
                    {
                        Debug.LogWarningFormat(
                            "Autofix failed with missing {0} feature on {1} platform.",
                            arFaceFeatureName, targetGroup);
                        return;
                    }

                    feature.enabled = true;
                },
                fixItMessage = string.Format("Enable {0} feature.", arFaceFeatureName),
                error = true,
            });
#else
            const string packageName = "Unity OpenXR Android XR (v1.0.0-pre.2)";
            results.Add(new ValidationRule(this)
            {
                message = string.Format("{0} package is required for this feature.", packageName),
                checkPredicate = () =>
                {
                    return _packageInstalled;
                },
                fixIt = () =>
                {
                    const string packageIdentifier = "com.unity.xr.androidxr-openxr@1.0.0-pre.2";
                    if (_addRequest == null)
                    {
                        _addRequest = UnityEditor.PackageManager.Client.Add(packageIdentifier);
                        Debug.LogWarningFormat("[{0}] Sending a request to add package {1}.",
                            UiName, packageName);
                    }
                    else
                    {
                        switch (_addRequest.Status)
                        {
                            case UnityEditor.PackageManager.StatusCode.InProgress:
                                Debug.LogWarningFormat("[{0}] Waiting for adding package {1}",
                                    UiName, packageName);
                                break;
                            case UnityEditor.PackageManager.StatusCode.Success:
                                Debug.LogFormat("[{0}] Successfully added package {1} ({2}).",
                                    UiName, packageName, packageIdentifier);
                                _addRequest = null;
                                _packageInstalled = true;
                                break;
                            case UnityEditor.PackageManager.StatusCode.Failure:
                                Debug.LogWarningFormat(
                                    "[{0}] Failed to add package {1} with error {2}. " +
                                    "Please try again later or manually instal package {3}.",
                                    UiName, packageName, packageIdentifier,
                                    _addRequest.Error.message);
                                _addRequest = null;
                                break;
                        }
                    }
                },
                fixItMessage = string.Format("Install {0} package or newer.", packageName),
                error = true,
            });
#endif // UNITY_OPEN_XR_ANDROID_XR
        }
#endif
    }
}
