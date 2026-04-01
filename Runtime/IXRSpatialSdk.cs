// <copyright file="IXRSpatialSdk.cs" company="Google LLC">
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

namespace Google.XR.Extensions
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.XR.OpenXR;
    using UnityEngine.XR.OpenXR.Features;

#if UNITY_EDITOR
    using System.Reflection;
    using UnityEditor;
#endif

    /// <summary>
    /// The XR Spatial API levels to specify in the manifest to filter applications from devices
    /// that don't meet its hardware and software feature requirements.
    /// Check individual <see cref="OpenXRFeature"/> for their minimal required levels.
    /// </summary>
    public enum XRSpatialSdkVersions
    {
        /// <summary>
        /// Uses the highest supported API level that currently available.
        /// </summary>
        [InspectorName("Automatic (highest supported)")]
        XRSpatialApiLevelAuto = 0,

        /// <summary> XR Spatial API level 1, the first Android XR release. </summary>
        [InspectorName("XR Spatial Api Level 1")]
        XRSpatialApiLevel1 = 1,

        /// <summary> XR Spatial API level 3, Android XR OTA 1. </summary>
        [InspectorName("XR Spatial Api Level 3")]
        XRSpatialApiLevel3 = 3,
    }

    /// <summary>
    /// The interface for <see cref="OpenXRFeature"/> to define XR Spatial SDK requirement.
    /// </summary>
    public interface IXRSpatialSdk
    {
        /// <summary>
        /// Gets the target version of the given feature if applicable.
        /// </summary>
        /// <returns>The target version to meet feature requirements.</returns>
        XRSpatialSdkVersions GetTargetVersion();
    }

    /// <summary>
    /// Helper class for the usage of <see cref="XRSpatialSdkVersions"/>.
    /// </summary>
    public static class XRSpatialSdkExtensions
    {
        /// <summary>
        /// Gets display option of the given version for GUI content.
        /// </summary>
        /// <param name="version">The given version.</param>
        /// <returns>The display option.</returns>
        public static string GetDisplayOption(this XRSpatialSdkVersions version)
        {
            switch (version)
            {
                case XRSpatialSdkVersions.XRSpatialApiLevelAuto:
                    return "Automatic (highest supported)";
                default:
                    return version.ToString();
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Validate the given version if it meets all feature requirements which have been enabled
        /// on Android platform.
        /// </summary>
        /// <param name="targetVersion">The target version.</param>
        /// <param name="minVersion">The minimal required version based on all enabled features.
        /// </param>
        /// <param name="featureNames">The names of all features that requires
        /// <paramref name="minVersion"/>.</param>
        /// <returns>True if the target version meets feature requirements. Otherwise, return false.
        /// </returns>
        public static bool ValidateActiveFeatures(this XRSpatialSdkVersions targetVersion,
            ref XRSpatialSdkVersions minVersion, ref List<string> featureNames)
        {
            minVersion = XRSpatialSdkVersions.XRSpatialApiLevel1;
            featureNames.Clear();
            bool result = true;
            var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
            if (settings == null)
            {
                return result;
            }

            foreach (var feature in settings.GetFeatures())
            {
                if (feature == null || !feature.enabled || feature is not IXRSpatialSdk)
                {
                    continue;
                }

                var spatialSDK = feature as IXRSpatialSdk;
                var requiredVersion = spatialSDK.GetTargetVersion();
                if (requiredVersion > minVersion)
                {
                    minVersion = spatialSDK.GetTargetVersion();
                    featureNames.Clear();
                    featureNames.Add(feature.GetOpenXRFeatureName());
                }
                else if (requiredVersion == minVersion)
                {
                    featureNames.Add(feature.GetOpenXRFeatureName());
                }

                result &= CheckVersionRequirement(targetVersion, spatialSDK.GetTargetVersion());
            }

            return result;
        }

        private static bool CheckVersionRequirement(
            XRSpatialSdkVersions target, XRSpatialSdkVersions required)
        {
            return target == XRSpatialSdkVersions.XRSpatialApiLevelAuto || target >= required;
        }

        private static string GetOpenXRFeatureName(this OpenXRFeature feature)
        {
            BindingFlags memberFlags = BindingFlags.NonPublic | BindingFlags.Instance;
            return (string)feature.GetType().GetField("nameUi", memberFlags).GetValue(feature);
        }
#endif
    }
}
