// <copyright file="XRTrackableFeature.cs" company="Google LLC">
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
    using Google.XR.Extensions.Internal;
    using UnityEngine;
    using UnityEngine.XR.ARSubsystems;
    using UnityEngine.XR.Management;
    using UnityEngine.XR.OpenXR;
    using UnityEngine.XR.OpenXR.Features;

#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.XR.OpenXR.Features;
#endif

#if UNITY_OPEN_XR_ANDROID_XR
    using ARPlaneFeature = UnityEngine.XR.OpenXR.Features.Android.ARPlaneFeature;
#endif

    /// <summary>
    /// This <see cref="OpenXRInteractionFeature"/> configures new extension
    /// <c>XR_ANDROID_trackables</c> at runtime and provides <see cref="XRPlaneSubsystem"/>
    /// implementation that works on Android XR platform.
    ///
    /// Note: due to the dependency on <see cref="XRSessionFeature"/>, its priority must be lower
    /// than session feature so the feature registration happens after XrInstanceManager creation.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = UiName,
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        Company = "Google",
        OpenxrExtensionStrings = ExtensionString,
        Desc = "Enable Android XR Trackables at runtime. " +
            "And provide the implementation of XRPlaneSubsystem",
        Version = "1.0.0",
        Category = FeatureCategory.Feature,
        FeatureId = FeatureId,
        Priority = 99)]
#endif
    public class XRTrackableFeature : OpenXRFeature
    {
        /// <summary>
        /// The UI name shows on the XR Plug-in Management panel, help users to understand
        /// validation errors and expected fixes.
        /// </summary>
        public const string UiName = "Android XR (Extensions): Plane";

        /// <summary>
        /// The feature ID string.
        /// </summary>
        public const string FeatureId = "com.google.xr.extensions.trackables";

        /// <summary>
        /// The OpenXR Extension string. Used to check if this extensions is
        /// available or enabled.
        /// </summary>
        public const string ExtensionString = "XR_ANDROID_trackables";

        /// <summary>
        /// Runtime permission required to enable scene understanding.
        /// </summary>
        public static readonly AndroidXRPermission RequiredPermission =
            AndroidXRPermission.SceneUnderstanding;

        internal static bool? _extensionEnabled = null;

        private XRPlaneSubsystem _planeSubsystem = null;

        /// <summary>
        /// Gets if the required OpenXR extension is enabled.
        /// When OpenXR runtime is waiting, it returns <c>null</c>. Otherwise, it indicates
        /// whether the XR_ANDROID_trackables extension is available on current device.
        /// </summary>
        public static bool? IsExtensionEnabled => _extensionEnabled;

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

            return XRInstanceManagerApi.Register(ApiXrFeature.Trackable);
        }

        /// <inheritdoc/>
        protected override void OnInstanceDestroy(ulong xrInstance)
        {
            // Due the sharing of XrTrackableProvider among all trackables,
            // DO NOT unregister ApiXrFeature.Trackable from a single feature.
            // Leave to XrInstanceManager to destroy XrTrackableProvider.
        }

        /// <inheritdoc/>
        protected override void OnSubsystemCreate()
        {
            var descriptors = new List<XRPlaneSubsystemDescriptor>();
            SubsystemManager.GetSubsystemDescriptors(descriptors);
            if (descriptors.Count < 1)
            {
                Debug.LogWarning("Failed to find XRPlaneSubsystemDescriptor.");
                return;
            }

            // Create custom XRPlaneSubsystem.
            CreateSubsystem<XRPlaneSubsystemDescriptor, XRPlaneSubsystem>(
                descriptors, AndroidXRPlaneSubsystem._id);

            XRLoader xrLoader = XRSessionFeature.GetXRLoader();
            if (xrLoader != null)
            {
                _planeSubsystem = xrLoader.GetLoadedSubsystem<XRPlaneSubsystem>();
            }
            else
            {
                Debug.LogWarning("Failed to find any active loader.");
            }

            if (_planeSubsystem == null)
            {
                Debug.LogErrorFormat(
                    "Failed to find descriptor '{0}' - Plane Detection will not do anything!",
                    AndroidXRPlaneSubsystem._id);
                return;
            }
            else
            {
                Debug.LogFormat("Created {0}.", _planeSubsystem.GetType());
            }
        }

        /// <inheritdoc/>
        protected override void OnSubsystemStart()
        {
            Debug.Log($"{ApiConstants.LogTag}:: Start AndroidXRPlaneSubsystem.");
            if (_planeSubsystem != null)
            {
                _planeSubsystem.Start();
            }
        }

        /// <inheritdoc/>
        protected override void OnSubsystemStop()
        {
            Debug.Log($"{ApiConstants.LogTag}:: Stop AndroidXRPlaneSubsystem.");
            if (_planeSubsystem != null)
            {
                _planeSubsystem.Stop();
            }
        }

        /// <inheritdoc/>
        protected override void OnSubsystemDestroy()
        {
            Debug.Log($"{ApiConstants.LogTag}:: Destroy AndroidXRPlaneSubsystem.");
            if (_planeSubsystem != null)
            {
                _planeSubsystem.Destroy();
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
            const string arPlaneUiNanem = "AndroidXR: AR Plane";
            results.Add(new ValidationRule(this)
            {
                // Use UI names to help users to understand the problem and suggestion.
                message = string.Format(
                    "{0} is incompatible and duplicate with {1}, " +
                    "please choose one of the AR Plane implementations.",
                    XRTrackableFeature.UiName, arPlaneUiNanem),
                checkPredicate = () =>
                {
                    var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);
                    if (settings != null)
                    {
                        var aRPlane = settings.GetFeature<ARPlaneFeature>();
                        return aRPlane == null || !aRPlane.enabled;
                    }
                    else
                    {
                        return true;
                    }
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

                    var arPlane = settings.GetFeature<ARPlaneFeature>();
                    if (arPlane == null)
                    {
                        Debug.LogWarningFormat(
                            "Autofix failed with missing {0} on {1} platform.",
                            arPlaneUiNanem, targetGroup);
                        return;
                    }

                    // Prefer XRTrackableFeature over ARPlaneFeature.
                    arPlane.enabled = false;
                },
                error = true
            });
#endif
        }
#endif // UNITY_EDITOR
    }
}
