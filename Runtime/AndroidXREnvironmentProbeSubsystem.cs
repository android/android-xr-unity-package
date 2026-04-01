// <copyright file="AndroidXREnvironmentProbeSubsystem.cs" company="Google LLC">
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
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Google.XR.Extensions.Internal;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using UnityEngine;
    using UnityEngine.Experimental.Rendering;
    using UnityEngine.Rendering;
    using UnityEngine.Scripting;
    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.ARSubsystems;

    /// <summary>
    /// The Android XR implementation of the <c><see cref="XREnvironmentProbeSubsystem"/></c>
    /// so it can work seamlessly with <c><see cref="AREnvironmentProbeManager"/></c>.
    /// </summary>
    [Preserve]
    public sealed class AndroidXREnvironmentProbeSubsystem : XREnvironmentProbeSubsystem
    {
        /// <summary>
        /// The graphics formats supported by the provider for the environment probe. If the system
        /// does not support any of these formats, the subsystem will not be able to provide an
        /// environment probe.
        /// </summary>
        public GraphicsFormat[] ProviderSupportedGraphicsFormats =>
            AndroidXRProvider._formatsToCheck;

        internal static string _id => AndroidXRProvider._id;

        private AndroidXRProvider _androidXRProvider => provider as AndroidXRProvider;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterDescriptor()
        {
#pragma warning disable CS0162 // Unreachable code detected: used at runtime.
            if (!ApiConstants.AndroidPlatform)
            {
                return;
            }

            XREnvironmentProbeSubsystemDescriptor.Cinfo cinfo =
                new XREnvironmentProbeSubsystemDescriptor.Cinfo()
                {
                    id = AndroidXRProvider._id,
                    providerType = typeof(AndroidXRProvider),
                    subsystemTypeOverride = typeof(AndroidXREnvironmentProbeSubsystem),
                    supportsManualPlacement = true,
                    supportsRemovalOfManual = true,
                    supportsAutomaticPlacement = false,
                    supportsRemovalOfAutomatic = false,
                    supportsEnvironmentTexture = true,
                    supportsEnvironmentTextureHDR = true,
                };

            XREnvironmentProbeSubsystemDescriptor.Register(cinfo);
#pragma warning restore CS0162
        }

        [SuppressMessage(
            "UnityRules.UnityStyleRules", "US1109:PublicPropertiesMustBeUpperCamelCase",
            Justification = "Override Unity interface.")]
        internal class AndroidXRProvider : Provider
        {
            internal static readonly string _id = "AndroidXR-CubemapLightEstimation";
            internal static readonly GraphicsFormat[] _formatsToCheck =
            {
                GraphicsFormat.R32G32B32A32_SFloat,
                GraphicsFormat.R32G32B32_SFloat,
            };

            private bool _isActive = false;
            private XREnvironmentProbe _probe = default;
            private int _cubemapResolution = 0;
            private Cubemap _cubemap = null;
            private Cubemap _rotatedCubemap = null;
            private RenderTexture _renderTexture = null;
            private CommandBuffer _cmdBuffer = null;
            private Material _rotatedCubemapProjector = null;
            private GraphicsFormat _currentFormat = GraphicsFormat.None;

            /// <inheritdoc/>
            public override void Start()
            {
                _currentFormat = GraphicsFormat.None;
                foreach (GraphicsFormat format in _formatsToCheck)
                {
                    if (SystemInfo.IsFormatSupported(
                        format, GraphicsFormatUsage.SetPixels | GraphicsFormatUsage.Render))
                    {
                        _currentFormat = format;
                        break;
                    }
                }

                XRCubemapLightEstimationApi.XRCubemapLightingColorFormat colorFormat = default;
                if (_currentFormat == GraphicsFormat.None)
                {
                    Debug.LogError("Float color formats not supported by system");
                    return;
                }
                else if (_currentFormat == GraphicsFormat.R32G32B32A32_SFloat)
                {
                    colorFormat =
                        XRCubemapLightEstimationApi.XRCubemapLightingColorFormat.R32G32B32A32Sfloat;
                }
                else if (_currentFormat == GraphicsFormat.R32G32B32_SFloat)
                {
                    colorFormat =
                        XRCubemapLightEstimationApi.XRCubemapLightingColorFormat.R32G32B32Sfloat;
                }

                if (_rotatedCubemapProjector == null)
                {
                    Shader cubemapProjector = Shader.Find("Google XR Extensions/CubemapProjector");
                    if (cubemapProjector == null)
                    {
                        Debug.LogError("Failed to find cubemap projector shader");
                        return;
                    }

                    _rotatedCubemapProjector = new Material(cubemapProjector);
                }

                if (_cmdBuffer == null)
                {
                    _cmdBuffer = new CommandBuffer();
                }

                // Managed through AREnvironmentProbeManager's OnEnable() callback.
                Debug.Log($"{ApiConstants.LogTag}: Start AndroidXREnvironmentProbeSubsystem.");

                _isActive = true;
                XRCubemapLightEstimationApi.Enable(colorFormat);
            }

            /// <inheritdoc/>
            public override void Stop()
            {
                // Managed through AREnvironmentProbeManager's OnDisable() callback.
                Debug.Log($"{ApiConstants.LogTag}: Stop AndroidXREnvironmentProbeSubsystem.");

                _isActive = false;
                XRCubemapLightEstimationApi.Disable();
            }

            /// <inheritdoc/>
            public override void Destroy()
            {
                _cmdBuffer.Clear();
                _cmdBuffer = null;
                UnityEngine.Object.Destroy(_rotatedCubemapProjector);
                UnityEngine.Object.Destroy(_rotatedCubemap);
                UnityEngine.Object.Destroy(_renderTexture);
                UnityEngine.Object.Destroy(_cubemap);

                // Managed through OpenXRFeature.OnSubsystemDestroy event.
                Debug.Log($"{ApiConstants.LogTag}: Destroy AndroidXREnvironmentProbeSubsystem.");
            }

            /// <inheritdoc/>
            public override TrackableChanges<XREnvironmentProbe> GetChanges(
                XREnvironmentProbe defaultEnvironmentProbe, Allocator allocator)
            {
                if (!_isActive)
                {
                    Debug.LogWarning("AndroidXREnvironmentProbeSubsystem is in active. " +
                                     "No changes will be provided");
                }

                int addedCount = 0;
                int updatedCount = 0;
                int removedCount = 0; // The last probe will remain unchanged if estimate is invalid

                XRCubemapLightEstimationApi.XRCubemapLightEstimate estimate = default;

                if (XRCubemapLightEstimationApi.TryGetCubemapLightEstimate(ref estimate))
                {
                    if (_cubemap == null || _cubemap.width != estimate.CubemapResolution)
                    {
                        if (_cubemap != null)
                        {
                            UnityEngine.Object.Destroy(_cubemap);
                        }

                        if (_rotatedCubemap != null)
                        {
                            UnityEngine.Object.Destroy(_rotatedCubemap);
                        }

                        _cubemapResolution = (int)estimate.CubemapResolution;

                        _cubemap = new Cubemap(
                            width: _cubemapResolution,
                            format: _currentFormat,
                            flags: TextureCreationFlags.None,
                            mipCount: 1);

                        _rotatedCubemap = new Cubemap(
                            width: _cubemapResolution,
                            format: _currentFormat,
                            flags: TextureCreationFlags.None,
                            mipCount: 1);

                        if (_renderTexture != null)
                        {
                            UnityEngine.Object.Destroy(_renderTexture);
                        }

                        _renderTexture = new RenderTexture(
                            width: _cubemapResolution,
                            height: _cubemapResolution * 6,
                            depth: 0,
                            format: _currentFormat);
                        _renderTexture.Create();

                        _cmdBuffer.Clear();

                        // Reproject rotated cubemap onto axis aligned cubemap faces
                        _rotatedCubemapProjector.SetTexture("_cubemap", _rotatedCubemap);
                        _cmdBuffer.Blit(null, _renderTexture, _rotatedCubemapProjector);

                        // Copy the axis aligned cubemap faces to the axis aligned cubemap
                        for (int i = 0; i < 6; i++)
                        {
                            _cmdBuffer.CopyTexture(
                                src: _renderTexture,
                                srcElement: 0,
                                srcMip: 0,
                                srcX: 0,
                                srcY: i * _cubemapResolution,
                                srcWidth: _cubemapResolution,
                                srcHeight: _cubemapResolution,
                                dst: _cubemap,
                                dstElement: i,
                                dstMip: 0,
                                dstX: 0,
                                dstY: 0);
                        }

                        var textureDescriptor = new XRTextureDescriptor(
                            nativeTexture: _cubemap.GetNativeTexturePtr(),
                            width: _cubemap.width,
                            height: _cubemap.height,
                            mipmapCount: _cubemap.mipmapCount,
                            format: _cubemap.format,
                            propertyNameId: 0,
                            depth: 1,
                            textureType: XRTextureType.Cube);

                        _probe = new XREnvironmentProbe(
                            trackableId: new TrackableId(1, 1),
                            scale: Vector3.one,
                            pose: Pose.identity,
                            size: Vector3.one * 1000f,
                            descriptor: textureDescriptor,
                            trackingState: TrackingState.Tracking,
                            nativePtr: IntPtr.Zero);

                        addedCount = 1;
                    }
                    else
                    {
                        updatedCount = 1;
                    }

                    Matrix4x4 worldToCubemap = Matrix4x4.Rotate(
                        Quaternion.Inverse(estimate.CubemapRotation));
                    _rotatedCubemapProjector.SetMatrix("_worldToCubemap", worldToCubemap);
                    int facePixelCount = _cubemapResolution * _cubemapResolution;
                    int channelCount = 0;
                    if (_currentFormat == GraphicsFormat.R32G32B32A32_SFloat)
                    {
                        channelCount = 4;
                    }
                    else if (_currentFormat == GraphicsFormat.R32G32B32_SFloat)
                    {
                        channelCount = 3;
                    }

                    int faceStride = facePixelCount * channelCount;
                    NativeArray<float> floatBuffer = default;
                    unsafe
                    {
                        floatBuffer =
                                NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<float>(
                                    estimate.CubemapBuffer.ToPointer(),
                                    faceStride * 6,
                                    Allocator.None);
                    }

                    int srcOffset = 0;
                    for (int i = 0; i < 6; i++)
                    {
                        // VERY IMPORTANT: In Unity the right, left, top, and bottom faces appear
                        // z-mirrored. So flip the z faces to eliminate the seams. But this
                        // z-mirrors the cubemap, so the shader will need to account for it to
                        // unmirror it.
                        CubemapFace face = (CubemapFace)i;
                        if (face == CubemapFace.PositiveZ)
                        {
                            face = CubemapFace.NegativeZ;
                        }
                        else if (face == CubemapFace.NegativeZ)
                        {
                            face = CubemapFace.PositiveZ;
                        }

                        _rotatedCubemap.SetPixelData(floatBuffer, 0, face, srcOffset);
                        srcOffset += faceStride;
                    }

                    _rotatedCubemap.Apply();

                    // Now that the rotated cubemap is loaded, perform the graphics work to
                    // reproject it and copy onto axis aligned cubemap
                    Graphics.ExecuteCommandBuffer(_cmdBuffer);
                }
                else
                {
                    Debug.LogWarning("Failed to get Cubemap Light estimate");
                }

                // There is only one probe which will either be added or updated.
                return new TrackableChanges<XREnvironmentProbe>(
                    addedCount, updatedCount, removedCount, allocator, _probe);
            }
        }
    }
}
