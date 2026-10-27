// <copyright file="ARShareableAnchorManager.cs" company="Google LLC">
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

namespace Google.XR.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using Google.XR.Extensions.ARSubsystems;
    using Google.XR.Extensions.Internal;
    using Unity.Collections;
    using UnityEngine;
    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.ARSubsystems;

    /// <summary>
    /// A trackable manager that enables placing, tracking, saving and loading
    /// shareable anchors.
    /// </summary>
    /// <remarks>
    /// A shareable anchor is a pose (position and rotation) in the physical
    /// environment that is tracked by an XR device. It can then be saved on a
    /// host for sharing among different clients, so that each client can
    /// retrieve the same anchor from the physical environment.
    /// </remarks>
    [SuppressMessage(
        "UnityRules.UnityStyleRules", "US1109:PublicPropertiesMustBeUpperCamelCase",
        Justification = "Match Unity interface.")]
    [DefaultExecutionOrder(ARUpdateOrder.k_AnchorManager + 1)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Unity.XR.CoreUtils.XROrigin))]
    [AddComponentMenu("XR/AR Foundation/AR Shareable Anchor Manager")]
    public sealed class ARShareableAnchorManager :
        ARTrackableManager<XRShareableAnchorSubsystem,
                           XRShareableAnchorSubsystemDescriptor,
                           XRShareableAnchorSubsystem.Provider,
                           XRShareableAnchor,
                           ARShareableAnchor>
    {
        [SerializeField]
        [Tooltip("If not null, this prefab is instantiated for each shareable anchor.")]
        GameObject _shareableAnchorPrefab;

        Dictionary<TrackableId, GeospatialData> _activeGeospatialAnchors =
            new Dictionary<TrackableId, GeospatialData>();

        /// <summary>
        /// Gets or sets the prefab which is used to instantiate each
        /// <see cref="ARShareableAnchor"/>. May be <see langword="null"/>.
        /// </summary>
        /// <remarks>
        /// The purpose of this property is to extend the functionality of
        /// <see cref="ARShareableAnchor"/>s.
        /// It is not the recommended way to instantiate content associated
        /// with an <see cref="ARShareableAnchor"/>.
        /// </remarks>
        public GameObject shareableAnchorPrefab
        {
            get => _shareableAnchorPrefab;
            set => _shareableAnchorPrefab = value;
        }

        /// <summary>
        /// Gets the active instance of the <see cref="ARShareableAnchorManager"/>.
        /// </summary>
        internal static ARShareableAnchorManager _instance { get; private set; }

        /// <summary>
        /// The name to assign to the GameObject instantiated for each
        /// <see cref="ARShareableAnchor"/>.
        /// </summary>
        protected override string gameObjectName => "ShareableAnchor";

        /// <summary>
        /// Gets the <see cref="ARShareableAnchor"/> with given
        /// <paramref name="trackableId"/>, or <see langword="null"/> if
        /// no such anchor exists.
        /// </summary>
        /// <param name="trackableId">
        /// The <see cref="TrackableId"/> of the <see cref="ARShareableAnchor"/> to retrieve.
        /// </param>
        /// <returns>
        /// The <see cref="ARShareableAnchor"/> or <see langword="null"/>.
        /// </returns>
        public ARShareableAnchor GetShareableAnchor(TrackableId trackableId) =>
            m_Trackables.TryGetValue(trackableId, out var anchor) ? anchor : null;

        /// <summary>
        /// Attempts to create a new anchor at the given <paramref name="pose"/>.
        /// </summary>
        /// <param name="pose">
        /// The pose, in Unity world space, of the anchor.</param>
        /// <returns>
        /// A new <see cref="ARShareableAnchor"/> if successful,
        /// otherwise <see langword="null"/>.
        /// </returns>
        public ARShareableAnchor TryAddAnchor(Pose pose)
        {
            if (subsystem == null)
            {
                Debug.LogError(
                    "Subsystem is null. " +
                    "Please enable the manager and wait for a running subsystem.");
                return null;
            }
            else if (!subsystem.running)
            {
                Debug.LogError(
                    "Subsystem is not running. " +
                    "Please enable the manager and wait for a running subsystem.");
                return null;
            }

            return subsystem.TryAddAnchor(pose, out var sessionRelativeData) ?
                CreateTrackableImmediate(sessionRelativeData) : null;
        }

        /// <summary>
        /// Attempts to remove a shareable anchor.
        /// </summary>
        /// <param name="anchor">
        /// The <see cref="ARShareableAnchor"/> to remove.</param>
        /// <returns>
        /// Returns <see langword="true"/> if successful,
        /// otherwise <see langword="false"/>.</returns>
        public bool TryRemoveAnchor(ARShareableAnchor anchor)
        {
            if (anchor == null)
            {
                Debug.LogError("Cannot remove a null anchor.");
                return false;
            }
            else if (subsystem == null)
            {
                Debug.LogError(
                    "Subsystem is null. " +
                    "Please enable the manager and wait for a running subsystem.");
                return false;
            }
            else if (!subsystem.running)
            {
                Debug.LogError(
                    "Subsystem is not running. " +
                    "Please enable the manager and wait for a running subsystem.");
                return false;
            }
            else if (!subsystem.TryRemoveAnchor(anchor.trackableId))
            {
                return false;
            }

            DestroyPendingTrackable(anchor.trackableId);
            return true;
        }

        /// <summary>
        /// Attempts to persistently save the given anchor so that it can be
        /// loaded in a future session.
        /// Use the <see cref="SerializableGuid"/> returned by this method
        /// as an input parameter to <see cref="TryLoadAnchorAsync"/> or
        /// <see cref="TryEraseAnchorAsync"/>.
        /// </summary>
        /// <param name="anchor">
        /// The <see cref="ARShareableAnchor"/> to save. You can create a
        /// shareable anchor using <see cref="TryAddAnchor"/>.
        /// </param>
        /// <param name="host">
        /// The target host to save this anchor.</param>
        /// <param name="cancellationToken">
        /// An optional <see cref="CancellationToken"/> that you can use to
        /// cancel the operation in progress if the loaded provider
        /// <see cref="XRShareableAnchorSubsystemDescriptor.supportsAsyncCancellation"/>.
        /// </param>
        /// <returns>
        /// The result of the async operation, containing a new persistent
        /// GUID if the operation succeeded.
        /// If the operation failed, you may also check with
        /// <see cref="XrResultExtensions.IsPersistenceResult"/> for any persistence reasons.
        /// You are responsible to <see langword="await"/> this result.
        /// </returns>
        /// <exception cref="OperationCanceledException">Thrown if the operation
        /// has been canceled either by the caller or the Unity main thread.</exception>
        /// <seealso cref="XRShareableAnchorSubsystemDescriptor.supportsSaveAnchor"/>
        public Awaitable<Result<SerializableGuid>> TrySaveAnchorAsync(
            ARShareableAnchor anchor,
            XRShareableAnchorHosts host,
            CancellationToken cancellationToken = default)
        {
            if (anchor == null ||
                host == XRShareableAnchorHosts.None)
            {
                return AndroidXRSubsystemUtils.CreateAwaitableWithResult(
                    XRResultStatus.StatusCode.ValidationFailure, SerializableGuid.empty);
            }
            else if (subsystem == null)
            {
                return AndroidXRSubsystemUtils.CreateAwaitableWithResult(
                    XRResultStatus.StatusCode.ProviderUninitialized, SerializableGuid.empty);
            }
            else if (!subsystem.running)
            {
                return AndroidXRSubsystemUtils.CreateAwaitableWithResult(
                    XRResultStatus.StatusCode.ProviderNotStarted, SerializableGuid.empty);
            }

            return subsystem.TrySaveAnchorAsync(anchor.trackableId, host, cancellationToken);
        }

        /// <summary>
        /// Attempts to erase the persistent saved data associated with an
        /// anchor given its persistent GUID.
        /// </summary>
        /// <param name="shareableId">
        /// A persistent GUID created by <see cref="TrySaveAnchorAsync"/>.
        /// </param>
        /// <param name="host">
        /// The target host to erase this persistent id.</param>
        /// <param name="cancellationToken">
        /// An optional <see cref="CancellationToken"/> that you can use to
        /// cancel the operation in progress if the loaded provider
        /// <see cref="XRShareableAnchorSubsystemDescriptor.supportsAsyncCancellation"/>.
        /// </param>
        /// <returns>
        /// The result of the async operation.
        /// If the operation failed, you may also check with
        /// <see cref="XrResultExtensions.IsPersistenceResult"/> to determine whether it failed
        /// with <see cref="PersistenceStatusCode.PersistUuidNotFound"/>.
        /// You are responsible to <see langword="await"/> this result.
        /// </returns>
        /// <exception cref="OperationCanceledException">Thrown if the operation
        /// has been canceled either by the caller or the Unity main thread.</exception>
        /// <seealso cref="XRShareableAnchorSubsystemDescriptor.supportsEraseAnchor"/>
        public Awaitable<XRResultStatus> TryEraseAnchorAsync(
            SerializableGuid shareableId,
            XRShareableAnchorHosts host,
            CancellationToken cancellationToken = default)
        {
            if (shareableId.Equals(SerializableGuid.empty) ||
                host == XRShareableAnchorHosts.None)
            {
                return AndroidXRSubsystemUtils.CreateAwaitableWithStatus(
                    XRResultStatus.StatusCode.ValidationFailure);
            }
            else if (subsystem == null)
            {
                return AndroidXRSubsystemUtils.CreateAwaitableWithStatus(
                    XRResultStatus.StatusCode.ProviderUninitialized);
            }
            else if (!subsystem.running)
            {
                return AndroidXRSubsystemUtils.CreateAwaitableWithStatus(
                    XRResultStatus.StatusCode.ProviderNotStarted);
            }

            return subsystem.TryEraseAnchorAsync(shareableId, host, cancellationToken);
        }

        /// <summary>
        /// Attempts to load a shareable anchor given its persistent GUID.
        /// </summary>
        /// <param name="shareableId">
        /// A persistent GUID created by <see cref="TrySaveAnchorAsync"/>.
        /// </param>
        /// <param name="host">
        /// The target host to load this persistent id.</param>
        /// <param name="cancellationToken">
        /// An optional <see cref="CancellationToken"/> that you can use to
        /// cancel the operation in progress if the loaded provider
        /// <see cref="XRShareableAnchorSubsystemDescriptor.supportsAsyncCancellation"/>.
        /// </param>
        /// <returns>
        /// The result of the async operation, containing the newly added
        /// shareable anchor if the operation succeeded.
        /// If the operation failed, you may also check with
        /// <see cref="XrResultExtensions.IsPersistenceResult"/> to determine whether it failed
        /// with <see cref="PersistenceStatusCode.PersistUuidNotFound"/>.
        /// You are responsible to <see langword="await"/> this result.</returns>
        /// <exception cref="OperationCanceledException">Thrown if the operation
        /// has been canceled either by the caller or the Unity main thread.</exception>
        /// <seealso cref="XRShareableAnchorSubsystemDescriptor.supportsLoadAnchor"/>
        public async Awaitable<Result<ARShareableAnchor>> TryLoadAnchorAsync(
            SerializableGuid shareableId,
            XRShareableAnchorHosts host,
            CancellationToken cancellationToken = default)
        {
            if (shareableId.Equals(SerializableGuid.empty) ||
                host == XRShareableAnchorHosts.None)
            {
                return await AndroidXRSubsystemUtils.CreateAwaitableWithResult<ARShareableAnchor>(
                    XRResultStatus.StatusCode.ValidationFailure, null);
            }
            else if (subsystem == null)
            {
                return await AndroidXRSubsystemUtils.CreateAwaitableWithResult<ARShareableAnchor>(
                    XRResultStatus.StatusCode.ProviderUninitialized, null);
            }
            else if (!subsystem.running)
            {
                return await AndroidXRSubsystemUtils.CreateAwaitableWithResult<ARShareableAnchor>(
                    XRResultStatus.StatusCode.ProviderNotStarted, null);
            }

            var result = await subsystem.TryLoadAnchorAsync(shareableId, host, cancellationToken);
            var returnValue = result.status.IsSuccess() ?
                CreateTrackableImmediate(result.value) : null;
            return new Result<ARShareableAnchor>(result.status, returnValue);
        }

        /// <summary>
        /// Attempts to get a <see cref="NativeArray{SerializableGuid}"/>
        /// containing all saved persistent GUIDs from the given host.
        /// </summary>
        /// <param name="host">
        /// The host to retrieve all saved anchors.</param>
        /// <param name="allocator">
        /// The allocation strategy to use for the resulting
        /// <see cref="NativeArray{SerializableGuid}"/>.
        /// </param>
        /// <param name="cancellationToken">
        /// An optional <see cref="CancellationToken"/> that you can use to
        /// cancel the operation in progress if the loaded provider
        /// <see cref="XRShareableAnchorSubsystemDescriptor.supportsAsyncCancellation"/>.
        /// </param>
        /// <returns>
        /// The result of the async operation, containing a
        /// <see cref="NativeArray{SerializableGuid}"/> of persistent GUIDs
        /// allocated with the given <paramref name="allocator"/> if the
        /// operation succeeded.
        /// You are responsible to <see langword="await"/> this result.
        /// </returns>
        /// <exception cref="OperationCanceledException">Thrown if the operation
        /// has been canceled either by the caller or the Unity main thread.</exception>
        /// <seealso cref="XRShareableAnchorSubsystemDescriptor.supportsGetSavedAnchorIds"/>
        public Awaitable<Result<NativeArray<SerializableGuid>>> TryGetSavedAnchorIdsAsync(
            XRShareableAnchorHosts host,
            Allocator allocator,
            CancellationToken cancellationToken = default)
        {
            if (host == XRShareableAnchorHosts.None)
            {
                return AndroidXRSubsystemUtils.CreateAwaitableWithResult(
                    XRResultStatus.StatusCode.ValidationFailure,
                    default(NativeArray<SerializableGuid>));
            }
            else if (subsystem == null)
            {
                return AndroidXRSubsystemUtils.CreateAwaitableWithResult(
                    XRResultStatus.StatusCode.ProviderUninitialized,
                    default(NativeArray<SerializableGuid>));
            }
            else if (!subsystem.running)
            {
                return AndroidXRSubsystemUtils.CreateAwaitableWithResult(
                    XRResultStatus.StatusCode.ProviderNotStarted,
                    default(NativeArray<SerializableGuid>));
            }

            return subsystem.TryGetSavedAnchorIdsAsync(host, allocator, cancellationToken);
        }

        /// <summary>
        /// Attempts to create a shareable anchor "attached" to the given
        /// geospatial pose.
        /// </summary>
        /// <param name="geospatialPose">
        /// The geospatial pose of the target location.</param>
        /// <returns>
        /// The result of the operation, containing the newly added
        /// shareable anchor if the operation succeeded.
        /// The anchor will start with
        /// <see cref="XRShareableAnchorHosts.Geospatial"/> and
        /// <see cref="XRGeospatialSurfaceType.Default"/>.
        /// </returns>
        /// <seealso cref="XRShareableAnchorSubsystemDescriptor.supportsAttachGeospatialAnchor"/>
        public Result<ARShareableAnchor> TryAttachGeospatialAnchor(XRGeospatialPose geospatialPose)
        {
            if (!geospatialPose.IsCoordinateValid)
            {
                return new Result<ARShareableAnchor>(
                    new XRResultStatus(XRResultStatus.StatusCode.ValidationFailure), null);
            }
            else if (subsystem == null)
            {
                return new Result<ARShareableAnchor>(
                    new XRResultStatus(XRResultStatus.StatusCode.ProviderUninitialized), null);
            }
            else if (!subsystem.running)
            {
                return new Result<ARShareableAnchor>(
                    new XRResultStatus(XRResultStatus.StatusCode.ProviderNotStarted), null);
            }

            var result = subsystem.TryAttachGeospatialAnchor(geospatialPose);
            ARShareableAnchor returnValue = null;
            if (result.status.IsSuccess())
            {
                _activeGeospatialAnchors[result.value.trackableId] = new GeospatialData()
                {
                    SurfaceType = XRGeospatialSurfaceType.Default,
                    GeospatialPose = geospatialPose,
                };
                returnValue = CreateTrackableImmediate(result.value);
            }

            return new Result<ARShareableAnchor>(result.status, returnValue);
        }

        /// <summary>
        /// Attempts to create a shareable anchor "attached" to the given
        /// geospatial pose relative to the location's surface.
        /// </summary>
        /// <param name="type">
        /// The surface type of the geospatial anchor, determining how to
        /// apply the relative altitude from <paramref name="geospatialPose"/>.
        /// When passing <see cref="XRGeospatialSurfaceType.Default"/>, it
        /// works the same as <see cref="TryAttachGeospatialAnchor"/>.
        /// </param>
        /// <param name="geospatialPose">
        /// The geospatial pose of the target location where <see cref="XRGeospatialPose.Altitude"/>
        /// represents the altitude in meters relative to the surface.
        /// </param>
        /// <param name="cancellationToken">
        /// An optional <see cref="CancellationToken"/> that you can use to
        /// cancel the operation in progress if the loaded provider
        /// <see cref="XRShareableAnchorSubsystemDescriptor.supportsAsyncCancellation"/>.
        /// </param>
        /// <returns>
        /// The result of the async operation, containing the newly added
        /// shareable anchor if the operation succeeded.
        /// The anchor will start with
        /// <see cref="XRShareableAnchorHosts.Geospatial"/> and
        /// <paramref name="type"/>.
        /// You are responsible to <see langword="await"/> this result.</returns>
        /// <exception cref="OperationCanceledException">Thrown if the operation
        /// has been canceled either by the caller or the Unity main thread.</exception>
        /// <seealso cref="XRShareableAnchorSubsystemDescriptor.supportsAttachSurfaceAnchor"/>
        public async Awaitable<Result<ARShareableAnchor>> TryAttachSurfaceAnchorAsync(
            XRGeospatialSurfaceType type,
            XRGeospatialPose geospatialPose,
            CancellationToken cancellationToken = default)
        {
            if (!geospatialPose.IsCoordinateValid)
            {
                return await AndroidXRSubsystemUtils.CreateAwaitableWithResult<ARShareableAnchor>(
                    XRResultStatus.StatusCode.ValidationFailure, null);
            }
            else if (subsystem == null)
            {
                return await AndroidXRSubsystemUtils.CreateAwaitableWithResult<ARShareableAnchor>(
                    XRResultStatus.StatusCode.ProviderUninitialized, null);
            }
            else if (!subsystem.running)
            {
                return await AndroidXRSubsystemUtils.CreateAwaitableWithResult<ARShareableAnchor>(
                    XRResultStatus.StatusCode.ProviderNotStarted, null);
            }

            var result = await subsystem.TryAttachSurfaceAnchorAsync(
                type, geospatialPose, cancellationToken);
            ARShareableAnchor returnValue = null;
            if (result.status.IsSuccess())
            {
                _activeGeospatialAnchors[result.value.trackableId] = new GeospatialData()
                {
                    SurfaceType = type,
                    GeospatialPose = geospatialPose,
                };
                returnValue = CreateTrackableImmediate(result.value);
            }

            return new Result<ARShareableAnchor>(result.status, returnValue);
        }

        /// <summary>
        /// Get the prefab to instantiate for each <see cref="ARShareableAnchor"/>.
        /// </summary>
        /// <returns>
        /// The prefab to instantiate for each <see cref="ARShareableAnchor"/>.
        /// </returns>
        protected override GameObject GetPrefab() => _shareableAnchorPrefab;

        /// <inheritdoc/>
        protected override void Awake()
        {
            base.Awake();
            _instance = this;
        }

        /// <inheritdoc/>
        protected override void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }

            base.OnDestroy();
        }

        /// <inheritdoc/>
        protected override void OnEnable()
        {
            base.OnEnable();
            trackablesChanged.AddListener(OnTrackablesChangedInternal);
        }

        /// <inheritdoc/>
        protected override void OnDisable()
        {
            trackablesChanged.RemoveListener(OnTrackablesChangedInternal);
            base.OnDisable();
        }

        /// <inheritdoc/>
        protected override void OnAfterSetSessionRelativeData(
            ARShareableAnchor trackable, XRShareableAnchor sessionRelativeData)
        {
            if (_activeGeospatialAnchors.TryGetValue(
                sessionRelativeData.trackableId, out var geospatialData))
            {
                trackable.UpdateGeospatialAnchor(
                    geospatialData.SurfaceType, geospatialData.GeospatialPose);
            }
        }

        private void OnTrackablesChangedInternal(
            ARTrackablesChangedEventArgs<ARShareableAnchor> eventArgs)
        {
            foreach (var pair in eventArgs.removed)
            {
                _activeGeospatialAnchors.Remove(pair.Key);
            }
        }

        private struct GeospatialData
        {
            /// <summary>
            /// The initial surface type used to attach the geospatial anchor.
            /// </summary>
            public XRGeospatialSurfaceType SurfaceType;

            /// <summary>
            /// The initial geospatial pose used to attach the geospatial anchor.
            /// </summary>
            public XRGeospatialPose GeospatialPose;
        }
    }
}
