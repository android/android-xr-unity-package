// <copyright file="AndroidXRFeatureUtils.cs" company="Google LLC">
//
// Copyright 2025 Google LLC
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

namespace Google.XR.Extensions.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.XR;
    using UnityEngine.XR.Management;
    using UnityEngine.XR.OpenXR;
    using UnityEngine.XR.OpenXR.Features;
    using static UnityEngine.XR.OpenXR.Features.OpenXRFeature;

#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.PackageManager;
    using UnityEditor.PackageManager.Requests;
#endif

    internal static class AndroidXRFeatureUtils
    {
#if UNITY_EDITOR
        private static Dictionary<string, AddRequest> _packageRequests = new();

        public static ValidationRule GetPackageDependencyRule(
            OpenXRFeature feature, string featureName,
            string package, string displayName, string version)
        {
            return new ValidationRule(feature)
            {
                message = string.Format("{0} package is required.", displayName),
                checkPredicate = () =>
                {
                    return IsPackageInstalled(package);
                },
                fixIt = () =>
                {
                    RequestPackageInstallation(featureName, package, displayName, version);
                },
                fixItMessage = string.Format("Install {0} package.", displayName),
                error = true,
            };
        }

        public static ValidationRule GetFeatureDependencyRule(
            OpenXRFeature feature, Type requiredFeatureType, string requiredFeatureName,
            BuildTargetGroup buildTarget)
        {
            return new ValidationRule(feature)
            {
                message = string.Format("{0} feature is required.", requiredFeatureName),
                checkPredicate = () =>
                {
                    var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(buildTarget);
                    if (settings == null)
                    {
                        return false;
                    }

                    var requiredFeature = settings.GetFeature(requiredFeatureType);
                    return requiredFeature != null && requiredFeature.enabled;
                },
                fixIt = () =>
                {
                    var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(buildTarget);
                    if (settings == null)
                    {
                        Debug.LogWarningFormat(
                            "Autofix failed with missing OpenXRSettings on {0} platform.",
                            buildTarget);
                        return;
                    }

                    var requiredFeature = settings.GetFeature(requiredFeatureType);
                    if (requiredFeature == null)
                    {
                        Debug.LogWarningFormat(
                            "Autofix failed with missing {0} feature on {1} platform.",
                            requiredFeatureName, buildTarget);
                        return;
                    }

                    requiredFeature.enabled = true;
                },
                fixItMessage = string.Format("Enable {0} feature.", requiredFeatureName),
                error = true,
            };
        }

        public static ValidationRule GetFeatureConflictRule(
            OpenXRFeature feature, Type conflictFeatureType, string conflictFeatureName,
            BuildTargetGroup buildTarget)
        {
            return new ValidationRule(feature)
            {
                message =
                    string.Format("{0} is not compatible with this feature.", conflictFeatureName),
                checkPredicate = () =>
                {
                    var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(buildTarget);
                    if (settings == null)
                    {
                        return true;
                    }

                    var conflictFeature = settings.GetFeature(conflictFeatureType);
                    return conflictFeature == null || !conflictFeature.enabled;
                },
                fixIt = () =>
                {
                    var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(buildTarget);
                    if (settings == null)
                    {
                        Debug.LogWarningFormat(
                            "Autofix failed with missing OpenXRSettings on {0} platform.",
                            buildTarget);
                        return;
                    }

                    var conflictFeature = settings.GetFeature(conflictFeatureType);
                    if (conflictFeature == null)
                    {
                        Debug.LogWarningFormat(
                            "Autofix failed with missing {0} feature on {1} platform.",
                            conflictFeatureName, buildTarget);
                        return;
                    }

                    conflictFeature.enabled = false;
                },
                fixItMessage = string.Format("Disable {0} feature.", conflictFeatureName),
                error = true,
            };
        }

        public static ValidationRule GetSubsystemConflictRule(
            OpenXRFeature feature, string featureName,
            Type conflictFeatureType, string conflictFeatureName,
            string subsystemName, BuildTargetGroup buildTarget)
        {
            return new ValidationRule(feature)
            {
                message = string.Format(
                    "Duplicate with {0}, both are {1} provider.\n" +
                    "Disable either <b>{2}</b> or <b>{0}</b> under " +
                    "<b>XR Plug-in Management > OpenXR > {3}</b>.",
                    conflictFeatureName, subsystemName, featureName, buildTarget),
                checkPredicate = () =>
                {
                    var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(buildTarget);
                    if (settings == null)
                    {
                        return true;
                    }

                    var target = settings.GetFeature(feature.GetType());
                    var conflict = settings.GetFeature(conflictFeatureType);
                    return target == null || !target.enabled ||
                        conflict == null || !conflict.enabled;
                },
                error = true,
            };
        }

        public static ValidationRule GetExperimentalFeatureValidationCheck(
            OpenXRFeature feature, string featureUiName, BuildTargetGroup targetGroup)
        {
            return new ValidationRule(feature)
            {
                message =
                    "Experimental feature in use. This API may change in future versions and " +
                    "applications using it cannot be published in Google Play Store.",
                checkPredicate = () =>
                {
                    var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);
                    if (settings == null)
                    {
                        return false;
                    }

                    var editorFeature = settings.GetFeature(feature.GetType());
                    return editorFeature == null || !editorFeature.enabled;
                },
                fixItMessage = "Disable experimental feature.",
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

                    var editorFeature = settings.GetFeature(feature.GetType());
                    if (editorFeature == null)
                    {
                        Debug.LogWarningFormat(
                            "Autofix failed with missing {0} on {1} platform.",
                            featureUiName, targetGroup);
                        return;
                    }

                    feature.enabled = false;
                },
                error = false
            };
        }
#endif // UNITY_EDITOR

        public static bool CheckExtensions(string extensions)
        {
            // split by white-space characters, e.g. space, \t, \n, \r.
            string[] all = extensions.Split();
            foreach (string extension in all)
            {
                if (string.IsNullOrEmpty(extension))
                {
                    // String.Split() creates an array of size one where the
                    // first element is an empty string if the input string is empty.
                    continue;
                }

                if (!OpenXRRuntime.IsExtensionEnabled(extension))
                {
                    Debug.LogWarning($"{extension} is not available.");
                    return false;
                }
            }

            return true;
        }

        public static void CreateMeshingSubsystem(
            string featureUiName, Action<List<XRMeshSubsystemDescriptor>, string> CreateSubsystem)
        {
            List<XRMeshSubsystemDescriptor> meshDescriptors = new List<XRMeshSubsystemDescriptor>();
            XRLoader xrLoader = XRSessionFeature.GetXRLoader();
            XRMeshSubsystem subsyst = null;
            if (xrLoader != null)
            {
                // Maybe another Meshing feature has already created the subsystem
                subsyst = xrLoader.GetLoadedSubsystem<XRMeshSubsystem>();
            }
            else
            {
                Debug.LogWarning("Failed to find any active loader.");
                return;
            }

            if (subsyst == null)
            {
                CreateSubsystem(meshDescriptors, ApiConstants.MeshingProviderDescriptorId);
                subsyst = xrLoader.GetLoadedSubsystem<XRMeshSubsystem>();
                if (subsyst == null)
                {
                    Debug.LogErrorFormat(
                        "Failed to find descriptor {0} - {1} will not do anything",
                        ApiConstants.MeshingProviderDescriptorId,
                        featureUiName);
                    return;
                }
                else
                {
                    Debug.LogFormat(
                        "{0} subsystem provider has been created",
                        ApiConstants.MeshingProviderDescriptorId);
                }
            }
            else if (subsyst.subsystemDescriptor.id == ApiConstants.MeshingProviderDescriptorId)
            {
                Debug.LogFormat(
                    "{0} subsystem provider has already been created. {1} will use it",
                    ApiConstants.MeshingProviderDescriptorId,
                    featureUiName);
                return;
            }
            else
            {
                Debug.LogWarningFormat(
                    "Active XRMeshSubsystem descriptor id {0} mismatches {1} for feature {2}",
                    subsyst.subsystemDescriptor.id,
                    ApiConstants.MeshingProviderDescriptorId,
                    featureUiName);
                return;
            }
        }

#if UNITY_EDITOR
        private static bool IsPackageInstalled(string package)
        {
            var current = Client.List();
            if (!current.IsCompleted || current.Status == StatusCode.Failure)
            {
                // Due to the async initialization of Client List,
                // prefer assembly definition to confirm package installation.
                return false;
            }

            // Rely on Unity Editor for package upgrades.
            return current.Result.Any(p => p.name.Equals(package));
        }

        private static void RequestPackageInstallation(
            string feature, string package, string displayName, string version)
        {
            string packageId = $"{package}@{version}";
            var request = _packageRequests.GetValueOrDefault(packageId, null);
            if (request == null)
            {
                request = Client.Add(packageId);
                _packageRequests.Add(packageId, request);
                Debug.LogWarningFormat(
                    "[{0}] Sending request to add package {1}.",
                    feature, displayName);
                return;
            }

            switch (request.Status)
            {
                case StatusCode.InProgress:
                    Debug.LogWarningFormat(
                        "[{0}] Waiting for adding package {1}",
                        feature, displayName);
                    break;
                case StatusCode.Success:
                    Debug.LogFormat(
                        "[{0}] Successfully added package {1} ({2}).",
                        feature, displayName, packageId);
                    _packageRequests.Remove(packageId);
                    break;
                case StatusCode.Failure:
                    Debug.LogWarningFormat(
                        "[{0}] Failed to add package {1} with error {2}. " +
                        "Please try again later or manually install package {3}.",
                        feature, displayName, request.Error.message, packageId);
                    _packageRequests.Remove(packageId);
                    break;
            }
        }
#endif // UNITY_EDITOR
    }
}
