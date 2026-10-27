// <copyright file="SpatialShareableAnchorProducer.cs" company="Google LLC">
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
    using System.Threading;
    using System.Threading.Tasks;
    using Google.XR.Extensions;
    using Google.XR.Extensions.ARSubsystems;
    using Google.XR.Extensions.Internal;
    using UnityEngine;
    using UnityEngine.XR.ARSubsystems;
    using UnityEngine.XR.OpenXR.NativeTypes;

    using XrSpatialContextEXT = System.UInt64;
    using XrSpatialEntityEXT = System.UInt64;
    using XrSpatialEntityIdEXT = System.UInt64;
    using XrSpatialPersistenceContextEXT = System.UInt64;
    using XrSpatialSnapshotEXT = System.UInt64;

    /// <inheritdoc/>
    internal class SpatialShareableAnchorProducer :
        SpatialDataProducer<XRShareableAnchor, XRShareableAnchorBuilder>
    {
        private HashSet<XrSpatialEntityEXT> _singleHandleSet = new HashSet<XrSpatialEntityEXT>();
        private HashSet<XrUuid> _singleUuidSet = new HashSet<XrUuid>();

        private Dictionary<TrackableId, PersistSpatialEntityFuture> _persistFutures =
            new Dictionary<TrackableId, PersistSpatialEntityFuture>();

        private Dictionary<XrUuid, UnpersistSpatialEntityFuture> _unpersistFutures =
            new Dictionary<XrUuid, UnpersistSpatialEntityFuture>();

        private Dictionary<XrUuid, Task<XRResultStatus>> _discoveryTasks =
            new Dictionary<XrUuid, Task<XRResultStatus>>();

        /// <summary>
        /// The trackables created during snapshot queries. It should only
        /// happen on persisted anchors. Expect to receive their persistence
        /// data in later updates.
        /// Retains all entities until their persistence data is found.
        /// </summary>
        private HashSet<TrackableId> _newAddedTrackableIds = new HashSet<TrackableId>();

        // Double-indexed by both key and value to ensure O(1) access via either lookup or removal.
        private Dictionary<TrackableId, XrUuid> _trackableIdToUuids =
            new Dictionary<TrackableId, XrUuid>();

        private Dictionary<XrUuid, TrackableId> _uuidToTrackableIds =
            new Dictionary<XrUuid, TrackableId>();

        private HashSet<SurfaceAnchorFuture> _surfaceAnchorFutures =
            new HashSet<SurfaceAnchorFuture>();

        private HashSet<TrackableId> _surfaceAnchorIds = new HashSet<TrackableId>();

        private CancellationTokenSource _cleanupCts = null;

        public SpatialShareableAnchorProducer(XRShareableAnchorBuilder builder) : base(builder)
        {
        }

        public SpatialShareableAnchorProducer() : this(new XRShareableAnchorBuilder())
        {
        }

        public Result<XRShareableAnchor> TryAddAnchor(Pose pose)
        {
            var xrPose = new XrPosef(pose);
            var result = OpenXRNativeApi.xrCreateSpatialAnchorEXT(
                SpatialContext,
                xrPose,
                out XrSpatialEntityIdEXT entityId,
                out XrSpatialEntityEXT entityHandle);
            if (result.IsError())
            {
                Debug.LogErrorFormat("xrCreateSpatialAnchorEXT failed with status {0}", result);
                return new Result<XRShareableAnchor>(result.ToXRResultStatus(), default);
            }

            return GetOrCreateTrackableFromEntity(entityId, entityHandle, result);
        }

        public bool TryRemoveAnchor(TrackableId trackableId)
        {
            return TryRemoveEntity(trackableId.GetEntityId());
        }

        public async Awaitable<Result<SerializableGuid>> TrySaveAnchorAsync(
            TrackableId trackableId,
            XrSpatialPersistenceContextEXT persistenceContext,
            CancellationToken cancellationToken = default)
        {
            RefreshSavedUuids();
            if (_trackableIdToUuids.TryGetValue(trackableId, out var uuid))
            {
                return new Result<SerializableGuid>(uuid.ToSerializableGuid());
            }

            bool isCreator = false;
            if (!_persistFutures.TryGetValue(trackableId, out var future))
            {
                var createResult = PersistSpatialEntityFuture.TryCreate(
                    persistenceContext, trackableId, out future);
                if (createResult.IsError())
                {
                    Debug.LogErrorFormat(
                        "PersistSpatialEntityFuture.TryCreate failed with {0}",
                        createResult);
                    return new Result<SerializableGuid>(
                        createResult.ToXRResultStatus(), SerializableGuid.empty);
                }

                isCreator = true;
                _persistFutures[trackableId] = future;
            }

            Debug.Log($"Waiting for saving {trackableId}...");
            try
            {
                var persistResult = await future.TryGetResultAsync(cancellationToken);
                if (persistResult.status.IsError())
                {
                    return new Result<SerializableGuid>(
                        persistResult.status, SerializableGuid.empty);
                }

                var returnStatus = new XRResultStatus(
                    ConvertToStatusCode(persistResult.value.Result));
                var returnValue = SerializableGuid.empty;
                if (returnStatus.IsSuccess())
                {
                    uuid = persistResult.value.Uuid;
                    returnValue = uuid.ToSerializableGuid();
                    _trackableIdToUuids[trackableId] = uuid;
                    _uuidToTrackableIds[uuid] = trackableId;
                    Debug.Log($"Saved {trackableId} with uuid:\n{uuid}.");
                }

                return new Result<SerializableGuid>(returnStatus, returnValue);
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarningFormat(
                    "TrySaveAnchorAsync for {0} has been canceled.", trackableId);
                throw;
            }
            finally
            {
                if (isCreator)
                {
                    future?.Dispose();
                    _persistFutures.Remove(trackableId);
                }
            }
        }

        public async Awaitable<XRResultStatus> TryEraseAnchorAsync(
            XrUuid uuid, XrSpatialPersistenceContextEXT persistenceContext,
            CancellationToken cancellationToken = default)
        {
            RefreshSavedUuids();
            bool isCreator = false;
            if (!_unpersistFutures.TryGetValue(uuid, out var future))
            {
                var createResult = UnpersistSpatialEntityFuture.TryCreate(
                    persistenceContext, uuid, out future);
                if (createResult.IsError())
                {
                    Debug.LogErrorFormat(
                        "UnpersistSpatialEntityFuture.TryCreate failed with {0}",
                        createResult);
                    return createResult.ToXRResultStatus();
                }

                isCreator = true;
                _unpersistFutures[uuid] = future;
            }

            Debug.Log($"Waiting for erasing {uuid}...");
            try
            {
                var unpersistResult = await future.TryGetResultAsync(cancellationToken);
                if (unpersistResult.status.IsError())
                {
                    return unpersistResult.status;
                }

                var returnStatus = new XRResultStatus(
                    ConvertToStatusCode(unpersistResult.value));
                if (returnStatus.IsSuccess())
                {
                    Debug.Log($"Erased {uuid}.");
                    if (_uuidToTrackableIds.Remove(uuid, out var trackableId))
                    {
                        _trackableIdToUuids.Remove(trackableId);
                    }
                }

                return returnStatus;
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning($"TryEraseAnchorAsync for {uuid} has been canceled.");
                throw;
            }
            finally
            {
                if (isCreator)
                {
                    future?.Dispose();
                    _unpersistFutures.Remove(uuid);
                }
            }
        }

        public async Awaitable<Result<XRShareableAnchor>> TryLoadAnchorAsync(
            XrUuid uuid, XrSpatialPersistenceContextEXT persistenceContext,
            CancellationToken cancellationToken = default)
        {
            RefreshSavedUuids();
            if (_uuidToTrackableIds.TryGetValue(uuid, out var trackableId))
            {
                var trackable = GetTrackable(trackableId.GetEntityId());
                return new Result<XRShareableAnchor>(trackable);
            }

            bool isCreator = false;
            CancellationTokenSource linkedCts = null;
            try
            {
                if (!_discoveryTasks.TryGetValue(uuid, out var discoveryTask))
                {
                    if (_cleanupCts == null)
                    {
                        _cleanupCts = new CancellationTokenSource();
                    }

                    CancellationToken activeToken = _cleanupCts.Token;
                    if (cancellationToken.CanBeCanceled)
                    {
                        linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                            cancellationToken, _cleanupCts.Token);
                        activeToken = linkedCts.Token;
                    }

                    // The discovery task is owned by the creator, and will be canceled by
                    // either the creator or the producer cleanup.
                    discoveryTask = CreateAndDiscoverSnapshotAsync(
                        uuid, persistenceContext, activeToken);
                    isCreator = true;
                    _discoveryTasks[uuid] = discoveryTask;
                }

                // In case the duplicate request is canceled by the caller,
                // end this loading request earlier.
                var canceledByCaller = new TaskCompletionSource<bool>();
                using (cancellationToken.Register(() => canceledByCaller.TrySetResult(true)))
                {
                    var completedTask = await Task.WhenAny(discoveryTask, canceledByCaller.Task);
                    if (completedTask == canceledByCaller.Task)
                    {
                        throw new OperationCanceledException(cancellationToken);
                    }
                }

                // If the task has been cancelled within Task.WhenAny(),
                // re-throw OperationCanceledException with await.
                var returnStatus = await discoveryTask;
                var returnValue = XRShareableAnchor.defaultValue;
                if (returnStatus.IsSuccess())
                {
                    if (_uuidToTrackableIds.TryGetValue(uuid, out trackableId))
                    {
                        returnValue = GetTrackable(trackableId.GetEntityId());
                        Debug.Log($"Loaded {uuid} by trackable:\n{trackableId}");
                    }
                    else
                    {
                        returnStatus = new XRResultStatus(
                            PersistenceStatusCode.PersistUuidNotFound.ToXRCode());
                    }
                }

                return new Result<XRShareableAnchor>(returnStatus, returnValue);
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning($"TryLoadAnchorAsync for {uuid} has been canceled.");
                throw;
            }
            finally
            {
                if (isCreator)
                {
                    _discoveryTasks.Remove(uuid);
                }

                linkedCts?.Dispose();
            }
        }

        public Result<XRShareableAnchor> TryAttachGeospatialAnchor(
            XRGeospatialPose geospatialPose)
        {
            var result = XRGeospatialApi.CreateGeospatialAnchor(
                SpatialContext,
                XRGeospatialManager._nativeHandle,
                geospatialPose,
                out XrSpatialEntityIdEXT entityId);

            if (result.IsError())
            {
                Debug.LogErrorFormat(
                    "xrCreateGeospatialAnchorANDROID failed with status {0}", result);
                return new Result<XRShareableAnchor>(result.ToXRResultStatus(), default);
            }

            return GetOrCreateTrackableFromEntity(entityId);
        }

        public async Awaitable<Result<XRShareableAnchor>> TryAttachSurfaceAnchorAsync(
            XRGeospatialSurfaceType surfaceAnchorType,
            XRGeospatialPose geospatialPose,
            CancellationToken cancellationToken = default)
        {
            if (surfaceAnchorType == XRGeospatialSurfaceType.Default)
            {
                return TryAttachGeospatialAnchor(geospatialPose);
            }
            else if (_surfaceAnchorIds.Count + _surfaceAnchorFutures.Count >=
                XRGeospatialManager._instance.MaxNumberOfSurfaceAnchors)
            {
                return new Result<XRShareableAnchor>(
                    XrResult.LimitReached.ToStatus().ToXRResultStatus(), default);
            }

            var result = SurfaceAnchorFuture.TryCreate(
                SpatialContext,
                XRGeospatialManager._nativeHandle,
                surfaceAnchorType,
                geospatialPose,
                out var anchorFuture);

            if (result.IsError())
            {
                Debug.LogErrorFormat(
                    "SurfaceAnchorFuture.TryCreate failed with status {0}", result);
                return new Result<XRShareableAnchor>(result.ToXRResultStatus(), default);
            }

            _surfaceAnchorFutures.Add(anchorFuture);

            try
            {
                var completionResult = await anchorFuture.TryGetResultAsync(cancellationToken);
                if (completionResult.status.IsError())
                {
                    return new Result<XRShareableAnchor>(completionResult.status, default);
                }

                XrSpatialEntityIdEXT entityId = completionResult.value;
                var trackableResult = GetOrCreateTrackableFromEntity(entityId);
                if (trackableResult.status.IsSuccess())
                {
                    _surfaceAnchorIds.Add(trackableResult.value.trackableId);
                }
                else
                {
                    Debug.LogErrorFormat(
                        "CreateAndUpdateGeospatialEntity failed with status {0}",
                        trackableResult.status);
                }

                return trackableResult;
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("TryAttachSurfaceAnchorAsync has been canceled.");
                throw;
            }
            finally
            {
                anchorFuture?.Dispose();
                _surfaceAnchorFutures.Remove(anchorFuture);
            }
        }

        public void CleanupData()
        {
            if (_cleanupCts != null)
            {
                _cleanupCts.Cancel();
                _cleanupCts.Dispose();
                _cleanupCts = null;
                _discoveryTasks.Clear();
            }

            foreach (var persistFuture in _persistFutures.Values)
            {
                persistFuture?.Dispose();
            }

            foreach (var unpersistFuture in _unpersistFutures.Values)
            {
                unpersistFuture.Dispose();
            }

            _newAddedTrackableIds.Clear();
            _persistFutures.Clear();
            _unpersistFutures.Clear();
            foreach (var anchorFuture in _surfaceAnchorFutures)
            {
                anchorFuture?.Dispose();
            }

            _surfaceAnchorFutures.Clear();
            _surfaceAnchorIds.Clear();
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            base.Dispose();
            CleanupData();
            _uuidToTrackableIds.Clear();
            _trackableIdToUuids.Clear();
        }

        /// <inheritdoc/>
        protected override void AfterTrackableCreated(TrackableId trackableId)
        {
            _newAddedTrackableIds.Add(trackableId);
        }

        /// <inheritdoc/>
        protected override void OnTrackableDelete(TrackableId trackableId)
        {
            _newAddedTrackableIds.Remove(trackableId);
            if (_trackableIdToUuids.Remove(trackableId, out var uuid))
            {
                _uuidToTrackableIds.Remove(uuid);
            }

            _surfaceAnchorIds.Remove(trackableId);
        }

        private async Task<XRResultStatus> CreateAndDiscoverSnapshotAsync(
            XrUuid uuid, XrSpatialPersistenceContextEXT persistenceContext,
            CancellationToken cancellationToken)
        {
            _singleUuidSet.Add(uuid);
            var createResult = SpatialDiscoverySnapshotFuture.TryCreate(
                SpatialContext, persistenceContext, _singleUuidSet, out var future);
            _singleUuidSet.Remove(uuid);
            if (createResult.IsError())
            {
                Debug.LogErrorFormat(
                    "SpatialDiscoverySnapshotFuture.TryCreate failed with {0}",
                    createResult);
                return createResult.ToXRResultStatus();
            }

            Debug.Log($"Waiting for SpatialDiscoverySnapshotFuture.TryGetResultAsync...");
            XrSpatialSnapshotEXT snapshot = 0;
            try
            {
                var snapshotResult = await future.TryGetResultAsync(cancellationToken);
                if (snapshotResult.status.IsError())
                {
                    return snapshotResult.status;
                }

                snapshot = snapshotResult.value;
                var xrResult = QuerySnapshot(snapshot, true);
                RefreshSavedUuids();

                return xrResult.ToStatus().ToXRResultStatus();
            }
            finally
            {
                future?.Dispose();
                SpatialDiscoverySnapshotFuture.Destroy(snapshot);
            }
        }

        private Result<XRShareableAnchor> GetOrCreateTrackableFromEntity(
            XrSpatialEntityIdEXT entityId)
        {
            var xrResult = OpenXRNativeApi.xrCreateSpatialEntityFromIdEXT(
                SpatialContext, entityId, out XrSpatialEntityEXT entityHandle);
            if (xrResult.IsError())
            {
                Debug.LogErrorFormat(
                    "xrCreateSpatialEntityFromIdEXT(context={0:X16}, id={1:X16}) failed with {2}.",
                    SpatialContext, entityId, xrResult);
                return new Result<XRShareableAnchor>(
                    xrResult.ToStatus().ToXRResultStatus(), default);
            }

            return GetOrCreateTrackableFromEntity(entityId, entityHandle, xrResult.ToStatus());
        }

        private Result<XRShareableAnchor> GetOrCreateTrackableFromEntity(
            XrSpatialEntityIdEXT entityId,
            XrSpatialEntityEXT entityHandle,
            OpenXRResultStatus entityCreationResult)
        {
            if (!TryAddEntity(SpatialContext, entityId, entityHandle))
            {
                Debug.LogWarningFormat("XrSpatialEntityIdEXT {0:X16} already exists.", entityId);
                var existingTrackable = GetTrackable(entityId);
                var successStatus =
                    new XRResultStatus(XRResultStatus.StatusCode.UnqualifiedSuccess);
                return new Result<XRShareableAnchor>(successStatus, existingTrackable);
            }

            _singleHandleSet.Add(entityHandle);
            var updateStatus = UpdateTrackableData(_singleHandleSet);
            _singleHandleSet.Clear();
            if (updateStatus.IsError())
            {
                Debug.LogErrorFormat("UpdateTrackableData failed with status {0}", updateStatus);
                return new Result<XRShareableAnchor>(updateStatus.ToXRResultStatus(), default);
            }

            OpenXRResultStatus finalStatus = entityCreationResult;

            var trackable = GetTrackable(entityId);
            if (trackable.trackableId.GetEntityId() != entityId)
            {
                // The entity is returned as stopped state during the query,
                // and has then been removed from the tracking list.
                Debug.LogErrorFormat("XrSpatialEntityIdEXT {0:X16} is not tracking.", entityId);
                finalStatus = new OpenXRResultStatus(
                    OpenXRResultStatus.StatusCode.ValidationFailure);
            }

            return new Result<XRShareableAnchor>(finalStatus.ToXRResultStatus(), trackable);
        }

        private void RefreshSavedUuids()
        {
            _newAddedTrackableIds.RemoveWhere(trackableId =>
            {
                var trackable = GetTrackable(trackableId.GetEntityId());
                if (trackable.host != XRShareableAnchorHosts.None)
                {
                    _trackableIdToUuids[trackable.trackableId] = trackable.shareableId;
                    _uuidToTrackableIds[trackable.shareableId] = trackable.trackableId;
                    return true;
                }

                // Waiting for persistence components.
                return false;
            });
        }

        private XRResultStatus.StatusCode ConvertToStatusCode(
            XrSpatialPersistenceContextResultEXT result)
        {
            switch (result)
            {
                case XrSpatialPersistenceContextResultEXT.Success:
                    return XRResultStatus.StatusCode.PlatformQualifiedSuccess;
                case XrSpatialPersistenceContextResultEXT.EntityNotTracking:
                    return PersistenceStatusCode.EntityNotTracking.ToXRCode();
                case XrSpatialPersistenceContextResultEXT.PersistUuidNotFound:
                    return PersistenceStatusCode.PersistUuidNotFound.ToXRCode();
                default:
                    Debug.LogErrorFormat(
                        "Unknown XrSpatialPersistenceContextResultEXT: {0}.", result);
                    return XRResultStatus.StatusCode.UnknownError;
            }
        }
    }
}
