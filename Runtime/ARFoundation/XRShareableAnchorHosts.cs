// <copyright file="XRShareableAnchorHosts.cs" company="Google LLC">
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
    using System;
    using UnityEngine.XR.ARSubsystems;

    /// <summary>
    /// Identifies the host where the shareable anchor has been stored.
    /// </summary>
    [Flags]
    public enum XRShareableAnchorHosts : int
    {
        /// <summary>
        /// No host, the equivalence of <see cref="XRAnchor"/>.
        /// </summary>
        None = 0,

        /// <summary>
        /// The anchor has been stored on the local storage, available to be
        /// loaded from the same device.
        /// </summary>
        Local = 1 << 0,

        /// <summary>
        /// The anchor is created by <see cref="XRGeospatialPose"/>.
        /// </summary>
        Geospatial = 1 << 3,
    }
}
