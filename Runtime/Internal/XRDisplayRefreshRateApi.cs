// <copyright file="XRDisplayRefreshRateApi.cs" company="Google LLC">
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
    using System.Runtime.InteropServices;
    using UnityEngine;

    internal class XRDisplayRefreshRateApi
    {
        public static bool GetDisplayRefreshRateInfo(
             out XRDisplayRefreshRateInfo info)
        {
            info.SupportedRefreshRates = null;
            info.CurrentRefreshRate = 0;

            DisplayRefreshRateInfo nativeInfo;
            if (!ExternalApi.XrDisplayRefreshRate_getDisplayRefreshRateInfo(
                XRInstanceManagerApi.GetIntPtr(), out nativeInfo))
            {
                return false;
            }

            info.CurrentRefreshRate = nativeInfo.CurrentRefreshRate;

            info.SupportedRefreshRates = new float[4];
            info.SupportedRefreshRates[0] = nativeInfo.RefreshRate0;
            info.SupportedRefreshRates[1] = nativeInfo.RefreshRate1;
            info.SupportedRefreshRates[2] = nativeInfo.RefreshRate2;
            info.SupportedRefreshRates[3] = nativeInfo.RefreshRate3;
            Array.Resize(ref info.SupportedRefreshRates, (int)nativeInfo.NumRefreshRates);

            return true;
        }

        public static void RequestDisplayRefreshRate(
            float displayRefreshRate)
        {
            ExternalApi.XrDisplayRefreshRate_requestDisplayRefreshRate(
                XRInstanceManagerApi.GetIntPtr(), displayRefreshRate);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DisplayRefreshRateInfo
        {
            public float RefreshRate0;
            public float RefreshRate1;
            public float RefreshRate2;
            public float RefreshRate3;
            public float CurrentRefreshRate;
            public uint NumRefreshRates;
        }

        private struct ExternalApi
        {
            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrDisplayRefreshRate_getDisplayRefreshRateInfo(
                IntPtr manager, out DisplayRefreshRateInfo info);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void XrDisplayRefreshRate_requestDisplayRefreshRate(
                IntPtr manager, float displayRefreshRate);
        }
    }
}
