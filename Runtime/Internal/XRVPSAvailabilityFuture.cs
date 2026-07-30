// <copyright file="XRVPSAvailabilityFuture.cs" company="Google LLC">
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
    using UnityEngine.XR.ARSubsystems;
    using UnityEngine.XR.OpenXR.NativeTypes;

    using XrFutureEXT = System.UInt64;

    /// <summary>
    /// Supports VPS availability check with async operations.
    /// </summary>
    /// <remarks>
    /// Handles OpenXR native calls to <c>xrCheckVpsAvailabilityAsyncANDROID</c>
    /// and <c>xrCheckVpsAvailabilityCompleteANDROID</c>.
    /// </remarks>
    internal class XRVPSAvailabilityFuture : XRFutureHandler<XRVPSAvailability>
    {
        XRVPSAvailabilityFuture(XrFutureEXT xrFuture)
        {
            StartPolling(xrFuture);
        }

        public static OpenXRResultStatus TryCheck(
            double latitude, double longitude, out XRVPSAvailabilityFuture future)
        {
            future = null;
            var status = XRGeospatialApi.CheckVpsAsync(
                latitude, longitude, out XrFutureEXT xrFuture);
            if (status.IsError())
            {
                return status;
            }

            future = new XRVPSAvailabilityFuture(xrFuture);
            return status;
        }

        protected override Result<XRVPSAvailability> CompleteFuture(XrFutureEXT xrFuture)
        {
            var status = XRGeospatialApi.CheckVpsComplete(
                xrFuture, out XRVPSAvailability availability);
            if (status.IsError())
            {
                return new Result<XRVPSAvailability>(
                    status.ToXRResultStatus(), XRVPSAvailability.Unknown);
            }

            return new Result<XRVPSAvailability>(availability);
        }
    }
}
