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

    /// <summary>
    /// Unify the usage accessing native plugin from Unity OpenXR Plugin.
    ///
    /// Note: features depending on Unity OpenXR Plugin would be affected by package upgraded.
    /// Especially when the underlying functionality has been deprecated, corresponding OpenXR
    /// features will also be deprecated or removed from Google XR Extensions package.
    /// </summary>
    internal class UnityOpenXRNativeApi
    {
        public static void MetaSetSubsampledLayout(bool enabled)
        {
            int result = ExternalApi.MetaSetSubsampledLayout(enabled);
            Debug.LogFormat("MetaSetSubsampledLayout({0}): {1} with status {2}.",
                enabled, result >= 0  ? "succeed" : "failed", result);
        }

        public static void MetaSetSpaceWarp(bool enabled)
        {
            int result = ExternalApi.MetaSetSpaceWarp(enabled);
            Debug.LogFormat("MetaSetSpaceWarp({0}): {1} with status {2}.",
                enabled, result >= 0 ? "succeed" : "failed", result);
        }

        public static void FBSetFoveationLevel(
            ulong xrSession, XRFoveationLevel foveationLevel,
            float verticalOffset, bool foveationDynamic)
        {
            int result = ExternalApi.FBSetFoveationLevel(
                xrSession, (int)foveationLevel, verticalOffset, foveationDynamic ? 1 : 0);
            Debug.LogFormat("FBSetFoveationLevel({0}, {1}, {2}, {3}): {4} with status {5}.",
                xrSession, foveationLevel, verticalOffset, foveationDynamic,
                result >= 0 ? "succeed" : "failed", result);
        }

        private struct ExternalApi
        {
            [DllImport(ApiConstants.UnityOpenXRLib)]
            public static extern int MetaSetSubsampledLayout(bool enabled);

            [DllImport(ApiConstants.UnityOpenXRLib)]
            public static extern int MetaSetSpaceWarp(bool enable);

            [DllImport(ApiConstants.UnityOpenXRLib)]
            public static extern int FBSetFoveationLevel(
                ulong session, int level, float verticalOffset, int dynamic);
        }
    }
}
