// <copyright file="FaceTrackerApi.cs" company="Google LLC">
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
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using UnityEngine;

    internal class FaceTrackerApi
    {
        public static void GetXRFaceState(int parameterCount, ref XRFaceState outState)
        {
            if (outState.Parameters == null || outState.Parameters.Length != parameterCount)
            {
                outState.Parameters = new float[parameterCount];
            }

            outState.TrackingState = XRFaceTrackingStates.Stopped;
            outState.Timestamp = 0;
            ExternalApi.XrFaceTracking_getFaceState(
                XRInstanceManagerApi.GetIntPtr(), parameterCount, outState.Parameters,
                ref outState.TrackingState, ref outState.Timestamp, ref outState.IsValid);
        }

        public static void SetTrackingEnabled(bool enable)
        {
            ExternalApi.XrFaceTracking_setTrackingEnabled(XRInstanceManagerApi.GetIntPtr(), enable);
        }

        public static bool IsFaceCalibrated()
        {
            return ExternalApi.XrFaceTracking_isFaceCalibrated(XRInstanceManagerApi.GetIntPtr());
        }

        private struct ExternalApi
        {
            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void XrFaceTracking_getFaceState(
                IntPtr manager,
                int parameter_count,
                float[] out_parameters,
                ref XRFaceTrackingStates out_state,
                ref ulong out_time_stamp,
                ref bool out_is_valid);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void XrFaceTracking_setTrackingEnabled(
                IntPtr manager, bool enable);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrFaceTracking_isFaceCalibrated(IntPtr manager);
        }
    }
}
