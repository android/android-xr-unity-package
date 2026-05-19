// <copyright file="XRPassthroughFeature.cs" company="Google LLC">
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
    using System.Runtime.InteropServices;
    using Google.XR.Extensions.Internal;
    using UnityEngine;
    using UnityEngine.XR.OpenXR;
#if XR_COMPOSITION_LAYERS
    using UnityEngine.XR.OpenXR.CompositionLayers;
    using UnityEngine.XR.OpenXR.Features;
    using UnityEngine.XR.OpenXR.Features.CompositionLayers;
#else
    using UnityEngine.XR.OpenXR.Features;
#endif

#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.XR.OpenXR.Features;
#endif

    /// <summary>
    /// This <c><see cref="OpenXRInteractionFeature"/></c> configures the
    /// <c>XR_ANDROID_composition_layer_passthrough_mesh</c> and
    /// <c>XR_ANDROID_passthrough_camera_state</c> extensions
    /// at OpenXR runtime and provides passthrough geometry capabilities in the OpenXR platform.
    ///
    /// Use <c><see cref="Unity.XR.CompositionLayers.CompositionLayer"/></c> with Passthrough layer
    /// type to access passthrough cutout at runtime.
    /// Note: a valid <c><see cref="MeshFilter.mesh"/></c> is required to configure the layer
    /// geometry.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = UiName,
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        Company = "Google",
        OpenxrExtensionStrings = ExtensionStrings,
        Desc = "Define passthrough layers.",
        Version = "1.0.0",
        Category = FeatureCategory.Feature,
        FeatureId = FeatureId)]
#endif
    public class XRPassthroughFeature : OpenXRFeature, IXRSpatialSdk
    {
        /// <summary>
        /// The UI name shows on the XR Plug-in Management panel, help users to understand
        /// validation errors and expected fixes.
        /// </summary>
        public const string UiName = "Android XR (Extensions): Passthrough Composition Layer";

        /// <summary>
        /// The feature ID string.
        /// </summary>
        public const string FeatureId = "com.google.xr.extensions.passthrough_composition_layer";

        /// <summary>
        /// The OpenXR Extension strings. Used to check if this extensions is available or enabled.
        /// </summary>
        public const string ExtensionStrings =
            "XR_ANDROID_composition_layer_passthrough_mesh " +
            "XR_ANDROID_passthrough_camera_state";

        internal static bool? _extensionEnabled = null;
        private static bool _extensionAvailable = false;
        private static bool _extensionSupported = false;

#if XR_COMPOSITION_LAYERS
        // Indicate if has registered composition layer handler.
        bool _isSubscribed;
#endif

        /// <summary>
        /// Gets if the required OpenXR extension is enabled.
        /// When OpenXR runtime is waiting, it returns <c>null</c>. Otherwise, it indicates
        /// whether the <c>XR_ANDROID_composition_layer_passthrough_mesh</c> extension is enabled.
        /// </summary>
        public static bool? IsExtensionEnabled => _extensionEnabled;

        /// <summary>
        /// Get the state of the passthrough camera.
        /// </summary>
        /// <returns>The current state of the passthrough camera.</returns>
        public static XRPassthroughCameraStates GetState()
        {
            XRPassthroughCameraStates state = XRPassthroughCameraStates.Disabled;
            if (_extensionEnabled.HasValue && _extensionEnabled.Value)
            {
                ExternalApi.OpenXRAndroid_getPassthroughCameraState(
                    XRInstanceManagerApi.GetIntPtr(), ref state);
            }

            return state;
        }

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

            string[] extensions = ExtensionStrings.Split(" ");
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

            bool registered = XRInstanceManagerApi.Register(ApiXrFeature.Passthrough);
            registered &= XRInstanceManagerApi.Register(ApiXrFeature.PassthroughCameraState);
            return registered;
        }

        /// <inheritdoc/>
        protected override void OnInstanceDestroy(ulong xrInstance)
        {
            XRInstanceManagerApi.Unregister(ApiXrFeature.Passthrough);
            XRInstanceManagerApi.Unregister(ApiXrFeature.PassthroughCameraState);
        }

        /// <inheritdoc/>
        protected override void OnSystemChange(ulong xrSystem)
        {
            if (!_extensionAvailable || !XRInstanceManagerApi.CheckSystemSupport(
                ApiXrFeature.Passthrough, ref _extensionSupported))
            {
                return;
            }

            if (!_extensionSupported && !XRInstanceManagerApi.CheckSystemSupport(
                ApiXrFeature.PassthroughCameraState, ref _extensionSupported))
            {
                return;
            }

            _extensionEnabled = _extensionSupported;
            if (!_extensionSupported)
            {
                Debug.LogWarningFormat(
                    "System {0} does not support feature {1}.", xrSystem, UiName);
            }
        }

#if XR_COMPOSITION_LAYERS
        /// <inheritdoc/>
        protected override void OnEnable()
        {
            if (OpenXRLayerProvider.isStarted)
            {
                CreateAndRegisterLayerHandler();
            }
            else
            {
                OpenXRLayerProvider.Started += CreateAndRegisterLayerHandler;
                _isSubscribed = true;
            }
        }

        /// <inheritdoc/>
        protected override void OnDisable()
        {
            if (_isSubscribed)
            {
                OpenXRLayerProvider.Started -= CreateAndRegisterLayerHandler;
                _isSubscribed = false;
            }
        }

        /// <inheritdoc/>
        protected void CreateAndRegisterLayerHandler()
        {
            if (enabled)
            {
                var layerHandler = new XRPassthroughLayerHandler();
                OpenXRLayerProvider.RegisterLayerHandler(
                    typeof(XRPassthroughLayerData), layerHandler);
            }
        }
#endif // XR_COMPOSITION_LAYERS

#if UNITY_EDITOR
        /// <inheritdoc/>
        protected override void GetValidationChecks(
            List<ValidationRule> results, BuildTargetGroup targetGroup)
        {
            if (targetGroup != BuildTargetGroup.Android)
            {
                return;
            }
#if XR_COMPOSITION_LAYERS
            results.Add(AndroidXRFeatureUtils.GetFeatureDependencyRule(
                feature: this,
                requiredFeatureType: typeof(OpenXRCompositionLayersFeature),
                requiredFeatureName: ApiConstants.CompositionLayerFeature,
                buildTarget: targetGroup));
#else
            results.Add(AndroidXRFeatureUtils.GetPackageDependencyRule(
                feature: this,
                featureName: UiName,
                package: ApiConstants.CompositionLayersPackage,
                displayName: ApiConstants.CompositionLayersPackageDisplayName,
                version: ApiConstants.CompositionLayersPackageVersion));
#endif // XR_COMPOSITION_LAYERS
        }
#endif // UNITY_EDITOR

        private struct ExternalApi
        {
            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void OpenXRAndroid_getPassthroughCameraState(
                IntPtr manager, ref XRPassthroughCameraStates out_state);
        }
    }
}
