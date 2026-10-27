// <copyright file="PersistSpatialEntityFuture.cs" company="Google LLC">
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
    /// Supports persisting spatial entities with async operations.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>
    /// Construct <see cref="XrSpatialEntityPersistInfoEXT"/> with the target entity.
    /// </description></item>
    /// <item><description>
    /// Handle OpenXR native calls to <see cref="OpenXRNativeApi.xrPersistSpatialEntityAsyncEXT"/>.
    /// and <see cref="OpenXRNativeApi.xrPersistSpatialEntityCompleteEXT"/>.
    /// </description></item>
    /// </list>
    /// </remarks>
    internal class PersistSpatialEntityFuture : XRFutureHandler<PersistSpatialEntityResult>
    {
        private XrSpatialPersistenceContextEXT _persistenceContext;

        PersistSpatialEntityFuture(
            XrSpatialPersistenceContextEXT persistenceContext,
            XrFutureEXT xrFuture)
        {
            _persistenceContext = persistenceContext;
            StartPolling(xrFuture);
        }

        public XrSpatialPersistenceContextEXT PersistenceContext => _persistenceContext;

        public static OpenXRResultStatus TryCreate(
            XrSpatialPersistenceContextEXT persistenceContext,
            TrackableId trackableId,
            out PersistSpatialEntityFuture future)
        {
            future = null;
            var entityId = trackableId.GetEntityId();
            var persistInfo = new XrSpatialEntityPersistInfoEXT(
                trackableId.GetContextHandle(), entityId);
            var result = OpenXRNativeApi.xrPersistSpatialEntityAsyncEXT(
                persistenceContext, persistInfo, out var xrFuture);

            if (result.IsError())
            {
                return result.ToStatus();
            }

            future = new PersistSpatialEntityFuture(persistenceContext, xrFuture);
            return result.ToStatus();
        }

        protected override Result<PersistSpatialEntityResult> CompleteFuture(XrFutureEXT xrFuture)
        {
            var result = OpenXRNativeApi.xrPersistSpatialEntityCompleteEXT(
                _persistenceContext, xrFuture, out var completion);
            if (result.IsError())
            {
                return new Result<PersistSpatialEntityResult>(
                    result.ToStatus().ToXRResultStatus(), default);
            }
            else if (completion.futureResult.IsError())
            {
                return new Result<PersistSpatialEntityResult>(
                    completion.futureResult.ToStatus().ToXRResultStatus(), default);
            }

            var returnValue = new PersistSpatialEntityResult(
                completion.persistResult, completion.persistUuid);
            return new Result<PersistSpatialEntityResult>(returnValue);
        }
    }
}
