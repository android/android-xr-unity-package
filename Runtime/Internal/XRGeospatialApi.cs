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
    using Google.XR.Extensions;
    using UnityEngine;
    using UnityEngine.XR.OpenXR.NativeTypes;

    using XrFutureEXT = System.UInt64;
    using XrGeospatialTracker = System.UInt64;
    using XrSpatialContextEXT = System.UInt64;
    using XrSpatialEntityIdEXT = System.UInt64;

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

        public static bool InitializeGeospatialAnchorHost()
        {
            return ExternalApi.XrGeospatial_initializeAnchorHost(
                XRInstanceManagerApi.GetIntPtr());
        }

        public static bool SetRequestedPlaneTracking(bool enable)
        {
            return ExternalApi.XrGeospatial_setRequestedPlaneTracking(
                XRInstanceManagerApi.GetIntPtr(), enable);
        }

        public static bool GetMaxSurfaceAnchorCount(out uint count)
        {
            count = 0;
            return ExternalApi.XrGeospatial_getMaxSurfaceAnchorCount(
                XRInstanceManagerApi.GetIntPtr(), ref count);
        }

        public static OpenXRResultStatus CreateGeospatialAnchor(
            XrSpatialContextEXT spatialContext,
            XrGeospatialTracker tracker,
            XRGeospatialPose pose,
            out XrSpatialEntityIdEXT anchorEntityId)
        {
            XrResult result = XrResult.FunctionUnsupported;
            anchorEntityId = 0;
            if (!ExternalApi.XrGeospatial_createGeospatialAnchor(
                XRInstanceManagerApi.GetIntPtr(),
                spatialContext,
                tracker,
                ref pose,
                ref anchorEntityId,
                ref result))
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

        public static OpenXRResultStatus CreateSurfaceAnchorAsync(
            XrSpatialContextEXT spatialContext,
            XrGeospatialTracker tracker,
            XRGeospatialSurfaceType surfaceAnchorType,
            XRGeospatialPose pose,
            out XrFutureEXT future)
        {
            XrResult result = XrResult.FunctionUnsupported;
            future = 0;
            if (!ExternalApi.XrGeospatial_createSurfaceAnchorAsync(
                XRInstanceManagerApi.GetIntPtr(),
                spatialContext,
                tracker,
                surfaceAnchorType,
                ref pose,
                ref future,
                ref result))
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

        public static OpenXRResultStatus CreateSurfaceAnchorComplete(
            XrSpatialContextEXT spatialContext,
            XrFutureEXT future,
            out XrSpatialEntityIdEXT anchorEntityId)
        {
            XrResult result = XrResult.FunctionUnsupported;
            anchorEntityId = 0;
            if (!ExternalApi.XrGeospatial_createSurfaceAnchorComplete(
                XRInstanceManagerApi.GetIntPtr(),
                spatialContext,
                future,
                ref anchorEntityId,
                ref result))
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

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrGeospatial_initializeAnchorHost(IntPtr manager);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrGeospatial_setRequestedPlaneTracking(
                IntPtr manager, bool enable);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrGeospatial_getMaxSurfaceAnchorCount(
                IntPtr manager, ref uint out_max_count);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrGeospatial_createGeospatialAnchor(
                IntPtr manager,
                XrSpatialContextEXT spatial_context,
                XrGeospatialTracker geospatial_tracker,
                ref XRGeospatialPose pose,
                ref XrSpatialEntityIdEXT out_anchor_entity_id,
                ref XrResult out_result);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrGeospatial_createSurfaceAnchorAsync(
                IntPtr manager,
                XrSpatialContextEXT spatial_context,
                XrGeospatialTracker geospatial_tracker,
                XRGeospatialSurfaceType surface_anchor_type,
                ref XRGeospatialPose pose,
                ref XrFutureEXT out_future,
                ref XrResult out_result);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrGeospatial_createSurfaceAnchorComplete(
                IntPtr manager,
                XrSpatialContextEXT spatial_context,
                XrFutureEXT future,
                ref XrSpatialEntityIdEXT out_anchor_entity_id,
                ref XrResult out_result);
        }
    }
}
