// <copyright file="XRSceneMeshingFeature.cs" company="Google LLC">
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

namespace Google.XR.Extensions
{
    using System.Collections.Generic;
    using Google.XR.Extensions.Internal;
    using UnityEngine.XR;
    using UnityEngine.XR.OpenXR;
    using UnityEngine.XR.OpenXR.Features;

#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.XR.OpenXR.Features;
#if UNITY_OPEN_XR_ANDROID_XR_1_2_0 && UNITY_6000_4_OR_NEWER
    using UnityEngine.XR.OpenXR.Features.Android;
#endif // UNITY_OPEN_XR_ANDROID_XR_1_2_0 && UNITY_6000_4_OR_NEWER
#endif  // UNITY_EDITOR

    /// <summary>
    /// This feature provides access to the <c>XR_ANDROID_scene_meshing</c> extension.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(
        UiName = UiName,
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        Company = "Google",
        Desc = "Configure Scene Meshing",
        Version = "1.0.0",
        Category = FeatureCategory.Feature,
        FeatureId = FeatureId,
        OpenxrExtensionStrings = ExtensionString, Priority = 99)]
#endif  // UNITY_EDITOR
    public class XRSceneMeshingFeature : OpenXRFeature, IXRSpatialSdk
    {
        /// <summary>
        /// The UI name shows on the XR Plug-in Management panel, help users to understand
        /// validation errors and expected fixes.
        /// </summary>
        public const string UiName = "Android XR (Extensions): Scene Meshing";

        /// <summary>
        /// The feature ID string.
        /// </summary>
        public const string FeatureId = "com.google.xr.extensions.scene_meshing";

        /// <summary>
        /// The required OpenXR extension.
        /// </summary>
        public const string ExtensionString = "XR_ANDROID_scene_meshing";

        /// <summary>
        /// Runtime permission required to enable scene understanding fine.
        /// </summary>
        public static readonly AndroidXRPermission RequiredPermission =
            AndroidXRPermission.SceneUnderstandingFine;

        internal static bool? _extensionEnabled = null;

        /// <summary>
        /// Gets if the required OpenXR extension is enabled.
        /// When OpenXR runtime is waiting, it returns <c>null</c>. Otherwise, it indicates
        /// whether the <c>XR_ANDROID_scene_meshing</c> extensions is
        /// available on current device.
        /// </summary>
        public static bool? IsExtensionEnabled => _extensionEnabled;

        /// <summary>
        /// Sets if the scene meshing provider should be enabled on start of XRMeshSubsystem.
        /// </summary>
        /// <param name="enabled">
        /// Decides whether the scene meshing provider should be enabled on start of XRMeshSubsystem.
        /// </param>
        public void SetProviderEnabledOnStart(bool enabled)
        {
            // For now this is a workaround until we are able to figure out an appropriate function
            // to override and call it there. OnEnable and OnDisable are only called when the
            // feature is loaded and unloaded.
            XRMeshProviderApi.SetProviderEnabledOnStart(ApiXrFeature.SceneMeshing, enabled);
        }

        /// <inheritdoc/>
        public XRSpatialSdkVersions GetTargetVersion()
        {
            return XRSpatialSdkVersions.XRSpatialApiLevel1;
        }

        /// <inheritdoc/>
        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            if (!base.OnInstanceCreate(xrInstance))
            {
                return false;
            }

            _extensionEnabled = OpenXRRuntime.IsExtensionEnabled(ExtensionString);

            return _extensionEnabled.Value &&
                   XRInstanceManagerApi.Register(ApiXrFeature.SceneMeshing);
        }

        /// <inheritdoc/>
        protected override void OnInstanceDestroy(ulong xrInstance)
        {
            XRInstanceManagerApi.Unregister(ApiXrFeature.SceneMeshing);
        }

        /// <inheritdoc/>
        protected override void OnSubsystemCreate()
        {
            AndroidXRFeatureUtils.CreateMeshingSubsystem(
                UiName, CreateSubsystem<XRMeshSubsystemDescriptor, XRMeshSubsystem>);
            XRSceneMeshingApi.SetProviderAvailable(true);
            XRMeshProviderApi.SetProviderEnabledOnStart(ApiXrFeature.SceneMeshing, true);
        }

        /// <inheritdoc/>
        protected override void OnSubsystemStart()
        {
            StartSubsystem<XRMeshSubsystem>();
        }

        /// <inheritdoc/>
        protected override void OnSubsystemStop()
        {
            StopSubsystem<XRMeshSubsystem>();
        }

        /// <inheritdoc/>
        protected override void OnSubsystemDestroy()
        {
            XRSceneMeshingApi.SetProviderAvailable(false);
            DestroySubsystem<XRMeshSubsystem>();
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
#if UNITY_OPEN_XR_ANDROID_XR_1_2_0 && UNITY_6000_4_OR_NEWER
            results.Add(AndroidXRFeatureUtils.GetSubsystemConflictRule(
                feature: this,
                featureName: UiName,
                conflictFeatureType: typeof(ARMeshFeature),
                conflictFeatureName: ApiConstants.UnityARSceneMeshFeature,
                subsystemName: typeof(XRMeshSubsystem).Name,
                buildTarget: targetGroup));
#endif // UNITY_OPEN_XR_ANDROID_XR_1_2_0 && UNITY_6000_4_OR_NEWER
        }
#endif // UNITY_EDITOR
    }
}
