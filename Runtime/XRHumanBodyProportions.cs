// <copyright file="XRHumanBodyProportions.cs" company="Google LLC">
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
    using System.Runtime.InteropServices;
    using UnityEngine;

    /// <summary>
    /// Defines the human body proportions to be used for computing the rest pose skeleton.
    /// </summary>
    [CreateAssetMenu(
        fileName = "HumanBodyProportions", menuName = "XR/Human Body Proportions", order = 1)]
    [StructLayout(LayoutKind.Sequential)]
    public class XRHumanBodyProportions : ScriptableObject
    {
        /// <summary>
        /// Distance from the head joint to the toes in the vertical axis.
        /// </summary>
        [Tooltip("Distance from the head joint to the toes in the vertical axis.")]
        [Min(0f)]
        public float HeadHeight;

        /// <summary>
        /// Distance between the two upper leg joints.
        /// </summary>
        [Tooltip("Distance between the two upper leg joints.")]
        [Min(0f)]
        public float HipsWidth;

        /// <summary>
        /// Distance between the spine joint and the hips joint in the vertical axis.
        /// </summary>
        [Tooltip("Distance between the spine joint and the hips joint in the vertical axis.")]
        [Min(0f)]
        public float HipsLength;

        /// <summary>
        /// Distance between the neck joint and the spine joint in the vertical axis.
        /// </summary>
        [Tooltip("Distance between the neck joint and the spine joint in the vertical axis.")]
        [Min(0f)]
        public float TorsoLength;

        /// <summary>
        /// Distance between the neck joint and the spine joint in the vertical axis.
        /// </summary>
        [Tooltip("Distance between the neck joint and the spine joint in the vertical axis.")]
        [Min(0f)]
        public float NeckLength;

        /// <summary>
        /// Distance between the two upper arm joints.
        /// </summary>
        [Tooltip("Distance between the two upper arm joints.")]
        [Min(0f)]
        public float ShoulderWidth;

        /// <summary>
        /// Average distance between the upper arm and the lower arm joints.
        /// </summary>
        [Tooltip("Average distance between the upper arm and the lower arm joints.")]
        [Min(0f)]
        public float UpperArmLength;

        /// <summary>
        /// Average distance between the lower arm and the hand joints.
        /// </summary>
        [Tooltip("Average distance between the lower arm and the hand joints.")]
        [Min(0f)]
        public float LowerArmLength;

        /// <summary>
        /// Average distance between the upper leg and the lower leg joints.
        /// </summary>
        [Tooltip("Average distance between the upper leg and the lower leg joints.")]
        [Min(0f)]
        public float UpperLegLength;

        /// <summary>
        /// Average distance between the lower leg and the foot joints.
        /// </summary>
        [Tooltip("Average distance between the lower leg and the foot joints.")]
        [Min(0f)]
        public float LowerLegLength;

        /// <summary>
        /// Average distance between the foot and the toes in the lower leg axis.
        /// </summary>
        [Tooltip("Average distance between the foot and the toes in the lower leg axis.")]
        [Min(0f)]
        public float AnkleHeight;

        /// <summary>
        /// Average distance between the foot and the toes in the lower leg plane.
        /// </summary>
        [Tooltip("Average distance between the foot and the toes in the lower leg plane.")]
        [Min(0f)]
        public float FootLength;

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <inheritdoc/>
        public override bool Equals(object other)
        {
            XRHumanBodyProportions otherProportions = other as XRHumanBodyProportions;
            if (otherProportions == null)
            {
                return false;
            }

            return HeadHeight == otherProportions.HeadHeight &&
                HipsWidth == otherProportions.HipsWidth &&
                HipsLength == otherProportions.HipsLength &&
                TorsoLength == otherProportions.TorsoLength &&
                NeckLength == otherProportions.NeckLength &&
                ShoulderWidth == otherProportions.ShoulderWidth &&
                UpperArmLength == otherProportions.UpperArmLength &&
                LowerArmLength == otherProportions.LowerArmLength &&
                UpperLegLength == otherProportions.UpperLegLength &&
                LowerLegLength == otherProportions.LowerLegLength &&
                AnkleHeight == otherProportions.AnkleHeight &&
                FootLength == otherProportions.FootLength;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format(
                "{0}: HeadHeight={1:F2}, HipsWidth={2:F2}, HipsLength={3:F3}, " +
                "TorsoLength={4:F2}, NeckLength:{5:F2}, ShoulderWidth={6:F2}, " +
                "UpperArmLength={7:F2}, LowerArmLength={8:F2}, UpperLegLength={9:F2}, " +
                "LowerLegLength={10:F2}, AnkleHeight={11:F2}, FootLength={12:F2}",
                name, HeadHeight, HipsWidth, HipsLength,
                TorsoLength, NeckLength, ShoulderWidth,
                UpperArmLength, LowerArmLength, UpperLegLength,
                LowerLegLength, AnkleHeight, FootLength);
        }
    }
}
