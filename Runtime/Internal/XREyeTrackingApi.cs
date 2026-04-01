// <copyright file="XREyeTrackingApi.cs" company="Google LLC">
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

namespace Google.XR.Extensions.Internal
{
    using System;
    using System.Runtime.InteropServices;
    using UnityEngine;

    internal class XREyeTrackingApi
    {
        public static void SetEnable(bool enable)
        {
            ExternalApi.XrEyeTracking_setEnabled(XRInstanceManagerApi.GetIntPtr(), enable);
        }

        public static ApiEyeStates GetFineEye(ref Pose left, ref Pose right)
        {
            ApiEyeStates states = ApiEyeStates.None;
            ExternalApi.XrEyeTracking_getFinePoses(
                XRInstanceManagerApi.GetIntPtr(), ref left, ref right, ref states);
            return states;
        }

        private struct ExternalApi
        {
            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void XrEyeTracking_setEnabled(IntPtr manager, bool enable);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void XrEyeTracking_getFinePoses(
                IntPtr manager, ref Pose left, ref Pose right, ref ApiEyeStates out_states);
        }
    }
}
