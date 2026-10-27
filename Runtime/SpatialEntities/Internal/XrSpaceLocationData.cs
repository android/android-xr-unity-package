// <copyright file="XrSpaceLocationData.cs" company="Google LLC">
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
    using System;
    using System.Runtime.InteropServices;

    using XrPosef = UnityEngine.XR.OpenXR.NativeTypes.XrPosef;

    /// <summary>
    /// Native wrappers of OpenXR XrFlags64 XrSpaceLocationFlags.
    /// </summary>
    /// <remarks>
    /// NOTE: <see cref="UnityEngine.XR.OpenXR.NativeTypes.XrSpaceLocationFlags"/>
    /// uses default enum (i.e. 32-bit int, 4 bytes) which is shorter than
    /// OpenXR XrFlags64, i.e. C++ uint64_t type, 8 bytes.
    /// They are equivalent in value comparison.
    /// </remarks>
    [Flags]
    internal enum XrSpaceLocationFlags : ulong
    {
        None = 0,
        OrientationValid = 1,
        PositionValid = 2,
        OrientationTracked = 4,
        PositionTracked = 8
    }

    /// <summary>
    /// Native wrapper of OpenXR struct XrSpaceLocationData.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct XrSpaceLocationData
    {
        public XrSpaceLocationFlags LocationFlags;
        public XrPosef Pose;

        public XrSpaceLocationData(XrSpaceLocationFlags locationFlags, XrPosef posef)
        {
            LocationFlags = locationFlags;
            Pose = posef;
        }
    }
}
