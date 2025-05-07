// <copyright file="XRMarkerDictionary.cs" company="Google LLC">
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
    /// Describes the type of Supported marker dictionaries.
    /// </summary>
    public enum XRMarkerDictionary
    {
        /// <summary>
        /// The predefined dictionary in the ArUco module,
        /// composed of 50 markers with size of 4x4 bits.
        /// </summary>
        ArUco4x4_50 = 0,

        /// <summary>
        /// The predefined dictionary in the ArUco module,
        /// composed of 100 markers with size of 4x4 bits.
        /// </summary>
        ArUco4x4_100 = 1,

        /// <summary>
        /// The predefined dictionary in the ArUco module,
        /// composed of 250 markers with size of 4x4 bits.
        /// </summary>
        ArUco4x4_250 = 2,

        /// <summary>
        /// The predefined dictionary in the ArUco module,
        /// composed of 1000 markers with size of 4x4 bits.
        /// </summary>
        ArUco4x4_1000 = 3,

        /// <summary>
        /// The predefined dictionary in the ArUco module,
        /// composed of 50 markers with size of 5x5 bits.
        /// </summary>
        ArUco5x5_50 = 4,

        /// The predefined dictionary in the ArUco module,
        /// composed of 100 markers with size of 5x5 bits.
        /// </summary>
        ArUco5x5_100 = 5,

        /// The predefined dictionary in the ArUco module,
        /// composed of 250 markers with size of 5x5 bits.
        /// </summary>
        ArUco5x5_250 = 6,

        /// The predefined dictionary in the ArUco module,
        /// composed of 1000 markers with size of 5x5 bits.
        /// </summary>
        ArUco5x5_1000 = 7,

        /// <summary>
        /// The predefined dictionary in the ArUco module,
        /// composed of 50 markers with size of 6x6 bits.
        /// </summary>
        ArUco6x6_50 = 8,

        /// <summary>
        /// The predefined dictionary in the ArUco module,
        /// composed of 100 markers with size of 6x6 bits.
        /// </summary>
        ArUco6x6_100 = 9,

        /// <summary>
        /// The predefined dictionary in the ArUco module,
        /// composed of 250 markers with size of 6x6 bits.
        /// </summary>
        ArUco6x6_250 = 10,

        /// <summary>
        /// The predefined dictionary in the ArUco module,
        /// composed of 1000 markers with size of 6x6 bits.
        /// </summary>
        ArUco6x6_1000 = 11,

        /// <summary>
        /// The predefined dictionary in the ArUco module,
        /// composed of 50 markers with size of 7x7 bits.
        /// </summary>
        ArUco7X7_50 = 12,

        /// <summary>
        /// The predefined dictionary in the ArUco module,
        /// composed of 100 markers with size of 7x7 bits.
        /// </summary>
        ArUco7X7_100 = 13,

        /// <summary>
        /// The predefined dictionary in the ArUco module,
        /// composed of 250 markers with size of 7x7 bits.
        /// </summary>
        ArUco7X7_250 = 14,

        /// <summary>
        /// The predefined dictionary in the ArUco module,
        /// composed of 1000 markers with size of 7x7 bits.
        /// </summary>
        ArUco7X7_1000 = 15,

        /// <summary>
        /// The predefined dictionary of AprilTag family 16H5.
        /// </summary>
        AprilTag_16H5 = 16,

        /// <summary>
        /// The predefined dictionary of AprilTag family 25H9.
        /// </summary>
        AprilTag_25H9 = 17,

        /// <summary>
        /// The predefined dictionary of AprilTag family 36H10.
        /// </summary>
        AprilTag_36H10 = 18,

        /// <summary>
        /// The predefined dictionary of AprilTag family 36H11.
        /// </summary>
        AprilTag_36H11 = 19,
    }
}
