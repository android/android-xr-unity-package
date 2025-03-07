// <copyright file="XRAnchorPersistStates.cs" company="Google LLC">
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
    /// Defines possible anchor persistence states.
    /// </summary>
    [Flags]
    public enum XRAnchorPersistStates
    {
        /// <summary> The anchor has not been submitted for persistence. </summary>
        NotRequested = 0,

        /// <summary> The anchor is pending for persistence. </summary>
        Pending = 1,

        /// <summary> The anchor has been persisted.  </summary>
        Persisted = 2,
    }
}
