// <copyright file="ARFaceExtensions.cs" company="Google LLC">
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

#if UNITY_OPEN_XR_ANDROID_XR
namespace Google.XR.Extensions
{
    using Google.XR.Extensions.Internal;
    using UnityEngine;
    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.OpenXR.Features.Android;

    /// <summary>
    /// Extensions to AR Foundation's <c><see cref="ARFace"/></c>.
    /// </summary>
    public static class ARFaceExtensions
    {
        /// <summary>
        /// Extension method to <see cref="ARFace"/> that attempts to retrieve fine eye poses.
        /// </summary>
        /// <param name="face"> <see cref="ARFace"/> to get the fine eye poses.</param>
        /// <param name="states">
        /// <see cref="AndroidOpenXREyeTrackingStates"/> to fill out with up-to-date data,
        /// if the method returns <see langword="true"/>.
        /// </param>
        /// <param name="left">
        /// <see cref="Pose"/> array to fill out with left fine eye pose,
        /// if <paramref name="states"/> contains <c>LeftEyePoseValid</c>.
        /// </param>
        /// <param name="right">
        /// <see cref="Pose"/> array to fill out with right fine eye pose,
        /// if <paramref name="states"/> contains <c>RightEyePoseValid</c>.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if successful and state was filled
        /// out with up-to-date data, returns <see langword="false"/> otherwise.
        /// </returns>
        public static bool TryGetFineEyePoses(
            this ARFace face, out AndroidOpenXREyeTrackingStates states,
            out Pose left, out Pose right)
        {
            states = AndroidOpenXREyeTrackingStates.None;
            left = Pose.identity;
            right = Pose.identity;
            if (face.trackingState >= UnityEngine.XR.ARSubsystems.TrackingState.Limited)
            {
                ApiEyeStates apiStates = XREyeTrackingApi.GetFineEye(ref left, ref right);
                states = apiStates.ToFaceStates();
                return true;
            }
            else
            {
                Debug.LogWarningFormat("Face {0} has invalid tracking state: {1}.",
                    face.trackableId, face.trackingState);
                return false;
            }
        }
    }
}
#endif // UNITY_OPEN_XR_ANDROID_XR
