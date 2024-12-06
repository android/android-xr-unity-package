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
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using UnityEngine;
    using UnityEngine.XR.ARSubsystems;

    internal class XRTrackableApi
    {
        public static void SetTracking(ApiXrTrackableType type, bool enable)
        {
            ExternalApi.XrTrackable_setTracking(XRInstanceManagerApi.GetIntPtr(), type, enable);
        }

        public static void SetPlaneDetectionMode(PlaneDetectionMode mode)
        {
            ExternalApi.XrTrackable_setPlaneDetectionMode(
                XRInstanceManagerApi.GetIntPtr(), (uint)mode);
        }

        public static PlaneDetectionMode GetPlaneDetectionMode()
        {
            uint mode = 0;
            ExternalApi.XrTrackable_getPlaneDetectionMode(
                XRInstanceManagerApi.GetIntPtr(), ref mode);
            return (PlaneDetectionMode)mode;
        }

        public static IntPtr AcquirePlaneChanges(ref IntPtr added, ref uint addedCount,
            ref IntPtr updated, ref uint updatedCount, ref IntPtr removed, ref uint removedCount,
            ref uint elementSize)
        {
            IntPtr planeChanges = IntPtr.Zero;
            if (ExternalApi.XrTrackable_acquirePlaneChanges(
                XRInstanceManagerApi.GetIntPtr(), ref planeChanges))
            {
                ExternalApi.XrPlaneChanges_getAddedCount(planeChanges, ref addedCount);
                ExternalApi.XrPlaneChanges_getAdded(planeChanges, ref added);
                ExternalApi.XrPlaneChanges_getUpdatedCount(planeChanges, ref updatedCount);
                ExternalApi.XrPlaneChanges_getUpdated(planeChanges, ref updated);
                ExternalApi.XrPlaneChanges_getRemovedCount(planeChanges, ref removedCount);
                ExternalApi.XrPlaneChanges_getRemoved(planeChanges, ref removed);
                ExternalApi.XrPlaneChanges_getPlaneSize(planeChanges, ref elementSize);
            }

            return planeChanges;
        }

        public static void ReleasePlaneChanges(IntPtr planeChanges)
        {
            ExternalApi.XrPlaneChanges_destroy(planeChanges);
        }

        public static uint GetBoundaySize(TrackableId planeId)
        {
            uint size = 0;
            IntPtr nativeList = IntPtr.Zero;
            ExternalApi.XrTrackable_acquirePlaneBoundary(
                XRInstanceManagerApi.GetIntPtr(), planeId, ref size, nativeList);
            return size;
        }

        public static unsafe bool AcquireBoundary(
            TrackableId planeId, NativeArray<Vector2> boundary)
        {
            uint size = (uint)boundary.Length;
            return ExternalApi.XrTrackable_acquirePlaneBoundary(
                XRInstanceManagerApi.GetIntPtr(),
                planeId, ref size, (IntPtr)boundary.GetUnsafePtr());
        }

        public static IntPtr AcquireObjectChanges(ref IntPtr added, ref uint addedCount,
            ref IntPtr updated, ref uint updatedCount, ref IntPtr removed, ref uint removedCount,
            ref uint elementSize)
        {
            IntPtr objectChanges = IntPtr.Zero;
            if (ExternalApi.XrTrackable_acquireObjectChanges(
                XRInstanceManagerApi.GetIntPtr(), ref objectChanges))
            {
                ExternalApi.XrObjectChanges_getAddedCount(objectChanges, ref addedCount);
                ExternalApi.XrObjectChanges_getAdded(objectChanges, ref added);
                ExternalApi.XrObjectChanges_getUpdatedCount(objectChanges, ref updatedCount);
                ExternalApi.XrObjectChanges_getUpdated(objectChanges, ref updated);
                ExternalApi.XrObjectChanges_getRemovedCount(objectChanges, ref removedCount);
                ExternalApi.XrObjectChanges_getRemoved(objectChanges, ref removed);
                ExternalApi.XrObjectChanges_getObjectSize(objectChanges, ref elementSize);
            }

            return objectChanges;
        }

        public static void ReleaseObjectChanges(IntPtr objectChanges)
        {
            ExternalApi.XrObjectChanges_destroy(objectChanges);
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

        public static void UpdateReferenceGuids(Dictionary<XRObjectLabel, Guid> references)
        {
            ExternalApi.XrTrackable_resetReferenceGuids(XRInstanceManagerApi.GetIntPtr());
            foreach (var (label, guid) in references)
            {
                ExternalApi.XrTrackable_setReferenceGuid(
                    XRInstanceManagerApi.GetIntPtr(), label, guid.ToByteArray());
            }
        }

        private struct ExternalApi
        {
            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void XrTrackable_setTracking(
                IntPtr manager, ApiXrTrackableType type, bool enable);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void XrTrackable_setPlaneDetectionMode(IntPtr manager, uint mode);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void XrTrackable_getPlaneDetectionMode(
                IntPtr manager, ref uint out_mode);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrTrackable_acquirePlaneChanges(
                IntPtr manager, ref IntPtr out_container);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrPlaneChanges_getAdded(
                IntPtr plane_changes, ref IntPtr out_list);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrPlaneChanges_getAddedCount(
                IntPtr plane_changes, ref uint out_count);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrPlaneChanges_getUpdated(
                IntPtr plane_changes, ref IntPtr out_list);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrPlaneChanges_getUpdatedCount(
                IntPtr plane_changes, ref uint out_count);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrPlaneChanges_getRemoved(
                IntPtr plane_changes, ref IntPtr out_list);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrPlaneChanges_getRemovedCount(
                IntPtr plane_changes, ref uint out_count);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrPlaneChanges_getPlaneSize(
                IntPtr plane_changes, ref uint out_size);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void XrPlaneChanges_destroy(IntPtr plane_changes);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrTrackable_acquirePlaneBoundary(
                IntPtr manager, TrackableId plane_id, ref uint out_vertex_count,
                IntPtr boundary_list);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void XrTrackable_resetReferenceGuids(IntPtr manager);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void XrTrackable_setReferenceGuid(
                IntPtr manager, XRObjectLabel label, byte[] reference_guid);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrTrackable_getObjectExtents(
                IntPtr manager, TrackableId object_id, ref Vector3 out_extents);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void XrTrackable_getObjectLabel(
            IntPtr manager, TrackableId object_id, ref XRObjectLabel label);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrTrackable_acquireObjectChanges(
                IntPtr manager, ref IntPtr out_container);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrObjectChanges_getAdded(
                IntPtr object_changes, ref IntPtr out_list);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrObjectChanges_getAddedCount(
                IntPtr object_changes, ref uint out_count);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrObjectChanges_getUpdated(
                IntPtr object_changes, ref IntPtr out_list);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrObjectChanges_getUpdatedCount(
                IntPtr object_changes, ref uint out_count);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrObjectChanges_getRemoved(
                IntPtr object_changes, ref IntPtr out_list);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrObjectChanges_getRemovedCount(
                IntPtr object_changes, ref uint out_count);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrObjectChanges_getObjectSize(
                IntPtr object_changes, ref uint out_size);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void XrObjectChanges_destroy(IntPtr object_changes);
        }
    }
}
