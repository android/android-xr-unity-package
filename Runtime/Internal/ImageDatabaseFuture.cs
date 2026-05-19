// <copyright file="ImageDatabaseFuture.cs" company="Qualcomm Technologies, Inc.">
//
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
    using UnityEngine.XR.ARSubsystems;

    using XrFutureEXT = System.UInt64;
    using XrTrackableImageDatabase = System.UInt64;

    /// <summary>
    /// Supports native image tracking database creation and configuration with async operations.
    /// </summary>
    internal class ImageDatabaseFuture : XRFutureHandler<XrTrackableImageDatabase>
    {
        private ImageDatabaseFuture(XrFutureEXT future)
        {
            StartPolling(future);
        }

        public static bool TryCreate(
            XRImageDatabaseEntry[] nativeEntries,
            out ImageDatabaseFuture future)
        {
            if (nativeEntries == null || nativeEntries.Length == 0)
            {
                future = null;
                return false;
            }

            XrFutureEXT xrFuture = 0;
            var result = XRTrackableApi.ConfigureImageTrackingAsync(nativeEntries,
                                                                    ref xrFuture);
            if (!result)
            {
                future = null;
                return false;
            }

            future = new ImageDatabaseFuture(xrFuture);
            return true;
        }

        public static void Destroy(ref XrTrackableImageDatabase xrTrackableImageDatabase)
        {
            if (xrTrackableImageDatabase == 0)
            {
                return;
            }

            // Deconfigures image tracking and destroys the native resources.
            XRTrackableApi.DeconfigureImageTracking(xrTrackableImageDatabase);

            xrTrackableImageDatabase = 0;
        }

        protected override Result<XrTrackableImageDatabase> CompleteFuture(XrFutureEXT xrFuture)
        {
            XrTrackableImageDatabase xrTrackableImageDatabase = 0;
            var success = XRTrackableApi.CompleteImageTrackingConfigurationFuture(
                             xrFuture, ref xrTrackableImageDatabase);

            if (!success)
            {
                return new Result<XrTrackableImageDatabase>(
                    new XRResultStatus(XRResultStatus.StatusCode.UnknownError),
                    0);
            }

            return new Result<XrTrackableImageDatabase>(xrTrackableImageDatabase);
        }
    }
}
