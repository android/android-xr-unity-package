//-----------------------------------------------------------------------
// <copyright file="XRImageDatabaseEntry.cs" company="Qualcomm Technologies, Inc.">
//
// Copyright Qualcomm Technologies, Inc. and/or its affiliates. All rights reserved.
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
    using System;
    using System.Runtime.InteropServices;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.Jobs;
    using UnityEngine;
    using UnityEngine.XR.ARSubsystems;

    /// <summary>
    /// Represents an entry in an OpenXR image database (<c>XrTrackableImageFormatANDROID</c>),
    /// which is used to configure image tracking.
    /// This struct is intended for interoperability between managed (C#) and
    /// unmanaged/native (C++) code and can be used to pass image references to native code.
    /// It handles the allocation of native memory and copies pixel data into a buffer layout
    /// expected by OpenXR.
    /// The caller is responsible for disposing instances to prevent memory leaks.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct XRImageDatabaseEntry : IDisposable
    {
        // Indicates the desired tracking mode for the reference image.
        internal XRImageTrackingMode _trackingMode;

        // Indicates the width of the image in meters. If zero, the image size will be estimated.
        internal float _physicalWidth;

        // Indicates the width of the image in pixels.
        internal UInt32 _imageWidth;

        // Indicates the height of the image in pixels
        internal UInt32 _imageHeight;

        // Indicating the format of the OpenXR image data buffer.
        internal XrTrackableImageFormat _format;

        // Indicates the byte length of buffer (number of elements in the array)
        internal UInt32 _bufferSize;

        // Pointer to the first element (first pixel-value of the texture)
        internal IntPtr _buffer;

        // Used only for runtime configuration.
        internal Guid _referenceGuid;

        public XRImageDatabaseEntry(
            XRReferenceImage referenceImage, XRImageTrackingMode trackingMode,
            bool preferSizeEstimation, bool systemSupportsSizeEstimation, out JobHandle jobHandle)
        {
            // texture must be readable in order to get the pixel data via
            // texture.GetRawTextureData<byte>()
            if (!referenceImage.texture.isReadable)
            {
                throw new InvalidOperationException(
                    "Texture must be readable to copy its pixel data.");
            }

            if (!systemSupportsSizeEstimation && !referenceImage.specifySize)
            {
                throw new InvalidOperationException(
                    "The system does not support physical size estimation. The " +
                    $"reference image '{referenceImage.name}' has no size specified " +
                    "and will be ignored.");
            }

            if (referenceImage.texture.width == 0 || referenceImage.texture.height == 0)
            {
                throw new InvalidOperationException("Texture has invalid size.");
            }

            _referenceGuid = referenceImage.guid;
            _trackingMode = trackingMode;
            _physicalWidth = systemSupportsSizeEstimation &&
                (preferSizeEstimation || !referenceImage.specifySize) ? 0f : referenceImage.width;
            _imageWidth = Convert.ToUInt32(referenceImage.texture.width);
            _imageHeight = Convert.ToUInt32(referenceImage.texture.height);

            // allocate and copy pixel data to unmanaged buffer
            switch (referenceImage.texture.format)
            {
                case TextureFormat.RGBA32:
                    _format = XrTrackableImageFormat.R8G8B8A8;
                    jobHandle = CreateUnmanagedImageBufferFromTextureRgba32(
                        referenceImage.texture, out _buffer, out _bufferSize);
                    break;
                default:
                    _format = XrTrackableImageFormat.R8G8B8A8;
                    _buffer = IntPtr.Zero;
                    _bufferSize = 0;
                    throw new NotSupportedException(
                        $"Texture format '{referenceImage.texture.format}' is not supported.");
            }
        }

        /// <summary>
        /// Frees the allocate unmanaged memory.
        /// Must only be called after the returned <see cref="JobHandle"/> has completed.
        /// </summary>
        public void Dispose()
        {
            if (_buffer == IntPtr.Zero)
            {
                return;
            }

            Marshal.FreeHGlobal(_buffer);
            _buffer = IntPtr.Zero;
            _bufferSize = 0;
        }

        /// <summary>
        /// Allocates an unmanaged image buffer and schedules a job to copy RGBA32 pixel data from
        /// a readable Unity <see cref="Texture2D"/> into that buffer using the layout expected by
        /// OpenXR, thereby performing a vertical flip of the rows.
        ///
        /// Unity pixel data buffer layout:
        ///      - row-major order, bottom-to-top.
        ///      - origin (0,0): bottom-left.
        ///      - RGBA32 (R8G8B8A8).
        ///
        /// OpenXR pixel data buffer layout:
        ///      - row-major order, top-to-bottom.
        ///      - origin (0,0): top-left.
        ///      - RGBA32 (R8G8B8A8).
        /// </summary>
        /// <param name="texture">
        /// The original RGBA32 texture. The texture must be readable.
        /// </param>
        /// <param name="buffer">
        /// Outputs a pointer to the newly allocated unmanaged buffer containing the
        /// pixel data in OpenXR's expected layout.
        /// </param>
        /// <param name="bufferSize">
        /// Outputs the size (byte count) of the allocated <paramref name="buffer"/>.
        /// </param>
        /// <remarks>
        /// This method will allocate unmanaged memory via <see cref="Marshal.AllocHGlobal(int)"/>.
        /// The caller is responsible for freeing the allocated memory via <see cref="Dispose"/>.
        /// </remarks>
        /// <returns>A <see cref="JobHandle"/> for the scheduled <see cref="VerticalFlipCopyJob"/>.
        /// The caller must ensure that the job has completed before accessing or freeing
        /// the <paramref name="buffer"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if <paramref name="texture"/> does not use the
        /// <see cref="TextureFormat.RGBA32"/> format.
        /// </exception>
        private static unsafe JobHandle CreateUnmanagedImageBufferFromTextureRgba32(
            Texture2D texture, out IntPtr buffer, out UInt32 bufferSize)
        {
            if (texture.format != TextureFormat.RGBA32)
            {
                throw new InvalidOperationException(
                    $"Expected TextureFormat.RGBA32, but got '{texture.format}'.");
            }

            // Retrieve pixel data.
            var pixelDataPtr = texture.GetRawTextureData<byte>();

            // Allocate unmanaged pixel data buffer.
            var bytes = pixelDataPtr.Length;
            buffer = Marshal.AllocHGlobal(bytes);
            bufferSize = Convert.ToUInt32(bytes);

            // Copy rgba32 pixel data to allocated buffer (vertical flip of rows).
            // Unity buffer layout: row-major order, bottom-to-top.
            var sourcePtr = (byte*)pixelDataPtr.GetUnsafeReadOnlyPtr();

            // OpenXR buffer layout: row-major order, top-to-bottom.
            var destPtr = (byte*)buffer.ToPointer();

            // Number of rows in the source image.
            var rows = texture.height;

            // Number of bytes per row; rgba -> 4 bytes per pixel.
            var bytesPerRow = texture.width * 4;

            var job = new VerticalFlipCopyJob
            {
                SrcPtr = sourcePtr,
                DestPtr = destPtr,
                LastRowIdx = rows - 1,
                BytesPerRow = bytesPerRow
            };
            return job.ScheduleParallel(rows, 64, default);
        }

        /// <summary>
        /// Parallel job that copies raw texture data from a source buffer to a destination buffer
        /// while applying a vertical flip (reversing the order of rows).
        /// The job executes once per row.
        /// </summary>
        private struct VerticalFlipCopyJob : IJobFor
        {
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public unsafe byte* SrcPtr; // Unity layout: row-major order, bottom-to-top
            [WriteOnly, NativeDisableUnsafePtrRestriction]
            public unsafe byte* DestPtr; // OpenXR layout: row-major order, top-to-bottom

            [ReadOnly]
            public int LastRowIdx; // Index of the last row (texture.height - 1)
            [ReadOnly]
            public int BytesPerRow; // Number of bytes per row (texture.width * bytes per pixel)

            // Executed once per row in the source texture with srcRowIdx: [0, LastRowIdx].
            public unsafe void Execute(int srcRowIdx)
            {
                // Corresponding row in dest (vertical flip).
                var destRowIdx = LastRowIdx - srcRowIdx;

                // Points to the first byte in the src row.
                var srcOffset = srcRowIdx * BytesPerRow;

                // Points to the first byte in the dest row.
                var destOffset = destRowIdx * BytesPerRow;

                // Copies the entire row from source to destination.
                UnsafeUtility.MemCpy(
                    DestPtr + destOffset, SrcPtr + srcOffset, BytesPerRow);
            }
        }
    }
}
