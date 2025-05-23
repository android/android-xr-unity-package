// <copyright file="XrCompositionLayerPassthrough.cs" company="Google LLC">
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

#if XR_COMPOSITION_LAYERS
namespace Google.XR.Extensions.Internal
{
    using System.Runtime.InteropServices;
    using UnityEngine;
    using UnityEngine.XR.OpenXR.NativeTypes;

    /// <summary>
    /// Represents OpenXR struct XrCompositionLayerPassthroughANDROID.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct XrCompositionLayerPassthrough
    {
        public uint Type;
        public void* Next;
        public XrCompositionLayerFlags LayerFlags;
        public ulong Space;
        public XrPosef Pose;
        public XrVector3f Scale;
        public float Opacity;
        public ulong Layer;  // XrPassthroughLayerANDROID
    }
}
#endif
