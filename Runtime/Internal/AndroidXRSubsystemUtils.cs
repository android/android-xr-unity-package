// <copyright file="AndroidXRSubsystemUtils.cs" company="Google LLC">
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

namespace Google.XR.Extensions.Internal
{
    using UnityEngine;
    using UnityEngine.XR.ARSubsystems;
    using UnityEngine.XR.OpenXR.NativeTypes;

    internal static class AndroidXRSubsystemUtils
    {
        /// <summary>
        /// Constructs a completed awaitable for async operations.
        /// </summary>
        /// <typeparam name="T">
        /// The value type returned within the result.</typeparam>
        /// <param name="statusCode">
        /// The status code to construct <see cref="XRResultStatus"/> of the
        /// <see cref="Result{T}"/>.
        /// </param>
        /// <param name="returnValue">
        /// The value returned on operation completion.</param>
        /// <returns>
        /// A completed <see cref="Awaitable{Result{T}}"/>.
        /// </returns>
        public static Awaitable<Result<T>> CreateAwaitableWithResult<T>(
            XRResultStatus.StatusCode statusCode, T returnValue)
        {
            return CreateAwaitableWithResult(new XRResultStatus(statusCode), returnValue);
        }

        /// <summary>
        /// Constructs a completed <see cref="Awaitable{XRResultStatus}"/> for
        /// async operations.
        /// </summary>
        /// <param name="statusCode">
        /// The status code to construct <see cref="XRResultStatus"/>.
        /// </param>
        /// <returns>
        /// A completed <see cref="Awaitable{XRResultStatus}"/>.
        /// </returns>
        public static Awaitable<XRResultStatus> CreateAwaitableWithStatus(
            XRResultStatus.StatusCode statusCode)
        {
            return CreateAwaitableWithStatus(new XRResultStatus(statusCode));
        }

        /// <summary>
        /// Constructs a completed awaitable for async operations.
        /// </summary>
        /// <typeparam name="T">
        /// The value type returned within the result.</typeparam>
        /// <param name="status">
        /// The <see cref="XRResultStatus"/> of the <see cref="Result{T}"/>.
        /// </param>
        /// <param name="returnValue">
        /// The value returned on operation completion.</param>
        /// <returns>
        /// A completed <see cref="Awaitable{Result{T}}"/>.
        /// </returns>
        public static Awaitable<Result<T>> CreateAwaitableWithResult<T>(
            XRResultStatus status, T returnValue)
        {
            var result = new Result<T>(status, returnValue);
            var completionSource = new AwaitableCompletionSource<Result<T>>();
            completionSource.SetResult(result);
            return completionSource.Awaitable;
        }

        /// <summary>
        /// Constructs a completed awaitable for async operations.
        /// </summary>
        /// <typeparam name="T">
        /// The value type returned within the result.</typeparam>
        /// <param name="status">
        /// The <see cref="XrResult"/> of the <see cref="Result{T}"/>.
        /// </param>
        /// <param name="returnValue">
        /// The value returned on operation completion.</param>
        /// <returns>
        /// A completed <see cref="Awaitable{Result{T}}"/>.
        /// </returns>
        public static Awaitable<Result<T>> CreateAwaitableWithResult<T>(
            XrResult status, T returnValue)
        {
            var openXRStatus = status.ToStatus();
            var xrStatus = openXRStatus.ToXRResultStatus();
            var result = new Result<T>(xrStatus, returnValue);
            var completionSource = new AwaitableCompletionSource<Result<T>>();
            completionSource.SetResult(result);
            return completionSource.Awaitable;
        }

        /// <summary>
        /// Constructs a completed <see cref="Awaitable{XRResultStatus}"/> for
        /// async operations.
        /// </summary>
        /// <param name="status">
        /// The <see cref="XRResultStatus"/>.
        /// </param>
        /// <returns>
        /// A completed <see cref="Awaitable{XRResultStatus}"/>.
        /// </returns>
        public static Awaitable<XRResultStatus> CreateAwaitableWithStatus(XRResultStatus status)
        {
            var completionSource = new AwaitableCompletionSource<XRResultStatus>();
            completionSource.SetResult(status);
            return completionSource.Awaitable;
        }
    }
}
