//-----------------------------------------------------------------------
// <copyright file="XRImageTrackingFeature.cs" company="Qualcomm Technologies, Inc.">
//
// Copyright Qualcomm Technologies, Inc. and/or its affiliates. All rights reserved.
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
    using UnityEngine.XR.OpenXR.Features;

#if UNITY_EDITOR
    using Unity.XR.CoreUtils;
    using UnityEditor;
    using UnityEngine.XR.ARFoundation;
    using UnityEditor.XR.OpenXR.Features;
#endif // UNITY_EDITOR

    /// <summary>
    /// This <see cref="OpenXRFeature"/> configures Android XR extensions <c>XR_ANDROID_trackables</c>,
    /// <c>XR_ANDROID_trackables_image</c>, and <c>XR_EXT_future</c> at runtime and provides a
    /// <see cref="XRImageTrackingSubsystem"/> implementation that works on the Android XR platform.
    ///
    /// Note: due to the dependency on <see cref="XRSessionFeature"/>, its priority must be lower
    /// than it so the feature registration happens after XrInstanceManager creation.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = UiName,
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        Company = "Qualcomm",
        OpenxrExtensionStrings = ExtensionStrings,
        Desc = "Enable Android XR Image Tracking at runtime and " +
               "provide the implementation of XRImageTrackingSubsystem.",
        Version = "1.0.0",
        Category = FeatureCategory.Feature,
        FeatureId = FeatureId,
        Priority = 95)]
#endif // UNITY_EDITOR
    public class XRImageTrackingFeature : OpenXRFeature, IXRSpatialSdk
    {
        /// <summary>
        /// The UI name shows on the XR Plug-in Management panel, help users to understand
        /// validation errors and expected fixes.
        /// </summary>
        public const string UiName = "Android XR (Extensions): Image Tracking";

        /// <summary>
        /// The feature ID string.
        /// </summary>
        public const string FeatureId = "com.google.xr.extensions.image_tracking";

        /// <summary>
        /// The OpenXR Extension string. Used to check if these extensions are available or enabled.
        /// </summary>
        public const string ExtensionStrings =
            "XR_ANDROID_trackables " +
            "XR_ANDROID_trackables_image " +
            "XR_EXT_future";

        /// <summary>
        /// Runtime permission required to enable scene understanding.
        /// </summary>
        public static readonly AndroidXRPermission RequiredPermission =
            AndroidXRPermission.SceneUnderstandingCoarse;

        private static bool _extensionAvailable = false;
        private static bool _extensionSupported = false;

        [Tooltip(
            "Indicate if preferring physical size estimation when it's supported at runtime. " +
            "Note: not all runtimes support estimation, " +
            "you may still need to specify the physical size in Marker references.")]
        [SerializeField]
        private bool _preferSizeEstimation = false;

        /// <summary>
        /// Gets if the required set of OpenXR extensions is enabled.
        /// When OpenXR runtime is waiting, it returns <c>null</c>. Otherwise, it indicates
        /// whether the XR_ANDROID_trackables and XR_ANDROID_trackables_image extensions are
        /// available and supported on the current device.
        /// </summary>
        public static bool? IsExtensionEnabled { get; private set; } = null;

        /// <summary>
        /// Gets the subsystem instance to be used in extension methods.
        /// </summary>
        internal static AndroidXRImageTrackingSubsystem _subsystemInstance { get; private set; }

        /// <inheritdoc/>
        public XRSpatialSdkVersions GetTargetVersion()
        {
            // For the usage of XR_ANDROID_trackables_image.
            return XRSpatialSdkVersions.XRSpatialApiLevel3;
        }

        /// <inheritdoc/>
        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            if (!base.OnInstanceCreate(xrInstance))
            {
                return false;
            }

            // Validate if the required OpenXR extensions were successfully enabled.
            _extensionAvailable = AndroidXRFeatureUtils.CheckExtensions(ExtensionStrings);
            if (!_extensionAvailable)
            {
                Debug.LogWarning($"The feature <i>{UiName}</i> is missing an OpenXR " +
                                  "extension and will be disabled");
                IsExtensionEnabled = false;
                return false;
            }

            Debug.Log($"Register OpenXRFeature '{UiName}'.");
            return XRInstanceManagerApi.Register(ApiXrFeature.Trackable);
        }

        /// <inheritdoc/>
        protected override void OnInstanceDestroy(ulong xrInstance)
        {
            // Cleanup is managed centrally for all XR*TrackingFeatures by the XrInstanceManager
            // due to the sharing of a single XrTrackableProvider among all tracking features.
            // DO NOT unregister ApiXrFeature.Trackable from within an individual tracking features.
        }

        /// <inheritdoc/>
        protected override void OnSystemChange(ulong xrSystem)
        {
            if (IsExtensionEnabled.HasValue && !IsExtensionEnabled.Value)
            {
                Debug.LogWarningFormat("Skipping subsystem initialization for {0}: " +
                                       "feature is not supported by the system.", UiName);
                return;
            }

            if (_extensionAvailable && XRTrackableApi.TryGetSystemSupport(
                ApiXrTrackableType.Image, ref _extensionSupported))
            {
                IsExtensionEnabled = _extensionSupported;
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
            var descriptors = new List<XRImageTrackingSubsystemDescriptor>();
            SubsystemManager.GetSubsystemDescriptors(descriptors);
            if (descriptors.Count < 1)
            {
                Debug.LogWarning("Failed to find XRImageTrackingSubsystemDescriptor.");
                return;
            }

            var xrLoader = XRSessionFeature.GetXRLoader();
            if (!xrLoader)
            {
                Debug.LogWarning("Failed to find any active loader.");
                return;
            }

            // Get loaded custom XRImageTrackingSubsystem as it might have been created by other
            // features.
            _subsystemInstance = xrLoader.GetLoadedSubsystem<XRImageTrackingSubsystem>() as
                AndroidXRImageTrackingSubsystem;

            // Create custom XRImageTrackingSubsystem if there is no loaded
            // XRImageTrackingSubsystem.
            if (_subsystemInstance == null)
            {
                _subsystemInstance = CreateCustomSubsystem();
            }

            if (_subsystemInstance == null)
            {
                Debug.LogError(
                    $"Failed to find descriptor '{AndroidXRImageTrackingSubsystem._id}' " +
                    "- Image Tracking will not do anything!");
                return;
            }

            _subsystemInstance.InitTracking(ApiXrTrackableType.Image, _preferSizeEstimation);
            Debug.LogFormat($"Created {_subsystemInstance.GetType()}.");
            return;

            AndroidXRImageTrackingSubsystem CreateCustomSubsystem()
            {
                // Create custom XRImageTrackingSubsystem.
                CreateSubsystem<XRImageTrackingSubsystemDescriptor, XRImageTrackingSubsystem>(
                    descriptors, AndroidXRImageTrackingSubsystem._id);
                return xrLoader.GetLoadedSubsystem<XRImageTrackingSubsystem>() as
                    AndroidXRImageTrackingSubsystem;
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

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!enabled) // The feature should only be validated if it is enabled.
            {
                return;
            }
            var xrOrigin = FindFirstObjectByType<XROrigin>();
            var imageManager = xrOrigin?.GetComponent<ARTrackedImageManager>();
            if (!imageManager)
            {
                return;
            }
            if (imageManager.referenceLibrary == null)
            {
                Debug.LogError("ARTrackedImageManager is missing a reference library.");
                return;
            }
            var allImagesValid =
                AndroidXRRuntimeImageLibrary.ValidateImages(imageManager.referenceLibrary,
                    out var validImageCount, out var invalidImageCount);
            if (!allImagesValid)
            {
                Debug.LogError(
                    $"ARTrackedImageManager's reference library contains {invalidImageCount} " +
                    "invalid image references, which will be ignored for image tracking.");
            }
            if (validImageCount == 0)
            {
                Debug.LogWarning(
                    "ARTrackedImageManager's reference library does not contain any valid image " +
                    "targets. Image tracking will not do anything.");
            }
        }
#endif // UNITY_EDITOR
    }
}
