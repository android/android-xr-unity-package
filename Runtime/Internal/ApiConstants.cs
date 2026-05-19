// <copyright file="ApiConstants.cs" company="Google LLC">
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

namespace Google.XR.Extensions.Internal
{
    internal static class ApiConstants
    {
        public const string LogTag = "GoogleAndroidXRExtensions";
#if UNITY_ANDROID || UNITY_EDITOR_WIN
        // Support scenarios:
        // 1. Targeting Android platform and running in runtime.
        // 2. Running in Windows Editor.
        public const string OpenXRAndroidApi = "unity_openxr_android";
        public const bool AndroidPlatform = true;
#else
        public const string OpenXRAndroidApi = "NOT_AVAILABLE";
        public const bool AndroidPlatform = false;
#endif // UNITY_ANDROID || UNITY_EDITOR_WIN
        public const string MeshingProviderDescriptorId = "AndroidXRMeshProvider";

        /// <summary>
        /// Library name of the native plugin built within Unity OpenXR Plugin package.
        /// </summary>
        public const string UnityOpenXRLib = "UnityOpenXR";

        // Unity dependencies with recommended versions:
        public const string UnityAXRPackage = "com.unity.xr.androidxr-openxr";
        public const string UnityAXRPackageDisplayName = "Unity OpenXR Android XR";
        public const string UnityAXRPackageVersion = "1.2.0";
        public const string UnityARFaceFeature = "Android XR: AR Face";
        public const string UnityARBoundingBoxFeature = "Android XR: AR Bounding Box";
        public const string UnityARSceneMeshFeature = "Android XR: AR Scene Meshing";

        public const string CompositionLayersPackage = "com.unity.xr.compositionlayers";
        public const string CompositionLayersPackageDisplayName = "XR Composition Layers";
        public const string CompositionLayersPackageVersion = "2.3.0";
        public const string CompositionLayerFeature = "Composition Layers Support";
    }
}
