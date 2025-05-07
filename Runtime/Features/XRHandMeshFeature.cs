// <copyright file="XRHandMeshFeature.cs" company="Google LLC">
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
    using UnityEngine.XR;
    using UnityEngine.XR.OpenXR;
    using UnityEngine.XR.OpenXR.Features;

#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.XR.OpenXR.Features;
#endif  // UNITY_EDITOR

    /// <summary>
    /// This feature provides access to the <c>XR_ANDROID_hand_mesh</c> extension.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(
        UiName = UiName,
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        Company = "Google",
        Desc = "Configure Hand Mesh",
        Version = "1.0.0",
        Category = FeatureCategory.Feature,
        FeatureId = FeatureId,
        OpenxrExtensionStrings = ExtensionString, Priority = 99)]
#endif  // UNITY_EDITOR
    public class XRHandMeshFeature : OpenXRFeature
    {
        /// <summary>
        /// The UI name shows on the XR Plug-in Management panel, help users to understand
        /// validation errors and expected fixes.
        /// </summary>
        public const string UiName = "Android XR (Extensions): Hand Mesh";

        /// <summary>
        /// The feature ID string.
        /// </summary>
        public const string FeatureId = "com.google.xr.extensions.hand_mesh";

        /// <summary>
        /// The required OpenXR extension.
        /// </summary>
        public const string ExtensionString = "XR_ANDROID_hand_mesh";

        /// <summary>
        /// Runtime permission required to enable hand tracking.
        /// </summary>
        public static readonly AndroidXRPermission RequiredPermission =
            AndroidXRPermission.HandTracking;

        internal static bool? _extensionEnabled = null;

        /// <summary>
        /// Gets if the required OpenXR extension is enabled.
        /// When OpenXR runtime is waiting, it returns <c>null</c>. Otherwise, it indicates
        /// whether the <c>XR_ANDROID_hand_mesh</c> extensions is
        /// available on current device.
        /// </summary>
        public static bool? IsExtensionEnabled => _extensionEnabled;

        /// <inheritdoc/>
        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            if (!base.OnInstanceCreate(xrInstance))
            {
                return false;
            }

            _extensionEnabled = OpenXRRuntime.IsExtensionEnabled(ExtensionString);

            return _extensionEnabled.Value &&
                   XRInstanceManagerApi.Register(ApiXrFeature.HandMesh);
        }

        /// <inheritdoc/>
        protected override void OnInstanceDestroy(ulong xrInstance)
        {
            XRInstanceManagerApi.Unregister(ApiXrFeature.HandMesh);
        }

        /// <inheritdoc/>
        protected override void OnSubsystemCreate()
        {
            AndroidXRFeatureUtils.CreateMeshingSubsystem(
                UiName, CreateSubsystem<XRMeshSubsystemDescriptor, XRMeshSubsystem>);
        }

        /// <inheritdoc/>
        protected override void OnSubsystemStart()
        {
            XRHandMeshApi.Enable();
            StartSubsystem<XRMeshSubsystem>();
        }

        /// <inheritdoc/>
        protected override void OnSubsystemStop()
        {
            XRHandMeshApi.Disable();
            StopSubsystem<XRMeshSubsystem>();
        }

        /// <inheritdoc/>
        protected override void OnSubsystemDestroy()
        {
            DestroySubsystem<XRMeshSubsystem>();
        }
    }
}
