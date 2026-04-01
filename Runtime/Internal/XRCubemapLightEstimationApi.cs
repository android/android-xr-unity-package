// <copyright file="XRCubemapLightEstimationApi.cs" company="Google LLC">
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
    using System.Runtime.InteropServices;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using UnityEngine;
    using UnityEngine.XR.OpenXR.NativeTypes;

    internal class XRCubemapLightEstimationApi
    {
        internal enum XRCubemapLightingColorFormat
        {
            R32G32B32Sfloat = 1,
            R32G32B32A32Sfloat = 2,
            R16G16B16A16Sfloat = 3,
        }

        public static void Enable(XRCubemapLightingColorFormat colorFormat)
        {
            ExternalApi.XrCubemapLightEstimation_enable(
                XRInstanceManagerApi.GetIntPtr(), colorFormat);
        }

        public static void Disable()
        {
            ExternalApi.XrCubemapLightEstimation_disable(XRInstanceManagerApi.GetIntPtr());
        }

        public static bool TryGetCubemapLightEstimate(ref XRCubemapLightEstimate lightEstimate)
        {
            return ExternalApi.XrCubemapLightEstimation_tryGetLightEstimate(
                XRInstanceManagerApi.GetIntPtr(), ref lightEstimate);
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct XRCubemapLightEstimate
        {
            public ulong LastUpdatedTimeNs;
            public ulong CubemapExposureTimeNs;
            public Quaternion CubemapRotation;
            public uint CubemapResolution;
            public IntPtr CubemapBuffer;
        }

        private struct ExternalApi
        {
            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void XrCubemapLightEstimation_enable(
                IntPtr manager, XRCubemapLightingColorFormat colorFormat);
            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void XrCubemapLightEstimation_disable(IntPtr manager);
            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrCubemapLightEstimation_tryGetLightEstimate(
                IntPtr manager, ref XRCubemapLightEstimate lightEstimate);
        }
    }
}
