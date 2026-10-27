// <copyright file="SpatialComponentList.cs" company="Google LLC">
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
    using System.Runtime.InteropServices;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using UnityEngine.XR.OpenXR.NativeTypes;

    /// <summary>
    /// Generalized component list used for custom components.
    /// </summary>
    /// <typeparam name="T">The data type of the component list.</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    internal struct SpatialComponentList<T> where T : unmanaged
    {
        public XrStructureType Type;

        public unsafe void* Next;

        public uint ComponentCount;

        public unsafe T* Components;

        public unsafe SpatialComponentList(
            XrStructureType type, void* next, NativeArray<T> componentArray)
        {
            Type = type;
            Next = next;
            ComponentCount = (uint)componentArray.Length;
            Components = (T*)componentArray.GetUnsafePtr();
        }

        public static unsafe void* CreateWithAllocator(
            XrStructureType structureType, void* next, int size,
            Allocator allocator = Allocator.Temp)
        {
            var nativeArray = new NativeArray<T>(size, allocator);
            var list = new SpatialComponentList<T>(structureType, next, nativeArray);
            var nativeWrapper = new NativeArray<SpatialComponentList<T>>(1, allocator);
            nativeWrapper[0] = list;
            return nativeWrapper.GetUnsafePtr();
        }
    }
}
