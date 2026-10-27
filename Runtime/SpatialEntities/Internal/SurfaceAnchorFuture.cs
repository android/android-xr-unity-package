// <copyright file="SurfaceAnchorFuture.cs" company="Google LLC">
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

namespace Google.XR.Extensions.SpatialEntities.Internal
{
    using Google.XR.Extensions.Internal;
    using UnityEngine.XR.ARSubsystems;
    using UnityEngine.XR.OpenXR.NativeTypes;

    using XrFutureEXT = System.UInt64;
    using XrGeospatialTracker = System.UInt64;
    using XrSpatialContextEXT = System.UInt64;
    using XrSpatialEntityIdEXT = System.UInt64;

    internal class SurfaceAnchorFuture : XRFutureHandler<XrSpatialEntityIdEXT>
    {
        private XrSpatialContextEXT _spatialContext;

        SurfaceAnchorFuture(XrSpatialContextEXT spatialContext, XrFutureEXT xrFuture)
        {
            _spatialContext = spatialContext;
            StartPolling(xrFuture);
        }

        public static OpenXRResultStatus TryCreate(
            XrSpatialContextEXT spatialContext,
            XrGeospatialTracker geospatialTracker,
            XRGeospatialSurfaceType surfaceAnchorType,
            XRGeospatialPose geospatialPose,
            out SurfaceAnchorFuture future)
        {
            future = null;
            var result = XRGeospatialApi.CreateSurfaceAnchorAsync(
                spatialContext,
                geospatialTracker,
                surfaceAnchorType,
                geospatialPose,
                out XrFutureEXT xrFuture);

            if (result.IsError())
            {
                return result;
            }

            future = new SurfaceAnchorFuture(spatialContext, xrFuture);
            return result;
        }

        protected override Result<XrSpatialEntityIdEXT> CompleteFuture(XrFutureEXT xrFuture)
        {
            var status = XRGeospatialApi.CreateSurfaceAnchorComplete(
                _spatialContext, xrFuture, out XrSpatialEntityIdEXT entityId);
            return new Result<XrSpatialEntityIdEXT>(status.ToXRResultStatus(), entityId);
        }
    }
}
