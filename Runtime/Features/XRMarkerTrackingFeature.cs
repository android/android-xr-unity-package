// <copyright file="XRMarkerTrackingFeature.cs" company="Google LLC">
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
    using UnityEngine.SceneManagement;
    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.ARSubsystems;
    using UnityEngine.XR.Management;
    using UnityEngine.XR.OpenXR;
    using UnityEngine.XR.OpenXR.Features;

#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.XR.OpenXR.Features;
#endif

    /// <summary>
    /// This <see cref="OpenXRInteractionFeature"/> configures Android XR extensions
    /// <c>XR_ANDROID_trackables</c> and <c>XR_ANDROID_trackables_marker</c> at runtime and
    /// provides <see cref="XRImageTrackingSubsystem"/> implementation that works on Android XR
    /// platform.
    ///
    /// Note: due to the dependency on <see cref="XRSessionFeature"/>, its priority must be lower
    /// than it so the feature registration happens after XrInstanceManager creation.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = UiName,
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        Company = "Google",
        OpenxrExtensionStrings = ExtensionStrings,
        Desc = "Enable Android XR Marker Tracking at runtime. " +
            "And provide the implementation of XRImageTrackingSubsystem",
        Version = "1.0.0",
        Category = FeatureCategory.Feature,
        FeatureId = FeatureId,
        Priority = 95)]
#endif
    public class XRMarkerTrackingFeature : OpenXRFeature, IXRSpatialSdk
    {
        /// <summary>
        /// The UI name shows on the XR Plug-in Management panel, help users to understand
        /// validation errors and expected fixes.
        /// </summary>
        public const string UiName = "Android XR (Extensions): Image Tracking (Marker)";

        /// <summary>
        /// The feature ID string.
        /// </summary>
        public const string FeatureId = "com.google.xr.extensions.marker_tracking";

        /// <summary>
        /// The OpenXR Extension string. Used to check if this extensions is
        /// available or enabled.
        /// </summary>
        public const string ExtensionStrings =
            "XR_ANDROID_trackables " +
            "XR_ANDROID_trackables_marker";

        /// <summary>
        /// Runtime permission required to enable scene understanding.
        /// </summary>
        public static readonly AndroidXRPermission RequiredPermission =
            AndroidXRPermission.SceneUnderstandingCoarse;

        internal static bool? _extensionEnabled = null;
        private static bool _extensionAvailable = false;
        private static bool _extensionSupported = false;

        [Tooltip("Indicate if preferring size estimation when it's supported at runtime. " +
            "Note: not all runtimes support estimation, " +
            "you may still need to specify the physical size in Marker references.")]
        [SerializeField]
        private bool _preferEstimation = false;

        /// <summary>
        /// Gets if the required OpenXR extension is enabled.
        /// When OpenXR runtime is waiting, it returns <c>null</c>. Otherwise, it indicates
        /// whether the XR_ANDROID_trackables and XR_ANDROID_trackables_marker extensions are
        /// available and supported on current device.
        /// </summary>
        public static bool? IsExtensionEnabled => _extensionEnabled;

        /// <summary>
        /// Gets the subsystem instance that to be used in extension methods.
        /// </summary>
        internal static AndroidXRImageTrackingSubsystem _subsystemInstance { get; private set; }

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

            string[] extensions = ExtensionStrings.Split();
            _extensionAvailable = false;
            foreach (string extension in extensions)
            {
                if (!OpenXRRuntime.IsExtensionEnabled(extension))
                {
                    _extensionEnabled = false;
                    return false;
                }
            }

            _extensionAvailable = true;

            // Basic tracking function uses XrTrackableProvider.
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
        protected override void OnSystemChange(ulong xrSystem)
        {
            if (_extensionAvailable && XRTrackableApi.TryGetSystemSupport(
                ApiXrTrackableType.Marker, ref _extensionSupported))
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
            var descriptors = new List<XRImageTrackingSubsystemDescriptor>();
            SubsystemManager.GetSubsystemDescriptors(descriptors);
            if (descriptors.Count < 1)
            {
                Debug.LogWarning("Failed to find XRImageTrackingSubsystemDescriptor.");
                return;
            }

            XRLoader xrLoader = XRSessionFeature.GetXRLoader();
            if (xrLoader != null)
            {
                // XRImageTrackingSubsystem might have been created by other features.
                _subsystemInstance = xrLoader.GetLoadedSubsystem<XRImageTrackingSubsystem>() as
                    AndroidXRImageTrackingSubsystem;
            }
            else
            {
                Debug.LogWarning("Failed to find any active loader.");
                return;
            }

            if (_subsystemInstance == null)
            {
                // Create custom XRImageTrackingSubsystem.
                CreateSubsystem<XRImageTrackingSubsystemDescriptor, XRImageTrackingSubsystem>(
                    descriptors, AndroidXRImageTrackingSubsystem._id);
                _subsystemInstance = xrLoader.GetLoadedSubsystem<XRImageTrackingSubsystem>() as
                    AndroidXRImageTrackingSubsystem;
            }

            if (_subsystemInstance == null)
            {
                Debug.LogErrorFormat(
                    "Failed to find descriptor '{0}' - Marker Tracking will not do anything!",
                    AndroidXRImageTrackingSubsystem._id);
                return;
            }
            else
            {
                _subsystemInstance.InitTracking(
                    ApiXrTrackableType.Marker, _preferEstimation);
                Debug.LogFormat("Created {0}.", _subsystemInstance.GetType());
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
            // The feature should only be validated if it is enabled.
            if (!enabled)
            {
                return;
            }

            Scene activeScene = SceneManager.GetActiveScene();
            GameObject[] rootGameObjects = activeScene.GetRootGameObjects();
            foreach (GameObject rootGameObject in rootGameObjects)
            {
                ARTrackedImageManager imageManager =
                    rootGameObject.GetComponentInChildren<ARTrackedImageManager>();
                if (imageManager != null && imageManager.referenceLibrary != null)
                {
                    AndroidXRRuntimeImageLibrary.ValidateMarker(imageManager.referenceLibrary);
                    return;
                }
            }
        }
#endif
    }
}
