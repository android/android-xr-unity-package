// <copyright file="XRShareableAnchorSubsystem.cs" company="Google LLC">
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

namespace Google.XR.Extensions.ARSubsystems
{
    using System.Threading;
    using Google.XR.Extensions.Internal;
    using Unity.Collections;
    using UnityEngine;
    using UnityEngine.SubsystemsImplementation;
    using UnityEngine.XR.ARSubsystems;

    /// <summary>
    /// This subsystem provides information regarding shareable anchors.
    /// A shareable anchor is a pose (position and rotation) in the physical
    /// environment that is tracked by an XR device. It can then be saved on a
    /// host for sharing among different clients, so that each client can
    /// retrieve the same anchor from the physical environment.
    /// Anchors are updated as the device refines its understanding of the
    /// environment, allowing you to reliably place virtual content at a
    /// real-world pose and sync among all clients.
    /// </summary>
    /// <remarks>
    /// This base class must be implemented by providers that supply actual
    /// shareable anchor capabilities.
    /// </remarks>
    public class XRShareableAnchorSubsystem :
        TrackingSubsystem<XRShareableAnchor,
                          XRShareableAnchorSubsystem,
                          XRShareableAnchorSubsystemDescriptor,
                          XRShareableAnchorSubsystem.Provider>
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        ValidationUtility<XRShareableAnchor> _validationUtility = new();
#endif

        /// <summary>
        /// Reserved for test only.
        /// </summary>
        public XRShareableAnchorSubsystem()
        {
        }

        /// <inheritdoc/>
        public override TrackableChanges<XRShareableAnchor> GetChanges(Allocator allocator)
        {
            var changes = provider.GetChanges(XRShareableAnchor.defaultValue, allocator);
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            _validationUtility.ValidateAndDisposeIfThrown(changes);
#endif
            return changes;
        }

        /// <summary>
        /// Attempts to create a new anchor at the given <paramref name="pose"/>.
        /// </summary>
        /// <param name="pose">
        /// The pose, in Unity world space, of the new anchor.</param>
        /// <param name="anchor">
        /// The output anchor. Valid only if this method returns
        /// <see langword="true"/>. The anchor will start with
        /// <see cref="XRShareableAnchorHosts.None"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the new anchor was added.
        /// Otherwise, <see langword="false"/>.
        /// </returns>
        public bool TryAddAnchor(Pose pose, out XRShareableAnchor anchor)
        {
            return provider.TryAddAnchor(pose, out anchor);
        }

        /// <summary>
        /// Attempts to remove an existing anchor with <paramref name="anchorId"/>.
        /// If it has been saved by <see cref="TrySaveAnchorAsync"/>, the
        /// persistent data is not affected.
        /// </summary>
        /// <param name="anchorId">
        /// The <see cref="TrackableId"/> of an existing anchor to remove.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the anchor was removed.
        /// Otherwise, <see langword="false"/>.
        /// </returns>
        public bool TryRemoveAnchor(TrackableId anchorId)
        {
            return provider.TryRemoveAnchor(anchorId);
        }

        /// <summary>
        /// Attempts to persistently save the given anchor so that it can be
        /// loaded in a future session.
        /// Use the <see cref="SerializableGuid"/> returned by this method
        /// as an input parameter to <see cref="TryLoadAnchorAsync"/> or
        /// <see cref="TryEraseAnchorAsync"/>.
        /// </summary>
        /// <param name="anchorId">
        /// The <see cref="TrackableId"/> of the shareable anchor to save.
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
        /// You are responsible to <see langword="await"/> this result.
        /// </returns>
        /// <seealso cref="XRShareableAnchorSubsystemDescriptor.supportsSaveAnchor"/>
        public Awaitable<Result<SerializableGuid>> TrySaveAnchorAsync(
            TrackableId anchorId,
            XRShareableAnchorHosts host,
            CancellationToken cancellationToken = default)
        {
            return provider.TrySaveAnchorAsync(anchorId, host, cancellationToken);
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
        /// You are responsible to <see langword="await"/> this result.
        /// </returns>
        /// <seealso cref="XRShareableAnchorSubsystemDescriptor.supportsEraseAnchor"/>
        public Awaitable<XRResultStatus> TryEraseAnchorAsync(
            SerializableGuid shareableId,
            XRShareableAnchorHosts host,
            CancellationToken cancellationToken = default)
        {
            return provider.TryEraseAnchorAsync(shareableId, host, cancellationToken);
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
        /// You are responsible to <see langword="await"/> this result.</returns>
        /// <seealso cref="XRShareableAnchorSubsystemDescriptor.supportsLoadAnchor"/>
        public Awaitable<Result<XRShareableAnchor>> TryLoadAnchorAsync(
            SerializableGuid shareableId,
            XRShareableAnchorHosts host,
            CancellationToken cancellationToken = default)
        {
            return provider.TryLoadAnchorAsync(shareableId, host, cancellationToken);
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
        /// <seealso cref="XRShareableAnchorSubsystemDescriptor.supportsGetSavedAnchorIds"/>
        public Awaitable<Result<NativeArray<SerializableGuid>>> TryGetSavedAnchorIdsAsync(
            XRShareableAnchorHosts host,
            Allocator allocator,
            CancellationToken cancellationToken = default)
        {
            return provider.TryGetSavedAnchorIdsAsync(
                host, allocator, cancellationToken);
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
        public Result<XRShareableAnchor> TryAttachGeospatialAnchor(XRGeospatialPose geospatialPose)
        {
            return provider.TryAttachGeospatialAnchor(geospatialPose);
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
        /// <seealso cref="XRShareableAnchorSubsystemDescriptor.supportsAttachSurfaceAnchor"/>
        public Awaitable<Result<XRShareableAnchor>> TryAttachSurfaceAnchorAsync(
            XRGeospatialSurfaceType type,
            XRGeospatialPose geospatialPose,
            CancellationToken cancellationToken = default)
        {
            return provider.TryAttachSurfaceAnchorAsync(
                type, geospatialPose, cancellationToken);
        }

        /// <summary>
        /// An abstract class to be implemented by providers of this subsystem.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "UnityRules.UnityStyleRules", "US1300:LinesMustBe100CharactersOrShorter",
            Justification = "In-code reference.")]
        public abstract class Provider : SubsystemProvider<XRShareableAnchorSubsystem>
        {
            /// <summary>
            /// Gets the changes (added, updated, and removed anchors) since
            /// the last call.
            /// </summary>
            /// <param name="defaultAnchor">
            /// The default shareable anchor instance (used for memory layout).
            /// </param>
            /// <param name="allocator">
            /// The allocator used for the returned data.</param>
            /// <returns>
            /// A <see cref="TrackableChanges{XRShareableAnchor}"/> describing
            /// changes since the last call.
            /// </returns>
            public abstract TrackableChanges<XRShareableAnchor> GetChanges(
                XRShareableAnchor defaultAnchor,
                Allocator allocator);

            /// <summary>
            /// Attempts to create a new anchor at the given <paramref name="pose"/>.
            /// </summary>
            /// <param name="pose">
            /// The pose, in Unity world space, of the new anchor.</param>
            /// <param name="anchor">
            /// The output anchor. Valid only if this method returns
            /// <see langword="true"/>. The anchor will start with
            /// <see cref="XRShareableAnchorHosts.None"/>.
            /// </param>
            /// <returns>
            /// <see langword="true"/> if the new anchor was added.
            /// Otherwise, <see langword="false"/>.
            /// </returns>
            public abstract bool TryAddAnchor(Pose pose, out XRShareableAnchor anchor);

            /// <summary>
            /// Attempts to remove an existing anchor with <paramref name="anchorId"/>.
            /// If it has been saved by <see cref="TrySaveAnchorAsync"/>, the
            /// persistent data is not affected.
            /// </summary>
            /// <param name="anchorId">
            /// The <see cref="TrackableId"/> of an existing anchor to remove.
            /// </param>
            /// <returns>
            /// <see langword="true"/> if the anchor was removed.
            /// Otherwise, <see langword="false"/>.
            /// </returns>
            public abstract bool TryRemoveAnchor(TrackableId anchorId);

            /// <summary>
            /// Attempts to persistently save the given anchor so that it can be
            /// loaded in a future session.
            /// Use the <see cref="SerializableGuid"/> returned by this method
            /// as an input parameter to <see cref="TryLoadAnchorAsync"/> or
            /// <see cref="TryEraseAnchorAsync"/>.
            /// </summary>
            /// <param name="anchorId">
            /// The <see cref="TrackableId"/> of the shareable anchor to save.
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
            /// You are responsible to <see langword="await"/> this result.
            /// </returns>
            /// <seealso cref="XRShareableAnchorSubsystemDescriptor.supportsSaveAnchor"/>
            public virtual Awaitable<Result<SerializableGuid>> TrySaveAnchorAsync(
                TrackableId anchorId,
                XRShareableAnchorHosts host,
                CancellationToken cancellationToken = default)
            {
                return AndroidXRSubsystemUtils.CreateAwaitableWithResult(
                    XRResultStatus.StatusCode.Unsupported,
                    SerializableGuid.empty);
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
            /// You are responsible to <see langword="await"/> this result.
            /// </returns>
            /// <seealso cref="XRShareableAnchorSubsystemDescriptor.supportsEraseAnchor"/>
            public virtual Awaitable<XRResultStatus> TryEraseAnchorAsync(
                SerializableGuid shareableId,
                XRShareableAnchorHosts host,
                CancellationToken cancellationToken = default)
            {
                return AndroidXRSubsystemUtils.CreateAwaitableWithStatus(
                    XRResultStatus.StatusCode.Unsupported);
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
            /// You are responsible to <see langword="await"/> this result.</returns>
            /// <seealso cref="XRShareableAnchorSubsystemDescriptor.supportsLoadAnchor"/>
            public virtual Awaitable<Result<XRShareableAnchor>> TryLoadAnchorAsync(
                SerializableGuid shareableId,
                XRShareableAnchorHosts host,
                CancellationToken cancellationToken = default)
            {
                return AndroidXRSubsystemUtils.CreateAwaitableWithResult(
                    XRResultStatus.StatusCode.Unsupported,
                    XRShareableAnchor.defaultValue);
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
            /// <seealso cref="XRShareableAnchorSubsystemDescriptor.supportsGetSavedAnchorIds"/>
            public virtual Awaitable<Result<NativeArray<SerializableGuid>>>
                TryGetSavedAnchorIdsAsync(
                    XRShareableAnchorHosts host,
                    Allocator allocator,
                    CancellationToken cancellationToken = default)
            {
                return AndroidXRSubsystemUtils.CreateAwaitableWithResult(
                    XRResultStatus.StatusCode.Unsupported,
                    default(NativeArray<SerializableGuid>));
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
            /// </returns>
            /// <seealso cref="XRShareableAnchorSubsystemDescriptor.supportsAttachGeospatialAnchor"/>
            public virtual Result<XRShareableAnchor> TryAttachGeospatialAnchor(
                XRGeospatialPose geospatialPose)
            {
                var status = new XRResultStatus(XRResultStatus.StatusCode.Unsupported);
                return new Result<XRShareableAnchor>(status, XRShareableAnchor.defaultValue);
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
            /// The geospatial pose of the target location.</param>
            /// <param name="cancellationToken">
            /// An optional <see cref="CancellationToken"/> that you can use to
            /// cancel the operation in progress if the loaded provider
            /// <see cref="XRShareableAnchorSubsystemDescriptor.supportsAsyncCancellation"/>.
            /// </param>
            /// <returns>
            /// The result of the async operation, containing the newly added
            /// shareable anchor if the operation succeeded.
            /// You are responsible to <see langword="await"/> this result.</returns>
            /// <seealso cref="XRShareableAnchorSubsystemDescriptor.supportsAttachSurfaceAnchor"/>
            public virtual Awaitable<Result<XRShareableAnchor>> TryAttachSurfaceAnchorAsync(
                XRGeospatialSurfaceType type,
                XRGeospatialPose geospatialPose,
                CancellationToken cancellationToken = default)
            {
                return AndroidXRSubsystemUtils.CreateAwaitableWithResult(
                    XRResultStatus.StatusCode.Unsupported,
                    XRShareableAnchor.defaultValue);
            }
        }
    }
}
