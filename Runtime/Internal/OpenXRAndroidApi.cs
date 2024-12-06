// <copyright file="OpenXRAndroidApi.cs" company="Google LLC">
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
    using UnityEngine.XR.OpenXR.NativeTypes;

    internal class OpenXRAndroidApi
    {
        public static void SetBlendMode(XrEnvironmentBlendMode mode)
        {
            ExternalApi.OpenXRAndroid_setBlendMode(XRInstanceManagerApi.GetIntPtr(), mode);
        }

        public static XrEnvironmentBlendMode GetBlendMode()
        {
            XrEnvironmentBlendMode mode = XrEnvironmentBlendMode.Additive;
            ExternalApi.OpenXRAndroid_getBlendMode(XRInstanceManagerApi.GetIntPtr(), ref mode);
            return mode;
        }

        public static XrEnvironmentBlendMode[] EnumerateEnvironmentBlendModes(
            int view_configuration_type, ulong system_id)
        {
            const uint MaxBlendModes = 16;
            XrEnvironmentBlendMode[] blendModes = new XrEnvironmentBlendMode[MaxBlendModes];
            unsafe
            {
                fixed (XrEnvironmentBlendMode* blendModesPtr = blendModes)
                {
                    uint numBlendModes = 0;
                    ExternalApi.OpenXRAndroid_enumerateEnvironmentBlendModes(
                                    XRInstanceManagerApi.GetIntPtr(), view_configuration_type,
                                    system_id, blendModesPtr, MaxBlendModes, ref numBlendModes);
                    Array.Resize(ref blendModes, (int)numBlendModes);
                }
            }

            return blendModes;
        }

        private struct ExternalApi
        {
            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void OpenXRAndroid_setBlendMode(
                IntPtr manager, XrEnvironmentBlendMode mode);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void OpenXRAndroid_getBlendMode(
                IntPtr manager, ref XrEnvironmentBlendMode out_mode);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static unsafe extern IntPtr OpenXRAndroid_enumerateEnvironmentBlendModes(
                IntPtr manager, int view_configuration_type, ulong system_id,
                XrEnvironmentBlendMode* blend_modes, uint capacity, ref uint count_output);
        }
    }
}
