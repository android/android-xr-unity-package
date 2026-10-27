// <copyright file="SpatialLifecycleFeatureBuildHooks.cs" company="Google LLC">
//
// Copyright 2026 Google LLC
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

namespace Google.XR.Extensions.SpatialEntities.Editor.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Google.XR.Extensions.Editor.Internal;
    using Google.XR.Extensions.SpatialEntities;
    using Unity.XR.Management.AndroidManifest.Editor;
    using UnityEditor;
    using UnityEditor.Build.Reporting;
    using UnityEditor.XR.OpenXR.Features;
    using UnityEngine;
    using UnityEngine.Android;
    using UnityEngine.XR.OpenXR;

#if UNITY_SPATIAL_ENTITIES
    using UnityEngine.XR.OpenXR.Features.SpatialEntities;
#endif // UNITY_SPATIAL_ENTITIES

    internal class SpatialLifecycleFeatureBuildHooks : OpenXRFeatureBuildHooks
    {
        /// <inheritdoc/>
        [SuppressMessage("UnityRules.UnityStyleRules",
                         "US1109:PublicPropertiesMustBeUpperCamelCase",
                         Justification = "Overridden property.")]
        public override int callbackOrder => 2;

        /// <inheritdoc/>
        [SuppressMessage("UnityRules.UnityStyleRules",
                         "US1109:PublicPropertiesMustBeUpperCamelCase",
                         Justification = "Overridden property.")]
        public override Type featureType => typeof(SpatialLifecycleFeature);

        /// <inheritdoc/>
        public override ManifestRequirement ProvideManifestRequirement()
        {
            List<ManifestElement> requiredManifest = new List<ManifestElement>();
            List<ManifestElement> emptyElement = new List<ManifestElement>();

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

            if (CheckCameraPermission())
            {
                requiredManifest.AddPermission(Permission.Camera);
                Debug.LogFormat("Inject permission manifest: {0}", Permission.Camera);
            }

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
        protected override void OnPreprocessBuildExt(BuildReport report)
        {
        }

        /// <inheritdoc/>
        protected override void OnPostprocessBuildExt(BuildReport report)
        {
        }

        /// <inheritdoc/>
        protected override void OnPostGenerateGradleAndroidProjectExt(string path)
        {
        }

        private bool CheckSceneUnderstandingCoarsePermission()
        {
            var androidSettings =
                OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
            if (androidSettings == null)
            {
                return false;
            }

            bool required = false;
#if UNITY_SPATIAL_ENTITIES
            SpatialPlaneFeature planeFeature = androidSettings.GetFeature<SpatialPlaneFeature>();
            required |= planeFeature != null && planeFeature.enabled;
            SpatialMarkerFeature markerFeature = androidSettings.GetFeature<SpatialMarkerFeature>();
            required |= markerFeature != null && markerFeature.enabled;
#endif // UNITY_SPATIAL_ENTITIES
            SpatialBoundingBoxFeature boundingBoxFeature =
                androidSettings.GetFeature<SpatialBoundingBoxFeature>();
            required |= boundingBoxFeature != null && boundingBoxFeature.enabled;
            SpatialShareableAnchorFeature shareableAnchorFeature =
                androidSettings.GetFeature<SpatialShareableAnchorFeature>();
            required |= shareableAnchorFeature != null && shareableAnchorFeature.enabled;
            return required;
        }

        private bool CheckSceneUnderstandingFinePermission()
        {
            var androidSettings =
                OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
            if (androidSettings == null)
            {
                return false;
            }

            bool required = false;
            return required;
        }

        private bool CheckCameraPermission()
        {
            var androidSettings =
                OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
            if (androidSettings == null)
            {
                return false;
            }

            bool required = false;
            return required;
        }
    }
}
