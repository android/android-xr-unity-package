// <copyright file="XRTrackableApi.cs" company="Google LLC">
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
    using System.Text;
    using UnityEngine;
    using UnityEngine.XR.ARSubsystems;

    internal class XRTrackableApi
    {
        public static void SetTracking(ApiXrTrackableType type, bool enable)
        {
            ExternalApi.XrTrackable_setTracking(XRInstanceManagerApi.GetIntPtr(), type, enable);
        }

        public static IntPtr AcquireObjectChanges(ref IntPtr added, ref uint addedCount,
            ref IntPtr updated, ref uint updatedCount, ref IntPtr removed, ref uint removedCount,
            ref uint elementSize)
        {
            IntPtr objectChanges = IntPtr.Zero;
            ExternalApi.XrTrackable_acquireObjectChanges(
                XRInstanceManagerApi.GetIntPtr(), ref objectChanges,
                ref added, ref addedCount,
                ref updated, ref updatedCount,
                ref removed, ref removedCount,
                ref elementSize);

            return objectChanges;
        }

        public static void ReleaseObjectChanges(IntPtr objectChanges)
        {
            ExternalApi.XrTrackable_destroyObjectChanges(objectChanges);
        }

        public static Vector3 GetObjectExtents(TrackableId trackableId)
        {
            Vector3 extents = new Vector3();
            ExternalApi.XrTrackable_getObjectExtents(
                XRInstanceManagerApi.GetIntPtr(), trackableId, ref extents);
            return extents;
        }

        public static XRObjectLabel GetObjectLabel(TrackableId trackableId)
        {
            XRObjectLabel label = XRObjectLabel.Unknown;
            ExternalApi.XrTrackable_getObjectLabel(
                XRInstanceManagerApi.GetIntPtr(), trackableId, ref label);
            return label;
        }

        public static void UpdateObjectReferenceGuids(Dictionary<XRObjectLabel, Guid> references)
        {
            ExternalApi.XrTrackable_resetObjectGuids(XRInstanceManagerApi.GetIntPtr());
            foreach (var (label, guid) in references)
            {
                ExternalApi.XrTrackable_setObjectGuid(
                    XRInstanceManagerApi.GetIntPtr(), label, guid.ToByteArray());
            }
        }

        public static bool TryGetQrCodeProperties(ref uint maxCount, ref bool supportEstimation)
        {
            return ExternalApi.XrTrackable_getQrCodeProperties(
                XRInstanceManagerApi.GetIntPtr(), ref maxCount, ref supportEstimation);
        }

        public static void ConfigureQrCode(Guid reference, float edge, bool staticOnly)
        {
            ExternalApi.XrTrackable_setQrCodeGuid(
                XRInstanceManagerApi.GetIntPtr(), reference.ToByteArray());
            ExternalApi.XrTrackable_configureQrCode(
                XRInstanceManagerApi.GetIntPtr(), edge, staticOnly);
        }

        public static bool IsQrCode(TrackableId trackable)
        {
            ApiXrTrackableType type = ApiXrTrackableType.NotValid;
            ExternalApi.XrTrackable_getTrackableType(
                XRInstanceManagerApi.GetIntPtr(), trackable, ref type);
            return type == ApiXrTrackableType.QrCode;
        }

        public static string GetQrCodeData(TrackableId trackable)
        {
            uint len = 0;
            ExternalApi.XrTrackable_getQrCodeData(
                XRInstanceManagerApi.GetIntPtr(), trackable, 0, ref len, null);
            if (len == 0)
            {
                return string.Empty;
            }

            StringBuilder stringBuilder = new StringBuilder((int)len);
            ExternalApi.XrTrackable_getQrCodeData(
                XRInstanceManagerApi.GetIntPtr(), trackable, (uint)stringBuilder.Capacity,
                ref len, stringBuilder);
            return stringBuilder.ToString();
        }

        public static bool TryGetMarkerProperties(ref uint maxCount, ref bool supportEstimation)
        {
            return ExternalApi.XrTrackable_getMarkerProperties(
                XRInstanceManagerApi.GetIntPtr(), ref maxCount, ref supportEstimation);
        }

        public static void ConfigureMarker(bool staticOnly, List<XRMarkerDatabaseEntry> entries)
        {
            ExternalApi.XrTrackable_configureMarker(XRInstanceManagerApi.GetIntPtr(),
                staticOnly, (uint)entries.Count, entries.ToArray());
        }

        public static bool IsMarker(TrackableId trackable)
        {
            ApiXrTrackableType type = ApiXrTrackableType.NotValid;
            ExternalApi.XrTrackable_getTrackableType(
                XRInstanceManagerApi.GetIntPtr(), trackable, ref type);
            return type == ApiXrTrackableType.Marker;
        }

        public static bool GetMarkerData(
            TrackableId trackable, ref XRMarkerDictionary dictionary, ref int id)
        {
            return ExternalApi.XrTrackable_getMarkerData(
                XRInstanceManagerApi.GetIntPtr(), trackable, ref dictionary, ref id);
        }

        public static IntPtr AcquireImageChanges(ref IntPtr added, ref uint addedCount,
            ref IntPtr updated, ref uint updatedCount, ref IntPtr removed, ref uint removedCount,
            ref uint elementSize)
        {
            IntPtr imageChanges = IntPtr.Zero;
            ExternalApi.XrTrackable_acquireImageChanges(
                XRInstanceManagerApi.GetIntPtr(), ref imageChanges,
                ref added, ref addedCount,
                ref updated, ref updatedCount,
                ref removed, ref removedCount,
                ref elementSize);

            return imageChanges;
        }

        public static void ReleaseImageChanges(IntPtr imageChanges)
        {
            ExternalApi.XrTrackable_destroyImageChanges(imageChanges);
        }

        private struct ExternalApi
        {
            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void XrTrackable_setTracking(
                IntPtr manager, ApiXrTrackableType type, bool enable);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void XrTrackable_getTrackableType(
                IntPtr manager, TrackableId trackable_id, ref ApiXrTrackableType out_type);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void XrTrackable_resetObjectGuids(IntPtr manager);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void XrTrackable_setObjectGuid(
                IntPtr manager, XRObjectLabel label, byte[] reference_guid);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrTrackable_getObjectExtents(
                IntPtr manager, TrackableId object_id, ref Vector3 out_extents);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void XrTrackable_getObjectLabel(
            IntPtr manager, TrackableId object_id, ref XRObjectLabel label);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrTrackable_acquireObjectChanges(
                IntPtr manager, ref IntPtr out_container,
                ref IntPtr out_added_list, ref uint out_added_count,
                ref IntPtr out_updated_list, ref uint out_updated_count,
                ref IntPtr out_removed_list, ref uint out_removed_count,
                ref uint out_object_size);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void XrTrackable_destroyObjectChanges(IntPtr object_changes);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrTrackable_getQrCodeProperties(
                IntPtr manager, ref uint out_max_count, ref bool out_support_estimation);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void XrTrackable_setQrCodeGuid(
                IntPtr manager, byte[] reference_guid);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void XrTrackable_configureQrCode(
                IntPtr manager, float edge, bool static_only);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void XrTrackable_getQrCodeData(
                IntPtr manager, TrackableId trackable, uint max_len, ref uint out_len,
                StringBuilder buffer);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrTrackable_getMarkerProperties(IntPtr manager,
                ref uint out_max_count, ref bool out_support_estimation);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void XrTrackable_configureMarker(
                IntPtr manager, bool static_only, uint entry_count,
                XRMarkerDatabaseEntry[] entries);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrTrackable_getMarkerData(
                IntPtr manger, TrackableId trackable, ref XRMarkerDictionary dictionary,
                ref int id);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrTrackable_acquireImageChanges(
                IntPtr manager, ref IntPtr out_container,
                ref IntPtr out_added_list, ref uint out_added_count,
                ref IntPtr out_updated_list, ref uint out_updated_count,
                ref IntPtr out_removed_list, ref uint out_removed_count,
                ref uint out_image_size);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void XrTrackable_destroyImageChanges(IntPtr plane_changes);
        }
    }
}
