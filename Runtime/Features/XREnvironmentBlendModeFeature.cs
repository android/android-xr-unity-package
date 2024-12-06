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
    using Google.XR.Extensions.Internal;
    using UnityEngine;
    using UnityEngine.XR.OpenXR.Features;
    using UnityEngine.XR.OpenXR.NativeTypes;

#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.XR.OpenXR.Features;
#endif

    /// <summary>
    /// This <see cref="OpenXRInteractionFeature"/> configures
    /// <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XrEnvironmentBlendMode">
    /// XrEnvironmentBlendMode</see> at OpenXR runtime.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = UiName,
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
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
        public List<XrEnvironmentBlendMode> SupportedEnvironmentBlendModes
            = new List<XrEnvironmentBlendMode>();

        [SerializeField]
        private XrEnvironmentBlendMode _requestMode = XrEnvironmentBlendMode.Opaque;

        private ulong _systemId;

        /// <summary>
        /// Gets or sets the requested blend mode at OpenXR runtime.
        /// When this feature is enabled, the value passed here will
        /// be passed to xrEndFrame.
        /// The value must be a member of  <see cref="SupportedEnvironmentBlendModes"/>
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
        /// The current blend mode that is used by xrEndFrame.
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
    }
}
