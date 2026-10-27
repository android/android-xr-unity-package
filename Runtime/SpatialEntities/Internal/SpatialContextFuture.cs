// <copyright file="SpatialContextFuture.cs" company="Google LLC">
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
    using System.Runtime.InteropServices;
    using Google.XR.Extensions.Internal;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using UnityEngine;
    using UnityEngine.XR.ARSubsystems;
    using UnityEngine.XR.OpenXR.NativeTypes;

    using XrFutureEXT = System.UInt64;
    using XrSpatialContextEXT = System.UInt64;
    using XrSpatialPersistenceContextEXT = System.UInt64;

    /// <summary>
    /// Supports XrSpatialContextEXT creation with async operations.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>
    /// Construct <see cref="XrSpatialContextCreateInfoEXT"/> with supported next chains,
    /// e.g. <see cref="XrSpatialContextPersistenceConfigEXT"/>.
    /// </description></item>
    /// <item><description>
    /// Handle OpenXR native calls to <c>xrCreateSpatialContextAsyncEXT</c>
    /// and <c>xrCreateSpatialContextCompleteEXT</c>.
    /// </description></item>
    /// </list>
    /// </remarks>
    internal class SpatialContextFuture : XRFutureHandler<XrSpatialContextEXT>
    {
        SpatialContextFuture(XrFutureEXT xrFuture)
        {
            StartPolling(xrFuture);
        }

        public static OpenXRResultStatus TryCreate(
            ISpatialCapabilityConfiguration configuration,
            out SpatialContextFuture future)
        {
            var configs = new List<ISpatialCapabilityConfiguration>() { configuration };
            return TryCreate(configs, null, out future);
        }

        public static OpenXRResultStatus TryCreate(
            ISpatialCapabilityConfiguration configuration,
            IEnumerable<XrSpatialPersistenceContextEXT> persistenceContexts,
            out SpatialContextFuture future)
        {
            var configs = new List<ISpatialCapabilityConfiguration>() { configuration };
            return TryCreate(configs, persistenceContexts, out future);
        }

        public static OpenXRResultStatus TryCreate(
            IEnumerable<ISpatialCapabilityConfiguration> configurations,
            out SpatialContextFuture future)
        {
            return TryCreate(configurations, null, out future);
        }

        public static OpenXRResultStatus TryCreate(
            IEnumerable<ISpatialCapabilityConfiguration> configurations,
            IEnumerable<XrSpatialPersistenceContextEXT> persistenceContexts,
            out SpatialContextFuture future)
        {
            var validConfigs = new List<ISpatialCapabilityConfiguration>();
            if (configurations != null)
            {
                foreach (var config in configurations)
                {
                    if (config != null && config.enabledComponentCount > 0)
                    {
                        validConfigs.Add(config);
                    }
                }
            }

            var validContexts = new List<XrSpatialPersistenceContextEXT>();
            if (persistenceContexts != null)
            {
                foreach (var contexts in persistenceContexts)
                {
                    if (contexts != 0)
                    {
                        validContexts.Add(contexts);
                    }
                }
            }

            if (!validConfigs.Any())
            {
                future = null;
                return new OpenXRResultStatus(
                    OpenXRResultStatus.StatusCode.ValidationFailure);
            }

            var createInfo = new SpatialContextCreateInfo(
                validConfigs, validContexts);
            var result = OpenXRNativeApi.xrCreateSpatialContextAsyncEXT(
                createInfo.Info, out var xrFuture);
            if (result.IsError())
            {
                future = null;
                createInfo.Dispose();
                return result;
            }

            future = new SpatialContextFuture(xrFuture);
            createInfo.Dispose();
            return result;
        }

        public static void Destroy(XrSpatialContextEXT xrSpatialContext)
        {
            if (xrSpatialContext == 0)
            {
                return;
            }

            var result = OpenXRNativeApi.xrDestroySpatialContextEXT(xrSpatialContext);
            if (result.IsError())
            {
                Debug.LogError($"xrDestroySpatialContextEXT failed with {result}");
            }
        }

        protected override Result<XrSpatialContextEXT> CompleteFuture(XrFutureEXT xrFuture)
        {
            var result = OpenXRNativeApi.xrCreateSpatialContextCompleteEXT(
                xrFuture, out var completion);
            if (result.IsError())
            {
                return new Result<XrSpatialContextEXT>(result.ToXRResultStatus(), 0);
            }
            else if (completion.futureResult.IsError())
            {
                return new Result<XrSpatialContextEXT>(
                    completion.futureResult.ToStatus().ToXRResultStatus(), 0);
            }
            else
            {
                return new Result<XrSpatialContextEXT>(completion.spatialContext);
            }
        }

        public struct SpatialContextCreateInfo : IDisposable
        {
            NativeArray<IntPtr> _configPtrs;
            NativeArray<XrSpatialPersistenceContextEXT> _pstContexts;
            unsafe XrSpatialContextPersistenceConfigEXT* _pstConfigPtr;
            XrSpatialContextCreateInfoEXT _createInfo;
            Allocator _allocator;

            public unsafe SpatialContextCreateInfo(
                IEnumerable<ISpatialCapabilityConfiguration> configurations,
                IEnumerable<XrSpatialPersistenceContextEXT> persistenceContexts,
                Allocator allocator = Allocator.Temp)
            {
                var configCount = configurations == null ? 0 : configurations.Count();
                _allocator = allocator;
                _configPtrs = new NativeArray<IntPtr>(configCount, _allocator);
                int index = 0;
                if (configurations != null)
                {
                    foreach (var config in configurations)
                    {
                        var ptr = Marshal.AllocHGlobal(Marshal.SizeOf(config.GetType()));
                        Marshal.StructureToPtr(config, ptr, false);
                        _configPtrs[index++] = ptr;
                    }
                }

                var pstCount = persistenceContexts == null ? 0 : persistenceContexts.Count();
                _pstContexts =
                    new NativeArray<XrSpatialPersistenceContextEXT>(pstCount, _allocator);
                index = 0;
                if (persistenceContexts != null)
                {
                    foreach (var context in persistenceContexts)
                    {
                        _pstContexts[index++] = context;
                    }
                }

                if (pstCount == 0)
                {
                    _pstConfigPtr = null;
                    _createInfo = new XrSpatialContextCreateInfoEXT(_configPtrs);
                }
                else
                {
                    SpatialTypeUtils.UnsafeAlloc(out _pstConfigPtr, false, _allocator);
                    *_pstConfigPtr = new XrSpatialContextPersistenceConfigEXT(_pstContexts);
                    _createInfo = new XrSpatialContextCreateInfoEXT(_pstConfigPtr, _configPtrs);
                }
            }

            public XrSpatialContextCreateInfoEXT Info => _createInfo;

            public unsafe void Dispose()
            {
                foreach (var ptr in _configPtrs)
                {
                    Marshal.FreeHGlobal(ptr);
                }

                if (_pstConfigPtr != null)
                {
                    UnsafeUtility.Free(_pstConfigPtr, _allocator);
                    _pstConfigPtr = null;
                }

                _configPtrs.Dispose();
                _pstContexts.Dispose();
            }
        }
    }
}
