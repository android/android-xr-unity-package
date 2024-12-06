// <copyright file="XRPassthroughApi.cs" company="Google LLC">
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
    using System;
    using System.Runtime.InteropServices;
    using UnityEngine;

    internal class XRPassthroughApi
    {
        // Creates a layer from the specified mesh, then returns the layer's handle.
        public static ulong CreateLayer(Mesh mesh, bool clockwise)
        {
            MeshData meshData = GetMeshData(mesh);
            ExternalApi.XrPassthrough_createLayer(XRInstanceManagerApi.GetIntPtr(),
                                                  meshData.VertexCount,
                                                  meshData.VertexData.AddrOfPinnedObject(),
                                                  meshData.TriangleCount,
                                                  meshData.IndexData.AddrOfPinnedObject(),
                                                  clockwise,
                                                  out ulong layer);

            meshData.IndexData.Free();
            meshData.VertexData.Free();

            return layer;
        }

        // Destroys a layer.
        public static void DestroyLayer(ulong layer)
        {
            ExternalApi.XrPassthrough_destroyLayer(XRInstanceManagerApi.GetIntPtr(), layer);
        }

        private static MeshData GetMeshData(Mesh mesh)
        {
            Vector3[] vertices = mesh.vertices;
            int[] triangles32 = mesh.triangles;
            ushort[] triangles = Array.ConvertAll(triangles32, val => checked((ushort)val));

            MeshData meshData = new MeshData();
            meshData.VertexCount = (uint)mesh.vertexCount;
            meshData.TriangleCount = (uint)triangles.Length / 3;
            meshData.VertexData = GCHandle.Alloc(vertices, GCHandleType.Pinned);
            meshData.IndexData = GCHandle.Alloc(triangles, GCHandleType.Pinned);

            return meshData;
        }

        private struct MeshData
        {
            public uint VertexCount;
            public uint TriangleCount;
            public GCHandle VertexData;
            public GCHandle IndexData;
        }

        private struct ExternalApi
        {
            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrPassthrough_createLayer(
                IntPtr manager, uint vertexCount, IntPtr vertexBuffer,
                uint triangleCount, IntPtr indexBuffer, bool clockwise,
                out ulong layer);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrPassthrough_destroyLayer(
                IntPtr manager, ulong layer);
        }
    }
}
