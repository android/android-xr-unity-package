// <copyright file="SpatialContainerSettings.cs" company="Google LLC">
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

namespace Google.XR.Extensions.Samples.SpatialContainer
{
    using UnityEngine;

    /// <summary>
    /// Provides settings for Spatial Container features, like JXR launcher injection.
    /// </summary>
    [CreateAssetMenu(
        fileName = "SpatialContainerSettings",
        menuName = "Google XR/Spatial Container Settings")]
    public class SpatialContainerSettings : ScriptableObject
    {
        /// <summary>
        /// Gets or sets a value indicating whether to inject the Jetpack Compose
        /// XR Launcher during the Android build process.
        /// </summary>
        [Tooltip("Inject Jetpack Compose XR Launcher during Android build.")]
        public bool InjectJxrLauncher = false;
    }
}
