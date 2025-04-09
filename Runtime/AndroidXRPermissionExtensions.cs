// <copyright file="AndroidXRPermissionExtensions.cs" company="Google LLC">
//
// Copyright 2024 Google LLC
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
    /// Permissions required by Android XR tracking services.
    /// </summary>
    public enum AndroidXRPermission
    {
        /// <summary>
        /// Permission to enable coarse scene understanding features that relies on motion tracking,
        /// ToF sensor, and the VST RGB-left cameras.
        /// </summary>
        SceneUnderstandingCoarse,

        /// <summary>
        /// Permission to enable fine scene understanding features that relies on motion tracking,
        /// such as depth texture and scene meshing.
        /// </summary>
        SceneUnderstandingFine,

        /// <summary>
        /// Permission to enable hand tracking.
        /// </summary>
        HandTracking,

        /// <summary>
        /// Permission to enable coarse eye tracking.
        /// </summary>
        EyeTrackingCoarse,

        /// <summary>
        /// Permission to enable eye gaze interaction and fine eye tracking.
        /// </summary>
        EyeTrackingFine,

        /// <summary>
        /// Permission to enable face tracking.
        /// </summary>
        FaceTracking,
    }

    /// <summary>
    /// Helper class for <see cref="AndroidXRPermission"/>.
    /// </summary>
    public static class AndroidXRPermissionExtensions
    {
        /// <summary>
        /// Convert <see cref="AndroidXRPermission"/> to permission string that can be used in
        /// runtime permission request.
        /// </summary>
        /// <param name="permission">The Android XR permission.</param>
        /// <returns>The permission string injected to manifest.</returns>
        public static string ToPermissionString(this AndroidXRPermission permission)
        {
            switch (permission)
            {
                case AndroidXRPermission.SceneUnderstandingCoarse:
                    return "android.permission.SCENE_UNDERSTANDING_COARSE";
                case AndroidXRPermission.SceneUnderstandingFine:
                    return "android.permission.SCENE_UNDERSTANDING_FINE";
                case AndroidXRPermission.HandTracking:
                    return "android.permission.HAND_TRACKING";
                case AndroidXRPermission.EyeTrackingCoarse:
                    return "android.permission.EYE_TRACKING_COARSE";
                case AndroidXRPermission.EyeTrackingFine:
                    return "android.permission.EYE_TRACKING_FINE";
                case AndroidXRPermission.FaceTracking:
                    return "android.permission.FACE_TRACKING";
                default:
                    Debug.LogError($"Invalid AndroidXRPermission: {permission}");
                    return string.Empty;
            }
        }
    }
}
