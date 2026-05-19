// <copyright file="XRRecommendedSettings.cs" company="Google LLC">
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
    /// The possible input modality states within the default environment.
    /// </summary>
    [System.Flags]
    public enum XrInputModalities
    {
        /// <summary>
        /// Unknown input modality.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Head input modality.
        /// </summary>
        Head = 1 << 0,

        /// <summary>
        /// Controller input modality.
        /// </summary>
        Controller = 1 << 1,

        /// <summary>
        /// Hands input modality.
        /// </summary>
        Hands = 1 << 2,

        /// <summary>
        /// Mouse input modality.
        /// </summary>
        Mouse = 1 << 3,

        /// <summary>
        /// Eyes and/or Hands input modality.
        /// </summary>
        GazeAndGesture = 1 << 4,
    }

    /// <summary>
    /// Contains the recommended settings for the app.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct XrRecommendedSettings
    {
        /// <summary>
        /// The prelaunch blend mode state of the default environment.
        /// </summary>
        public XrEnvironmentBlendMode BlendModeState;

        /// <summary>
        /// The prelaunch passthrough opacity of the default environment.
        /// </summary>
        public float PassthroughOpacity;

        /// <summary>
        /// The prelaunch input modality state within the default environment.
        /// </summary>
        public XrInputModalities InputModalityState;
    }
}
