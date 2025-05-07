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
    using UnityEngine;
    using UnityEngine.XR;
    using UnityEngine.XR.Management;
    using UnityEngine.XR.OpenXR;
    using UnityEngine.XR.OpenXR.Features;
    using static UnityEngine.XR.OpenXR.Features.OpenXRFeature;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    internal static class AndroidXRFeatureUtils
    {
#if UNITY_EDITOR
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
#endif
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
    }
}
