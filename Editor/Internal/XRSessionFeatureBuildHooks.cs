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
    using UnityEditor;
    using UnityEditor.Build.Reporting;
    using UnityEditor.XR.OpenXR.Features;
    using UnityEngine;
    using UnityEngine.XR.OpenXR;
    using UnityEngine.XR.OpenXR.Features;
    using UnityEngine.XR.OpenXR.Features.Interactions;

#if XR_MGMT_4_4_0_OR_NEWER
    using Unity.XR.Management.AndroidManifest.Editor;
#endif

#if UNITY_XR_HAND
    using UnityEngine.XR.Hands.OpenXR;
#endif
#if UNITY_OPEN_XR_ANDROID_XR
    using ARPlaneFeature = UnityEngine.XR.OpenXR.Features.Android.ARPlaneFeature;
#endif
#if UNITY_OPEN_XR_ANDROID_XR_0_2_0
    using ARRaycastFeature = UnityEngine.XR.OpenXR.Features.Android.ARRaycastFeature;
#endif
#if UNITY_OPEN_XR_ANDROID_XR_0_4_0
    using AROcclusionFeature = UnityEngine.XR.OpenXR.Features.Android.AROcclusionFeature;
#endif

    internal class XRSessionFeatureBuildHooks : OpenXRFeatureBuildHooks
    {
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

#if XR_MGMT_4_4_0_OR_NEWER
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
                        { "name", "android.window.PROPERTY_ACTIVITY_XR_START_MODE" },
                        { "value", "ACTIVITY_START_MODE_UNMANAGED_FULL_SPACE" }
                    }
            };
#endif // XR_MGMT_4_4_0_OR_NEWER

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

            bool spacewarpEnabled = sessionFeature != null &&
                sessionFeature.enabled && sessionFeature.SpaceWarp;
            Debug.LogFormat("SetBootConfigBoolean: xr-meta-enabled=" + spacewarpEnabled);
            builder.SetBootConfigBoolean("xr-meta-enabled", spacewarpEnabled);
        }

        /// <inheritdoc/>
        protected override void OnPreprocessBuildExt(BuildReport report)
        {
#if !XR_MGMT_4_4_0_OR_NEWER
            XRSessionFeature activeFeature =
                AndroidXRBuildUtils.GetActiveFeature<XRSessionFeature>();
            if (activeFeature == null || !activeFeature.enabled || !activeFeature.ImmersiveXR)
            {
                return;
            }

            Debug.LogWarning(
                "XRSessionFeature relies on XR Plugin Management to add manifest elements.\n" +
                "Please upgrade it to 4.4.0+ or manually inject following manifest elements:\n" +
                "<intent-filter>\n" +
                " <category android:name=\"org.khronos.openxr.intent.category.IMMERSIVE_HMD\"/>\n" +
                "</intent-filter>\n" +
                "<property\n" +
                "    android:name=\"android.window.PROPERTY_ACTIVITY_XR_START_MODE\"\n" +
                "    android:value=\"ACTIVITY_START_MODE_UNMANAGED_FULL_SPACE\"/>");
#endif // !XR_MGMT_4_4_0_OR_NEWER
        }

        /// <inheritdoc/>
        protected override void OnPostGenerateGradleAndroidProjectExt(string path)
        {
        }

        /// <inheritdoc/>
        protected override void OnPostprocessBuildExt(BuildReport report)
        {
        }

#if XR_MGMT_4_4_0_OR_NEWER
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

            if (CheckSceneUnderstandingPermission())
            {
                requiredManifest.Add(
                    GetAndroidXRPermissionElement(AndroidXRPermission.SceneUnderstanding));
                Debug.LogFormat("Inject permission manifest: {0}",
                    AndroidXRPermission.SceneUnderstanding.ToPermissionString());
            }

            if (CheckEyeTrackingPermission())
            {
                requiredManifest.Add(
                    GetAndroidXRPermissionElement(AndroidXRPermission.EyeTracking));
                Debug.LogFormat("Inject permission manifest: {0}",
                    AndroidXRPermission.EyeTracking.ToPermissionString());
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

        private bool CheckSceneUnderstandingPermission()
        {
            XRTrackableFeature trackableFeature =
                AndroidXRBuildUtils.GetActiveFeature<XRTrackableFeature>();
            XRAnchorFeature anchorFeature =
                AndroidXRBuildUtils.GetActiveFeature<XRAnchorFeature>();
#if UNITY_OPEN_XR_ANDROID_XR
            ARPlaneFeature arPlaneFeature =
                AndroidXRBuildUtils.GetActiveFeature<ARPlaneFeature>();
#endif
#if UNITY_OPEN_XR_ANDROID_XR_0_2_0
            ARRaycastFeature arRaycastFeature =
                AndroidXRBuildUtils.GetActiveFeature<ARRaycastFeature>();
#endif
#if UNITY_OPEN_XR_ANDROID_XR_0_4_0
            AROcclusionFeature occlusionFeature =
                AndroidXRBuildUtils.GetActiveFeature<AROcclusionFeature>();
#endif
            XRObjectTrackingFeature objectTrackingFeature =
                AndroidXRBuildUtils.GetActiveFeature<XRObjectTrackingFeature>();

            return (trackableFeature != null && trackableFeature.enabled) ||
#if UNITY_OPEN_XR_ANDROID_XR
                (arPlaneFeature != null && arPlaneFeature.enabled) ||
#endif
#if UNITY_OPEN_XR_ANDROID_XR_0_2_0
                (arRaycastFeature != null && arRaycastFeature.enabled) ||
#endif
#if UNITY_OPEN_XR_ANDROID_XR_0_4_0
                (occlusionFeature != null && occlusionFeature.enabled) ||
#endif
                (objectTrackingFeature != null && objectTrackingFeature.enabled) ||
                (anchorFeature != null && anchorFeature.enabled);
        }

        private bool CheckEyeTrackingPermission()
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
#endif // XR_MGMT_4_4_0_OR_NEWER
    }
}
