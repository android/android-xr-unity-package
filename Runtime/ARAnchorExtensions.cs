// <copyright file="ARAnchorExtensions.cs" company="Google LLC">
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
    using UnityEngine.XR.ARFoundation;

    /// <summary>
    /// Extensions to AR Foundation's <c><see cref="ARAnchor"/></c> class.
    /// </summary>
    public static class ARAnchorExtensions
    {
        /// <summary>
        /// Gets the persistent id associated with the given anchor. It can be used in
        /// <see cref="ARAnchorManagerExtensions.TryLoad(ARAnchorManager, Guid)"/> to retrieve the
        /// anchor across multiple sessions.
        /// If it's not a persisted anchor returns <c>Guid.Empty</c>.
        /// </summary>
        /// <param name="anchor">The <see cref="ARAnchor"/> instance.</param>
        /// <returns>Returns the persistent id associated with the anchor.</returns>
        public static Guid GetPersistentId(this ARAnchor anchor)
        {
            if (XRAnchorFeature._subsystemInstance == null)
            {
                return Guid.Empty;
            }

            return XRAnchorFeature._subsystemInstance.GetPersistentId(anchor.trackableId);
        }
    }
}
