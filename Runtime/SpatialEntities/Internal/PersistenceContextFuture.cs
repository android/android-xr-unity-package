// <copyright file="PersistenceContextFuture.cs" company="Google LLC">
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
    using UnityEngine;
    using UnityEngine.XR.ARSubsystems;
    using UnityEngine.XR.OpenXR.NativeTypes;

    using XrFutureEXT = System.UInt64;
    using XrSpatialPersistenceContextEXT = System.UInt64;

    /// <summary>
    /// Supports XrSpatialPersistenceContextEXT creation with async operations.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>
    /// Construct <see cref="XrSpatialPersistenceContextCreateInfoEXT"/> with
    /// the target <see cref="XrSpatialPersistenceScopeEXT"/>.
    /// </description></item>
    /// <item><description>
    /// Handle OpenXR native calls to <c>xrCreateSpatialPersistenceContextAsyncEXT</c>
    /// and <c>xrCreateSpatialPersistenceContextCompleteEXT</c>.
    /// </description></item>
    /// </list>
    /// </remarks>
    internal class PersistenceContextFuture : XRFutureHandler<PersistenceContextResult>
    {
        private XrSpatialPersistenceScopeEXT _scope;

        PersistenceContextFuture(XrSpatialPersistenceScopeEXT scope, XrFutureEXT xrFuture)
        {
            _scope = scope;
            StartPolling(xrFuture);
        }

        public XrSpatialPersistenceScopeEXT Scope => _scope;

        public static OpenXRResultStatus TryCreate(
            XrSpatialPersistenceScopeEXT scope, out PersistenceContextFuture future)
        {
            future = null;
            if (!SpatialContextSupport.Initialized)
            {
                return new OpenXRResultStatus(OpenXRResultStatus.StatusCode.ProviderUninitialized);
            }
            else if (!SpatialContextSupport.IsSupported(scope))
            {
                return new OpenXRResultStatus(OpenXRResultStatus.StatusCode.Unsupported);
            }

            var createInfo = new XrSpatialPersistenceContextCreateInfoEXT(scope);
            var result = OpenXRNativeApi.xrCreateSpatialPersistenceContextAsyncEXT(
                createInfo, out var xrFuture);
            if (result.IsError())
            {
                return result;
            }

            future = new PersistenceContextFuture(scope, xrFuture);
            return result;
        }

        public static void Destroy(XrSpatialPersistenceContextEXT context)
        {
            if (context == 0)
            {
                return;
            }

            var result = OpenXRNativeApi.xrDestroySpatialPersistenceContextEXT(context);
            if (result.IsError())
            {
                Debug.LogError($"xrDestroySpatialPersistenceContextEXT failed with {result}");
            }
        }

        protected override Result<PersistenceContextResult> CompleteFuture(
            XrFutureEXT xrFuture)
        {
            var result = OpenXRNativeApi.xrCreateSpatialPersistenceContextCompleteEXT(
                xrFuture, out var completion);
            if (result.IsError())
            {
                return new Result<PersistenceContextResult>(
                    result.ToXRResultStatus(), default);
            }
            else if (completion.futureResult.IsError())
            {
                return new Result<PersistenceContextResult>(
                    completion.futureResult.ToStatus().ToXRResultStatus(), default);
            }

            var returnValue = new PersistenceContextResult(
                completion.createResult, completion.persistenceContext);
            return new Result<PersistenceContextResult>(returnValue);
        }
    }
}
