// <copyright file="AndroidXRSpatialEntityFeature.cs" company="Google LLC">
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

namespace Google.XR.Extensions.SpatialEntities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Google.XR.Extensions.Internal;
    using Google.XR.Extensions.SpatialEntities.Internal;
    using UnityEngine;
    using UnityEngine.XR.ARSubsystems;
    using UnityEngine.XR.OpenXR.Features;
    using UnityEngine.XR.OpenXR.NativeTypes;

    using XrSpatialContextEXT = System.UInt64;
    using XrSpatialPersistenceContextEXT = System.UInt64;
    using XrSpatialSnapshotEXT = System.UInt64;

    /// <summary>
    /// Base class for public Spatial Entity features provided by this package.
    /// </summary>
    /// <remarks>
    /// Derived features must have a lower priority than <see cref="SpatialLifecycleFeature"/>
    /// to ensure global contents are initialized first.
    /// </remarks>
    public abstract class AndroidXRSpatialEntityFeature : OpenXRFeature
    {
        /// <summary>
        /// Callback after spatial context created.
        /// Used by spatial subsystems to inject context dependencies.
        /// </summary>
        protected Action<XrSpatialContextEXT> _onSpatialContextCreated;

        /// <summary>
        /// Callback before spatial context destroy.
        /// Used by spatial subsystems to disconnect context injection.
        /// </summary>
        protected Action<XrSpatialContextEXT> _onSpatialContextDestroy;

        /// <summary>
        /// Callback after discovery snapshot created.
        /// Used by spatial subsystems to track entities.
        /// </summary>
        protected Action<XrSpatialContextEXT, XrSpatialSnapshotEXT> _onSpatialSnapshotCreated;

        /// <summary>
        /// Callback after persistence context created.
        /// </summary>
        protected Action<XrSpatialPersistenceScopeEXT, XrSpatialPersistenceContextEXT>
            _onPersistenceContextCreated;

        /// <summary>
        /// Callback before persistence context destroy.
        /// </summary>
        protected Action<XrSpatialPersistenceScopeEXT, XrSpatialPersistenceContextEXT>
            _onPersistenceContextDestroy;

        // Spatial features may create multiple contexts, but only one pending
        // future is allowed to keep configurator data consistent.
        private SpatialContextFuture _spatialContextFuture = null;
        private HashSet<XrSpatialContextEXT> _spatialContexts =
            new HashSet<XrSpatialContextEXT>();

        // Spatial features may create multiple snapshots simultaneously.
        private HashSet<SpatialDiscoverySnapshotFuture> _discoverySnapshotFutures =
            new HashSet<SpatialDiscoverySnapshotFuture>();

        private PersistenceContextFactory _persistenceFactory = new PersistenceContextFactory();

        private CancellationTokenSource _tokenSource = new CancellationTokenSource();

        /// <summary>
        /// Indicates whether the feature allows multiple spatial contexts
        /// running in parallel.
        /// </summary>
        internal virtual bool _allowMultiContexts => false;

        /// <summary>
        /// Indicates whether the feature is using native contexts, such as the
        /// spatial context handle created by
        /// <see cref="UnityEngine.XR.OpenXR.Features.SpatialEntities.SpatialSetupFeature"/>,
        /// a centralized setup feature which combines multiple configurations
        /// into a single spatial context, including Plane, Marker, Anchor with persistence.
        /// </summary>
        /// <remarks>
        /// If any inherited feature is true, <see cref="SpatialLifecycleFeature"/>
        /// will register <see cref="ApiXrFeature.SpatialEntity"/> and handle
        /// spatial entity interceptions.
        /// </remarks>
        internal virtual bool _useNativeContext => false;

        /// <summary>
        /// Internal debugging.
        /// </summary>
        internal ulong _nativeHandle => _spatialContexts.FirstOrDefault();

        /// <summary>
        /// Gets the cancellation token controlling all feature-owned async operations.
        /// It can then be canceled by calling <see cref="Cancel"/>.
        /// </summary>
        protected CancellationToken _cancelToken => _tokenSource.Token;

        /// <summary>
        /// Constructed and updated by individual features before calling
        /// <see cref="CreateSpatialContextAsync(bool)"/>.
        /// </summary>
        /// <returns>The configuration used to create spatial context.</returns>
        protected abstract ISpatialCapabilityConfiguration GetConfiguration();

        /// <summary>
        /// Constructed and updated by individual features before calling
        /// <see cref="CreatePersistenceContextsAsync"/>.
        /// </summary>
        /// <returns>
        /// The persistence scopes used to create persistence contexts.
        /// </returns>
        protected virtual HashSet<XrSpatialPersistenceScopeEXT> GetScopes() => null;

        /// <inheritdoc/>
        protected override void OnSessionDestroy(ulong xrSession)
        {
            // Ensure all pending futures are cancelled and handles are destroyed
            // when the session ends to avoid polling invalid handles.
            Cancel();
            DestroyAllSpatialContexts();
            DestroyAllPersistenceContexts();
        }

        /// <summary>
        /// Attempts to create an independent spatial context and handle
        /// exception automatically, such as cancellation.
        /// </summary>
        /// <param name="forceCreate">
        /// Indicates whether to force re-creation when there is already a valid
        /// context. If true, it will destroy all existing contexts and create a
        /// new context based on the current <see cref="GetConfiguration()"/>.
        /// </param>
        /// <param name="withPersistence">
        /// Indicates whether to create persistence contexts for the spatial context.
        /// </param>
        protected async void StartCreatingSpatialContext(
            bool forceCreate = false, bool withPersistence = false)
        {
            try
            {
                if (withPersistence)
                {
                    await CreatePersistenceContextsAsync();
                }

                await CreateSpatialContextAsync(forceCreate);
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("StartCreatingSpatialContext is canceled.");
            }
            catch (Exception e)
            {
                Debug.LogException(e, this);
            }
        }

        /// <summary>
        /// Attempts to create an independent spatial context for the given feature.
        /// It must be called by or after <see cref="OpenXRFeature.OnSessionCreate(ulong)"/>
        /// with a valid <see cref="GetConfiguration()"/>.
        /// </summary>
        /// <param name="forceCreate">
        /// Indicates whether to force re-creation when there is already a valid context.
        /// </param>
        /// <returns>The result of the async operation.</returns>
        protected async Awaitable<Result<XrSpatialContextEXT>> CreateSpatialContextAsync(
            bool forceCreate = false)
        {
            var status = new XRResultStatus(XRResultStatus.StatusCode.ValidationFailure);
            var result = new Result<XrSpatialContextEXT>(status, 0);
            if (forceCreate)
            {
                Debug.Log("Clear existing contexts by forceCreate option.");
                DestroyAllSpatialContexts();
            }
            else if (_spatialContextFuture != null)
            {
                Debug.LogError("Previous CreateSpatialContextAsync is still running.");
                return result;
            }
            else if (!_allowMultiContexts && _spatialContexts.Any())
            {
                Debug.LogError("SpatialContext is not null. Multiple-context is not allowed.");
                return result;
            }

            // Verify configuration.
            var config = GetConfiguration();
            if (config == null || config.enabledComponentCount == 0)
            {
                Debug.LogError("Configuration is invalid.");
                return result;
            }

            var createStatus = SpatialContextFuture.TryCreate(
                config, _persistenceFactory.Contexts, out _spatialContextFuture);
            if (createStatus.IsError())
            {
                Debug.LogErrorFormat(
                    "Failed to create SpatialContextFuture with {0}", result);
                return new Result<XrSpatialContextEXT>(createStatus.ToXRResultStatus(), 0);
            }

            Debug.Log("Waiting for SpatialContextFuture.TryGetResultAsync");
            status = new XRResultStatus(XRResultStatus.StatusCode.UnknownError);
            result = new Result<XrSpatialContextEXT>(status, 0);
            try
            {
                result = await _spatialContextFuture.TryGetResultAsync(_cancelToken);
                if (result.status.IsError())
                {
                    Debug.LogErrorFormat(
                        "SpatialContextFuture.TryGetResultAsync failed with {0}",
                        result.status.ToOpenXRStatus());
                }
                else
                {
                    Debug.LogFormat(
                        "SpatialContextFuture.TryGetResultAsync succeeded with result {0}.",
                        result.value);
                    _spatialContexts.Add(result.value);
                    _onSpatialContextCreated?.Invoke(result.value);
                    SpatialLifecycleFeature.RegisterDiscoveryCallback(
                        result.value, OnReceivedDiscoveryEvent);
                }
            }
            finally
            {
                // In case TryGetResultAsync failed with exception, e.g. cancellation,
                // clear stored futures.
                _spatialContextFuture?.Dispose();
                _spatialContextFuture = null;
            }

            return result;
        }

        /// <summary>
        /// Destroys the given spatial context handle.
        /// </summary>
        /// <param name="context">The target context to destroy.</param>
        protected void DestroySpatialContext(XrSpatialContextEXT context)
        {
            if (!_spatialContexts.Contains(context))
            {
                return;
            }

            _onSpatialContextDestroy?.Invoke(context);
            _spatialContexts.Remove(context);
            SpatialLifecycleFeature.UnregisterDiscoveryCallback(context);
            SpatialContextFuture.Destroy(context);
        }

        /// <summary>
        /// Destroys all spatial context handles and disposes native resources.
        /// </summary>
        protected void DestroyAllSpatialContexts()
        {
            foreach (var snapshotFuture in _discoverySnapshotFutures)
            {
                snapshotFuture.Dispose();
            }

            _discoverySnapshotFutures.Clear();

            _spatialContextFuture?.Dispose();
            _spatialContextFuture = null;

            foreach (var context in _spatialContexts.ToList())
            {
                DestroySpatialContext(context);
            }

            _spatialContexts.Clear();
        }

        /// <summary>
        /// Attempts to create persistence contexts for the given feature.
        /// It must be called by or after <see cref="OpenXRFeature.OnSessionCreate"/>
        /// with a valid <see cref="GetScopes()"/>, otherwise, it will clear
        /// existing contexts.
        /// These persistence contexts will then be used by
        /// <see cref="CreateSpatialContextAsync"/>.
        /// </summary>
        /// <returns>The results of the async operation.</returns>
        protected async Awaitable<bool> CreatePersistenceContextsAsync()
        {
            var enabledScopes = GetScopes();
            if (enabledScopes == null || !enabledScopes.Any())
            {
                _persistenceFactory.Reset();
                return false;
            }

            Debug.Log("Waiting for PersistenceContextFactory.Initialize...");
            bool usePersistence = false;
            try
            {
                usePersistence = await _persistenceFactory.Initialize(
                    enabledScopes, _cancelToken);
                if (usePersistence)
                {
                    foreach (var scope in _persistenceFactory.EnabledScopes)
                    {
                        _persistenceFactory.TryGetContext(scope, out var context);
                        _onPersistenceContextCreated?.Invoke(scope, context);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("PersistenceContextFactory.Initialize is canceled.");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            Debug.Log("PersistenceContextFactory.Initialize completed.");
            return usePersistence;
        }

        /// <summary>
        /// Destroys all persistence context handles and dispose native resources.
        /// </summary>
        protected void DestroyAllPersistenceContexts()
        {
            foreach (var scope in _persistenceFactory.EnabledScopes)
            {
                _persistenceFactory.TryGetContext(scope, out var context);
                _onPersistenceContextDestroy?.Invoke(scope, context);
            }

            _persistenceFactory.Reset();
        }

        /// <summary>
        /// Cancel all pending async operations. Expected to be called by
        /// <see cref="OpenXRFeature.OnSessionDestroy"/>.
        /// </summary>
        protected void Cancel()
        {
            _tokenSource.Cancel();
            _tokenSource.Dispose();
            _tokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Reset all callback events.
        /// </summary>
        protected virtual void ResetEvents()
        {
            _onSpatialContextCreated = null;
            _onSpatialContextDestroy = null;
            _onSpatialSnapshotCreated = null;
            _onPersistenceContextCreated = null;
            _onPersistenceContextDestroy = null;
        }

        /// <summary>
        /// Called when the enabled state of a feature changes.
        /// </summary>
        protected override void OnEnabledChange()
        {
#if UNITY_EDITOR
            // Enable lifecycle feature to manage global contexts.
            SpatialLifecycleFeature.RefreshEnabledState();
#endif // UNITY_EDITOR
        }

        private void OnReceivedDiscoveryEvent(XrSpatialContextEXT context)
        {
            if (_onSpatialSnapshotCreated == null || context == 0)
            {
                return;
            }

            StartCreatingSnapshotThenDestroy(context);
        }

        private async void StartCreatingSnapshotThenDestroy(XrSpatialContextEXT context)
        {
            try
            {
                var result = await CreateDiscoverySnapshotAsync(context);
                if (result.status.IsSuccess())
                {
                    try
                    {
                        _onSpatialSnapshotCreated?.Invoke(context, result.value);
                    }
                    finally
                    {
                        SpatialDiscoverySnapshotFuture.Destroy(result.value);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("CreateDiscoverySnapshotAsync is canceled.");
            }
            catch (Exception e)
            {
                Debug.LogException(e, this);
            }
        }

        private async Awaitable<Result<XrSpatialSnapshotEXT>> CreateDiscoverySnapshotAsync(
            XrSpatialContextEXT context)
        {
            var result = SpatialDiscoverySnapshotFuture.TryCreate(context, out var snapshotFuture);
            if (result.IsError())
            {
                Debug.LogErrorFormat(
                    "Failed to create SpatialDiscoverySnapshotFuture with {0}", result);
                return new Result<XrSpatialSnapshotEXT>(result.ToXRResultStatus(), 0);
            }

            // Skip await log to avoid spamming message.
            _discoverySnapshotFutures.Add(snapshotFuture);
            Result<XrSpatialSnapshotEXT> snapshotResult = new Result<XrSpatialSnapshotEXT>(
                new XRResultStatus(XRResultStatus.StatusCode.UnknownError), 0);
            try
            {
                snapshotResult = await snapshotFuture.TryGetResultAsync(_cancelToken);
                if (snapshotResult.status.IsError())
                {
                    Debug.LogErrorFormat(
                        "SpatialDiscoverySnapshotFuture.TryGetResultAsync failed with {0}",
                        snapshotResult.status.ToOpenXRStatus());
                }
            }
            finally
            {
                // In case TryGetResultAsync failed with exception, e.g. cancellation,
                // clear stored futures.
                _discoverySnapshotFutures.Remove(snapshotFuture);
                snapshotFuture.Dispose();
            }

            return snapshotResult;
        }
    }
}
