// <copyright file="SpatialEntitiesFeatureSet.cs" company="Google LLC">
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
    using Google.XR.Extensions.SpatialEntities;
    using UnityEditor;
    using UnityEditor.XR.OpenXR.Features;

    [OpenXRFeatureSet(
        // Features showing in this feature set from Editor,
        // desired to include all Spatial Entities features provided by this package.
        FeatureIds = new string[]
        {
            SpatialLifecycleFeature.FeatureId,
            SpatialBoundingBoxFeature.FeatureId,
            SpatialShareableAnchorFeature.FeatureId,
        },
        // Features enabled and disabled along with selecting and
        // deselecting the feature set from Editor.
        RequiredFeatureIds = new string[]
        {
        },
        // Features enabled along with selecting the feature set from Editor,
        // desired to include independent features which can work without
        // Unity OpenXR: Spatial Entities package.
        DefaultFeatureIds = new string[]
        {
            SpatialBoundingBoxFeature.FeatureId,
        },
        UiName = UiName,
        FeatureSetId = FeatureSetId,
        Description =
            "Feature group that provides additional Spatial Entities capabilities " +
            "on Android XR, and subsystem implementation for AR Foundation interfaces.",
        SupportedBuildTargets = new BuildTargetGroup[]
        {
            BuildTargetGroup.Android,
        })]
    internal class SpatialEntitiesFeatureSet
    {
        /// <summary>
        /// UI name shows under OpenXR settings. Matching Unity feature set "Spatial Entities".
        /// </summary>
        public const string UiName = "Android XR Spatial Entities";

        /// <summary>
        /// Feature set Id.
        /// </summary>
        public const string FeatureSetId = "com.google.androidxr.spatialentities";
    }
}
