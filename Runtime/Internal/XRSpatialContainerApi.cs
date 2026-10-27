// <copyright file="XRSpatialContainerApi.cs" company="Google LLC">
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

namespace Google.XR.Extensions.Internal
{
    using System;
    using System.Runtime.InteropServices;
    using Google.XR.Extensions;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using UnityEngine;
    using UnityEngine.XR.OpenXR.NativeTypes;

    internal class XRSpatialContainerApi
    {
        public static bool TryGetState(out XRSpatialContainerStateFlags state)
        {
            state = XRSpatialContainerStateFlags.None;
            return ExternalApi.XrSpatialContainer_getState(
                XRInstanceManagerApi.GetIntPtr(), out state);
        }

        public static void Configure(ref Vector3 bounds, SpatialContainerChangedDelegate callback)
        {
            ExternalApi.XrSpatialContainer_configure(
                XRInstanceManagerApi.GetIntPtr(), ref bounds, callback);
        }

        public static bool TryGetBounds(ref Vector3 outBounds)
        {
            return ExternalApi.XrSpatialContainer_getBounds(
                XRInstanceManagerApi.GetIntPtr(), ref outBounds);
        }

        public static bool TryRequestBoundsMode(bool immersive)
        {
            return ExternalApi.XrSpatialContainer_requestBoundsMode(
                XRInstanceManagerApi.GetIntPtr(), immersive);
        }

        public static bool TryRequestVisibility(bool visible)
        {
            return ExternalApi.XrSpatialContainer_requestVisibility(
                XRInstanceManagerApi.GetIntPtr(), visible);
        }

        private struct ExternalApi
        {
            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrSpatialContainer_getState(
                IntPtr manager, out XRSpatialContainerStateFlags state);
            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void XrSpatialContainer_configure(
                IntPtr manager, ref Vector3 bounds, SpatialContainerChangedDelegate callback);
            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrSpatialContainer_getBounds(
                IntPtr manager, ref Vector3 outBounds);
            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrSpatialContainer_requestBoundsMode(
                IntPtr manager, bool immersive);
            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrSpatialContainer_requestVisibility(
                IntPtr manager, bool visible);
        }
    }
}
