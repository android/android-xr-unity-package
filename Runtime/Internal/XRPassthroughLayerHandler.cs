// <copyright file="XRPassthroughLayerHandler.cs" company="Google LLC">
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
    using System.Collections.Generic;
    using Unity.XR.CompositionLayers;
    using Unity.XR.CompositionLayers.Extensions;
    using Unity.XR.CompositionLayers.Layers;
    using Unity.XR.CompositionLayers.Services;
    using UnityEngine;
    using UnityEngine.XR.OpenXR;
    using UnityEngine.XR.OpenXR.CompositionLayers;
    using UnityEngine.XR.OpenXR.NativeTypes;

    /// <summary>
    /// Demonstrates custom handler for the custom quad layer.
    /// </summary>
    internal class XRPassthroughLayerHandler :
        OpenXRCustomLayerHandler<XrCompositionLayerPassthrough>
    {
        // Matches XRPassthroughLayerData instances added into the scene.
        private Dictionary<ulong, XrPosef> _layerPoses = new Dictionary<ulong, XrPosef>();

        /// <inheritdoc/>
        protected override bool CreateSwapchain(
            CompositionLayerManager.LayerInfo layer, out SwapchainCreateInfo swapchainCreateInfo)
        {
            // Swapchain not needed for this layer, manually invoke OnCreatedSwapchain().
            swapchainCreateInfo = default;
            OnCreatedSwapchain(layer, default);
            return false;
        }

        /// <inheritdoc/>
        protected override bool CreateNativeLayer(CompositionLayerManager.LayerInfo layer,
            SwapchainCreatedOutput swapchainOutput, out XrCompositionLayerPassthrough nativeLayer)
        {
            unsafe
            {
                MeshRenderer renderer = layer.Layer.GetComponent<MeshRenderer>();
                if (renderer != null && renderer.enabled)
                {
                    renderer.enabled = false;
                    Debug.LogWarningFormat(
                        "[{0}] Disable MeshRenderer for Passthrough Composition Layer.",
                        layer.Layer.gameObject.name);
                }

                MeshFilter meshFilter = layer.Layer.GetComponent<MeshFilter>();
                if (meshFilter == null)
                {
                    Debug.LogErrorFormat(
                        "[{0}] Missing MeshFilter to create Passthrough Composition Layer.",
                        layer.Layer.gameObject.name);
                    nativeLayer = new XrCompositionLayerPassthrough();
                    return false;
                }

                // Create XrPassthroughLayerANDROID.
                XRPassthroughLayerData data = layer.Layer.LayerData as XRPassthroughLayerData;
                ulong passthroughLayer = XRPassthroughApi.CreateLayer(
                    meshFilter.sharedMesh, data.Clockwise);
                if (passthroughLayer == 0)
                {
                    Debug.LogErrorFormat(
                        "[{0}] Failed to create native handler XrPassthroughLayerANDROID.",
                        layer.Layer.gameObject.name);
                    nativeLayer = new XrCompositionLayerPassthrough();
                    return false;
                }
                else
                {
                    Debug.LogFormat("[{0}] Create XrPassthroughLayerANDROID: {1}",
                        layer.Layer.gameObject.name, passthroughLayer);
                }

                Transform transform = layer.Layer.GetComponent<Transform>();
                _layerPoses[passthroughLayer] = GetXrPose(transform);
                nativeLayer = new XrCompositionLayerPassthrough()
                {
                    // XR_TYPE_COMPOSITION_LAYER_PASSTHROUGH_ANDROID,
                    Type = 1000462002,
                    Next = null,
                    LayerFlags = 0,
                    Space = OpenXRLayerUtility.GetCurrentAppSpace(),
                    Pose = _layerPoses[passthroughLayer],
                    Scale = new XrVector3f(
                        transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z),
                    Opacity = data.Opacity,
                    Layer = passthroughLayer,
                };
                return true;
            }
        }

        /// <inheritdoc/>
        protected override bool ModifyNativeLayer(CompositionLayerManager.LayerInfo layerInfo,
            ref XrCompositionLayerPassthrough nativeLayer)
        {
            var data = layerInfo.Layer.LayerData as XRPassthroughLayerData;
            var transform = layerInfo.Layer.GetComponent<Transform>();
            if (data.UpdateTransformPerFrame)
            {
                _layerPoses[nativeLayer.Layer] = GetXrPose(transform);
            }

            nativeLayer.Pose = _layerPoses[nativeLayer.Layer];
            nativeLayer.Scale = new XrVector3f(
                transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
            nativeLayer.Opacity = data.Opacity;

            return true;
        }

        /// <inheritdoc/>
        protected override bool ActiveNativeLayer(CompositionLayerManager.LayerInfo layerInfo,
            ref XrCompositionLayerPassthrough nativeLayer)
        {
            var data = layerInfo.Layer.LayerData as XRPassthroughLayerData;
            var transform = layerInfo.Layer.GetComponent<Transform>();
            if (data.UpdateTransformPerFrame)
            {
                _layerPoses[nativeLayer.Layer] = GetXrPose(transform);
            }

            nativeLayer.Pose = _layerPoses[nativeLayer.Layer];
            nativeLayer.Space = OpenXRLayerUtility.GetCurrentAppSpace();

            return base.ActiveNativeLayer(layerInfo, ref nativeLayer);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            foreach (var pair in _layerPoses)
            {
                Debug.Log("Destroy XrPassthroughLayerANDROID: " + pair.Key);
                XRPassthroughApi.DestroyLayer(pair.Key);
            }

            _layerPoses.Clear();
            base.Dispose(disposing);
        }

        private XrPosef GetXrPose(Transform transform)
        {
            Pose pose = OpenXRUtility.ComputePoseToWorldSpace(
                transform, CompositionLayerManager.mainCameraCache);
            return new XrPosef(pose.position, pose.rotation);
        }
    }
}
#endif
