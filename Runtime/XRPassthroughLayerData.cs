// <copyright file="XRPassthroughLayerData.cs" company="Google LLC">
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
namespace Google.XR.Extensions
{
    using System;
    using Unity.XR.CompositionLayers.Layers;
    using UnityEngine;

    /// <summary>
    /// Example of defining a layer data script for a passthrough layer.
    ///
    /// This composition layer type will appear in the type dropdown of the
    /// <see cref="Unity.XR.CompositionLayers.CompositionLayer"/>
    /// component with what the name is defined as.
    ///
    /// Note: it requires a valid <see cref="MeshFilter"/> with non-empty
    /// <see cref="MeshFilter.mesh"/> for the geometry to create a passthrough layer at runtime.
    /// </summary>
    [Serializable]
    [CompositionLayerData(
        Provider = "Unity",
        Name = "Passthrough (Android XR)",
        Description = "A passthrough composition layer on Android XR.",
        SuggestedExtenstionTypes = new Type[] { typeof(MeshFilter) })]
    public class XRPassthroughLayerData : LayerData
    {
        [SerializeField]
        private bool _clockwise = true;

        [SerializeField]
        [Range(0f, 1.0f)]
        private float _opacity = 1.0f;

        [SerializeField]
        private bool _updateTransformPerFrame = false;

        /// <summary>
        /// Gets or sets a value indicating whether to use winding order clockwide of the give mesh
        /// from <see cref="MeshFilter.mesh"/>.
        /// </summary>
        public bool Clockwise
        {
            get => _clockwise;
            set => _clockwise = UpdateValue(_clockwise, value);
        }

        /// <summary>
        /// Gets or sets opacity of the passthrough texture in the range [0, 1].
        /// </summary>
        public float Opacity
        {
            get => _opacity;
            set => _opacity = UpdateValue(_opacity, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether to update the mesh transform associated with
        /// the passthrough layer per frame.
        ///
        /// If false (the default), the mesh transform is created once at creation and then kept
        /// constant. Otherwise, the mesh transform is updated once per frame based the relative
        /// pose to the current main camera.
        /// </summary>
        public bool UpdateTransformPerFrame
        {
            get => _updateTransformPerFrame;
            set => _updateTransformPerFrame = UpdateValue(_updateTransformPerFrame, value);
        }

        /// <inheritdoc/>
        public override void CopyFrom(LayerData layerData)
        {
            if (layerData is XRPassthroughLayerData passthroughLayerData)
            {
                _clockwise = passthroughLayerData.Clockwise;
                _opacity = passthroughLayerData.Opacity;
                _updateTransformPerFrame = passthroughLayerData.UpdateTransformPerFrame;
            }
        }
    }
}
#endif
