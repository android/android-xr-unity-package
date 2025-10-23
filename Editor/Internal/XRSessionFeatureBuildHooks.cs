// <copyright file="XRSessionFeatureBuildHooks.cs" company="Google LLC">
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

namespace Google.XR.Extensions.Editor.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using Google.XR.Extensions;
    using Unity.XR.Management.AndroidManifest.Editor;
    using UnityEditor;
    using UnityEditor.Build.Reporting;
    using UnityEditor.XR.OpenXR.Features;
    using UnityEngine;
    using UnityEngine.XR.OpenXR;
    using UnityEngine.XR.OpenXR.Features;
    using UnityEngine.XR.OpenXR.Features.Interactions;

#if UNITY_XR_HAND
    using UnityEngine.XR.Hands.OpenXR;
#endif

    internal class XRSessionFeatureBuildHooks : OpenXRFeatureBuildHooks
    {
        private static readonly ManifestElement _immersiveHMD =
            new ManifestElement()
            {
                ElementPath = new List<string>
                    {
                        "manifest", "application", "activity", "intent-filter", "category"
                    },
                Attributes = new Dictionary<string, string>
                    {
                        { "name", "org.khronos.openxr.intent.category.IMMERSIVE_HMD" }
                    }
            };

        private static readonly ManifestElement _immersiveXR =
            new ManifestElement()
            {
                ElementPath = new List<string>
                    {
                        "manifest", "application", "activity", "property",
                    },
                Attributes = new Dictionary<string, string>
                    {
                        { "name", "android.window.PROPERTY_XR_ACTIVITY_START_MODE" },
                        { "value", "XR_ACTIVITY_START_MODE_FULL_SPACE_UNMANAGED" }
                    }
            };

        private static readonly ManifestElement _libopenxrso =
            new ManifestElement()
            {
                ElementPath = new List<string>
                    {
                        "manifest", "application", "uses-native-library"
                    },
                Attributes = new Dictionary<string, string>
                    {
                        { "name", "libopenxr.google.so" },
                        { "required", "false" }
                    }
            };

        private static readonly ManifestElement _openxrFeature =
            new ManifestElement()
            {
                ElementPath = new List<string>
                    {
                        "manifest", "uses-feature"
                    },
                Attributes = new Dictionary<string, string>
                    {
                        { "name", "android.software.xr.api.openxr" },
                        { "required", "true" },
                        //// Require minimum OpenXR version 1.1 (no patch version specified)
                        //// Major version bits: 0xffff0000
                        //// Minor version bits: 0x0000ffff
                        { "version", "0x00010001" }
                    }
            };

        private static readonly ManifestElement _openxrExperimentalFeature =
            new ManifestElement()
            {
                ElementPath = new List<string>
                    {
                        "manifest", "application", "meta-data"
                    },
                Attributes = new Dictionary<string, string>
                    {
                        { "name", "com.android.extensions.xr.uses_experimental_openxr_feature" },
                        { "value", "true" }
                    }
            };

        /// <inheritdoc/>
        [SuppressMessage("UnityRules.UnityStyleRules",
                         "US1109:PublicPropertiesMustBeUpperCamelCase",
                         Justification = "Overridden property.")]
        public override int callbackOrder => 1; // The order of this callback does not matter

        /// <inheritdoc/>
        [SuppressMessage("UnityRules.UnityStyleRules",
                         "US1109:PublicPropertiesMustBeUpperCamelCase",
                         Justification = "Overridden property.")]
        public override Type featureType => typeof(XRSessionFeature);

        public override ManifestRequirement ProvideManifestRequirement()
        {
            bool unityOpenXRAndroidXR = AndroidXRBuildUtils.IsUnityAndroidXRActive();
            List<ManifestElement> requiredManifest = new List<ManifestElement>();
            XRSessionFeature activeFeature =
                AndroidXRBuildUtils.GetActiveFeature<XRSessionFeature>();
            bool immersive =
                activeFeature != null && activeFeature.enabled && activeFeature.ImmersiveXR;
            if (immersive && !unityOpenXRAndroidXR)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("Inject manifest elements for Immersive XR:");
                requiredManifest.Add(_immersiveHMD);
                stringBuilder.Append(
                    "\n" + AndroidXRBuildUtils.PrintManifestElement(_immersiveHMD));
                requiredManifest.Add(_immersiveXR);
                stringBuilder.Append(
                    "\n" + AndroidXRBuildUtils.PrintManifestElement(_immersiveXR));
                Debug.Log(stringBuilder);
            }

            if (CheckSceneUnderstandingCoarsePermission())
            {
                requiredManifest.Add(
                    GetAndroidXRPermissionElement(AndroidXRPermission.SceneUnderstandingCoarse));
                Debug.LogFormat("Inject permission manifest: {0}",
                    AndroidXRPermission.SceneUnderstandingCoarse.ToPermissionString());
            }

            if (CheckSceneUnderstandingFinePermission())
            {
                requiredManifest.Add(
                    GetAndroidXRPermissionElement(AndroidXRPermission.SceneUnderstandingFine));
                Debug.LogFormat("Inject permission manifest: {0}",
                    AndroidXRPermission.SceneUnderstandingFine.ToPermissionString());
            }

            if (CheckEyeTrackingCoarsePermission())
            {
                requiredManifest.Add(
                    GetAndroidXRPermissionElement(AndroidXRPermission.EyeTrackingCoarse));
                Debug.LogFormat("Inject permission manifest: {0}",
                    AndroidXRPermission.EyeTrackingCoarse.ToPermissionString());
            }

            if (CheckEyeTrackingFinePermission())
            {
                requiredManifest.Add(
                    GetAndroidXRPermissionElement(AndroidXRPermission.EyeTrackingFine));
                Debug.LogFormat("Inject permission manifest: {0}",
                    AndroidXRPermission.EyeTrackingFine.ToPermissionString());
            }

            if (CheckHandTrackingPermission())
            {
                requiredManifest.Add(
                    GetAndroidXRPermissionElement(AndroidXRPermission.HandTracking));
                Debug.LogFormat("Inject permission manifest: {0}",
                    AndroidXRPermission.HandTracking.ToPermissionString());
            }

            if (CheckBodyTrackingPermission())
            {
                requiredManifest.Add(
                    GetAndroidXRPermissionElement(AndroidXRPermission.BodyTracking));
                Debug.LogFormat("Inject permission manifest: {0}",
                    AndroidXRPermission.BodyTracking.ToPermissionString());
            }

            if (CheckExperimentalPermission())
            {
                requiredManifest.Add(_openxrExperimentalFeature);
                Debug.LogWarningFormat("Inject experimental feature meta-data manifest. " +
                    "Note: Applications with this meta-data can access ANDROIDX extensions " +
                    "but cannot be published in Google Play Store.");
            }

            requiredManifest.Add(_libopenxrso);
            Debug.LogFormat("Inject native library manifest: libopenxr.google.so");

            requiredManifest.Add(_openxrFeature);
            Debug.LogFormat("Inject feature manifest: android.software.xr.api.openxr");

            List<ManifestElement> emptyElement = new List<ManifestElement>();
            return new ManifestRequirement
            {
                SupportedXRLoaders = new HashSet<Type>()
                {
                    typeof(OpenXRLoader)
                },
                NewElements =
                    requiredManifest.Count > 0 ? requiredManifest : emptyElement,
                RemoveElements =
                    requiredManifest.Count > 0 ? emptyElement : requiredManifest,
            };
        }

        /// <inheritdoc/>
        protected override void OnProcessBootConfigExt(
            BuildReport report, BootConfigBuilder builder)
        {
            XRFoveationFeature foveationFeature =
                AndroidXRBuildUtils.GetActiveFeature<XRFoveationFeature>();
            FoveatedRenderingFeature foveatedRendering =
                AndroidXRBuildUtils.GetActiveFeature<FoveatedRenderingFeature>();
            XRSessionFeature sessionFeature =
                AndroidXRBuildUtils.GetActiveFeature<XRSessionFeature>();
            bool needsFragmentDensityMapEnabled = (sessionFeature != null &&
                sessionFeature.enabled && sessionFeature.VulkanSubsampling) ||
                (foveationFeature != null && foveationFeature.enabled) ||
                (foveatedRendering != null && foveatedRendering.enabled);
            Debug.LogFormat(
                "SetBootConfigBoolean: xr-vulkan-extension-fragment-density-map-enabled="
                + needsFragmentDensityMapEnabled);
            builder.SetBootConfigBoolean("xr-vulkan-extension-fragment-density-map-enabled",
                needsFragmentDensityMapEnabled);
        }

        /// <inheritdoc/>
        protected override void OnPreprocessBuildExt(BuildReport report)
        {
        }

        /// <inheritdoc/>
        protected override void OnPostGenerateGradleAndroidProjectExt(string path)
        {
        }

        /// <inheritdoc/>
        protected override void OnPostprocessBuildExt(BuildReport report)
        {
        }

        private static ManifestElement GetAndroidXRPermissionElement(AndroidXRPermission permission)
        {
            return new ManifestElement()
            {
                ElementPath = new List<string> { "manifest", "uses-permission" },
                Attributes = new Dictionary<string, string>
                    {
                        { "name", permission.ToPermissionString() }
                    }
            };
        }

        private bool CheckSceneUnderstandingCoarsePermission()
        {
            XRObjectTrackingFeature objectTrackingFeature =
                AndroidXRBuildUtils.GetActiveFeature<XRObjectTrackingFeature>();
            XRQrCodeTrackingFeature qrCodeFeature =
                AndroidXRBuildUtils.GetActiveFeature<XRQrCodeTrackingFeature>();
            XRMarkerTrackingFeature markerFeature =
                AndroidXRBuildUtils.GetActiveFeature<XRMarkerTrackingFeature>();

            return
                (objectTrackingFeature != null && objectTrackingFeature.enabled) ||
                (qrCodeFeature != null && qrCodeFeature.enabled) ||
                (markerFeature != null && markerFeature.enabled);
        }

        private bool CheckSceneUnderstandingFinePermission()
        {
            bool sceneMeshingInUse = false;
            XRSceneMeshingFeature sceneMeshing =
                AndroidXRBuildUtils.GetActiveFeature<XRSceneMeshingFeature>();
            sceneMeshingInUse = sceneMeshing != null && sceneMeshing.enabled;
            return sceneMeshingInUse;
        }

        private bool CheckEyeTrackingCoarsePermission()
        {
            FoveatedRenderingFeature foveatedRendering =
                AndroidXRBuildUtils.GetActiveFeature<FoveatedRenderingFeature>();
            return (foveatedRendering != null && foveatedRendering.enabled) ||
                CheckEyeTrackingCalibrationPermission();
        }

        private bool CheckEyeTrackingFinePermission()
        {
            EyeGazeInteraction eyeGazeInteraction =
                AndroidXRBuildUtils.GetActiveFeature<EyeGazeInteraction>();
            FoveatedRenderingFeature foveatedRendering =
                AndroidXRBuildUtils.GetActiveFeature<FoveatedRenderingFeature>();
            return (eyeGazeInteraction != null && eyeGazeInteraction.enabled) ||
                (foveatedRendering != null && foveatedRendering.enabled) ||
                CheckEyeTrackingCalibrationPermission();
        }

        private bool CheckEyeTrackingCalibrationPermission()
        {
            return false;
        }

        private bool CheckHandTrackingPermission()
        {
#if UNITY_XR_HAND
            HandTracking handTracking =
                AndroidXRBuildUtils.GetActiveFeature<HandTracking>();
            bool handTrackingInUse = handTracking != null && handTracking.enabled;
#else
            bool handTrackingInUse = false;
#endif // UNITY_XR_HAND
            XRHandMeshFeature handMesh =
                AndroidXRBuildUtils.GetActiveFeature<XRHandMeshFeature>();
            bool handMeshInUse = handMesh != null && handMesh.enabled;
            return handTrackingInUse || handMeshInUse;
        }

        private bool CheckBodyTrackingPermission()
        {
            XRBodyTrackingFeature bodyTracking =
                AndroidXRBuildUtils.GetActiveFeature<XRBodyTrackingFeature>();
            return bodyTracking != null && bodyTracking.enabled;
        }

        private bool CheckExperimentalPermission()
        {
            bool bodyTrackingInUse = false;
            XRBodyTrackingFeature bodyTracking =
                AndroidXRBuildUtils.GetActiveFeature<XRBodyTrackingFeature>();
            bodyTrackingInUse = bodyTracking != null && bodyTracking.enabled;
            bool systemStateInUse = false;
            XRSystemStateFeature systemState =
                AndroidXRBuildUtils.GetActiveFeature<XRSystemStateFeature>();
            systemStateInUse = systemState != null && systemState.enabled;
            bool advancedLightingInUse = false;
            return bodyTrackingInUse || systemStateInUse || advancedLightingInUse;
        }
    }
}
