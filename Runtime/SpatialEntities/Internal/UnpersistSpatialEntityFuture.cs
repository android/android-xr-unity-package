// <copyright file="UnpersistSpatialEntityFuture.cs" company="Google LLC">
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
    using XrSpatialPersistenceContextEXT = System.UInt64;

    /// <summary>
    /// Supports unpersisting spatial entities with async operations.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>
    /// Construct <see cref="XrSpatialEntityUnpersistInfoEXT"/> with the given uuids.
    /// </description></item>
    /// <item><description>
    /// Handle OpenXR native calls to
    /// <see cref="OpenXRNativeApi.xrUnpersistSpatialEntityAsyncEXT"/>.
    /// and <see cref="OpenXRNativeApi.xrUnpersistSpatialEntityCompleteEXT"/>.
    /// </description></item>
    /// </list>
    /// </remarks>
    internal class UnpersistSpatialEntityFuture :
        XRFutureHandler<XrSpatialPersistenceContextResultEXT>
    {
        private XrSpatialPersistenceContextEXT _persistenceContext;

        UnpersistSpatialEntityFuture(
            XrSpatialPersistenceContextEXT persistenceContext,
            XrFutureEXT xrFuture)
        {
            _persistenceContext = persistenceContext;
            StartPolling(xrFuture);
        }

        public XrSpatialPersistenceContextEXT PersistenceContext => _persistenceContext;

        public static OpenXRResultStatus TryCreate(
            XrSpatialPersistenceContextEXT persistenceContext,
            XrUuid persistUuid,
            out UnpersistSpatialEntityFuture future)
        {
            future = null;
            var unpersistInfo = new XrSpatialEntityUnpersistInfoEXT(persistUuid);
            var result = OpenXRNativeApi.xrUnpersistSpatialEntityAsyncEXT(
                persistenceContext, unpersistInfo, out var xrFuture);

            if (result.IsError())
            {
                return result.ToStatus();
            }

            future = new UnpersistSpatialEntityFuture(persistenceContext, xrFuture);
            return result.ToStatus();
        }

        protected override Result<XrSpatialPersistenceContextResultEXT> CompleteFuture(
            XrFutureEXT xrFuture)
        {
            var result = OpenXRNativeApi.xrUnpersistSpatialEntityCompleteEXT(
                _persistenceContext, xrFuture, out var completion);
            if (result.IsError())
            {
                return new Result<XrSpatialPersistenceContextResultEXT>(
                    result.ToStatus().ToXRResultStatus(), default);
            }
            else if (completion.futureResult.IsError())
            {
                return new Result<XrSpatialPersistenceContextResultEXT>(
                    completion.futureResult.ToStatus().ToXRResultStatus(), default);
            }

            return new Result<XrSpatialPersistenceContextResultEXT>(completion.unpersistResult);
        }
    }
}
