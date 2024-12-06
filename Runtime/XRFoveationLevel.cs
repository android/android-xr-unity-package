// <copyright file="XRFoveationLevel.cs" company="Google LLC">
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

    /// <summary>Enum corresponding to XrFoveationLevelFB.</summary>
    public enum XRFoveationLevel
    {
        /// <summary>
        /// Corresponds to XR_FOVEATION_LEVEL_NONE_FB - No foveation.
        /// </summary>
        None = 0,

        /// <summary>
        /// Corresponds to XR_FOVEATION_LEVEL_LOW_FB - Less foveation
        /// (higher periphery visual fidelity, lower performance).
        /// </summary>
        Low = 1,

        /// <summary>
        /// Corresponds to XR_FOVEATION_LEVEL_MEDIUM_FB - Medium foveation
        /// (medium periphery visual fidelity, medium performance).
        /// </summary>
        Medium = 2,

        /// <summary>
        /// Corresponds to XR_FOVEATION_LEVEL_HIGH_FB - High foveation
        /// (lower periphery visual fidelity, higher performance).
        /// </summary>
        High = 3,
    }
}
