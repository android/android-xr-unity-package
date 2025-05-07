// <copyright file="XRAvatarSkeletonJointID.cs" company="Google LLC">
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
    /// <summary>
    /// Represents the type of an skeleton joint. If you wish to convert it to an index,
    /// Use <see cref="XRAvatarSkeletonJointIDUtility.ToIndex(XRAvatarSkeletonJointID)"/>
    /// on the joint ID.
    /// </summary>
    public enum XRAvatarSkeletonJointID
    {
        /// <summary>
        /// Invalid ID.
        /// </summary>
        Invalid = 0,

        /// <summary>
        /// Marks the beginning of joints, or start of an array of data related to them.
        /// Casting this to an integer type will not result in a correct start. Use
        /// <see cref="XRAvatarSkeletonJointIDUtility.ToIndex(XRAvatarSkeletonJointID)"/> instead.
        /// </summary>
        BeginMarker = 1,

        /// <summary>
        /// Joint for hips.
        /// </summary>
        Hips = BeginMarker,

        /// <summary>
        /// Joint for the spine.
        /// </summary>
        Spine,

        /// <summary>
        /// Joint for ribs.
        /// </summary>
        Ribs,

        /// <summary>
        /// Joint for the chest.
        /// </summary>
        Chest,

        /// <summary>
        /// Joint for the neck.
        /// </summary>
        Neck,

        /// <summary>
        /// Joint for the head.
        /// </summary>
        Head,

        /// <summary>
        /// Joint for the left shoulder.
        /// </summary>
        LeftShoulder,

        /// <summary>
        /// Joint for the right shoulder.
        /// </summary>
        RightShoulder,

        /// <summary>
        /// Joint for the left upper arm.
        /// </summary>
        LeftUpperArm,

        /// <summary>
        /// Joint for the right upper arm.
        /// </summary>
        RightUpperArm,

        /// <summary>
        /// Joint for the left lower arm.
        /// </summary>
        LeftLowerArm,

        /// <summary>
        /// Joint for the right lower arm.
        /// </summary>
        RightLowerArm,

        /// <summary>
        /// Joint for the left hand.
        /// </summary>
        LeftHand,

        /// <summary>
        /// Joint for the right hand.
        /// </summary>
        RightHand,

        /// <summary>
        /// Joint for the left upper leg.
        /// </summary>
        LeftUpperLeg,

        /// <summary>
        /// Joint for the right upper leg.
        /// </summary>
        RightUpperLeg,

        /// <summary>
        /// Joint for the left lower leg.
        /// </summary>
        LeftLowerLeg,

        /// <summary>
        /// Joint for the right lower leg.
        /// </summary>
        RightLowerLeg,

        /// <summary>
        /// Joint for the left foot.
        /// </summary>
        LeftFoot,

        /// <summary>
        /// Joint for the right foot.
        /// </summary>
        RightFoot,

        /// <summary>
        /// Joint for left toes.
        /// </summary>
        LeftToes,

        /// <summary>
        /// Joint for right toes.
        /// </summary>
        RightToes,

        /// <summary>
        /// Marks the end of joints, or size of an array of data related to them.
        /// Casting this to an integer type will not result in a correct count. Use
        /// <see cref="XRAvatarSkeletonJointIDUtility.ToIndex(XRAvatarSkeletonJointID)"/> instead.
        /// </summary>
        EndMarker,
    }
}
