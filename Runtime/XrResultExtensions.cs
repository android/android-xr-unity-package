// <copyright file="XrResultExtensions.cs" company="Google LLC">
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
    using UnityEngine.XR.ARSubsystems;
    using UnityEngine.XR.OpenXR.NativeTypes;
    /// <summary>
    /// Extension class for <see cref="XrResult"/>, including helpers of
    /// <see cref="OpenXRResultStatus"/> and <see cref="XRResultStatus"/>.
    /// </summary>
    public static class XrResultExtensions
    {
        /// <summary>
        /// Converts <see cref="XrResult"/> to <see cref="OpenXRResultStatus"/>.
        /// </summary>
        /// <param name="result">
        /// The <see cref="XrResult"/> returned from OpenXR APIs.</param>
        /// <returns>The <see cref="OpenXRResultStatus"/> equivalence.</returns>
        public static OpenXRResultStatus ToStatus(this XrResult result)
        {
            return new OpenXRResultStatus(result);
        }

        /// <summary>
        /// Converts OpenXR status type into AR Foundation status type, for the
        /// compatibility with <c>Result</c>.
        /// </summary>
        /// <param name="openXRResult">
        /// The <see cref="OpenXRResultStatus"/> returned from OpenXR APIs.
        /// </param>
        /// <returns>The <see cref="XRResultStatus"/> equivalence.</returns>
        public static XRResultStatus ToXRResultStatus(this OpenXRResultStatus openXRResult)
        {
            XRResultStatus.StatusCode status =
                (XRResultStatus.StatusCode)openXRResult.statusCode;
            if (!Enum.IsDefined(typeof(XRResultStatus.StatusCode), status))
            {
                status = XRResultStatus.StatusCode.UnknownError;
            }

            int nativeCode = (int)openXRResult.nativeStatusCode;
            return new XRResultStatus(status, nativeCode);
        }

        /// <summary>
        /// Converts AR Foundation status type to OpenXR status to better
        /// analyze native error code <see cref="XrResult"/>.
        /// </summary>
        /// <param name="xrStatus">
        /// The <see cref="XRResultStatus"/> returned from OpenXR wrappers.
        /// </param>
        /// <returns>The <see cref="OpenXRResultStatus"/> equivalence.</returns>
        public static OpenXRResultStatus ToOpenXRStatus(this XRResultStatus xrStatus)
        {
            OpenXRResultStatus.StatusCode status =
                (OpenXRResultStatus.StatusCode)xrStatus.statusCode;
            if (!Enum.IsDefined(typeof(OpenXRResultStatus.StatusCode), status))
            {
                status = OpenXRResultStatus.StatusCode.UnknownError;
            }

            XrResult xrResult = (XrResult)xrStatus.nativeStatusCode;
            return new OpenXRResultStatus(status, xrResult);
        }

        /// <summary>
        /// Determines if the <see cref="OpenXRResultStatus"/> represents
        /// unsupported cases.
        /// </summary>
        /// <param name="status">
        /// The <see cref="OpenXRResultStatus"/> returned from OpenXR APIs.
        /// </param>
        /// <returns>True if it's an unsupported status.</returns>
        public static bool IsUnsupported(this OpenXRResultStatus status)
        {
            return status.statusCode == OpenXRResultStatus.StatusCode.Unsupported ||
                status.nativeStatusCode == XrResult.FeatureUnsupported ||
                status.nativeStatusCode == XrResult.FunctionUnsupported;
        }
    }
}
