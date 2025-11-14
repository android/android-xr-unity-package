// <copyright file="XRFaceTrackingFeatureBuildHooks.cs" company="Google LLC">
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
    using Unity.XR.Management.AndroidManifest.Editor;
    using UnityEditor.Build.Reporting;
    using UnityEditor.XR.OpenXR.Features;
    using UnityEngine;
    using UnityEngine.XR.OpenXR;

    internal class XRFaceTrackingFeatureBuildHooks : OpenXRFeatureBuildHooks
    {
        private static readonly ManifestElement _permissionManifest = new ManifestElement()
        {
            ElementPath = new List<string> { "manifest", "uses-permission" },
            Attributes = new Dictionary<string, string>
            {
                { "name", XRFaceTrackingFeature.RequiredPermission.ToPermissionString() }
            }
        };

        /// <inheritdoc/>
        [SuppressMessage("UnityRules.UnityStyleRules",
                         "US1109:PublicPropertiesMustBeUpperCamelCase",
                         Justification = "Overridden property.")]
        public override int callbackOrder => 1; // The order of this callback does not matter

        /// <inheritdoc/>
        [SuppressMessage("UnityRules.UnityStyleRules",
                         "US1109:PublicPropertiesMustBeUpperCamelCase",
                         Justification = "Overridden property.")]
        public override Type featureType => typeof(XRFaceTrackingFeature);

        public override ManifestRequirement ProvideManifestRequirement()
        {
            XRFaceTrackingFeature activeFeature =
                AndroidXRBuildUtils.GetActiveFeature<XRFaceTrackingFeature>();
            bool requiredManifest = activeFeature != null && activeFeature.enabled;
            List<ManifestElement> injectManifests = new List<ManifestElement>();
            if (requiredManifest)
            {
                injectManifests.Add(_permissionManifest);

                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("Inject manifest elements for Face Tracking:");
                foreach (var element in injectManifests)
                {
                    stringBuilder.Append("\n" + AndroidXRBuildUtils.PrintManifestElement(element));
                }

                Debug.Log(stringBuilder);
            }

            List<ManifestElement> emptyElement = new List<ManifestElement>();
            return new ManifestRequirement
            {
                SupportedXRLoaders = new HashSet<Type>()
                {
                    typeof(OpenXRLoader)
                },
                NewElements = requiredManifest ? injectManifests : emptyElement,
                RemoveElements = requiredManifest ? emptyElement : injectManifests,
            };
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
    }
}
