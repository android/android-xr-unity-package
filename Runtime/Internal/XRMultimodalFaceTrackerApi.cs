// <copyright file="XRMultimodalFaceTrackerApi.cs" company="Google LLC">
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
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using UnityEngine;

    internal class XRMultimodalFaceTrackerApi
    {
        public static bool GetTrackerMode(ref XRFaceTrackerMode outTrackerMode)
        {
            return ExternalApi.XrMultimodalFaceTracking_getFaceTrackingDataSource(
                XRInstanceManagerApi.GetIntPtr(), ref outTrackerMode);
        }

        public static bool Enable(XRFaceTrackerMode requestedMode)
        {
            return ExternalApi.XrMultimodalFaceTracking_enable(
                XRInstanceManagerApi.GetIntPtr(), requestedMode);
        }

        private struct ExternalApi
        {
            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrMultimodalFaceTracking_enable(
                IntPtr manager, XRFaceTrackerMode requestedMode);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrMultimodalFaceTracking_getFaceTrackingDataSource(
                IntPtr manager, ref XRFaceTrackerMode outTrackerMode);
        }
    }
}
