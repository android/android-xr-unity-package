// <copyright file="UnityOpenXRNativeApi.cs" company="Google LLC">
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
    using System.Runtime.InteropServices;
    using UnityEngine;

    internal class UnityOpenXRNativeApi
    {
        public static void MetaSetSubsampledLayout(bool enabled)
        {
            bool result = ExternalApi.MetaSetSubsampledLayout(enabled) >= 0;
            Debug.LogFormat("MetaSetSubsampledLayout({0}): {1}",
                enabled, result ? "succeed" : "failed");
        }

        public static void MetaSetSpaceWarp(bool enabled)
        {
            bool result = ExternalApi.MetaSetSpaceWarp(enabled) >= 0;
            Debug.LogFormat("MetaSetSpaceWarp({0}): {1}",
                enabled, result ? "succeed" : "failed");
        }

        private struct ExternalApi
        {
            [DllImport("UnityOpenXR")]
            public static extern int MetaSetSubsampledLayout(bool enabled);

            [DllImport("UnityOpenXR")]
            public static extern int MetaSetSpaceWarp(bool enable);
        }
    }
}