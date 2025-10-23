// <copyright file="TransparentBackgroundRendererFeature.cs" company="Google LLC">
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

#if UNITY_URP
namespace Google.XR.Extensions
{
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.RenderGraphModule;
    using UnityEngine.Rendering.Universal;

    /// <summary>
    /// This ScriptableRendererFeature forces the camera's color buffer to be cleared
    /// to a transparent color at the beginning of the frame. This is essential for
    /// XR applications like passthrough where you need to see the real world
    /// behind your virtual content.
    /// </summary>
    public class TransparentBackgroundRendererFeature : ScriptableRendererFeature
    {
        private TransparentBackgroundPass _scriptablePass;

        /// <inheritdoc/>
        public override void Create()
        {
            _scriptablePass = new TransparentBackgroundPass
            {
                renderPassEvent = RenderPassEvent.BeforeRendering
            };
        }

        /// <inheritdoc/>
        public override void AddRenderPasses(
            ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(_scriptablePass);
        }

        private class TransparentBackgroundPass : ScriptableRenderPass
        {
            /// <inheritdoc/>
            public override void RecordRenderGraph(
                RenderGraph renderGraph, ContextContainer frameData)
            {
                const string passName = "Transparent Background Pass";

                using (var builder = renderGraph.AddRasterRenderPass<TransparentBackgroundPassData>(
                           passName, out var passData))
                {
                    builder.SetRenderFunc(
                        (TransparentBackgroundPassData data, RasterGraphContext context) =>
                    {
                        context.cmd.ClearRenderTarget(true, true, data._clearColor);
                    });
                }
            }

            private class TransparentBackgroundPassData
            {
                internal readonly Color _clearColor = Color.clear;
            }
        }
    }
}
#endif // UNITY_URP
