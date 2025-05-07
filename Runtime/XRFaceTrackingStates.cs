// <copyright file="XRFaceTrackingStates.cs" company="Google LLC">
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
    using System;

    /// <summary>
    /// This is an enumeration of the possible face tracking states.
    /// </summary>
    public enum XRFaceTrackingStates
    {
        /// <summary>
        /// Indicates that face tracking is paused but may be resumed in the future.
        /// </summary>
        Paused,

        /// <summary> Tracking is stopped and is currently not tracking. </summary>
        Stopped,

        /// <summary> The face is currently tracked and its pose is current. </summary>
        Tracking
    }
}
