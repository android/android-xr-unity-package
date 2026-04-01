// <copyright file="XRCubemapLightEstimationFeature.cs" company="Google LLC">
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
    using UnityEngine.XR.ARSubsystems;
    using UnityEngine.XR.Management;
    using UnityEngine.XR.OpenXR;
    using UnityEngine.XR.OpenXR.Features;

#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.XR.OpenXR.Features;
#endif  // UNITY_EDITOR

    /// <summary>
    /// This feature provides access to the <c>XR_ANDROID_light_estimation_cubemap</c> extension
    /// and implements <see cref="XREnvironmentProbeSubsystem"/> .
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(
        UiName = UiName,
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        Company = "Google",
        Desc = "Configure Cubemap Light Estimation",
        Version = "1.0.0",
        Category = FeatureCategory.Feature,
        FeatureId = FeatureId,
        OpenxrExtensionStrings = ExtensionStrings, Priority = 99)]
#endif  // UNITY_EDITOR
    public class XRCubemapLightEstimationFeature : OpenXRFeature, IXRSpatialSdk
    {
        /// <summary>
        /// The UI name shows on the XR Plug-in Management panel, help users to understand
        /// validation errors and expected fixes.
        /// </summary>
        public const string UiName =
            "Android XR (Extensions): Cubemap Light Estimation";

        /// <summary>
        /// The feature ID string.
        /// </summary>
        public const string FeatureId = "com.google.xr.extensions.cubemap_light_estimation";

        /// <summary>
        /// The required OpenXR extension.
        /// </summary>
        public const string ExtensionStrings =
            "XR_ANDROID_light_estimation XR_ANDROID_light_estimation_cubemap";

        /// <summary>
        /// Runtime permission required to enable scene understanding fine.
        /// </summary>
        public static readonly AndroidXRPermission RequiredPermission =
            AndroidXRPermission.SceneUnderstandingFine;

        internal static bool? _extensionEnabled = null;
        private static bool _extensionAvailable = false;
        private static bool _extensionSupported = false;

        private AndroidXREnvironmentProbeSubsystem _subsystemInstance = null;

        /// <summary>
        /// Gets if the required OpenXR extension is enabled.
        /// When OpenXR runtime is waiting, it returns <c>null</c>. Otherwise, it indicates
        /// whether the <c>XR_ANDROID_light_estimation_cubemap</c> extensions is
        /// available and supported on current device.
        /// </summary>
        public static bool? IsExtensionEnabled => _extensionEnabled;

        /// <inheritdoc/>
        public XRSpatialSdkVersions GetTargetVersion()
        {
            // For the usage of XR_ANDROID_light_estimation_cubemap.
            return XRSpatialSdkVersions.XRSpatialApiLevel3;
        }

        /// <inheritdoc/>
        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            if (!base.OnInstanceCreate(xrInstance))
            {
                return false;
            }

            _extensionAvailable = true;
            string[] extensions = ExtensionStrings.Split();
            foreach (string extension in extensions)
            {
                if (string.IsNullOrEmpty(extension))
                {
                    // ExtensionStrings.Split() creates an array of size one where the first element
                    // is an empty string if the input string is empty.
                    continue;
                }

                bool extensionEnabled = OpenXRRuntime.IsExtensionEnabled(extension);
                if (!extensionEnabled)
                {
                    Debug.LogErrorFormat(
                        "{0} is not supported by current runtime, failed to enable {1}.",
                        extension, UiName);
                    _extensionAvailable = false;
                    break;
                }
            }

            if (!_extensionAvailable)
            {
                _extensionEnabled = false;
            }

            return _extensionAvailable &&
                   XRInstanceManagerApi.Register(ApiXrFeature.CubemapLightEstimation);
        }

        /// <inheritdoc/>
        protected override void OnInstanceDestroy(ulong xrInstance)
        {
            XRInstanceManagerApi.Unregister(ApiXrFeature.CubemapLightEstimation);
        }

        /// <inheritdoc/>
        protected override void OnSystemChange(ulong xrSystem)
        {
            if (_extensionAvailable && XRInstanceManagerApi.CheckSystemSupport(
                ApiXrFeature.CubemapLightEstimation, ref _extensionSupported))
            {
                _extensionEnabled = _extensionSupported;
                if (!_extensionSupported)
                {
                    Debug.LogWarningFormat(
                        "System {0} does not support feature {1}.", xrSystem, UiName);
                }
            }
        }

        /// <inheritdoc/>
        protected override void OnSubsystemCreate()
        {
            var descriptors = new List<XREnvironmentProbeSubsystemDescriptor>();
            SubsystemManager.GetSubsystemDescriptors(descriptors);
            if (descriptors.Count < 1)
            {
                Debug.LogWarning("Failed to find XREnvironmentProbeSubsystemDescriptor.");
                return;
            }

            // Create custom XREnvironmentProbeSubsystem.
            CreateSubsystem<XREnvironmentProbeSubsystemDescriptor, XREnvironmentProbeSubsystem>(
                descriptors, AndroidXREnvironmentProbeSubsystem._id);

            XRLoader xrLoader = XRSessionFeature.GetXRLoader();
            if (xrLoader != null)
            {
                _subsystemInstance = xrLoader.GetLoadedSubsystem<XREnvironmentProbeSubsystem>() as
                    AndroidXREnvironmentProbeSubsystem;
            }
            else
            {
                Debug.LogWarning("Failed to find any active loader.");
                return;
            }

            if (_subsystemInstance == null)
            {
                Debug.LogErrorFormat(
                    "Failed to find descriptor '{0}' - Cubemap Light Estimation will not run!",
                    AndroidXREnvironmentProbeSubsystem._id);
                return;
            }
        }

        /// <inheritdoc/>
        protected override void OnSubsystemDestroy()
        {
            if (_subsystemInstance != null)
            {
                _subsystemInstance.Destroy();
                _subsystemInstance = null;
            }
        }
    }
}
