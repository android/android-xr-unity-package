// <copyright file="ApiEnums.cs" company="Google LLC">
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

namespace Google.XR.Extensions.Internal
{
    using System;
    using UnityEngine.XR.ARSubsystems;

    internal enum ApiXrTrackableType
    {
        NotValid = 0,
        Plane = 1,
        Depth = 2,
        Object = 1000466000,
    }

    internal static class ApiEnums
    {
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
    }
}
