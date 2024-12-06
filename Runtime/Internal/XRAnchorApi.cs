// <copyright file="XRAnchorApi.cs" company="Google LLC">
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

namespace Google.XR.Extensions.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using UnityEngine;
    using UnityEngine.XR.ARSubsystems;

    internal class XRAnchorApi
    {
        private static byte[] _uuidBuffer = new byte[16];

        public static bool TryAdd(Pose pose, out XRAnchor anchor)
        {
            anchor = new XRAnchor();
            return ExternalApi.XrAnchor_tryAdd(
                XRInstanceManagerApi.GetIntPtr(), pose, ref anchor);
        }

        public static bool TryLoad(Guid guid)
        {
            return ExternalApi.XrAnchor_tryLoadUuid(
                XRInstanceManagerApi.GetIntPtr(), guid.ToByteArray());
        }

        public static bool TryAttach(TrackableId trackableId, Pose pose, out XRAnchor anchor)
        {
            anchor = new XRAnchor();
            return ExternalApi.XrAnchor_tryAttach(
                XRInstanceManagerApi.GetIntPtr(), pose, trackableId, ref anchor);
        }

        public static bool TryRemove(TrackableId anchorId)
        {
            return ExternalApi.XrAnchor_tryRemove(XRInstanceManagerApi.GetIntPtr(), anchorId);
        }

        public static IntPtr AcquireChanges(ref IntPtr added, ref uint addedCount,
            ref IntPtr updated, ref uint updatedCount, ref IntPtr removed, ref uint removedCount,
            ref uint elementSize)
        {
            IntPtr anchorChange = IntPtr.Zero;
            if (ExternalApi.XrAnchor_acquireChanges(
                XRInstanceManagerApi.GetIntPtr(), ref anchorChange))
            {
                ExternalApi.XrAnchorChanges_getAddedCount(anchorChange, ref addedCount);
                ExternalApi.XrAnchorChanges_getAdded(anchorChange, ref added);
                ExternalApi.XrAnchorChanges_getUpdatedCount(anchorChange, ref updatedCount);
                ExternalApi.XrAnchorChanges_getUpdated(anchorChange, ref updated);
                ExternalApi.XrAnchorChanges_getRemovedCount(anchorChange, ref removedCount);
                ExternalApi.XrAnchorChanges_getRemoved(anchorChange, ref removed);
                ExternalApi.XrAnchorChanges_getAnchorSize(anchorChange, ref elementSize);
            }

            return anchorChange;
        }

        public static bool TryAcquirePersistedIds(ref List<Guid> guids)
        {
            IntPtr uuidList = IntPtr.Zero;
            if (!ExternalApi.XrAnchor_acquirePersistedIds(
                XRInstanceManagerApi.GetIntPtr(), ref uuidList))
            {
                return false;
            }

            uint size = 0;
            guids.Clear();
            ExternalApi.XrUuidList_getSize(uuidList, ref size);
            for (uint i = 0; i < size; ++i)
            {
                if (ExternalApi.XrUuidList_getItem(uuidList, i, _uuidBuffer))
                {
                    Guid uuid = new Guid(_uuidBuffer);
                    guids.Add(uuid);
                }
            }

            ExternalApi.XrUuidList_destroy(uuidList);
            return true;
        }

        public static void ReleaseChanges(IntPtr anchorChanges)
        {
            ExternalApi.XrAnchorChanges_destroy(anchorChanges);
        }

        public static bool SetPersistence(bool enabled)
        {
            return ExternalApi.XrAnchor_setPersistence(XRInstanceManagerApi.GetIntPtr(), enabled);
        }

        public static Guid GetPersistentId(TrackableId anchor)
        {
            if (ExternalApi.XrAnchor_getPersistentUuid(
                XRInstanceManagerApi.GetIntPtr(), anchor, _uuidBuffer))
            {
                return new Guid(_uuidBuffer);
            }

            return Guid.Empty;
        }

        public static bool PersistAnchor(TrackableId trackableId, ref Guid guid)
        {
            if (ExternalApi.XrAnchor_persist(
                XRInstanceManagerApi.GetIntPtr(), trackableId, _uuidBuffer))
            {
                guid = new Guid(_uuidBuffer);
                return true;
            }

            return false;
        }

        public static bool UnpersistByAnchor(TrackableId anchorId)
        {
            return ExternalApi.XrAnchor_unpersistByAnchor(
                XRInstanceManagerApi.GetIntPtr(), anchorId);
        }

        public static bool UnpersistByUuid(Guid guid)
        {
            return ExternalApi.XrAnchor_unpersistByUuid(
                XRInstanceManagerApi.GetIntPtr(), guid.ToByteArray());
        }

        public static bool GetPersistStateByAnchor(
            TrackableId anchorId, out XRAnchorPersistStates state)
        {
            state = XRAnchorPersistStates.NotRequested;
            return ExternalApi.XrAnchor_getPersistStateByAnchor(
                XRInstanceManagerApi.GetIntPtr(), anchorId, ref state);
        }

        public static bool GetPersistStateByUuid(Guid guid, out XRAnchorPersistStates state)
        {
            state = XRAnchorPersistStates.NotRequested;
            return ExternalApi.XrAnchor_getPersistStateByUuid(
                XRInstanceManagerApi.GetIntPtr(), guid.ToByteArray(), ref state);
        }

        private struct ExternalApi
        {
            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrAnchor_tryAdd(
                IntPtr manager, Pose pose, ref XRAnchor out_anchor);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrAnchor_tryAttach(
                IntPtr manager, Pose pose, TrackableId trackable_id, ref XRAnchor out_anchor);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrAnchor_tryRemove(IntPtr manager, TrackableId trackable_id);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrAnchor_acquireChanges(
                IntPtr manager, ref IntPtr out_container);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrAnchorChanges_getAdded(
                IntPtr anchor_changes, ref IntPtr out_list);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrAnchorChanges_getAddedCount(
                IntPtr anchor_changes, ref uint out_count);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrAnchorChanges_getUpdated(
                IntPtr anchor_changes, ref IntPtr out_list);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrAnchorChanges_getUpdatedCount(
                IntPtr anchor_changes, ref uint out_count);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrAnchorChanges_getRemoved(
                IntPtr anchor_changes, ref IntPtr out_list);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrAnchorChanges_getRemovedCount(
                IntPtr anchor_changes, ref uint out_count);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrAnchorChanges_getAnchorSize(
                IntPtr anchor_changes, ref uint out_size);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void XrAnchorChanges_destroy(IntPtr anchor_changes);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrAnchor_setPersistence(IntPtr manager, bool enable);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrAnchor_acquirePersistedIds(IntPtr manager,
                                  ref IntPtr out_list);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrUuidList_getSize(IntPtr list, ref uint out_count);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrUuidList_getItem(IntPtr list, uint index,
                        byte[] out_uuid);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void XrUuidList_destroy(IntPtr list);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrAnchor_tryLoadUuid(IntPtr manager, byte[] uuid);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrAnchor_getPersistentUuid(
                IntPtr manager, TrackableId anchor_id, byte[] out_uuid);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrAnchor_persist(
                IntPtr manager, TrackableId anchor_id, byte[] out_uuid);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrAnchor_unpersistByAnchor(
                IntPtr manager, TrackableId anchor_id);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrAnchor_unpersistByUuid(IntPtr manager, byte[] uuid);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrAnchor_getPersistStateByAnchor(
                IntPtr manager, TrackableId anchor_id, ref XRAnchorPersistStates out_state);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrAnchor_getPersistStateByUuid(
                IntPtr manager, byte[] uuid, ref XRAnchorPersistStates out_state);
        }
    }
}
