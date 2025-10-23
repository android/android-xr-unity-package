// <copyright file="XREnvironmentBlendModeFeature.cs" company="Google LLC">
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
    using System.Linq;
    using Google.XR.Extensions.Internal;
    using UnityEngine;
    using UnityEngine.Rendering;
#if UNITY_URP
    using UnityEngine.Rendering.Universal;
#endif
    using UnityEngine.XR.OpenXR;
    using UnityEngine.XR.OpenXR.Features;
#if UNITY_OPEN_XR_ANDROID_XR
    using UnityEngine.XR.OpenXR.Features.Android;
#endif
    using UnityEngine.XR.OpenXR.NativeTypes;

#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.XR.OpenXR.Features;
#endif

    /// <summary>
    /// This <c><see cref="OpenXRInteractionFeature"/></c> configures
    /// <c><see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XrEnvironmentBlendMode">
    /// XrEnvironmentBlendMode</see></c> at OpenXR runtime.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = UiName,
        BuildTargetGroups = new[] {
            BuildTargetGroup.Android,
        },
        Company = "Google",
        Desc = "Configure environment blend mode at runtime.",
        Version = "1.0.0",
        Category = FeatureCategory.Feature,
        FeatureId = FeatureId,
        Priority = 98)]
#endif
    public class XREnvironmentBlendModeFeature : OpenXRFeature
    {
        /// <summary>
        /// The UI name shows on the XR Plug-in Management panel, help users to understand
        /// validation errors and expected fixes.
        /// </summary>
        public const string UiName = "Environment Blend Mode";

        /// <summary>
        /// The feature ID string.
        /// </summary>
        public const string FeatureId = "com.google.xr.extensions.blendmode";

        /// <summary>
        /// The environment blend modes supported by the current system for the current view
        /// configuration change.
        /// </summary>
        [HideInInspector]
        public List<XrEnvironmentBlendMode> SupportedEnvironmentBlendModes
            = new List<XrEnvironmentBlendMode>();

        [SerializeField]
        private XrEnvironmentBlendMode _requestMode = XrEnvironmentBlendMode.Opaque;

        private ulong _systemId;

        /// <summary>
        /// Gets or sets the requested blend mode at OpenXR runtime.
        /// When this feature is enabled, the value passed here will
        /// be passed to <c>xrEndFrame</c>.
        /// The value must be a member of <c><see cref="SupportedEnvironmentBlendModes"/></c>
        /// </summary>
        public XrEnvironmentBlendMode RequestedEnvironmentBlendMode
        {
            get => _requestMode;
            set
            {
                _requestMode = value;
                OpenXRAndroidApi.SetBlendMode(value);
            }
        }

        /// <summary>
        /// The current blend mode that is used by <c>xrEndFrame</c>.
        ///
        /// Note: It takes one frame to apply the blend mode changes.
        /// </summary>
        public XrEnvironmentBlendMode CurrentBlendMode => OpenXRAndroidApi.GetBlendMode();

        /// <inheritdoc/>
        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            if (!base.OnInstanceCreate(xrInstance))
            {
                return false;
            }

            return XRInstanceManagerApi.Register(ApiXrFeature.BlendMode);
        }

        /// <inheritdoc/>
        protected override void OnInstanceDestroy(ulong xrInstance)
        {
            XRInstanceManagerApi.Unregister(ApiXrFeature.BlendMode);
        }

        /// <inheritdoc/>
        protected override void OnSessionBegin(ulong xrSession)
        {
            RequestedEnvironmentBlendMode = _requestMode;
        }

        /// <inheritdoc/>
        protected override void OnViewConfigurationTypeChange(int xrViewConfigurationType)
        {
            SupportedEnvironmentBlendModes = new List<XrEnvironmentBlendMode>(
                OpenXRAndroidApi.EnumerateEnvironmentBlendModes(xrViewConfigurationType,
                                                                _systemId));

            // fall back to a supported mode as necessary
            if (!SupportedEnvironmentBlendModes.Contains(RequestedEnvironmentBlendMode)
                && SupportedEnvironmentBlendModes.Count > 0)
            {
                var fallbackMode = SupportedEnvironmentBlendModes[0];
                Debug.LogWarning(
                    $"New view configuration type {xrViewConfigurationType} does not support " +
                    $"{RequestedEnvironmentBlendMode}, falling back to {fallbackMode}.");
                RequestedEnvironmentBlendMode = fallbackMode;
            }
        }

        /// <inheritdoc/>
        protected override void OnSystemChange(ulong xrSystem)
        {
            // cache XrSystemId for usage later, but don't enumerate environment blend modes
            // until we have a view configuration
            _systemId = xrSystem;
        }

#if UNITY_EDITOR
        /// <inheritdoc/>
        protected override void GetValidationChecks(
            List<ValidationRule> results, BuildTargetGroup targetGroup)
        {
            if (targetGroup != BuildTargetGroup.Android ||
                RequestedEnvironmentBlendMode != XrEnvironmentBlendMode.AlphaBlend)
            {
                return;
            }

#if UNITY_OPEN_XR_ANDROID_XR
            var arCameraEnabled = new ValidationRule(this)
            {
                message = "Enabling <b>" + UiName + "</b> is unnecessary with <b>Android XR: AR Camera</b>.",
                checkPredicate = () =>
                {
                    var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);
                    if (settings == null)
                    {
                        return false;
                    }

                    var arCamera = settings.GetFeature<ARCameraFeature>() as OpenXRFeature;
                    return arCamera == null || !arCamera.enabled;
                },
                fixItMessage = "Disable <b>" + UiName + "</b>",
                fixIt = () =>
                {
                    enabled = false;
                },
                error = false
            };
            results.Add(arCameraEnabled);

            if (!arCameraEnabled.checkPredicate())
            {
                return;
            }
#endif // UNITY_OPEN_XR_ANDROID_XR

#if UNITY_URP
            var urpAsset = GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;

            if (urpAsset == null)
            {
                return;
            }

            var rendererExistsCheck = new ValidationRule(this)
            {
                message = "The active URP Asset must have at least one Universal Renderer in its "
                    + "Renderer List.",
                error = true,
                checkPredicate = () =>
                {
                    return urpAsset.rendererDataList.ToArray()
                           .OfType<UniversalRendererData>().Any();
                }
            };
            results.Add(rendererExistsCheck);

            var featureEnabledCheck = new ValidationRule(this)
            {
                message = "The 'TransparentBackgroundRendererFeature' must be added and enabled in "
                    + "at least one Universal Renderer.",
                error = true,
                checkPredicate = () =>
                {
                    var universalRenderers = urpAsset.rendererDataList.ToArray()
                        .OfType<UniversalRendererData>().ToList();

                    if (!universalRenderers.Any())
                        return true;

                    return universalRenderers.Any(rendererData =>
                        rendererData.rendererFeatures.Any(feature =>
                            feature is TransparentBackgroundRendererFeature && feature.isActive));
                },
                fixItMessage = "Add and Enable 'TransparentBackgroundRendererFeature'",
                fixIt = () =>
                {
                    var rendererData = urpAsset.rendererDataList.ToArray()
                        .OfType<UniversalRendererData>().FirstOrDefault();

                    if (rendererData == null)
                    {
                        Debug.LogError("Could not find a Universal Renderer Data asset to modify.");
                        return;
                    }

                    var existingFeature = rendererData.rendererFeatures
                        .OfType<TransparentBackgroundRendererFeature>().FirstOrDefault();

                    if (existingFeature != null)
                    {
                        existingFeature.SetActive(true);
                    }
                    else
                    {
                        var newFeature = CreateInstance<TransparentBackgroundRendererFeature>();
                        newFeature.name = "TransparentBackgroundRendererFeature";
                        rendererData.rendererFeatures.Add(newFeature);
                        AssetDatabase.AddObjectToAsset(newFeature, rendererData);
                    }

                    EditorUtility.SetDirty(rendererData);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            };
            results.Add(featureEnabledCheck);
#endif // UNITY_URP
        }
#endif // UNITY_EDITOR
    }
}
