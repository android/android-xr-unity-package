// <copyright file="SpatialShareableAnchorSubsystem.cs" company="Google LLC">
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
    using System.Collections.Generic;
    using System.Threading;
    using Google.XR.Extensions.ARSubsystems;
    using Google.XR.Extensions.Internal;
    using Google.XR.Extensions.SpatialEntities.Internal;
    using Unity.Collections;
    using UnityEngine;
    using UnityEngine.Scripting;
    using UnityEngine.XR.ARSubsystems;
    using UnityEngine.XR.OpenXR;
    using UnityEngine.XR.OpenXR.NativeTypes;

    using XrSpatialContextEXT = System.UInt64;
    using XrSpatialPersistenceContextEXT = System.UInt64;
    using XrSpatialSnapshotEXT = System.UInt64;

    /// <summary>
    /// The spatial entity implementation of the <see cref="XRShareableAnchorSubsystem"/>.
    /// </summary>
    [Preserve]
    public sealed class SpatialShareableAnchorSubsystem : XRShareableAnchorSubsystem
    {
        internal const string _subsystemId = "AndroidXR-SpatialEntity-ShareableAnchor";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterDescriptor()
        {
            XRShareableAnchorSubsystemDescriptor.Cinfo cinfo =
                new XRShareableAnchorSubsystemDescriptor.Cinfo()
                {
                    id = _subsystemId,
                    providerType = typeof(SpatialEntityProvider),
                    subsystemTypeOverride = typeof(SpatialShareableAnchorSubsystem),
                    supportedHostsDelegate = GetSupportedHosts,
                    supportsAsyncCancellation = true,
                    supportsSaveAnchor = true,
                    supportsEraseAnchor = true,
                    supportsLoadAnchor = true,
                    supportsGetSavedAnchorIds = false,
                    supportsAttachGeospatialAnchor = true,
                    supportsAttachSurfaceAnchor = true,
                };

            XRShareableAnchorSubsystemDescriptor.Register(cinfo);
        }

        private static XRShareableAnchorHosts GetSupportedHosts()
        {
            XRShareableAnchorHosts supportedHosts = XRShareableAnchorHosts.None;
            if (SpatialContextSupport.Initialized)
            {
                if (SpatialContextSupport.IsSupported(XrSpatialPersistenceScopeEXT.LocalAnchors))
                {
                    supportedHosts |= XRShareableAnchorHosts.Local;
                }
            }

            return supportedHosts;
        }

        internal class SpatialEntityProvider : Provider
        {
            Dictionary<XrSpatialPersistenceScopeEXT, XrSpatialPersistenceContextEXT>
                _persistenceContexts =
                    new Dictionary<XrSpatialPersistenceScopeEXT, XrSpatialPersistenceContextEXT>();

            SpatialShareableAnchorProducer _dataProducer = new SpatialShareableAnchorProducer();

            private bool _autoLoadPersistedAnchors = false;

            /// <summary>
            /// Sets the value which indicate whether to automatically load persisted anchors.
            /// Must be called during subsystem creation so it can be taken effect before
            /// the subsystem is running.
            /// </summary>
            /// <param name="autoLoad">Whether to automatically load persisted anchors.</param>
            public void SetAutoLoad(bool autoLoad)
            {
                _autoLoadPersistedAnchors = autoLoad;
            }

            /// <inheritdoc/>
            public override bool TryAddAnchor(Pose pose, out XRShareableAnchor anchor)
            {
                if (_dataProducer.SpatialContext == 0)
                {
                    anchor = default;
                    return false;
                }

                var result = _dataProducer.TryAddAnchor(pose);
                anchor = result.value;
                return result.status.IsSuccess();
            }

            /// <inheritdoc/>
            public override bool TryRemoveAnchor(TrackableId anchorId)
            {
                return _dataProducer.TryRemoveAnchor(anchorId);
            }

            /// <inheritdoc/>
            public override Awaitable<Result<SerializableGuid>> TrySaveAnchorAsync(
                TrackableId anchorId,
                XRShareableAnchorHosts host,
                CancellationToken cancellationToken = default)
            {
                var trackable = _dataProducer.GetTrackable(anchorId.GetEntityId());
                if (anchorId.GetContextHandle() != _dataProducer.SpatialContext ||
                    trackable.trackingState != TrackingState.Tracking ||
                    trackable.host != XRShareableAnchorHosts.None)
                {
                    return AndroidXRSubsystemUtils.CreateAwaitableWithResult(
                        XRResultStatus.StatusCode.ValidationFailure,
                        SerializableGuid.empty);
                }

                var status = TryGetPersistenceContext(host, out var persistenceContext);
                if (status != XRResultStatus.StatusCode.UnqualifiedSuccess)
                {
                    return AndroidXRSubsystemUtils.CreateAwaitableWithResult(
                        status, SerializableGuid.empty);
                }

                return _dataProducer.TrySaveAnchorAsync(
                    anchorId, persistenceContext, cancellationToken);
            }

            /// <inheritdoc/>
            public override Awaitable<XRResultStatus> TryEraseAnchorAsync(
                SerializableGuid shareableId,
                XRShareableAnchorHosts host,
                CancellationToken cancellationToken = default)
            {
                var status = TryGetPersistenceContext(host, out var persistenceContext);
                if (status != XRResultStatus.StatusCode.UnqualifiedSuccess)
                {
                    return AndroidXRSubsystemUtils.CreateAwaitableWithStatus(status);
                }

                return _dataProducer.TryEraseAnchorAsync(
                    shareableId, persistenceContext, cancellationToken);
            }

            /// <inheritdoc/>
            public override Awaitable<Result<XRShareableAnchor>> TryLoadAnchorAsync(
                SerializableGuid shareableId,
                XRShareableAnchorHosts host,
                CancellationToken cancellationToken = default)
            {
                var status = TryGetPersistenceContext(host, out var persistenceContext);
                if (status != XRResultStatus.StatusCode.UnqualifiedSuccess)
                {
                    return AndroidXRSubsystemUtils.CreateAwaitableWithResult(
                        status, XRShareableAnchor.defaultValue);
                }

                return _dataProducer.TryLoadAnchorAsync(
                    shareableId, persistenceContext, cancellationToken);
            }

            /// <inheritdoc/>
            public override Result<XRShareableAnchor> TryAttachGeospatialAnchor(
                XRGeospatialPose geospatialPose)
            {
                var status = ValidateGeospatialTracker();
                if (status.IsError())
                {
                    return new Result<XRShareableAnchor>(status, XRShareableAnchor.defaultValue);
                }

                return _dataProducer.TryAttachGeospatialAnchor(geospatialPose);
            }

            /// <inheritdoc/>
            public override Awaitable<Result<XRShareableAnchor>> TryAttachSurfaceAnchorAsync(
                XRGeospatialSurfaceType type,
                XRGeospatialPose geospatialPose,
                CancellationToken cancellationToken = default)
            {
                var status = ValidateGeospatialTracker();
                if (status.IsError())
                {
                    return AndroidXRSubsystemUtils.CreateAwaitableWithResult(
                        status, XRShareableAnchor.defaultValue);
                }

                return _dataProducer.TryAttachSurfaceAnchorAsync(
                    type, geospatialPose, cancellationToken);
            }

            /// <inheritdoc/>
            public override void Start()
            {
                // Attempt to create spatial context if not yet present.
                // It's used to wait and grant runtime permissions.
                var feature = OpenXRSettings.Instance.GetFeature<SpatialShareableAnchorFeature>();
                if (feature != null)
                {
                    feature.OnSpatialProviderStart();
                }
            }

            /// <inheritdoc/>
            public override void Stop()
            {
                var feature = OpenXRSettings.Instance.GetFeature<SpatialShareableAnchorFeature>();
                if (feature != null)
                {
                    feature.OnSpatialProviderStop();
                }
            }

            /// <inheritdoc/>
            public override void Destroy()
            {
                // Native resources should have been reset by Stop().
                // Calling RemoveAllEntities() again for safety.
                _dataProducer.RemoveAllEntities();
                _dataProducer.Dispose();
            }

            /// <inheritdoc/>
            public override TrackableChanges<XRShareableAnchor> GetChanges(
                XRShareableAnchor defaultAnchor, Allocator allocator)
            {
                _dataProducer.UpdateTrackableData();
                var added = new NativeArray<XRShareableAnchor>(
                    _dataProducer.Added.Count, allocator, NativeArrayOptions.UninitializedMemory);
                var updated = new NativeArray<XRShareableAnchor>(
                    _dataProducer.Updated.Count, allocator, NativeArrayOptions.UninitializedMemory);
                var removed = new NativeArray<TrackableId>(
                    _dataProducer.Removed.Count, allocator, NativeArrayOptions.UninitializedMemory);

                int i = 0;
                foreach (var id in _dataProducer.Added)
                {
                    added[i++] = _dataProducer.GetTrackable(id);
                }

                i = 0;
                foreach (var id in _dataProducer.Updated)
                {
                    updated[i++] = _dataProducer.GetTrackable(id);
                }

                i = 0;
                foreach (var id in _dataProducer.Removed)
                {
                    removed[i++] = id;
                }

                _dataProducer.ClearChanges();
                return new TrackableChanges<XRShareableAnchor>(added, updated, removed);
            }

            #region  Spatial Feature Callbacks
            internal void OnPersistenceContextCreated(
                XrSpatialPersistenceScopeEXT scope, XrSpatialPersistenceContextEXT context)
            {
                _persistenceContexts[scope] = context;
                Debug.LogFormat("Added {0} context {1:X16}.", scope, context);
            }

            internal void OnPersistenceContextDestroy(
                XrSpatialPersistenceScopeEXT scope, XrSpatialPersistenceContextEXT context)
            {
                if (_persistenceContexts.Remove(scope))
                {
                    Debug.LogFormat("Removed {0} context {1:X16}.", scope, context);
                }
            }

            internal void OnSpatialContextCreated(
                XrSpatialContextEXT context, SpatialAnchorConfigurator configurator)
            {
                Debug.LogFormat("Created spatial context {0:X16} with components ({1}).",
                    context, string.Join(",", configurator.EnabledComponents));
                _dataProducer.UpdateContext(context, configurator.EnabledComponents);
            }

            internal void OnSpatialContextDestroy(XrSpatialContextEXT context)
            {
                Debug.LogFormat("Destroy spatial context {0:X16}.", context);
                _dataProducer.UpdateContext(0, null);
                _dataProducer.RemoveAllEntities();
                _dataProducer.CleanupData();
            }

            internal void OnDiscoverySnapshotCreated(
                XrSpatialContextEXT context, XrSpatialSnapshotEXT snapshot)
            {
                // When auto-load is enabled, query discovery snapshots to
                // populate persisted anchors and report via trackable changes.
                // When auto-load is disabled, persisted anchors will only be
                // explicitly added by TryLoadAnchorAsync.
                if (_autoLoadPersistedAnchors)
                {
                    _dataProducer.QuerySnapshot(snapshot);
                }
            }
            #endregion Spatial Feature Callbacks

            private XRResultStatus.StatusCode TryGetPersistenceContext(
                XRShareableAnchorHosts host,
                out XrSpatialPersistenceContextEXT persistenceContext)
            {
                persistenceContext = 0;
                if (!host.TryGetPersistenceScope(out var scope) ||
                    !SpatialContextSupport.IsSupported(scope))
                {
                    return XRResultStatus.StatusCode.Unsupported;
                }
                else if (!_persistenceContexts.TryGetValue(scope, out persistenceContext))
                {
                    return XRResultStatus.StatusCode.ProviderUninitialized;
                }
                else
                {
                    return XRResultStatus.StatusCode.UnqualifiedSuccess;
                }
            }

            private XRResultStatus ValidateGeospatialTracker()
            {
                if (!SpatialShareableAnchorFeature.IsGeospatialHostSupported.HasValue ||
                    XRGeospatialManager._instance == null ||
                    _dataProducer.SpatialContext == 0)
                {
                    return new XRResultStatus(XRResultStatus.StatusCode.ProviderUninitialized);
                }
                else if (!SpatialShareableAnchorFeature.IsGeospatialHostSupported.Value)
                {
                    return new XRResultStatus(XRResultStatus.StatusCode.Unsupported);
                }
                else if (XRGeospatialManager.State != XRGeospatialState.Running)
                {
                    return new XRResultStatus((int)XrResultAndroid.GeospatialTrackerNotRunning);
                }

                return new XRResultStatus(XRResultStatus.StatusCode.UnqualifiedSuccess);
            }
        }
    }
}
