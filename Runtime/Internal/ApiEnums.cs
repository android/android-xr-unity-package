// <copyright file="ApiEnums.cs" company="Google LLC">
//
// Copyright 2024 Google LLC
// Copyright Qualcomm Technologies, Inc. and/or its affiliates. All rights reserved.
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

namespace Google.XR.Extensions.Internal
{
    using System;
    using UnityEngine.XR.ARSubsystems;
#if UNITY_OPEN_XR_ANDROID_XR
    using UnityEngine.XR.OpenXR.Features.Android;
#endif // UNITY_OPEN_XR_ANDROID_XR

    // Matches OpenXR's XrTrackableTypeANDROID enum
    internal enum ApiXrTrackableType
    {
        NotValid = 0,
        Plane = 1,
        Depth = 1000463000,
        Object = 1000466000,
        Marker = 1000707000,
        QrCode = 1000708000,
    }

    internal enum ApiRequirement
    {
        Required = 0,
        Optional = 1,
    }

    /// <summary>
    /// Match AndroidOpenXREyeTrackingStates from Unity OpenXR Android XR,
    /// so it can be used internally without package dependency.
    /// </summary>
    [Flags]
    internal enum ApiEyeStates
    {
        None = 0,
        LeftEyePoseValid = 1 << 0,
        LeftEyeShut = 1 << 1,
        LeftEyeGazing = 1 << 2,
        RightEyePoseValid = 1 << 3,
        RightEyeShut = 1 << 4,
        RightEyeGazing = 1 << 5,
    }

    /// <summary>
    /// Matches OpenXR definition of <c>XrBodyJointSetANDROIDX1</c>.
    /// </summary>
    internal enum ApiBodyJointSet : int
    {
        UpperBody = 0,
        FullBody = 1,
    }

    internal static class ApiEnums
    {
        public static bool ToRequired(this ApiRequirement requirement)
        {
            return requirement == ApiRequirement.Required;
        }

        public static ApiRequirement ToRequirement(this bool required)
        {
            return required ? ApiRequirement.Required : ApiRequirement.Optional;
        }

        public static ApiXrTrackableType ToApiXrTrackableType(this TrackableType type)
        {
            switch (type)
            {
                case TrackableType.PlaneWithinPolygon:
                case TrackableType.PlaneWithinBounds:
                case TrackableType.PlaneWithinInfinity:
                case TrackableType.PlaneEstimated:
                case TrackableType.Planes:
                    return ApiXrTrackableType.Plane;
                case TrackableType.Depth:
                    return ApiXrTrackableType.Depth;
                default: return ApiXrTrackableType.NotValid;
            }
        }

        public static ApiBodyJointSet ToApiBodyJointSet(this XRBodyJointSet jointSet)
        {
            switch (jointSet)
            {
                case XRBodyJointSet.UpperBody:
                    return ApiBodyJointSet.UpperBody;
                case XRBodyJointSet.FullBody:
                    return ApiBodyJointSet.FullBody;
                default:
                    return ApiBodyJointSet.UpperBody;
            }
        }

        public static XRBodyJointSet ToXRBodyJointSet(this ApiBodyJointSet jointSet)
        {
            switch (jointSet)
            {
                case ApiBodyJointSet.UpperBody:
                    return XRBodyJointSet.UpperBody;
                case ApiBodyJointSet.FullBody:
                    return XRBodyJointSet.FullBody;
                default:
                    return XRBodyJointSet.UpperBody;
            }
        }
#if UNITY_OPEN_XR_ANDROID_XR
        public static AndroidOpenXREyeTrackingStates ToFaceStates(this ApiEyeStates apiEye)
        {
            AndroidOpenXREyeTrackingStates states = AndroidOpenXREyeTrackingStates.None;
            if (apiEye.HasFlag(ApiEyeStates.LeftEyePoseValid))
            {
                states |= AndroidOpenXREyeTrackingStates.LeftEyePoseValid;
            }

            if (apiEye.HasFlag(ApiEyeStates.LeftEyeGazing))
            {
                states |= AndroidOpenXREyeTrackingStates.LeftEyeGazing;
            }

            if (apiEye.HasFlag(ApiEyeStates.LeftEyeShut))
            {
                states |= AndroidOpenXREyeTrackingStates.LeftEyeShut;
            }

            if (apiEye.HasFlag(ApiEyeStates.RightEyePoseValid))
            {
                states |= AndroidOpenXREyeTrackingStates.RightEyePoseValid;
            }

            if (apiEye.HasFlag(ApiEyeStates.RightEyeGazing))
            {
                states |= AndroidOpenXREyeTrackingStates.RightEyeGazing;
            }

            if (apiEye.HasFlag(ApiEyeStates.RightEyeShut))
            {
                states |= AndroidOpenXREyeTrackingStates.RightEyeShut;
            }

            return states;
        }
#endif // UNITY_OPEN_XR_ANDROID_XR
    }
}
