// <copyright file="AndroidXRBuildUtils.cs" company="Google LLC">
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
    using System.Linq;
    using UnityEditor;
    using UnityEditor.XR.Management;
    using UnityEditor.XR.OpenXR.Features;
    using UnityEngine.XR.OpenXR;
    using UnityEngine.XR.OpenXR.Features;

#if XR_MGMT_4_4_0_OR_NEWER
    using Unity.XR.Management.AndroidManifest.Editor;
#endif

    internal static class AndroidXRBuildUtils
    {
        // UnityEditor.XR.OpenXR.Features.Android.AndroidFeatureSet is an internal class,
        // only accessible from its own package.
        internal const string _unityAndroidXRFeatureSetId = "com.unity.openxr.featureset.android";

        public static TOpenXRFeature GetActiveFeature<TOpenXRFeature>()
            where TOpenXRFeature : OpenXRFeature
        {
            if (OpenXRSettings.ActiveBuildTargetInstance == null ||
                OpenXRSettings.ActiveBuildTargetInstance.GetFeatures() == null)
            {
                return null;
            }

            return OpenXRSettings.ActiveBuildTargetInstance.GetFeature<TOpenXRFeature>();
        }

#if XR_MGMT_4_4_0_OR_NEWER
        public static string PrintManifestElement(ManifestElement manifestElement)
        {
            return string.Format("Path: {0}{1}",
                string.Join("/", manifestElement.ElementPath),
                string.Join("", manifestElement.Attributes.Select(
                    pair => string.Format("\n  Attribute: {0}={1}", pair.Key, pair.Value))));
        }
#endif

        internal static bool IsAnyAndroidXRFeatureEnabled()
        {
            var featureIds = OpenXRFeatureSetManager.GetFeatureSetWithId(
                BuildTargetGroup.Android, AndroidXRFeatureSet._featureSetId).featureIds;
            var features = FeatureHelpers.GetFeaturesWithIdsForBuildTarget(
                BuildTargetGroup.Android, featureIds);
            bool isAnyFeatureActive = features.Any(feature => feature.enabled);

            return isAnyFeatureActive &&
                EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android;
        }

        internal static bool IsAnySessionDependentEnabled()
        {
            var features = FeatureHelpers.GetFeaturesWithIdsForBuildTarget(
                BuildTargetGroup.Android, AndroidXRFeatureSet._sessionManagementDependentIds);
            bool isAnyFeatureActive = features.Any(feature => feature.enabled);

            return isAnyFeatureActive &&
                EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android;
        }

        internal static bool IsUnityAndroidXRActive()
        {
#if UNITY_OPEN_XR_ANDROID_XR
            var featureIds = OpenXRFeatureSetManager.GetFeatureSetWithId(
                BuildTargetGroup.Android, _unityAndroidXRFeatureSetId).featureIds;
            var openXRFeatures = FeatureHelpers.GetFeaturesWithIdsForBuildTarget(
                BuildTargetGroup.Android, featureIds);

            var isAnyFeatureActive = openXRFeatures.Any(feature => feature.enabled);

            return isAnyFeatureActive &&
                EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android;
#else
            return false;
#endif
        }
    }
}
