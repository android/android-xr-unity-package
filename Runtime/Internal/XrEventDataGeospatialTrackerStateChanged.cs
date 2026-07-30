// <copyright file="XrEventDataGeospatialTrackerStateChanged.cs" company="Google LLC">
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
    using System.Runtime.InteropServices;
    using UnityEngine.XR.OpenXR.NativeTypes;

    using XrGeospatialTracker = System.UInt64;
    using XrTime = System.Int64;

    /// <summary>
    /// Native wrapper of OpenXR struct XrEventDataGeospatialTrackerStateChangedANDROID.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct XrEventDataGeospatialTrackerStateChanged
    {
        public const XrStructureType EventType = (XrStructureType)1000789002U;

        public XrStructureType Type;
        public unsafe void* Next;
        public XrGeospatialTracker Tracker;
        public ApiGeospatialTrackerState State;
        public XrResult InitializationResult;
        public XrTime Time;

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Type={Type}\n" +
                   $"Tracker={Tracker}\n" +
                   $"State={State}\n" +
                   $"Result={InitializationResult}\n" +
                   $"Time={Time}";
        }
    }
}
