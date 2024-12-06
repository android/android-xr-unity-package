// <copyright file="AndroidXRAnchorSubsystem.cs" company="Google LLC">
//
// Copyright 2024 Google LLC
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
    using Google.XR.Extensions.Internal;
    using Unity.Collections;
    using UnityEngine;
    using UnityEngine.Scripting;
    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.ARSubsystems;

    /// <summary>
    /// The Android XR implementation of the <see cref="XRAnchorSubsystem"/> so it can work
    /// seamlessly with <see cref="ARAnchorManager"/>.
    /// </summary>
    [Preserve]
    public sealed class AndroidXRAnchorSubsystem : XRAnchorSubsystem
    {
        private Dictionary<TrackableId, Guid> _persistentAnchors =
            new Dictionary<TrackableId, Guid>();

        internal static string _id => AndroidXRProvider._id;

        /// <summary>
        /// Gets the persistent id associated with the given anchor. It can be used in
        /// <see cref="TryLoad(Guid)"/> to retrieve the persisted anchor across multiple sessions.
        /// If the <paramref name="anchor"/> is not persisted, returns <c>Guid.Empty</c>.
        /// </summary>
        /// <param name="anchor">The anchor to get the persistent id.</param>
        /// <returns>The persistent id of the given anchor.</returns>
        public Guid GetPersistentId(TrackableId anchor)
        {
            if (!_persistentAnchors.ContainsKey(anchor))
            {
                _persistentAnchors[anchor] =
                    XRAnchorApi.GetPersistentId(anchor);
            }

            return _persistentAnchors[anchor];
        }

        /// <summary>
        /// Gets all persisted ids that currently are saved on device's local storage,
        /// it can be used in <see cref="TryLoad(Guid)"/> to retrieve the persisted anchors across
        /// multiple sessions or removed by <see cref="Unpersist(Guid)"/>.
        /// </summary>
        /// <param name="guids">A list of all persisted ids.</param>
        /// <returns>Returns a boolean to indicate whether successfully retrieved all stored ids.
        /// </returns>
        public bool TryGetAllPersistedIds(ref List<Guid> guids)
        {
            return XRAnchorApi.TryAcquirePersistedIds(ref guids);
        }

        /// <summary>
        /// Try to load a persisted anchor from the given <paramref name="guid"/>.
        /// </summary>
        /// <param name="guid">The id associated with a persisted anchor.</param>
        /// <returns>Returns a boolean to indicate whether successfully loaded the given
        /// <paramref name="guid"/>.</returns>
        public bool TryLoad(Guid guid)
        {
            return XRAnchorApi.TryLoad(guid);
        }

        /// <summary>
        /// Try to save the <paramref name="anchor"/> on device's local storage.
        /// </summary>
        /// <param name="anchor">The anchor to save.</param>
        /// <returns>Returns a boolen to indicate whether successfully saved the anchor on device's
        /// local storage.</returns>
        public bool Persist(TrackableId anchor)
        {
            Guid guid = new Guid();
            if (XRAnchorApi.PersistAnchor(anchor, ref guid))
            {
                _persistentAnchors[anchor] = guid;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Try to remove a persisted anchor from device's local storage.
        /// </summary>
        /// <param name="guid">The persistent id associated with the anchor to remove.</param>
        /// <returns>Returns a boolean to indicate whether successfully removed the anchor from
        /// device's local storage.</returns>
        public bool Unpersist(Guid guid)
        {
            if (guid == Guid.Empty)
            {
                return false;
            }

            return XRAnchorApi.UnpersistByUuid(guid);
        }

        /// <summary>
        /// Try to remove a persisted anchor from device's local storage.
        /// </summary>
        /// <param name="trackableId">The anchor to remove.</param>
        /// <returns>Returns a boolean to indicate whether successfully removed the anchor from
        /// device's local storage.</returns>
        public bool Unpersist(TrackableId trackableId)
        {
            return XRAnchorApi.UnpersistByAnchor(trackableId);
        }

        /// <summary>
        /// Get persistence state of the given anchor.
        /// </summary>
        /// <param name="trackableId">The anchor to get state from.</param>
        /// <param name="state">The state of the anchor.</param>
        /// <returns>Returns a boolean to indicate whether successfully got persistence state.
        /// </returns>
        public bool GetPersistState(TrackableId trackableId, out XRAnchorPersistStates state)
        {
            return XRAnchorApi.GetPersistStateByAnchor(trackableId, out state);
        }

        /// <summary>
        /// Get persistence state of the given anchor by GUID.
        /// </summary>
        /// <param name="guid">The GUID associated with the anchor to get state from.</param>
        /// <param name="state">The state of the anchor.</param>
        /// <returns>Returns a boolean to indicate whether successfully got persistence state.
        /// </returns>
        public bool GetPersistState(Guid guid, out XRAnchorPersistStates state)
        {
            return XRAnchorApi.GetPersistStateByUuid(guid, out state);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterDescriptor()
        {
#pragma warning disable CS0162 // Unreachable code detected: used at runtime.
            if (!ApiConstants.AndroidPlatform)
            {
                return;
            }

            XRAnchorSubsystemDescriptor.Cinfo cinfo = new XRAnchorSubsystemDescriptor.Cinfo
            {
                id = AndroidXRProvider._id,
                providerType = typeof(AndroidXRAnchorSubsystem.AndroidXRProvider),
                subsystemTypeOverride = typeof(AndroidXRAnchorSubsystem),
                supportsTrackableAttachments = true
            };

            XRAnchorSubsystemDescriptor.Register(cinfo);
#pragma warning restore CS0162
        }

        [SuppressMessage(
            "UnityRules.UnityStyleRules", "US1109:PublicPropertiesMustBeUpperCamelCase",
            Justification = "Override Unity interface.")]
        class AndroidXRProvider : Provider
        {
            internal static readonly string _id = "AndroidXR-Anchor";

            /// <inheritdoc/>
            public override void Start()
            {
                // Currently, all subsystem lifecycle events are handled by general OpenXR events.
            }

            /// <inheritdoc/>
            public override void Stop()
            {
                // Currently, all subsystem lifecycle events are handled by general OpenXR events.
            }

            /// <inheritdoc/>
            public override void Destroy()
            {
                // Currently, all subsystem lifecycle events are handled by general OpenXR events.
            }

            /// <inheritdoc/>
            public override bool TryAddAnchor(Pose pose, out XRAnchor anchor)
            {
                Debug.Log("AndroidXRAnchorSubsystem: try add anchor at " + pose.ToString());
                return XRAnchorApi.TryAdd(pose, out anchor);
            }

            /// <inheritdoc/>
            public override bool TryAttachAnchor(
                TrackableId trackableToAffix, Pose pose, out XRAnchor anchor)
            {
                return XRAnchorApi.TryAttach(trackableToAffix, pose, out anchor);
            }

            /// <inheritdoc/>
            public override bool TryRemoveAnchor(TrackableId anchorId)
            {
                return XRAnchorApi.TryRemove(anchorId);
            }

            /// <inheritdoc/>
            public unsafe override TrackableChanges<XRAnchor> GetChanges(
                XRAnchor defaultAnchor, Allocator allocator)
            {
                IntPtr added = IntPtr.Zero;
                IntPtr updated = IntPtr.Zero;
                IntPtr removed = IntPtr.Zero;
                uint addedCount = 0;
                uint updatedCount = 0;
                uint removedCount = 0;
                uint elementSize = 0;
                IntPtr anchorChanges = XRAnchorApi.AcquireChanges(
                    ref added, ref addedCount, ref updated, ref updatedCount, ref removed,
                    ref removedCount, ref elementSize);

                if (anchorChanges == IntPtr.Zero)
                {
                    return new TrackableChanges<XRAnchor>(0, 0, 0, allocator);
                }

                try
                {
                    return new TrackableChanges<XRAnchor>(
                        added.ToPointer(), (int)addedCount,
                        updated.ToPointer(), (int)updatedCount,
                        removed.ToPointer(), (int)removedCount,
                        defaultAnchor, (int)elementSize, allocator);
                }
                finally
                {
                    XRAnchorApi.ReleaseChanges(anchorChanges);
                }
            }
        }
    }
}
