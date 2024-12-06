// <copyright file="XRFaceState.cs" company="Google LLC">
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

namespace Google.XR.Extensions
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// This struct contains the blendshape parameter weights, current
    /// status of the face tracker and face joint poses.
    /// </summary>
    public struct XRFaceState
    {
        /// <summary> The vector of current blendshape parameters. </summary>
        public float[] Parameters;

        /// <summary> The current state of the face tracker. </summary>
        public XRFaceTrackingStates TrackingState;

        /// <summary> Capture time of the current face state data. </summary>
        public ulong Timestamp;

        /// <summary>
        /// Is true if the data is valid even if it has not been provided in this frame.
        /// </summary>
        public bool IsValid;
    }
}
