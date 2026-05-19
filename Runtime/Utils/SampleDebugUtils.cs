// <copyright file="SampleDebugUtils.cs" company="Google LLC">
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

namespace Google.XR.Extensions
{
    using System;
    using System.Text;
    using UnityEngine.SubsystemsImplementation;
    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.OpenXR.Features;

    /// <summary>
    /// Utilities to debug Android XR samples.
    /// </summary>
    public static class SampleDebugUtils
    {
        /// <summary>
        /// Check whether the given feature is available to use based on its extension status.
        /// </summary>
        /// <param name="feature">The <see cref="OpenXRFeature"/> instance.</param>
        /// <param name="isExtensionEnabled">
        /// The nullable property that represents extensions status at runtime.</param>
        /// <param name="stringBuilder">
        /// The optional string builder to append debug information.</param>
        /// <returns>True if the feature is ready to use.</returns>
        public static bool CheckFeatureAvailability(
            OpenXRFeature feature, bool? isExtensionEnabled, StringBuilder stringBuilder = null)
        {
            if (feature == null)
            {
                stringBuilder?.Append($"{nameof(OpenXRFeature)} is null.\n");
                return false;
            }
            else if (isExtensionEnabled.HasValue)
            {
                // OpenXRLoader disables features that failed in OnInstanceCreate(ulong).
                // Verify extension first to differentiate from manually disabling.
                stringBuilder?.AppendFormat("{0} is {1}.\n", feature.GetType().Name,
                    isExtensionEnabled.Value ? "supported" : "unsupported");
                return isExtensionEnabled.Value;
            }
            else if (!feature.enabled)
            {
                stringBuilder?.Append($"{feature.GetType().Name} is disabled.\n");
                return false;
            }
            else
            {
                stringBuilder?.Append($"{feature.GetType().Name} is initializing.\n");
                return false;
            }
        }

        /// <summary>
        /// Check if the given subsystem is running.
        /// </summary>
        /// <param name="targetType">
        /// The target type of a <see cref="SubsystemWithProvider"/>.</param>
        /// <param name="subsystem">
        /// The nullable <see cref="SubsystemLifecycleManager.subsystem"/> property.</param>
        /// <param name="stringBuilder">
        /// The optional string builder to append debug information.</param>
        /// <returns>True if the subsystem is ready to use.</returns>
        public static bool CheckSubsystemReady(
            Type targetType, SubsystemWithProvider subsystem, StringBuilder stringBuilder = null)
        {
            if (subsystem == null)
            {
                stringBuilder?.Append($"{targetType.Name} is null.\n");
                return false;
            }
            else if (!subsystem.running)
            {
                stringBuilder?.Append($"{subsystem.GetType().Name} is not running.\n");
                return false;
            }
            else
            {
                stringBuilder?.Append($"{subsystem.GetType().Name} is running.\n");
                return true;
            }
        }
    }
}
