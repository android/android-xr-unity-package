// <copyright file="ApiConstants.cs" company="Google LLC">
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

namespace Google.XR.Extensions.SpatialEntities.Internal
{
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage(
        "UnityRules.UnityStyleRules", "US1103:PublicFieldsMustHaveNoPrefix",
        Justification = "Matching OpenXR extensions.")]
    [SuppressMessage(
        "UnityRules.UnityStyleRules", "US1107:PublicFieldsMustBeUpperCamelCase",
        Justification = "Matching OpenXR extensions.")]
    internal static class ApiConstants
    {
        public const string Assembly = "Google.XR.Extensions.SpatialEntities";

        public const int LifecyclePriority = 99;
        public const int SpatialFeaturePriority = 80;

        // Unity features mostly use default priority 0.
        public const int ExtendedFeaturePriority = -1;

        // OpenXR Spatial Entity Extensions
        public const string EXT_future = "XR_EXT_future";
        public const string EXT_spatial_entity = "XR_EXT_spatial_entity";
        public const string EXT_spatial_persistence = "XR_EXT_spatial_persistence";
        public const string EXT_spatial_persistence_operations =
            "XR_EXT_spatial_persistence_operations";

        public const string EXT_spatial_anchor = "XR_EXT_spatial_anchor";
        public const string EXT_spatial_plane_tracking = "XR_EXT_spatial_plane_tracking";
        public const string EXT_spatial_marker_tracking = "XR_EXT_spatial_marker_tracking";

        // ANDROID vendor extensions.
        public const string ANDROID_spatial_object_tracking = "XR_ANDROID_spatial_object_tracking";
        public const string ANDROID_geospatial = "XR_ANDROID_geospatial";
        public const string ANDROID_geospatial_anchor = "XR_ANDROID_geospatial_anchor";

        // Unity OpenXR: Spatial Entities package
        public const string UnitySpatialEntityPackage = "com.unity.xr.spatialentities-openxr";
        public const string UnitySpatialEntityPackageDisplayName = "Unity OpenXR Spatial Entities";
        public const string UnitySpatialEntityPackageVersion = "6.6.0";
        public const string UnitySpatialAnchorFeatureName = "Spatial Entities: Anchors";
        public const string UnitySpatialPlaneFeatureName = "Spatial Entities: Planes";
        public const string UnitySpatialMarkerFeatureName = "Spatial Entities: Markers";
    }
}
