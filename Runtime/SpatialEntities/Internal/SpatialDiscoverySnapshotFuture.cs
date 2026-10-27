// <copyright file="SpatialDiscoverySnapshotFuture.cs" company="Google LLC">
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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Google.XR.Extensions.Internal;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using UnityEngine;
    using UnityEngine.XR.ARSubsystems;
    using UnityEngine.XR.OpenXR.NativeTypes;

    using XrFutureEXT = System.UInt64;
    using XrSpatialContextEXT = System.UInt64;
    using XrSpatialPersistenceContextEXT = System.UInt64;
    using XrSpatialSnapshotEXT = System.UInt64;

    /// <summary>
    /// Supports XrSpatialSnapshotEXT creation with async operations.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>
    /// Handle OpenXR native calls to <c>xrCreateSpatialDiscoverySnapshotAsyncEXT</c>
    /// and <c>xrCreateSpatialDiscoverySnapshotCompleteEXT</c>.
    /// </description></item>
    /// </list>
    /// </remarks>
    internal class SpatialDiscoverySnapshotFuture : XRFutureHandler<XrSpatialSnapshotEXT>
    {
        XrSpatialContextEXT _xrSpatialContext = 0;

        SpatialDiscoverySnapshotFuture(
            XrSpatialContextEXT xrSpatialContext, XrFutureEXT xrFuture)
        {
            _xrSpatialContext = xrSpatialContext;
            StartPolling(xrFuture);
        }

        public static OpenXRResultStatus TryCreate(
            XrSpatialContextEXT xrSpatialContext,
            out SpatialDiscoverySnapshotFuture future)
        {
            return TryCreate(xrSpatialContext, 0, null, out future);
        }

        public static OpenXRResultStatus TryCreate(
            XrSpatialContextEXT xrSpatialContext,
            XrSpatialPersistenceContextEXT persistenceContext,
            IEnumerable<XrUuid> uuids,
            out SpatialDiscoverySnapshotFuture future)
        {
            if (xrSpatialContext == 0)
            {
                future = null;
                return new OpenXRResultStatus(
                    OpenXRResultStatus.StatusCode.ValidationFailure);
            }

            var createInfo = new SpatialDiscoverySnapshotCreateInfo(
                persistenceContext, uuids);
            var result = OpenXRNativeApi.xrCreateSpatialDiscoverySnapshotAsyncEXT(
                xrSpatialContext, createInfo.Info, out var xrFuture);
            createInfo.Dispose();
            if (result.IsError())
            {
                future = null;
                return result.ToStatus();
            }

            future = new SpatialDiscoverySnapshotFuture(xrSpatialContext, xrFuture);
            return result.ToStatus();
        }

        public static void Destroy(XrSpatialSnapshotEXT snapshot)
        {
            if (snapshot == 0)
            {
                return;
            }

            var result = OpenXRNativeApi.xrDestroySpatialSnapshotEXT(snapshot);
            if (result.IsError())
            {
                Debug.LogError($"xrDestroySpatialSnapshotEXT failed with {result}");
            }
        }

        protected override Result<XrSpatialSnapshotEXT> CompleteFuture(XrFutureEXT xrFuture)
        {
            var result = OpenXRNativeApi.xrCreateSpatialDiscoverySnapshotCompleteEXT(
                _xrSpatialContext, xrFuture, out var completion);
            if (result.IsError())
            {
                return new Result<XrSpatialSnapshotEXT>(result.ToXRResultStatus(), 0);
            }
            else if (completion.futureResult.IsError())
            {
                return new Result<XrSpatialSnapshotEXT>(
                    completion.futureResult.ToStatus().ToXRResultStatus(), 0);
            }
            else
            {
                return new Result<XrSpatialSnapshotEXT>(completion.snapshot);
            }
        }

        public struct SpatialDiscoverySnapshotCreateInfo : IDisposable
        {
            NativeArray<XrUuid> _uuids;
            unsafe XrSpatialDiscoveryPersistenceUuidFilterEXT* _uuidFilterPtr;

            XrSpatialDiscoverySnapshotCreateInfoEXT _createInfo;
            Allocator _allocator;

            public unsafe SpatialDiscoverySnapshotCreateInfo(
                XrSpatialPersistenceContextEXT persistenceContext,
                IEnumerable<XrUuid> uuids,
                Allocator allocator = Allocator.Temp)
            {
                var uuidCount = uuids == null ? 0 : uuids.Count();
                _allocator = allocator;
                void* nextPtr = null;
                if (uuidCount > 0)
                {
                    _uuids = new NativeArray<XrUuid>(
                        uuidCount, _allocator, NativeArrayOptions.UninitializedMemory);
                    int index = 0;
                    foreach (var uuid in uuids)
                    {
                        _uuids[index++] = uuid;
                    }

                    SpatialTypeUtils.UnsafeAlloc(out _uuidFilterPtr, false, _allocator);
                    *_uuidFilterPtr =
                        new XrSpatialDiscoveryPersistenceUuidFilterEXT(nextPtr, _uuids);
                    nextPtr = _uuidFilterPtr;
                }
                else
                {
                    _uuids = default;
                    _uuidFilterPtr = null;
                }

                // When no component types are passed into the create info,
                // the discovery snapshot contains all enabled types from XrSpatialContext.
                _createInfo = new XrSpatialDiscoverySnapshotCreateInfoEXT(
                    next: nextPtr,
                    componentTypeCount: 0u,
                    componentTypes: null);
            }

            public XrSpatialDiscoverySnapshotCreateInfoEXT Info => _createInfo;

            public unsafe void Dispose()
            {
                if (_uuidFilterPtr != null)
                {
                    UnsafeUtility.Free(_uuidFilterPtr, _allocator);
                    _uuidFilterPtr = null;
                }

                if (_uuids.IsCreated)
                {
                    _uuids.Dispose();
                }
            }
        }
    }
}
