// <copyright file="XRObjectTrackingFeature.cs" company="Google LLC">
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

    /// <summary>
    /// This <see cref="OpenXRInteractionFeature"/> configures Android XR extensions
    /// <c>XR_ANDROID_trackables</c> and <c>XR_ANDROID_trackables_object</c> at runtime and provides
    /// <see cref="XRObjectTrackingSubsystem"/> implementation that works on Android XR platform.
    ///
    /// Note: due to the dependency on <see cref="XRSessionFeature"/> and the shared native provider
    /// of <see cref="XRTrackableFeature"/>, its priority must be lower than both features so the
    /// feature registration happens after XrInstanceManager and XrTrackableProvider creation.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = UiName,
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        Company = "Google",
        OpenxrExtensionStrings = ExtensionStrings,
        Desc = "Enable Android XR Object Tracking at runtime. " +
            "And provide the implementation of XRObjectTrackingSubsystem",
        Version = "1.0.0",
        Category = FeatureCategory.Feature,
        FeatureId = FeatureId,
        Priority = 97)]
#endif
    public class XRObjectTrackingFeature : OpenXRFeature
    {
        /// <summary>
        /// The UI name shows on the XR Plug-in Management panel, help users to understand
        /// validation errors and expected fixes.
        /// </summary>
        public const string UiName = "Android XR (Extensions): Object Tracking";

        /// <summary>
        /// The feature ID string.
        /// </summary>
        public const string FeatureId = "com.google.xr.extensions.object_tracking";

        /// <summary>
        /// The OpenXR Extension string. Used to check if this extensions is
        /// available or enabled.
        /// To enable runtime confiugration, always enabling persistence extension
        /// when it's available.
        /// </summary>
        public const string ExtensionStrings =
            "XR_ANDROID_trackables " +
            "XR_ANDROID_trackables_object";

        /// <summary>
        /// Runtime permission required to enable scene understanding.
        /// </summary>
        public static readonly AndroidXRPermission RequiredPermission =
            AndroidXRPermission.SceneUnderstandingCoarse;

        internal static bool? _extensionEnabled = null;

        private XRObjectTrackingSubsystem _trackingSubsystem = null;

        /// <summary>
        /// Gets if the required OpenXR extension is enabled.
        /// When OpenXR runtime is waiting, it returns <c>null</c>. Otherwise, it indicates
        /// whether the XR_ANDROID_trackables and XR_ANDROID_trackables_object extensions are
        /// available on current device.
        /// </summary>
        public static bool? IsExtensionEnabled => _extensionEnabled;

        /// <inheritdoc/>
        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            if (!base.OnInstanceCreate(xrInstance))
            {
                return false;
            }

            string[] extensions = ExtensionStrings.Split(" ");
            _extensionEnabled = false;
            foreach (string extension in extensions)
            {
                if (!OpenXRRuntime.IsExtensionEnabled(extension))
                {
                    return false;
                }
            }

            _extensionEnabled = true;

            // Basic tracking funtion uses XrTrackableProvider.
            return XRInstanceManagerApi.Register(ApiXrFeature.Trackable);
        }

        /// <inheritdoc/>
        protected override void OnInstanceDestroy(ulong xrInstance)
        {
            // Due the sharing of XrTrackableProvider among all trackables,
            // DO NOT unregister ApiXrFeature.Trackable from a signle feature.
            // Leave to XrInstanceManager to destroy XrTrackableProvider.
        }

        /// <inheritdoc/>
        protected override void OnSubsystemCreate()
        {
            var descriptors = new List<XRObjectTrackingSubsystemDescriptor>();
            SubsystemManager.GetSubsystemDescriptors(descriptors);
            if (descriptors.Count < 1)
            {
                Debug.LogWarning("Failed to find XRObjectTrackingSubsystemDescriptor.");
                return;
            }

            // Create custom XRObjectTrackingSubsystem.
            CreateSubsystem<XRObjectTrackingSubsystemDescriptor, XRObjectTrackingSubsystem>(
                descriptors, AndroidXRObjectTrackingSubsystem._id);

            XRLoader xrLoader = XRSessionFeature.GetXRLoader();
            if (xrLoader != null)
            {
                _trackingSubsystem = xrLoader.GetLoadedSubsystem<XRObjectTrackingSubsystem>();
            }
            else
            {
                Debug.LogWarning("Failed to find any active loader.");
            }

            if (_trackingSubsystem == null)
            {
                Debug.LogErrorFormat(
                    "Failed to find descriptor '{0}' - Object Tracking will not do anything!",
                    AndroidXRObjectTrackingSubsystem._id);
                return;
            }
            else
            {
                Debug.LogFormat("Created {0}.", _trackingSubsystem.GetType());
            }
        }

        /// <inheritdoc/>
        protected override void OnSubsystemStart()
        {
            Debug.Log($"{ApiConstants.LogTag}:: Start AndroidXRObjectTrackingSubsystem.");
            if (_trackingSubsystem != null)
            {
                // TODO(b/322517976): track long term solution and update accordingly.
                // Temp solution to workaround subsystem crashes around null library.
                // It still results in spamming errors from ARTrackedObjectManager.
                Debug.Log("Setting an empty library before start.");
                _trackingSubsystem.library = new XRReferenceObjectLibrary();
                _trackingSubsystem.Start();
            }
        }

        /// <inheritdoc/>
        protected override void OnSubsystemStop()
        {
            Debug.Log($"{ApiConstants.LogTag}:: Stop AndroidXRObjectTrackingSubsystem.");
            if (_trackingSubsystem != null)
            {
                _trackingSubsystem.Stop();
            }
        }

        /// <inheritdoc/>
        protected override void OnSubsystemDestroy()
        {
            Debug.Log($"{ApiConstants.LogTag}:: Destroy AndroidXRObjectTrackingSubsystem.");
            if (_trackingSubsystem != null)
            {
                _trackingSubsystem.Destroy();
            }
        }
    }
}
