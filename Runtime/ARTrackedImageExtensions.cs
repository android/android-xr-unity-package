// <copyright file="ARTrackedImageExtensions.cs" company="Google LLC">
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
    using System;
    using Google.XR.Extensions.Internal;
    using UnityEngine;
    using UnityEngine.XR.ARFoundation;

    /// <summary>
    /// Extensions to AR Foundation's <c><see cref="ARTrackedImage"/></c> class.
    /// </summary>
    public static class ARTrackedImageExtensions
    {
        /// <summary>
        /// Check if the give image is a QR code.
        /// </summary>
        /// <param name="image">The <see cref="ARTrackedImage"/> instance.</param>
        /// <returns><c>true</c>, if it's a valid QR code image.</returns>
        public static bool IsQrCode(this ARTrackedImage image)
        {
            return XRTrackableApi.IsQrCode(image.trackableId);
        }

        /// <summary>
        /// Try to get the decoded data from a QR code image.
        /// </summary>
        /// <param name="image">
        /// The QR code <see cref="ARTrackedImage"/> image instance which returns <c>true</c>
        /// from <see cref="IsQrCode(ARTrackedImage)"/>.
        /// </param>
        /// <param name="decodedData">The decoded QR code data.</param>
        /// <returns><c>true</c>, if it succeed to get QR Code data and
        /// <paramref name="decodedData"/> contains valid data.</returns>
        /// <remarks>
        /// It can fail to get the data if the tracking server is still decoding or there is not
        /// data encoded with this QR code. You can try it again later if decoding finished.
        /// </remarks>
        public static bool TryGetQrCodeData(this ARTrackedImage image, out string decodedData)
        {
            if (!IsQrCode(image) || XRQrCodeTrackingFeature._subsystemInstance == null)
            {
                decodedData = null;
                return false;
            }

            return XRQrCodeTrackingFeature._subsystemInstance.TryGetQrCodeData(
                image.trackableId, out decodedData);
        }

        /// <summary>
        /// Check if the given image is a marker.
        /// </summary>
        /// <param name="image">The <see cref="ARTrackedImage"/> instance.</param>
        /// <returns><c>true</c>, if it's a valid marker image.</returns>
        public static bool IsMarker(this ARTrackedImage image)
        {
            return XRTrackableApi.IsMarker(image.trackableId);
        }

        /// <summary>
        /// Try to get the <see cref="XRMarkerDictionary"/> and the id from the dictionary of a
        /// marker image.
        /// </summary>
        /// <param name="image">
        /// The marker <see cref="ARTrackedImage"/> image instance which returns <c>true</c>
        /// from <see cref="IsMarker(ARTrackedImage)"/>.</param>
        /// <param name="dictionary">The <see cref="XRMarkerDictionary"/> it belongs to.</param>
        /// <param name="id">The id from the <paramref name="dictionary"/>.</param>
        /// <returns><c>true</c> if it gets marker data successfully and output by
        /// <paramref name="dictionary"/> and <paramref name="id"/>.</returns>
        public static bool TryGetMarkerData(
            this ARTrackedImage image, out XRMarkerDictionary dictionary, out int id)
        {
            if (!IsMarker(image) || XRMarkerTrackingFeature._subsystemInstance == null)
            {
                dictionary = XRMarkerDictionary.ArUco4x4_50;
                id = -1;
                return false;
            }

            return XRMarkerTrackingFeature._subsystemInstance.TryGetMarkeData(
                image.trackableId, out dictionary, out id);
        }
    }
}
