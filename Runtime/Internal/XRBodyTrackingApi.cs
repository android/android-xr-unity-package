// <copyright file="XRBodyTrackingApi.cs" company="Google LLC">
//
// Copyright 2025 Google LLC
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

    internal class XRBodyTrackingApi
    {
        public static void SetTracking(bool enable)
        {
            ExternalApi.XrBodyTracking_setTrackingEnabled(XRInstanceManagerApi.GetIntPtr(), enable);
        }

        public static void SetCalibration(bool automatic, XRHumanBodyProportions proportions)
        {
            ExternalApi.XrBodyTracking_setCalibration(
                XRInstanceManagerApi.GetIntPtr(), automatic, ref proportions);
        }

        public static unsafe void GetSkeleton(TrackableId id, NativeArray<XRHumanBodyJoint> joints)
        {
            uint size = (uint)joints.Length;
            ExternalApi.XrBodyTracking_getSkeleton(
                XRInstanceManagerApi.GetIntPtr(), id, ref size, (IntPtr)joints.GetUnsafePtr());
        }

        public static IntPtr AcquireChanges(ref IntPtr outAddedList, ref uint outAddedCount,
                                            ref IntPtr outUpdatedList, ref uint outUpdatedCount,
                                            ref IntPtr outRemovedList, ref uint outRemovedCount,
                                            ref uint outBodySize)
        {
            IntPtr bodyChanges = IntPtr.Zero;
            ExternalApi.XrBodyTracking_acquireChanges(
                XRInstanceManagerApi.GetIntPtr(), ref bodyChanges, ref outAddedList,
                ref outAddedCount, ref outUpdatedList, ref outUpdatedCount, ref outRemovedList,
                ref outRemovedCount, ref outBodySize);

            return bodyChanges;
        }

        public static void ReleaseChanges(IntPtr bodyChanges)
        {
            ExternalApi.XrBodyTracking_destroyChanges(bodyChanges);
        }

        private struct ExternalApi
        {
            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void XrBodyTracking_setTrackingEnabled(
                IntPtr manager, bool enabled);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void XrBodyTracking_setCalibration(
                IntPtr manager, bool automatic, ref XRHumanBodyProportions proportions);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void XrBodyTracking_getSkeleton(
                IntPtr manager, TrackableId id, ref uint joint_count, IntPtr out_joints);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrBodyTracking_acquireChanges(
                IntPtr manager, ref IntPtr out_container, ref IntPtr out_added_list,
                ref uint out_added_count, ref IntPtr out_updated_list, ref uint out_updated_count,
                ref IntPtr out_removed_list, ref uint out_removed_count, ref uint out_body_size);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void XrBodyTracking_destroyChanges(IntPtr anchor_changes);
        }
    }
}
