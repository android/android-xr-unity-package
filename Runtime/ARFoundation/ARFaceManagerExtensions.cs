// <copyright file="ARFaceManagerExtensions.cs" company="Google LLC">
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

#if UNITY_OPEN_XR_ANDROID_XR
namespace Google.XR.Extensions
{
    using Google.XR.Extensions.Internal;
    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.OpenXR.Features.Android;

    /// <summary>
    /// Extensions to AR Foundation's <c><see cref="ARFaceManager"/></c>.
    /// </summary>
    public static class ARFaceManagerExtensions
    {
        /// <summary>
        /// Extension method to <see cref="ARFaceManager"/> that attempts to retrieve current face
        /// tracking modality.
        /// </summary>
        /// <param name="faceManager"> <see cref="ARFaceManager"/> to get the modality from.</param>
        /// <param name="faceTrackerMode">
        /// <see cref="XRFaceTrackerMode"/> to fill out with up-to-date data,
        /// if the method returns <see langword="true"/>.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if successful and faceTrackerMode was filled
        /// out with up-to-date data, returns <see langword="false"/> otherwise.
        /// </returns>
        public static bool TryGetFaceTrackerMode(
            this ARFaceManager faceManager, out XRFaceTrackerMode faceTrackerMode)
        {
            faceTrackerMode = XRFaceTrackerMode.Unknown;
            if (faceManager.subsystem != null && faceManager.subsystem.running) {
                return XRMultimodalFaceTrackerApi.GetTrackerMode(ref faceTrackerMode);
            }
            return false;
        }
    }
}
#endif // UNITY_OPEN_XR_ANDROID_XR
