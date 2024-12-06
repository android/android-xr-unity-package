// <copyright file="XRPassthroughCameraStates.cs" company="Google LLC">
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
    /// Defines possible passthrough camera states.
    /// </summary>
    [Flags]
    public enum XRPassthroughCameraStates
    {
        /// <summary> The camera has been disabled by an app, the system or the user. </summary>
        Disabled = 0,

        /// <summary> The camera is still coming online and not yet ready to use. </summary>
        Initializing = 1,

        /// <summary>
        /// The camera is ready to use.
        /// </summary>
        Ready = 2,

        /// <summary>
        /// The camera is in an unrecoverable error state.
        /// </summary>
        Error = 3,
    }
}
