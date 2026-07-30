// <copyright file="XRGeospatialPoseResult.cs" company="Google LLC">
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
    using System.Runtime.InteropServices;
    using UnityEngine;
    using UnityEngine.XR.OpenXR.NativeTypes;

    /// <summary>
    /// Represents the result when converting a pose in Unity world space into
    /// <see cref="XRGeospatialPose"/>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct XRGeospatialPoseResult : IEquatable<XRGeospatialPoseResult>
    {
        /// <summary>
        /// Indicates the valid fields of the result.
        /// <see cref="XRGeospatialPose.Rotation"/> and
        /// <see cref="OrientationYawAccuracy"/> are valid When
        /// <see cref="XrSpaceLocationFlags.OrientationValid"/> is set.
        /// Other fields are determined by
        /// <see cref="XrSpaceLocationFlags.PositionValid"/>.
        /// </summary>
        public XrSpaceLocationFlags LocationFlags;

        /// <summary>
        /// The geospatial pose converted from the given Unity pose.
        /// </summary>
        public XRGeospatialPose Pose;

        /// <summary>
        /// The estimated horizontal accuracy of the Geospatial pose position,
        /// defined as the radius in meters of the circle of 68% confidence
        /// level around the given latitude and longitude.
        /// </summary>
        public double HorizontalAccuracy;

        /// <summary>
        /// The estimated vertical accuracy of the Geospatial pose position,
        /// defined as the distance in meters of 68% confidence level around
        /// the given altitude.
        /// In other words, there is a 68% chance that the true altitude is in
        /// range [Altitude - VerticalAccuracy, Altitude + VerticalAccuracy].
        /// </summary>
        public double VerticalAccuracy;

        /// <summary>
        /// The estimated yaw accuracy of the Geospatial pose orientation,
        /// defined as the radius in degrees of 68% confidence level around
        /// the given orientation.
        /// In other words, there is a 68% chance that the true orientation
        /// yaw angle is within this many degrees of the given yaw angle.
        /// </summary>
        public double OrientationYawAccuracy;

        /// <summary>
        /// Gets a value indicating whether the position is valid.
        /// </summary>
        /// <returns>True if valid.</returns>
        public bool IsPositionValid =>
            LocationFlags.HasFlag(XrSpaceLocationFlags.PositionValid);

        /// <summary>
        /// Gets a value indicating whether the rotation is valid.
        /// </summary>
        /// <returns>True if valid.</returns>
        public bool IsRotationValid =>
            LocationFlags.HasFlag(XrSpaceLocationFlags.OrientationValid) &&
            OrientationYawAccuracy > 0;

        /// <inheritdoc/>
        public bool Equals(XRGeospatialPoseResult other)
        {
            return Pose.Equals(other.Pose) &&
                LocationFlags == other.LocationFlags &&
                HorizontalAccuracy == other.HorizontalAccuracy &&
                VerticalAccuracy == other.VerticalAccuracy &&
                OrientationYawAccuracy == other.OrientationYawAccuracy;
        }
    }

    /// <summary>
    /// Represents the result when converting <see cref="XRGeospatialPose"/>
    /// into a pose in Unity world space.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct XRPoseResult : IEquatable<XRPoseResult>
    {
        /// <summary>
        /// Indicates the valid fields of <see cref="Pose"/>.
        /// </summary>
        public XrSpaceLocationFlags LocationFlags;

        /// <summary>
        /// The pose in Unity world space converted from the geospatial pose.
        /// </summary>
        public Pose Pose;

        /// <summary>
        /// Gets a value indicating whether the position is valid.
        /// </summary>
        /// <returns>True if valid.</returns>
        public bool IsPositionValid =>
            LocationFlags.HasFlag(XrSpaceLocationFlags.PositionValid);

        /// <summary>
        /// Gets a value indicating whether the position is tracked.
        /// </summary>
        /// <returns>True if tracked.</returns>
        public bool IsPositionTracked =>
            LocationFlags.HasFlag(XrSpaceLocationFlags.PositionTracked);

        /// <summary>
        /// Gets a value indicating whether the rotation is valid.
        /// </summary>
        /// <returns>True if valid.</returns>
        public bool IsRotationValid =>
            LocationFlags.HasFlag(XrSpaceLocationFlags.OrientationValid);

        /// <summary>
        /// Gets a value indicating whether the rotation is tracked.
        /// </summary>
        /// <returns>True if tracked.</returns>
        public bool IsRotationTracked =>
            LocationFlags.HasFlag(XrSpaceLocationFlags.OrientationTracked);

        /// <inheritdoc/>
        public bool Equals(XRPoseResult other)
        {
            return Pose == other.Pose && LocationFlags == other.LocationFlags;
        }
    }
}
