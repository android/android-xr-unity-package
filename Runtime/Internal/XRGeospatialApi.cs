// <copyright file="XRGeospatialApi.cs" company="Google LLC">
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

namespace Google.XR.Extensions.Internal
{
    using System;
    using System.Runtime.InteropServices;
    using UnityEngine;
    using UnityEngine.XR.OpenXR.NativeTypes;

    using XrFutureEXT = System.UInt64;
    using XrGeospatialTracker = System.UInt64;

    internal class XRGeospatialApi
    {
        public static OpenXRResultStatus CheckVpsAsync(
            double latitude, double longitude, out XrFutureEXT xrFuture)
        {
            xrFuture = 0;
            XrResult result = XrResult.FunctionUnsupported;
            if (!ExternalApi.XrGeospatial_checkVpsAsync(
                XRInstanceManagerApi.GetIntPtr(), latitude, longitude, ref xrFuture, ref result))
            {
                return new OpenXRResultStatus(OpenXRResultStatus.StatusCode.ProviderUninitialized);
            }
            else if (result == XrResult.HandleInvalid)
            {
                // XrSession == XR_NULL_HANDLE
                return new OpenXRResultStatus(OpenXRResultStatus.StatusCode.ProviderNotStarted);
            }

            return result.ToStatus();
        }

        public static OpenXRResultStatus CheckVpsComplete(
            XrFutureEXT future, out XRVPSAvailability availability)
        {
            availability = XRVPSAvailability.Unknown;
            XrResult result = XrResult.FunctionUnsupported;
            if (!ExternalApi.XrGeospatial_checkVpsComplete(
                XRInstanceManagerApi.GetIntPtr(), future, ref result, ref availability))
            {
                return new OpenXRResultStatus(OpenXRResultStatus.StatusCode.ProviderUninitialized);
            }
            else if (result == XrResult.HandleInvalid)
            {
                // XrSession == XR_NULL_HANDLE
                return new OpenXRResultStatus(OpenXRResultStatus.StatusCode.ProviderNotStarted);
            }

            return result.ToStatus();
        }

        public static XrResult Create(out XrGeospatialTracker tracker)
        {
            XrResult result = XrResult.FunctionUnsupported;
            tracker = 0;
            if (!ExternalApi.XrGeospatial_createTracker(
                    XRInstanceManagerApi.GetIntPtr(), ref tracker, ref result))
            {
                return XrResult.FeatureUnsupported;
            }

            return result;
        }

        public static void Destroy(XrGeospatialTracker tracker)
        {
            ExternalApi.XrGeospatial_destroyTracker(XRInstanceManagerApi.GetIntPtr(), tracker);
        }

        public static bool Locate(
            XrGeospatialTracker tracker, XRGeospatialPose pose, out XRPoseResult result)
        {
            result = default;
            return ExternalApi.XrGeospatial_locate(
                XRInstanceManagerApi.GetIntPtr(), tracker, ref pose, ref result);
        }

        public static bool Locate(
            XrGeospatialTracker tracker, Pose pose, out XRGeospatialPoseResult result)
        {
            result = default;
            return ExternalApi.XrGeospatial_locateFromPose(
                XRInstanceManagerApi.GetIntPtr(), tracker, ref pose, ref result);
        }

        private struct ExternalApi
        {
            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrGeospatial_checkVpsAsync(
                IntPtr manager, double latitude, double longitude,
                ref XrFutureEXT out_future, ref XrResult out_result);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrGeospatial_checkVpsComplete(
                IntPtr manager, XrFutureEXT future, ref XrResult out_result,
                ref XRVPSAvailability out_vps);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrGeospatial_createTracker(
                IntPtr manager, ref XrGeospatialTracker out_tracker, ref XrResult out_result);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void XrGeospatial_destroyTracker(
                IntPtr manager, XrGeospatialTracker tracker);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrGeospatial_locate(
                IntPtr manager, XrGeospatialTracker tracker, ref XRGeospatialPose pose,
                ref XRPoseResult out_result);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrGeospatial_locateFromPose(
                IntPtr manager, XrGeospatialTracker tracker, ref Pose pose,
                ref XRGeospatialPoseResult out_result);
        }
    }
}
