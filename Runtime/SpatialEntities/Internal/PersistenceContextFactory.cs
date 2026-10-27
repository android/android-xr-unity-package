// <copyright file="PersistenceContextFactory.cs" company="Google LLC">
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
    using System.Threading;
    using UnityEngine;
    using UnityEngine.XR.OpenXR.NativeTypes;

    using XrSpatialPersistenceContextEXT = System.UInt64;

    /// <summary>
    /// Manages the lifecycle of multiple XrSpatialPersistenceContextEXT handles.
    /// </summary>
    /// <remarks>
    /// Must call <see cref="Reset()"/> to release native resources after
    /// <see cref="Initialize"/>.
    /// </remarks>
    internal class PersistenceContextFactory
    {
        private Dictionary<XrSpatialPersistenceScopeEXT, XrSpatialPersistenceContextEXT>
            _persistenceContexts =
                new Dictionary<XrSpatialPersistenceScopeEXT, XrSpatialPersistenceContextEXT>();

        private List<PersistenceContextFuture> _persistenceFutures =
            new List<PersistenceContextFuture>();

        public HashSet<XrSpatialPersistenceScopeEXT> EnabledScopes =>
            _persistenceContexts.Keys.ToHashSet();

        public HashSet<XrSpatialPersistenceContextEXT> Contexts =>
            _persistenceContexts.Values.ToHashSet();

        public bool TryGetContext(
            XrSpatialPersistenceScopeEXT scope, out XrSpatialPersistenceContextEXT context)
            => _persistenceContexts.TryGetValue(scope, out context);

        public async Awaitable<bool> Initialize(
            HashSet<XrSpatialPersistenceScopeEXT> scopes,
            CancellationToken token = default)
        {
            Reset();
            foreach (var scope in scopes)
            {
                var result = PersistenceContextFuture.TryCreate(scope, out var future);
                if (result.IsError())
                {
                    Debug.LogErrorFormat(
                        "Failed to create PersistenceContextFuture with scope {0}: {1}",
                        scope.GetDisplayName(), result);
                    continue;
                }

                _persistenceFutures.Add(future);
            }

            foreach (var future in _persistenceFutures)
            {
                try
                {
                    var result = await future.TryGetResultAsync(token);
                    if (result.status.IsError())
                    {
                        Debug.LogErrorFormat(
                            "PersistenceContextFuture with {0} failed to get result: {1}",
                            future.Scope.GetDisplayName(), result.status.ToOpenXRStatus());
                        continue;
                    }
                    else if (result.value.Result.IsError())
                    {
                        Debug.LogErrorFormat(
                            "PersistenceContextFuture with {0} failed to get result: {1}",
                            future.Scope.GetDisplayName(), result.value.Result);
                        continue;
                    }

                    _persistenceContexts.Add(future.Scope, result.value.Context);
                }
                catch (Exception)
                {
                    Reset();
                    throw;
                }
            }

            // If nothing has been created, Dispose() can be skipped.
            return _persistenceContexts.Any();
        }

        public void Reset()
        {
            foreach (var future in _persistenceFutures)
            {
                future.Dispose();
            }

            foreach (var contextPair in _persistenceContexts)
            {
                PersistenceContextFuture.Destroy(contextPair.Value);
            }

            _persistenceContexts.Clear();
            _persistenceFutures.Clear();
        }
    }
}
