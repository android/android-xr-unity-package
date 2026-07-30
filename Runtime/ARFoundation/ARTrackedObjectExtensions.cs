// <copyright file="ARTrackedObjectExtensions.cs" company="Google LLC">
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
    using Google.XR.Extensions.Internal;
    using UnityEngine;
    using UnityEngine.XR.ARFoundation;

    /// <summary>
    /// Extensions to AR Foundation's <c><see cref="ARTrackedObject"/></c> class.
    /// </summary>
    public static class ARTrackedObjectExtensions
    {
        /// <summary>
        /// Gets the 3D extents of the object.
        /// </summary>
        /// <param name="trackedObject">The tracked object.</param>
        /// <returns>The 3D extents of the object.</returns>
        public static Vector3 GetExtents(this ARTrackedObject trackedObject)
        {
            return XRTrackableApi.GetObjectExtents(trackedObject.trackableId);
        }

        /// <summary>
        /// Gets the type of object that the system has identified.
        /// </summary>
        /// <param name="trackedObject">The tracked object.</param>
        /// <returns>The object label identified by the system.</returns>
        public static XRObjectLabel GetObjectLabel(this ARTrackedObject trackedObject)
        {
            return XRTrackableApi.GetObjectLabel(trackedObject.trackableId);
        }
    }
}
