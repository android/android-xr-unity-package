// <copyright file="XRGoogleCloudAuthResult.cs" company="Google LLC">
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
    using UnityEngine.XR.OpenXR.NativeTypes;

    /// <summary>
    /// The cause of the error of an operation accessing Google Cloud APIs.
    /// </summary>
    public enum XRGoogleCloudAuthError : int
    {
        /// <summary>
        /// No error occurred when invoking a Google Cloud API.
        /// </summary>
        None = 0,

        /// <summary>
        /// Quota exceeded when invoking a Google Cloud API.
        /// </summary>
        QuotaExceeded = -1,

        /// <summary>
        /// Failed to reach a Google Cloud API, possibly due to network
        /// connectivity issues or server availability.
        /// </summary>
        Unreachable = -2,

        /// <summary>
        /// An auth error occurred when invoking a Google Cloud API.
        /// It may be caused by an invalid credential which has been set for
        /// authentication.
        /// </summary>
        AuthError = -3,
    }

    /// <summary>
    /// The authentication result from Google Cloud APIs.
    /// </summary>
    public struct XRGoogleCloudAuthResult : IEquatable<XRGoogleCloudAuthResult>
    {
        /// <summary>
        /// Represents the status of a completed OpenXR operation.
        /// Use <see cref="OpenXRResultStatus.IsSuccess()"/> to confirm if
        /// succeed.
        /// Otherwise, check <see cref="OpenXRResultStatus.nativeStatusCode"/>
        /// or <see cref="CloudAuthError"/> for more details.
        /// </summary>
        public OpenXRResultStatus Status;

        /// <summary>
        /// The cause of authentication error when invoking a Google Cloud API.
        /// </summary>
        public XRGoogleCloudAuthError CloudAuthError;

        /// <summary>
        /// Constructs auth result with <see cref="OpenXRResultStatus"/> and
        /// <see cref="XRGoogleCloudAuthError"/>.
        /// </summary>
        /// <param name="status">
        /// The <see cref="OpenXRResultStatus"/> returned from relevant OpenXR APIs.
        /// </param>
        /// <param name="error">
        /// The <see cref="XRGoogleCloudAuthError"/> chained to the output.
        /// </param>
        public XRGoogleCloudAuthResult(
            OpenXRResultStatus status, XRGoogleCloudAuthError error)
        {
            Status = status;
            CloudAuthError = error;
        }

        /// <summary>
        /// Constructs auth result with <see cref="OpenXRResultStatus"/>.
        /// The OpenXR calls failed before accessing Google Cloud services, thus
        /// <see cref="CloudAuthError"/> defaults to
        /// <see cref="XRGoogleCloudAuthError.None"/>.
        /// </summary>
        /// <param name="status">
        /// The <see cref="OpenXRResultStatus"/> returned from relevant OpenXR APIs.
        /// </param>
        public XRGoogleCloudAuthResult(OpenXRResultStatus status) :
            this(status, XRGoogleCloudAuthError.None)
        {
        }

        /// <summary>
        /// Constructs auth result with <see cref="XRGoogleCloudAuthError"/>.
        /// The authentication setup failed, and the <see cref="Status"/> can be
        /// ignored.
        /// </summary>
        /// <param name="error">
        /// A <see cref="XRGoogleCloudAuthError"/> from dependent features.
        /// </param>
        public XRGoogleCloudAuthResult(XRGoogleCloudAuthError error) :
            this(new OpenXRResultStatus(OpenXRResultStatus.StatusCode.UnknownError), error)
        {
        }

        /// <inheritdoc/>
        public override int GetHashCode() =>
            HashCode.Combine(Status.GetHashCode(), CloudAuthError.GetHashCode());

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is XRGoogleCloudAuthResult args && Equals(args);
        }

        /// <inheritdoc/>
        public bool Equals(XRGoogleCloudAuthResult other)
        {
            return Status == other.Status && CloudAuthError == other.CloudAuthError;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return CloudAuthError == XRGoogleCloudAuthError.None ?
                $"{Status}" : $"XRGoogleCloudAuthError.{CloudAuthError}";
        }
    }
}
