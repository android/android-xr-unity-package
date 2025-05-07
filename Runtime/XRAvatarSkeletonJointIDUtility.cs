// <copyright file="XRAvatarSkeletonJointIDUtility.cs" company="Google LLC">
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
    using UnityEngine;

    /// <summary>
    /// Utility class for <see cref="XRAvatarSkeletonJointID"/>.
    /// </summary>
    public static class XRAvatarSkeletonJointIDUtility
    {
        /// <summary>
        /// Converts <see cref="XRAvatarSkeletonJointID"/> to its corresponding index into
        /// an array of joint data.
        /// </summary>
        /// <param name="jointId">ID of the joint to convert to an index.</param>
        /// <returns>
        /// The index matching the ID passed in.
        /// </returns>
        public static int ToIndex(this XRAvatarSkeletonJointID jointId) => (int)jointId - 1;

        /// <summary>
        /// Gets the corresponding <see cref="XRAvatarSkeletonJointID"/> from an index into
        /// an array of associated data.
        /// </summary>
        /// <param name="index">Index to convert to an ID.</param>
        /// <returns>
        /// The ID matching the index passed in.
        /// </returns>
        public static XRAvatarSkeletonJointID FromIndex(int index) =>
            (XRAvatarSkeletonJointID)(index + 1);

        /// <summary>
        /// Gets the count of joints provided by an avatar skeleton.
        /// </summary>
        /// <returns>The amount of joints of an avatar skeleton.</returns>
        public static int JointCount() => XRAvatarSkeletonJointID.EndMarker.ToIndex();
    }
}
