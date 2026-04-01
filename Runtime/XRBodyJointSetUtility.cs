// <copyright file="XRBodyJointSetUtility.cs" company="Google LLC">
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
    using System.Linq;

    /// <summary>
    /// Utility class for <see cref="XRBodyJointSet"/> and individual joint set IDs.
    /// </summary>
    public static class XRBodyJointSetUtility
    {
        /// <summary>
        /// Gets the joint set type enum of the given joint set.
        /// </summary>
        /// <param name="jointSet">The target joint set.</param>
        /// <returns>
        /// If it's a valid <see cref="XRBodyJointSet">, returns the enum type representing the
        /// joint set IDs. Otherwise, returns <c>null</c>.</returns>
        public static Type GetJointSetType(this XRBodyJointSet jointSet)
        {
            switch (jointSet)
            {
                case XRBodyJointSet.UpperBody:
                    return typeof(XRUpperBodyJointID);
                case XRBodyJointSet.FullBody:
                    return typeof(XRFullBodyJointID);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Gets the count of joints of the target joint set.
        /// </summary>
        /// <param name="jointSet">The target joint set type provided by
        /// <see cref="GetJointSetType(XRBodyJointSet)"/> </param>
        /// <returns>The amount of joints of the given joint set.</returns>
        public static int JointCount(Type jointSet)
        {
            if (jointSet == typeof(XRUpperBodyJointID))
            {
                return XRUpperBodyJointID.EndMarker.ToIndex();
            }
            else if (jointSet == typeof(XRFullBodyJointID))
            {
                return XRFullBodyJointID.EndMarker.ToIndex();
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Converts <see cref="XRUpperBodyJointID"/> to its corresponding index into
        /// an array of joint data.
        /// </summary>
        /// <param name="jointId">ID of the joint to convert to an index.</param>
        /// <returns>
        /// The index matching the ID passed in.
        /// </returns>
        public static int ToIndex(this XRUpperBodyJointID jointId) => (int)jointId - 1;

        /// <summary>
        /// Converts <see cref="XRFullBodyJointID"/> to its corresponding index into
        /// an array of joint data.
        /// </summary>
        /// <param name="jointId">ID of the joint to convert to an index.</param>
        /// <returns>
        /// The index matching the ID passed in.
        /// </returns>
        public static int ToIndex(this XRFullBodyJointID jointId) => (int)jointId - 1;

        /// <summary>
        /// Gets the corresponding <see cref="XRUpperBodyJointID"/> from an index into
        /// an array of associated data.
        /// </summary>
        /// <param name="index">Index to convert to an ID.</param>
        /// <param name="jointId">
        /// If it's a valid index, output the ID matching the index passed in.
        /// Otherwise, set to <see cref="XRUpperBodyJointID.Invalid"/>.
        /// </param>
        public static void FromIndex(int index, out XRUpperBodyJointID jointId)
        {
            int jointValue = index + 1;
            if (jointValue >= (int)XRUpperBodyJointID.BeginMarker &&
                jointValue <= (int)XRUpperBodyJointID.EndMarker)
            {
                jointId = (XRUpperBodyJointID)jointValue;
            }
            else
            {
                jointId = XRUpperBodyJointID.Invalid;
            }
        }

        /// <summary>
        /// Gets the corresponding <see cref="XRFullBodyJointID"/> from an index into
        /// an array of associated data.
        /// </summary>
        /// <param name="index">Index to convert to an ID.</param>
        /// <param name="jointId">
        /// If it's a valid index, output the ID matching the index passed in.
        /// Otherwise, set to <see cref="XRFullBodyJointID.Invalid"/>.
        /// </param>
        public static void FromIndex(int index, out XRFullBodyJointID jointId)
        {
            int jointValue = index + 1;
            if (jointValue >= (int)XRFullBodyJointID.BeginMarker &&
                jointValue <= (int)XRFullBodyJointID.EndMarker)
            {
                jointId = (XRFullBodyJointID)jointValue;
            }
            else
            {
                jointId = XRFullBodyJointID.Invalid;
            }
        }
    }
}
