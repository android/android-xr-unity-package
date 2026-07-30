// <copyright file="XRFaceTrackerMode.cs" company="Google LLC">
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

    /// <summary>
    /// This is an enumeration of the possible face tracking modalities.
    /// </summary>
    public enum XRFaceTrackerMode : int
    {
        /// <summary> Unknown face tracking mode. </summary>
        [UnityEngine.HideInInspector]
        Unknown = 0,

        /// <summary> Image-only face tracking. </summary>
        Image = 1,

        /// <summary> Audio-only face tracking. </summary>
        Audio = 2,

        /// <summary> Multimodal face tracking. </summary>
        Multimodal = 3,
    }
}
