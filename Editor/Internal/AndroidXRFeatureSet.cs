// <copyright file="AndroidXRFeatureSet.cs" company="Google LLC">
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
    using UnityEditor;
    using UnityEditor.XR.OpenXR.Features;

    [OpenXRFeatureSet(
        // Features showing in this feature set from Editor,
        // desired to include all OpenXR features provided by this package.
        FeatureIds = new string[] {
            XRSessionFeature.FeatureId,
            XREnvironmentBlendModeFeature.FeatureId,
            XRObjectTrackingFeature.FeatureId,
            XRMarkerTrackingFeature.FeatureId,
            XRQrCodeTrackingFeature.FeatureId,
            XRFaceTrackingFeature.FeatureId,
            XRPassthroughFeature.FeatureId,
            XRHandMeshFeature.FeatureId,
            XRFoveationFeature.FeatureId,
            XRUnboundedRefSpaceFeature.FeatureId,
            XRBodyTrackingFeature.FeatureId,
            XRSceneMeshingFeature.FeatureId,
            XRSystemStateFeature.FeatureId,
        },
        // Features enabled and disabled along with selecting and deselecting the feature set
        // from Editor.
        RequiredFeatureIds = new string[] {
            XRSessionFeature.FeatureId,
        },
        // Features enabled along with selecting the feature set from Editor,
        // desired to include all XR_ANDROID_* features provided by this package.
        DefaultFeatureIds = new string[] {
            XRObjectTrackingFeature.FeatureId,
            XRMarkerTrackingFeature.FeatureId,
            XRQrCodeTrackingFeature.FeatureId,
            XRFaceTrackingFeature.FeatureId,
            XRPassthroughFeature.FeatureId,
            XRHandMeshFeature.FeatureId,
        },
        UiName = _uiName,
        Description = "Feature group that provides Android XR experience and " +
            "subsystem implementation for AR Foundation functionality.",
        FeatureSetId = _featureSetId,
        SupportedBuildTargets = new BuildTargetGroup[] {
            BuildTargetGroup.Android,
        })]
    internal class AndroidXRFeatureSet
    {
        internal const string _uiName = "Android XR (Extensions)";
        internal const string _featureSetId = "com.google.androidxr.features";

        /// <summary>
        /// A list of features that depends on <see cref="XRSessionFeature"/> for
        /// lifecycle management, specifically for the usage of XRInstanceManagerApi.Register().
        /// </summary>
        internal static readonly string[] _sessionManagementDependentIds = new string[]
        {
            XREnvironmentBlendModeFeature.FeatureId,
            XRFaceTrackingFeature.FeatureId,
            XRFoveationFeature.FeatureId,
            XRHandMeshFeature.FeatureId,
            XRObjectTrackingFeature.FeatureId,
            XRMarkerTrackingFeature.FeatureId,
            XRQrCodeTrackingFeature.FeatureId,
            XRPassthroughFeature.FeatureId,
            XRUnboundedRefSpaceFeature.FeatureId,
            XRBodyTrackingFeature.FeatureId,
            XRSceneMeshingFeature.FeatureId,
            XRSystemStateFeature.FeatureId,
        };
    }
}
