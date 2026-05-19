// <copyright file="XRSessionFeatureBuildHooks.cs" company="Google LLC">
//
// Copyright 2024 Google LLC
// Copyright Qualcomm Technologies, Inc. and/or its affiliates. All rights reserved.
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
    using System.Linq;
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
                requiredManifest.AddImmersiveHmd();
                stringBuilder.Append(
                    "\n" + AndroidXRBuildUtils.PrintManifestElement(requiredManifest.Last()));
                requiredManifest.AddActivityStartMode(
                    AndroidXRManifest.XrActivityStartModeFullSpaceUnmanaged);
                stringBuilder.Append(
                    "\n" + AndroidXRBuildUtils.PrintManifestElement(requiredManifest.Last()));
                Debug.Log(stringBuilder);
            }

            if (activeFeature != null && activeFeature.UseXRSpatialApi)
            {
                requiredManifest.AddSpatialApiFeature(
                    activeFeature.XRSpatailApiRequired, activeFeature.TargetApiVersion);
                Debug.LogFormat("Inject manifest `{0}` with required={1}, value={2}",
                    AndroidXRManifest.XrApiSpatial, activeFeature.XRSpatailApiRequired,
                    activeFeature.TargetApiVersion);
                XRSpatialSdkVersions minVersion = XRSpatialSdkVersions.XRSpatialApiLevel1;
                List<string> featureNames = new List<string>();
                if (!activeFeature.TargetApiVersion.ValidateActiveFeatures(
                    ref minVersion, ref featureNames))
                {
                    Debug.LogWarningFormat(
                        "Following feature(s) are not supported by {0} and " +
                        "may not work properly at runtime: {2}.\n" +
                        "Ensure handling unsupported cases accordingly or select {1} instead.",
                        activeFeature.TargetApiVersion, minVersion,
                        string.Join(", ", featureNames));
                }
            }

            if (CheckSceneUnderstandingCoarsePermission())
            {
                requiredManifest.AddPermission(AndroidXRPermission.SceneUnderstandingCoarse);
                Debug.LogFormat("Inject permission manifest: {0}",
                    AndroidXRPermission.SceneUnderstandingCoarse.ToPermissionString());
            }

            if (CheckSceneUnderstandingFinePermission())
            {
                requiredManifest.AddPermission(AndroidXRPermission.SceneUnderstandingFine);
                Debug.LogFormat("Inject permission manifest: {0}",
                    AndroidXRPermission.SceneUnderstandingFine.ToPermissionString());
            }

            if (CheckEyeTrackingCoarsePermission())
            {
                requiredManifest.AddPermission(AndroidXRPermission.EyeTrackingCoarse);
                Debug.LogFormat("Inject permission manifest: {0}",
                    AndroidXRPermission.EyeTrackingCoarse.ToPermissionString());
            }

            if (CheckEyeTrackingFinePermission())
            {
                requiredManifest.AddPermission(AndroidXRPermission.EyeTrackingFine);
                Debug.LogFormat("Inject permission manifest: {0}",
                    AndroidXRPermission.EyeTrackingFine.ToPermissionString());
            }

            if (CheckFaceTrackingPermission())
            {
                requiredManifest.AddPermission(AndroidXRPermission.FaceTracking);
                Debug.LogFormat("Inject permission manifest: {0}",
                    AndroidXRPermission.FaceTracking.ToPermissionString());
            }

            if (CheckHandTrackingPermission())
            {
                requiredManifest.AddPermission(AndroidXRPermission.HandTracking);
                Debug.LogFormat("Inject permission manifest: {0}",
                    AndroidXRPermission.HandTracking.ToPermissionString());
            }

            if (CheckBodyTrackingPermission())
            {
                requiredManifest.AddPermission(AndroidXRPermission.BodyTracking);
                Debug.LogFormat("Inject permission manifest: {0}",
                    AndroidXRPermission.BodyTracking.ToPermissionString());
            }

            if (CheckExperimentalPermission())
            {
                requiredManifest.AddMetaData(
                    AndroidXRManifest.UseExperimentalOpenXRFeature, "true");
                Debug.LogWarningFormat("Inject experimental feature meta-data manifest. " +
                    "Note: Applications with this meta-data can access ANDROIDX extensions " +
                    "but cannot be published in Google Play Store.");
            }

            requiredManifest.AddNativeLibrary();
            Debug.LogFormat("Inject native library manifest: {0}",
                AndroidXRManifest.OpenXRNativeLibrary);

            requiredManifest.AddOpenXRApiFeature();
            Debug.LogFormat("Inject feature manifest: {0}", AndroidXRManifest.XrApiOpenXR);

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
            bool needsFragmentDensityMapEnabled =
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

        private bool CheckSceneUnderstandingCoarsePermission()
        {
            XRObjectTrackingFeature objectTrackingFeature =
                AndroidXRBuildUtils.GetActiveFeature<XRObjectTrackingFeature>();
            XRQrCodeTrackingFeature qrCodeFeature =
                AndroidXRBuildUtils.GetActiveFeature<XRQrCodeTrackingFeature>();
            XRMarkerTrackingFeature markerFeature =
                AndroidXRBuildUtils.GetActiveFeature<XRMarkerTrackingFeature>();
            XRImageTrackingFeature imageFeature =
                AndroidXRBuildUtils.GetActiveFeature<XRImageTrackingFeature>();

            return
                (objectTrackingFeature != null && objectTrackingFeature.enabled) ||
                (imageFeature != null && imageFeature.enabled) ||
                (qrCodeFeature != null && qrCodeFeature.enabled) ||
                (markerFeature != null && markerFeature.enabled) ||
                false;
        }

        private bool CheckSceneUnderstandingFinePermission()
        {
            bool finePermission = false;
            XRSceneMeshingFeature sceneMeshing =
                AndroidXRBuildUtils.GetActiveFeature<XRSceneMeshingFeature>();
            finePermission |= sceneMeshing != null && sceneMeshing.enabled;
            XRCubemapLightEstimationFeature cubemapLightEstimationFeature =
                AndroidXRBuildUtils.GetActiveFeature<XRCubemapLightEstimationFeature>();
            finePermission |= cubemapLightEstimationFeature != null &&
                    cubemapLightEstimationFeature.enabled;
            return finePermission;
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
            XRFineEyeFeature fineEyeFeature =
                AndroidXRBuildUtils.GetActiveFeature<XRFineEyeFeature>();
            return (eyeGazeInteraction != null && eyeGazeInteraction.enabled) ||
                (foveatedRendering != null && foveatedRendering.enabled) ||
                (fineEyeFeature != null && fineEyeFeature.enabled) ||
                CheckEyeTrackingCalibrationPermission();
        }

        private bool CheckEyeTrackingCalibrationPermission()
        {
            return false;
        }

        private bool CheckFaceTrackingPermission()
        {
            return false;
        }

        private bool CheckHandTrackingPermission()
        {
            bool handTrackingPermission = false;
#if UNITY_XR_HAND
            HandTracking handTracking =
                AndroidXRBuildUtils.GetActiveFeature<HandTracking>();
            handTrackingPermission |= handTracking != null && handTracking.enabled;
#endif // UNITY_XR_HAND
            return handTrackingPermission;
        }

        private bool CheckBodyTrackingPermission()
        {
            XRBodyTrackingFeature bodyTracking =
                AndroidXRBuildUtils.GetActiveFeature<XRBodyTrackingFeature>();
            return bodyTracking != null && bodyTracking.enabled;
        }

        private bool CheckExperimentalPermission()
        {
            bool experimentalPermission = false;
            XRBodyTrackingFeature bodyTracking =
                AndroidXRBuildUtils.GetActiveFeature<XRBodyTrackingFeature>();
            experimentalPermission |= bodyTracking != null && bodyTracking.enabled;
            AndroidXRTrackpadGesturesInteraction trackpadGestures =
                AndroidXRBuildUtils.GetActiveFeature<AndroidXRTrackpadGesturesInteraction>();
            experimentalPermission |= trackpadGestures != null && trackpadGestures.enabled;
            return experimentalPermission;
        }
    }
}
