// <copyright file="XRFaceTrackingFeature.cs" company="Google LLC">
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
    using UnityEngine;
    using UnityEngine.XR.OpenXR;
    using UnityEngine.XR.OpenXR.Features;

#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.XR.OpenXR.Features;
#endif

    /// <summary>
    /// This <see cref="OpenXRInteractionFeature"/> configures new extension
    /// <c>XR_ANDROID_face_tracking</c> and provides face blendshape parameter vectors at runtime.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = UiName,
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        Company = "Google",
        OpenxrExtensionStrings = FaceTrackingExtensionString,
        Desc = "Allow user face expressions to influence an avatar.",
        Version = "1.0.0",
        Category = FeatureCategory.Feature,
        FeatureId = FeatureId)]
#endif
    public class XRFaceTrackingFeature : OpenXRFeature
    {
        /// <summary>
        /// The UI name shows on the XR Plug-in Management panel, help users to understand
        /// validation errors and expected fixes.
        /// </summary>
        public const string UiName = "Android XR: Face Tracking";

        /// <summary>
        /// The feature ID string.
        /// </summary>
        public const string FeatureId = "com.google.xr.extensions.face_tracking";

        /// <summary>
        /// The OpenXR Extension string. Used to check if this extensions is
        /// available or enabled.
        /// </summary>
        public const string FaceTrackingExtensionString = "XR_ANDROID_face_tracking";

        /// <summary>
        /// Runtime permission required to enable face tracking.
        /// </summary>
        public static readonly AndroidXRPermission RequiredPermission =
            AndroidXRPermission.FaceTracking;

        internal static bool? _faceTrackingExtensionEnabled = null;

        /// <summary>
        /// Gets if the required OpenXR extension is enabled.
        /// When OpenXR runtime is waiting, it returns <c>null</c>. Otherwise, it indicates
        /// whether the XR_ANDROID_face_tracking extension is enabled.
        /// </summary>
        public static bool? IsFaceTrackingExtensionEnabled => _faceTrackingExtensionEnabled;

        /// <inheritdoc/>
        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            if (!base.OnInstanceCreate(xrInstance))
            {
                return false;
            }

            _faceTrackingExtensionEnabled =
                OpenXRRuntime.IsExtensionEnabled(FaceTrackingExtensionString);

            if (!_faceTrackingExtensionEnabled.Value)
            {
                return false;
            }

            var result = XRInstanceManagerApi.Register(ApiXrFeature.FaceTracking);
            return result;
        }

        /// <inheritdoc/>
        protected override void OnInstanceDestroy(ulong xrInstance)
        {
            XRInstanceManagerApi.Unregister(ApiXrFeature.FaceTracking);
        }

        private void OnValidate()
        {
            if (enabled)
            {
                Debug.LogFormat(
                    "Face tracking is enabled at runtime. " +
                    "Note: it requires runtime permission \"{0}\".",

                    RequiredPermission.ToPermissionString());
            }
        }
    }
}
