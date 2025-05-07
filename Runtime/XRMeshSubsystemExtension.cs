// <copyright file="XRMeshSubsystemExtension.cs" company="Google LLC">
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

namespace Google.XR.Extensions
{
    using System;
    using System.Runtime.InteropServices;
    using Google.XR.Extensions.Internal;
    using UnityEngine.XR;

    /// <summary>
    /// An enum representing the vertex semantics of a scene mesh.
    /// </summary>
    public enum XRMeshSemantics : byte
    {
        /// <summary> Represents a vertex with unknown semantic. </summary>
        OTHER = 0,

        /// <summary> Represents a vertex on the floor. </summary>
        FLOOR = 1,

        /// <summary> Represents a vertex on the ceiling. </summary>
        CEILING = 2,

        /// <summary> Represents a vertex on the wall. </summary>
        WALL = 3,

        /// <summary> Represents a vertex on the table. </summary>
        TABLE = 4,
    }

    /// <summary>
    /// Extensions to AR Foundation's <c><see cref="XRMeshSubsystem"/></c> class.
    /// </summary>
    public static class XRMeshSubsystemExtension
    {
        /// <summary>
        /// Checks if the given mesh id is a scene mesh id.
        /// </summary>
        /// <param name="meshId">The mesh id to check.</param>
        /// <returns>True if the mesh id is a scene mesh id, false otherwise.</returns>
        public static bool IsSceneMeshId(MeshId meshId)
        {
            return XRSceneMeshingApi.IsSceneMeshId(ref meshId);
        }

        /// <summary>
        /// Gets the vertex semantics of the given mesh id.
        /// </summary>
        /// <param name="meshId">The mesh id to get the vertex semantics of.</param>
        /// <returns>
        /// The vertex semantics of the mesh id as an array of <c>XRMeshSemantics</c>enums. If the
        /// mesh id is invalid or if the semantics could not be retrieved, the function will return
        /// null.
        /// </returns>
        public static XRMeshSemantics[] GetMeshSemantics(MeshId meshId)
        {
            XRMeshSemantics[] result = null;
            uint count = 0;
            IntPtr semantics = XRSceneMeshingApi.GetMeshSemantics(ref meshId, ref count);
            if (semantics != IntPtr.Zero && count > 0)
            {
                byte[] stagingArray = new byte[count];
                Marshal.Copy(semantics, stagingArray, 0, (int)count);
                result = new XRMeshSemantics[count];
                stagingArray.CopyTo(result, 0);
            }

            return result;
        }
    }
}
