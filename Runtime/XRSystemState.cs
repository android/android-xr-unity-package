// <copyright file="XRSystemState.cs" company="Google LLC">
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

namespace Google.XR.Extensions
{
    using System.Runtime.InteropServices;
    using UnityEngine.XR.OpenXR.NativeTypes;

    /// <summary>
    /// The input modality state of the system.
    /// </summary>
    public enum XrInputModality
    {
        /// <summary>
        /// Unknown input modality.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Hand input modality.
        /// </summary>
        Hand = 1,

        /// <summary>
        /// Controller input modality.
        /// </summary>
        Controller = 2,

        /// <summary>
        /// Mouse input modality.
        /// </summary>
        Mouse = 3,

        /// <summary>
        /// Eye input modality.
        /// </summary>
        Eye = 4,

        /// <summary>
        /// HMD fallback input modality.
        /// </summary>
        HmdFallback = 5,

        /// <summary>
        /// Dwell with head input modality.
        /// </summary>
        DwellWithHead = 6,

        /// <summary>
        /// Dwell with eye input modality.
        /// </summary>
        DwellWithEye = 7,
    }

    /// <summary>
    /// Contains system state information.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct XrSystemState
    {
        /// <summary>
        /// The current blend mode state of the environment.
        /// </summary>
        public XrEnvironmentBlendMode BlendModeState;

        /// <summary>
        /// The current passthrough opacity of the environment.
        /// </summary>
        public float PassthroughOpacity;

        /// <summary>
        /// The current input modality state of the system.
        /// </summary>
        public XrInputModality InputModalityState;
    }
}
