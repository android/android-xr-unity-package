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
    /// This <see cref="OpenXRInteractionFeature"/> configures the
    /// <c>XR_ANDROID_composition_layer_passthrough_mesh</c> and
    /// <c>XR_ANDROID_passthrough_camera_state</c> extensions
    /// at OpenXR runtime and provides passthrough geometry capabilities in the OpenXR platform.
    ///
    /// Use <see cref="Unity.XR.CompositionLayers.CompositionLayer"/> with Passthrough layer type
    /// to access passthrough cutout at runtime.
    /// Note: a valid <see cref="MeshFilter.mesh"/> is required to configure the layer geometry.
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
    public class XRPassthroughFeature : OpenXRFeature
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

#if XR_COMPOSITION_LAYERS
        // Indicate if has registered composition layer handler.
        bool _isSubscribed;
#else
#if UNITY_EDITOR
        static private UnityEditor.PackageManager.Requests.AddRequest _addRequest = null;
        static private bool _packageInstalled = false;
#endif
#endif

        /// <summary>
        /// Gets if the required OpenXR extension is enabled.
        /// When OpenXR runtime is waiting, it returns <c>null</c>. Otherwise, it indicates
        /// whether the XR_ANDROID_composition_layer_passthrough_mesh extension is enabled.
        /// </summary>
        public static bool? IsExensionEnabled => _extensionEnabled;

        /// <summary>
        /// Get the state of the passthrough camera.
        /// </summary>
        /// <returns>The current state of the passthrough camera.</returns>
        public static XRPassthroughCameraStates GetState()
        {
            XRPassthroughCameraStates state = XRPassthroughCameraStates.Disabled;
            ExternalApi.OpenXRAndroid_getPassthroughCameraState(XRInstanceManagerApi.GetIntPtr(),
                ref state);
            return state;
        }

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
            const string supportFeatureName = "Composition Layers Support";
            results.Add(new ValidationRule(this)
            {
                message = string.Format(
                    "{0} is required for this feature.", supportFeatureName),
                checkPredicate = () =>
                {
                    var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);
                    if (settings == null)
                    {
                        return false;
                    }

                    var supportFeature = settings.GetFeature<OpenXRCompositionLayersFeature>();
                    return supportFeature != null && supportFeature.enabled;
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

                    var feature = settings.GetFeature<OpenXRCompositionLayersFeature>();
                    if (feature == null)
                    {
                        Debug.LogWarningFormat(
                            "Autofix failed with missing {0} feature on {1} platform.",
                            supportFeatureName, targetGroup);
                        return;
                    }

                    feature.enabled = true;
                },
                error = true
            });
#else
            const string packageName = "XR Composition Layers (v1.0.0)";
            results.Add(new ValidationRule(this)
            {
                message = string.Format("{0} package is required for this feature.", packageName),
                checkPredicate = () =>
                {
                    return _packageInstalled;
                },
                fixIt = () =>
                {
                    const string packageIdentifier = "com.unity.xr.compositionlayers@1.0.0";
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
                error = true
            });
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
