// <copyright file="XRSceneMeshingApi.cs" company="Google LLC">
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
    using System.Runtime.InteropServices;
    using UnityEngine.XR;

    internal class XRSceneMeshingApi
    {
        public static bool Enable()
        {
            return ExternalApi.XrSceneMeshing_enableSceneMeshing(XRInstanceManagerApi.GetIntPtr());
        }

        public static bool Disable()
        {
            return ExternalApi.XrSceneMeshing_disableSceneMeshing(XRInstanceManagerApi.GetIntPtr());
        }

        public static bool IsSceneMeshId(ref MeshId mesh_id)
        {
            return ExternalApi.XrSceneMeshing_isSceneMeshId(
                XRInstanceManagerApi.GetIntPtr(), ref mesh_id);
        }

        public static IntPtr GetMeshSemantics(ref MeshId mesh_id, ref uint count)
        {
            return ExternalApi.XrSceneMeshing_getMeshSemantics(
                XRInstanceManagerApi.GetIntPtr(), ref mesh_id, ref count);
        }

        private struct ExternalApi
        {
            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrSceneMeshing_enableSceneMeshing(IntPtr manager);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrSceneMeshing_disableSceneMeshing(IntPtr manager);
            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrSceneMeshing_isSceneMeshId(
                IntPtr manager, ref MeshId mesh_id);
            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern IntPtr XrSceneMeshing_getMeshSemantics(
                IntPtr manager, ref MeshId mesh_id, ref uint count);
        }
    }
}
