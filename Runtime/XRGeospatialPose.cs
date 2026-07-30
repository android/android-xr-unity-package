// <copyright file="XRGeospatialPose.cs" company="Google LLC">
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

    /// <summary>
    /// Represents a position and orientation relative to the Earth.
    /// Coordinates are relative to the
    /// <see href="https://en.wikipedia.org/wiki/World_Geodetic_System">WGS84 Ellipsoid</see>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct XRGeospatialPose : IEquatable<XRGeospatialPose>
    {
        /// <summary>
        /// The orientation with respect to a coordinate system where
        /// +X=East, +Y=Up and +Z=South.
        /// Note: this coordinate system depends upon the precise position.
        /// </summary>
        public Quaternion Rotation;

        /// <summary>
        /// The latitude in degrees, between -90 and +90.
        /// </summary>
        public double Latitude;

        /// <summary>
        /// The longitude in degrees, between -180 and 180.
        /// </summary>
        public double Longitude;

        /// <summary>
        /// The altitude in meters.
        /// </summary>
        public double Altitude;

        /// <summary>
        /// Constructs a geospatial pose with data.
        /// </summary>
        /// <param name="rotation">The orientation of the pose.</param>
        /// <param name="latitude">The latitude in degrees.</param>
        /// <param name="longitude">The longitude in degrees.</param>
        /// <param name="altitude">The altitude in meters.</param>
        public XRGeospatialPose(
            Quaternion rotation, double latitude, double longitude, double altitude)
        {
            Rotation = rotation;
            Latitude = latitude;
            Longitude = longitude;
            Altitude = altitude;
        }

        /// <summary>
        /// Constructs a geospatial pose from coordinates with 0 altitude.
        /// </summary>
        /// <param name="latitude">The latitude in degrees.</param>
        /// <param name="longitude">The longitude in degrees.</param>
        public XRGeospatialPose(double latitude, double longitude) :
            this(Quaternion.identity, latitude, longitude, 0)
        {
        }

        /// <summary>
        /// Constructs a geospatial pose from location info.
        /// </summary>
        /// <param name="locationInfo">
        /// The <see cref="LocationInfo"/> with GPS data.
        /// </param>
        public XRGeospatialPose(LocationInfo locationInfo)
        {
            Rotation = Quaternion.identity;
            Latitude = locationInfo.latitude;
            Longitude = locationInfo.longitude;
            Altitude = locationInfo.altitude;
        }

        /// <summary>
        /// Gets a value indicating whether the geospatial pose has a valid
        /// coordinate. Used to avoid
        /// <see cref="XrResultAndroid.GeospatialCoordinateInvalid"/> failure.
        /// </summary>
        /// <returns>True if it's valid.</returns>
        public bool IsCoordinateValid =>
            Latitude >= -90D && Latitude <= 90D &&
            Longitude >= -180D && Longitude <= 180D;

        /// <inheritdoc/>
        public bool Equals(XRGeospatialPose other)
        {
            return Rotation == other.Rotation && Latitude == other.Latitude &&
                Longitude == other.Longitude && Altitude == other.Altitude;
        }

        /// <summary>
        /// Gets a formatted string.
        /// </summary>
        /// <param name="format">The format of floating point numbers.</param>
        /// <returns>The formatted string.</returns>
        public string ToString(string format)
        {
            return $"(Rot={Rotation.ToString(format)}," +
                   $"Lat={Latitude.ToString(format)}," +
                   $"Lng={Longitude.ToString(format)}," +
                   $"Alt={Altitude.ToString(format)})";
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"(Rot={Rotation}," +
                   $"Lat={Latitude}," +
                   $"Lng={Longitude}," +
                   $"Alt={Altitude})";
        }
    }
}
