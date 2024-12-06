// <copyright file="XRFoveationFeature.cs" company="Google LLC">
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
    using UnityEngine.Rendering;
    using UnityEngine.XR.OpenXR;
    using UnityEngine.XR.OpenXR.Features;

#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.XR.OpenXR.Features;
#endif

    /// <summary>
    /// This <see cref="OpenXRInteractionFeature"/> configures the
    /// <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XR_FB_foveation">
    /// XR_FB_foveation</see> extension at OpenXR runtime.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(
        UiName = UiName,
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        Company = "Google",
        Desc = "Configure Foveated Rendering at runtime.",
        Version = "1.0.0",
        Category = FeatureCategory.Feature,
        FeatureId = FeatureId,
        OpenxrExtensionStrings = ExtensionString)]
#endif
    public class XRFoveationFeature : OpenXRFeature
    {
        /// <summary>
        /// The UI name shows on the XR Plug-in Management panel, help users to understand
        /// validation errors and expected fixes.
        /// </summary>
        public const string UiName = "Foveation (Legacy)";

        /// <summary>
        /// The feature ID string.
        /// </summary>
        public const string FeatureId = "com.google.xr.extensions.foveation";

        /// <summary>
        /// The OpenXR Extension string. Used to check if this extensions is
        /// available or enabled.
        /// </summary>
        public const string ExtensionString = "XR_KHR_android_create_instance"
                                              + " XR_FB_foveation"
                                              + " XR_FB_foveation_configuration"
                                              + " XR_FB_swapchain_update_state"
                                              + " XR_FB_foveation_vulkan";

        private static ulong _xrSession;

        /// <summary>
        /// Configures foveation as if you were creating and setting a
        /// foveation profile via XrFoveationLevelProfileCreateInfoFB.
        /// </summary>
        /// <param name="foveationLevel">
        /// Corresponds to XrFoveationLevelProfileCreateInfoFB::level.
        /// </param>
        /// <param name="verticalOffset">
        /// Corresponds to XrFoveationLevelProfileCreateInfoFB::verticalOffset.
        /// </param>
        /// <param name="foveationDynamic">
        /// Corresponds to XrFoveationLevelProfileCreateInfoFB::dynamic.
        /// </param>
        public static void FBSetFoveationLevel(
            XRFoveationLevel foveationLevel, float verticalOffset, bool foveationDynamic)
        {
            FoveationApi.FBSetFoveationLevel(
                _xrSession, foveationLevel, verticalOffset, foveationDynamic);
        }

        /// <inheritdoc/>
        protected override void OnSessionCreate(ulong xrSession)
        {
            _xrSession = xrSession;
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

            results.Add(new ValidationRule(this)
            {
                message = "[Foveated Rendering] is preferred with Scriptable Render Pipeline.",
                checkPredicate = () =>
                {
                    return GraphicsSettings.defaultRenderPipeline == null;
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

                    var feature = settings.GetFeature<FoveatedRenderingFeature>();
                    if (feature == null)
                    {
                        Debug.LogWarningFormat(
                            "Autofix failed with missing {0} on {1} platform.",
                            UiName, targetGroup);
                        return;
                    }

                    // Switch to Foveated Rendering provided by OpenXR Plugin.
                    feature.enabled = true;
                    enabled = false;
                },
                error = true
            });
        }
#endif // UNITY_EDITOR
    }
}
