// <copyright file="XRGeospatialState.cs" company="Google LLC">
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

namespace Google.XR.Extensions
{
    using UnityEngine.XR.ARSubsystems;
    using UnityEngine.XR.OpenXR.NativeTypes;

    /// <summary>
    /// Represents the current state of geospatial services.
    /// </summary>
    public enum XRGeospatialState
    {
        /// <summary>
        /// Geospatial tracker has not been initialized and availability is
        /// unknown.
        /// It requires <see cref="XRGeospatialFeature"/> to be enabled to
        /// monitor tracking state.
        /// </summary>
        None = 0,

        /// <summary>
        /// Geospatial is unsupported by the system.
        /// Assessing geospatial APIs will return
        /// <see cref="XRResultStatus.StatusCode.Unsupported"/>.
        /// </summary>
        Unsupported,

        /// <summary>
        /// Geospatial is supported by the system but it's disabled by the
        /// current session.
        /// Accessing geospatial APIs will return
        /// <see cref="XRResultStatus.StatusCode.ProviderUninitialized"/> error.
        /// </summary>
        Disabled,

        /// <summary>
        /// Geospatial is supported by the system and it's enabled by the
        /// current session.
        /// This state will automatically change to <c>InitializationFailed</c>
        /// or <c>Running</c> based on its initialization result.
        /// </summary>
        Initializing,

        /// <summary>
        /// The geospatial tracker failed on initialization, and will not be
        /// usable.
        /// Use <see cref="XRGeospatialManager.InitializationResult"/> to
        /// understand the failure reasons.
        /// </summary>
        InitializationFailed,

        /// <summary>
        /// The geospatial tracker succeed on initialization, and it is running
        /// and usable.
        /// </summary>
        Running,

        /// <summary>
        /// The geospatial tracker stopped running.
        /// All previously created anchors have permanently stopped tracking,
        /// and should be destroyed. The application must not attempt to
        /// use the tracker in this state.
        /// Instead, try to restart <see cref="XRGeospatialManager"/> after
        /// resolving the issues.
        /// </summary>
        Stopped,
    }
}
