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
        public static void SetProviderAvailable(bool available)
        {
            ExternalApi.XrSceneMeshing_setProviderAvailable(
                XRInstanceManagerApi.GetIntPtr(), available);
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

        public static XRSceneMeshTrackingState GetSceneMeshTrackingState()
        {
            XRSceneMeshTrackingState state = XRSceneMeshTrackingState.ERROR;
            ExternalApi.XrSceneMeshing_getSceneMeshTrackingState(
                XRInstanceManagerApi.GetIntPtr(), ref state);
            return state;
        }

        private struct ExternalApi
        {
            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrSceneMeshing_setProviderAvailable(
                IntPtr manager, bool available);
            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrSceneMeshing_isSceneMeshId(
                IntPtr manager, ref MeshId mesh_id);
            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern IntPtr XrSceneMeshing_getMeshSemantics(
                IntPtr manager, ref MeshId mesh_id, ref uint count);
            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void XrSceneMeshing_getSceneMeshTrackingState(
                IntPtr manager, ref XRSceneMeshTrackingState out_state);
        }
    }
}
