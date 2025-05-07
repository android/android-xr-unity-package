// <copyright file="XRSessionFeature.cs" company="Google LLC">
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
    using System;
    using System.Collections.Generic;
    using Google.XR.Extensions.Internal;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.XR.ARSubsystems;
    using UnityEngine.XR.Management;
    using UnityEngine.XR.OpenXR;
    using UnityEngine.XR.OpenXR.Features;

#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.XR.OpenXR.Features;
#endif

#if UNITY_OPEN_XR_ANDROID_XR
    using ARSessionFeature = UnityEngine.XR.OpenXR.Features.Android.ARSessionFeature;
#endif

    /// <summary>
    /// This <c><see cref="OpenXRInteractionFeature"/></c> provides Android XR session management
    /// for all extended Android XR features, and common session configurations.
    ///
    /// It also provides Android XR implementation of <c><see cref="XRSessionSubsystem"/></c> if
    /// there is no session subsystem available.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = UiName,
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        Company = "Google",
        Desc = "Manage OpenXR sessions for all extened Android XR features, " +
            "and provide Android XR Session subsystem as needed.",
        Version = "1.0.0",
        OpenxrExtensionStrings = ExtensionStrings,
        Category = FeatureCategory.Feature,
        FeatureId = FeatureId,
        Priority = 100)]
#endif
    public class XRSessionFeature : OpenXRFeature
    {
        /// <summary>
        /// The UI name shows on the XR Plug-in Management panel, help users to understand
        /// validation errors and expected fixes.
        /// </summary>
        public const string UiName = "Android XR (Extensions): Session Management";

        /// <summary>
        /// The feature ID string.
        /// </summary>
        public const string FeatureId = "com.google.xr.extensions.session_management";

        /// <summary>
        /// The OpenXR Extension string. Used to check if this extensions is
        /// available or enabled.
        /// </summary>
        public const string ExtensionStrings = _subsamplingExtensions + " " + _spacewarpExtensions;

        /// <summary>
        /// A boolean that indicates the activity starts in XR Immersive mode,
        /// and will be launched in full-screen mode.
        /// </summary>
        public bool ImmersiveXR = true;

        private const string _subsamplingExtensions =
            "XR_META_vulkan_swapchain_create_info " +
            "XR_META_foveation_eye_tracked " +
            "XR_FB_foveation " +
            "XR_FB_foveation_configuration " +
            "XR_FB_swapchain_update_state";

        private const string _spacewarpExtensions = "XR_FB_space_warp";

        // XRSessionSubsystem implementation provided by Unity OpenXR: Android XR package.
        private static bool _arSessionFeatureInUse = false;

        [SerializeField]
        private bool _vulkanSubsampling = false;

        [SerializeField]
        private bool _spacewarp = false;

        private XRSessionSubsystem _sessionSubsystem = null;

        /// <summary>
        /// Gets or sets a value indicating whether to enable the usage of Vulkan Subsampling.
        ///
        /// Note: To toggle Vulkan Subsampling at runtime, you can set this field with <c>true</c>
        /// in Editor so the project can build with necessary BootConfig, then toggle this property
        /// at runtime.
        /// </summary>
        public bool VulkanSubsampling
        {
            get => _vulkanSubsampling;
            set
            {
                _vulkanSubsampling = value;
#if !UNITY_EDITOR
                UnityOpenXRNativeApi.MetaSetSubsampledLayout(_vulkanSubsampling);
#endif
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to enable the usage of URP Space Warp.
        /// It requires Vulkan graphics API.
        ///
        /// Note: To toggle URP Space Warp at runtime, you can set this field with <c>true</c>
        /// in Editor so the project can build with necessary BootConfig, then toggle this property
        /// at runtime.
        /// </summary>
        public bool SpaceWarp
        {
            get => _spacewarp;
            set
            {
                _spacewarp = value;
#if !UNITY_EDITOR
                UnityOpenXRNativeApi.MetaSetSpaceWarp(_spacewarp);
#endif
            }
        }

        internal static XRLoader GetXRLoader()
        {
            if (XRGeneralSettings.Instance.Manager.activeLoader == null)
            {
                // OnSubsystemCreate event happens during XRGeneralSettings:InitXRSDK()
                // and activeLoader hasn't been assigned yet.
                // Iterate all active loaders to find OpenXRLoader.
                foreach (var loader in XRGeneralSettings.Instance.Manager.activeLoaders)
                {
                    if (loader is OpenXRLoader)
                    {
                        return loader as OpenXRLoader;
                    }
                }
            }
            else
            {
                return XRGeneralSettings.Instance.Manager.activeLoader;
            }

            return null;
        }

        /// <inheritdoc/>
        protected override IntPtr HookGetInstanceProcAddr(IntPtr func)
        {
            return XRInstanceManagerApi.Intercept(func);
        }

        /// <inheritdoc/>
        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            if (!base.OnInstanceCreate(xrInstance))
            {
                return false;
            }

            string[] extensions = ExtensionStrings.Split();
            foreach (string extension in extensions)
            {
                bool extensionEnabled = OpenXRRuntime.IsExtensionEnabled(extension);
                if (!extensionEnabled)
                {
                    return false;
                }
            }

            if (_vulkanSubsampling)
            {
                UnityOpenXRNativeApi.MetaSetSubsampledLayout(true);
            }

            if (_spacewarp)
            {
                UnityOpenXRNativeApi.MetaSetSpaceWarp(true);
            }

            return XRInstanceManagerApi.Create(xrInstance, xrGetInstanceProcAddr);
        }

        /// <inheritdoc/>
        protected override void OnInstanceDestroy(ulong xrInstance)
        {
            UnityOpenXRNativeApi.MetaSetSubsampledLayout(false);
            UnityOpenXRNativeApi.MetaSetSpaceWarp(false);
            XRInstanceManagerApi.Destroy();
        }

        /// <inheritdoc/>
        protected override void OnSystemChange(ulong xrSystem)
        {
            XRInstanceManagerApi.OnSystemChange(xrSystem);
        }

        /// <inheritdoc/>
        protected override void OnSessionCreate(ulong xrSession)
        {
            XRInstanceManagerApi.OnSessionCreate(xrSession);
        }

        /// <inheritdoc/>
        protected override void OnSessionBegin(ulong xrSession)
        {
            XRInstanceManagerApi.OnSessionBegin(xrSession);
        }

        /// <inheritdoc/>
        protected override void OnSessionEnd(ulong xrSession)
        {
            XRInstanceManagerApi.OnSessionEnd(xrSession);
        }

        /// <inheritdoc/>
        protected override void OnSessionDestroy(ulong xrSession)
        {
            XRInstanceManagerApi.OnSessionDestroy(xrSession);
        }

        /// <inheritdoc/>
        protected override void OnSessionStateChange(int oldState, int newState)
        {
            XRInstanceManagerApi.OnSessionStateChange(oldState, newState);
        }

        /// <inheritdoc/>
        protected override void OnAppSpaceChange(ulong xrSpace)
        {
            XRInstanceManagerApi.OnAppSpaceChange(xrSpace);
        }

        /// <inheritdoc/>
        protected override void OnSubsystemCreate()
        {
            var descriptors = new List<XRSessionSubsystemDescriptor>();
            SubsystemManager.GetSubsystemDescriptors(descriptors);
            if (descriptors.Count < 1)
            {
                Debug.LogWarning("Failed to find XRSessionSubsystemDescriptor.");
                return;
            }

#if UNITY_OPEN_XR_ANDROID_XR
            var sessionFeature =
                OpenXRSettings.ActiveBuildTargetInstance.GetFeature<ARSessionFeature>();
            _arSessionFeatureInUse = (bool)(sessionFeature?.enabled);
#endif

            if (_arSessionFeatureInUse)
            {
                Debug.Log("Defer XRSessionSubsystem creation to AndroidOpenXRSessionSubsystem.");
                return;
            }

            // Create custom XRSessionSubsystem.
            CreateSubsystem<XRSessionSubsystemDescriptor, XRSessionSubsystem>(
                descriptors, AndroidXRSessionSubsystem._id);

            XRLoader xrLoader = GetXRLoader();

            if (xrLoader != null)
            {
                _sessionSubsystem = xrLoader.GetLoadedSubsystem<XRSessionSubsystem>();
            }
            else
            {
                Debug.LogWarning("Failed to find any active loader.");
                return;
            }

            if (_sessionSubsystem == null)
            {
                Debug.LogErrorFormat(
                    "Failed to find descriptor '{0}' - ARSession will not do anything!",
                    AndroidXRSessionSubsystem._id);
                return;
            }
            else
            {
                Debug.LogFormat("Created {0}.", _sessionSubsystem.GetType());
            }
        }

        /// <inheritdoc/>
        protected override void OnSubsystemDestroy()
        {
            if (_sessionSubsystem != null)
            {
                _sessionSubsystem.Destroy();
                _sessionSubsystem = null;
            }
        }
    }
}
