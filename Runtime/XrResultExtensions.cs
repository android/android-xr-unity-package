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
    /// The <c>XrResult</c> error codes provided by vendor extensions.
    /// For general types, refer to <see cref="XrResult"/>.
    /// </summary>
    public enum XrResultAndroid : int
    {
        /// <summary>
        /// The <c>XrResult</c> error code indicating that the developer did not
        /// properly setup Keyless Auth.
        /// OpenXR type: XR_ERROR_KEYLESS_AUTH_NOT_SETUP_ANDROID.
        /// </summary>
        KeylessNotSetup = -1000787000,

        /// <summary>
        /// The <c>XrResult</c> error code indicating that an error occurred when
        /// fetching Keyless Auth credentials.
        /// OpenXR type: XR_ERROR_KEYLESS_AUTH_FAILED_ANDROID.
        /// </summary>
        KeylessAuthFailed = -1000787001,

        /// <summary>
        /// The <c>XrResult</c> error code indicating that an operation failed
        /// because the Geospatial Tracker was not running when it started, or
        /// the Geospatial Tracker stopped running before the operation completed.
        /// OpenXR type: XR_ERROR_GEOSPATIAL_TRACKER_NOT_RUNNING_ANDROID.
        /// </summary>
        GeospatialTrackerNotRunning = -1000789000,

        /// <summary>
        /// The <c>XrResult</c> error code indicating that the given geospatial
        /// coordinates are invalid or out of range.
        /// OpenXR type: XR_ERROR_GEOSPATIAL_COORDINATES_INVALID_ANDROID.
        /// </summary>
        GeospatialCoordinateInvalid = -1000789001,

        /// <summary>
        /// The <c>XrResult</c> error code indicating that a problem happens
        /// with authentication to Google Cloud API.
        /// OpenXR type: XR_ERROR_GEOSPATIAL_CLOUD_AUTH_FAILED_ANDROID.
        /// </summary>
        GeospatialCloudAuthFailed = -1000789002,
    }

    /// <summary>
    /// Extension class for <see cref="XrResult"/>, including helpers of
    /// <see cref="OpenXRResultStatus"/> and <see cref="XRResultStatus"/>.
    /// </summary>
    public static class XrResultExtensions
    {
        /// <summary>
        /// Converts vendor error codes to <c>XrResult</c> type.
        /// </summary>
        /// <param name="android">The vendor error code.</param>
        /// <returns>The <see cref="XrResult"/> error.</returns>
        public static XrResult ToExt(this XrResultAndroid android) => (XrResult)android;

        /// <summary>
        /// Gets display name of a given result. It converts to
        /// <see cref="XrResultAndroid"/> when possible.
        /// </summary>
        /// <param name="result">
        /// A <see cref="XrResult"/> returned by OpenXR APIs.</param>
        /// <returns>The display name of the given result.</returns>
        public static string GetDisplayName(this XrResult result)
        {
            var androidType = (XrResultAndroid)result;
            return Enum.IsDefined(typeof(XrResultAndroid), androidType) ?
                androidType.ToString() : result.ToString();
        }

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

        /// <summary>
        /// Determines if setting authentication failed with keyless errors.
        /// </summary>
        /// <param name="status">The status returned from
        /// <see cref="XRGoogleCloudFeature.TrySetAuthenticationAsync(
        /// XRAuthenticationStrategy, string)"/>.</param>
        /// <returns>True if it's failed with keyless authentication.</returns>
        public static bool IsKeylessError(this OpenXRResultStatus status)
        {
            return status.nativeStatusCode == XrResultAndroid.KeylessNotSetup.ToExt() ||
                status.nativeStatusCode == XrResultAndroid.KeylessAuthFailed.ToExt();
        }

        /// <summary>
        /// Determines if the <see cref="OpenXRResultStatus"/> represents
        /// an <see cref="XrResultAndroid.GeospatialCloudAuthFailed"/> error.
        /// </summary>
        /// <param name="status">
        /// The <see cref="OpenXRResultStatus"/> returned from OpenXR APIs.
        /// </param>
        /// <returns>
        /// True if it's caused by <see cref="XrResultAndroid.GeospatialCloudAuthFailed"/>.
        /// </returns>
        public static bool IsCloudAuthError(this OpenXRResultStatus status)
        {
            return status.nativeStatusCode == XrResultAndroid.GeospatialCloudAuthFailed.ToExt();
        }
    }
}
