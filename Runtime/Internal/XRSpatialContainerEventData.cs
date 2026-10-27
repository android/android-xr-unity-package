// <copyright file="XRSpatialContainerEventData.cs" company="Google LLC">
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

namespace Google.XR.Extensions.Internal
{
    using System;
    using System.Runtime.InteropServices;
    using UnityEngine;

    using UnityEngine.XR.OpenXR.NativeTypes;

    internal delegate void SpatialContainerChangedDelegate(ulong spatialContainer);

    internal static class XRSpatialContainerEventData
    {
        public const int BoundsModeBounded = 1;
        public const int BoundsModeImmersive = 2;

        public const XrStructureType SpatialContainerClosedEXT =
            (XrStructureType)1000810003U;

        public const XrStructureType SpatialContainerBoundsChangedEXT =
            (XrStructureType)1000810006U;

        public const XrStructureType SpatialContainerVisibleChangedEXT =
            (XrStructureType)1000810010U;

        public const XrStructureType SpatialContainerVisibleRequestDeniedEXT =
            (XrStructureType)1000810011U;

        public const XrStructureType SpatialContainerInteractableChangedEXT =
            (XrStructureType)1000810012U;

        public const XrStructureType SpatialContainerBoundsModeRequestDeniedEXT =
            (XrStructureType)1000810014U;

        [StructLayout(LayoutKind.Sequential)]
        public struct XrEventDataSpatialContainerClosedEXT
        {
            public XrStructureType Type;
            public IntPtr Next;
            public ulong SpatialContainer;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct XrEventDataSpatialContainerBoundsChangedEXT
        {
            public XrStructureType Type;
            public IntPtr Next;
            public ulong SpatialContainer;
            public Vector3 Bounds;
            public uint InfiniteBounds;
            public int BoundsMode;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct XrEventDataSpatialContainerVisibleChangedEXT
        {
            public XrStructureType Type;
            public IntPtr Next;
            public ulong SpatialContainer;
            public uint Visible;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct XrEventDataSpatialContainerVisibleRequestDeniedEXT
        {
            public XrStructureType Type;
            public IntPtr Next;
            public ulong SpatialContainer;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct XrEventDataSpatialContainerInteractableChangedEXT
        {
            public XrStructureType Type;
            public IntPtr Next;
            public ulong SpatialContainer;
            public uint Interactable;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct XrEventDataSpatialContainerBoundsModeRequestDeniedEXT
        {
            public XrStructureType Type;
            public IntPtr Next;
            public ulong SpatialContainer;
        }
    }
}
