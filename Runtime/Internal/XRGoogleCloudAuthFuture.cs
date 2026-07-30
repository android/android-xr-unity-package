// <copyright file="XRGoogleCloudAuthFuture.cs" company="Google LLC">
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
    using System;
    using System.Runtime.InteropServices;
    using UnityEngine.XR.ARSubsystems;
    using UnityEngine.XR.OpenXR.NativeTypes;

    using XrFutureEXT = System.UInt64;

    /// <summary>
    /// Supports Google Cloud Authentication settings with async operations.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>
    /// Construct <c>XrGoogleCloudAuthInfoBaseHeaderANDROID</c> with supported
    /// strategies.
    /// </description></item>
    /// <item><description>
    /// Handle OpenXR native calls to <c>xrSetGoogleCloudAuthAsyncANDROID</c>
    /// and <c>xrSetGoogleCloudAuthCompleteANDROID</c>.
    /// </description></item>
    /// <item><description>
    /// Due to no return type, wrap <c>XrResult</c> as <c>bool</c> to reflect a
    /// succeed operation.
    /// </description></item>
    /// </list>
    /// </remarks>
    internal class XRGoogleCloudAuthFuture : XRFutureHandler<bool>
    {
        XRGoogleCloudAuthFuture(XrFutureEXT xrFuture)
        {
            StartPolling(xrFuture);
        }

        public static bool ValidateCredential(string credential)
        {
            if (string.IsNullOrEmpty(credential))
            {
                return false;
            }
            else if (credential.Contains(" "))
            {
                // Expect to replace by a formal regex check.
                return false;
            }

            return true;
        }

        public static OpenXRResultStatus TrySetAuth(
            XRAuthenticationStrategy strategy, string credential,
            out XRGoogleCloudAuthFuture future)
        {
            future = null;
            if (strategy == XRAuthenticationStrategy.None ||
                (strategy != XRAuthenticationStrategy.Keyless && string.IsNullOrEmpty(credential)))
            {
                return new OpenXRResultStatus(OpenXRResultStatus.StatusCode.ValidationFailure);
            }

            var apiType = strategy.ToApiCloudAuthType();
            bool nativeCallSucceed = false;
            XrFutureEXT xrFuture = 0;
            XrResult xrResult = XrResult.FeatureUnsupported;

            // Append '\0' null-terminator via [MarshalAs(UnmanagedType.LPUTF8Str)]
            // required by C-style strings as well as OpenXR APIs.
            // null string will arrive as nullptr in C++.
            nativeCallSucceed = ExternalApi.XrGoogleCloud_setAuthAsync(
                XRInstanceManagerApi.GetIntPtr(), apiType, credential, ref xrFuture, ref xrResult);
            if (!nativeCallSucceed)
            {
                return new OpenXRResultStatus(
                    OpenXRResultStatus.StatusCode.ProviderUninitialized);
            }

            var result = new OpenXRResultStatus(xrResult);
            if (result.IsError())
            {
                return result;
            }

            future = new XRGoogleCloudAuthFuture(xrFuture);
            return result;
        }

        protected override Result<bool> CompleteFuture(ulong xrFuture)
        {
            XrResult xrResult = XrResult.FeatureUnsupported;
            if (!ExternalApi.XrGoogleCloud_setAuthComplete(
                    XRInstanceManagerApi.GetIntPtr(), xrFuture, ref xrResult))
            {
                var status = new XRResultStatus(
                    XRResultStatus.StatusCode.ProviderUninitialized);
                return new Result<bool>(status, false);
            }
            else
            {
                return new Result<bool>(
                    xrResult.ToStatus().ToXRResultStatus(), xrResult.IsSuccess());
            }
        }

        private struct ExternalApi
        {
            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrGoogleCloud_setAuthAsync(
                IntPtr manager, ApiCloudAuthType type,
                [MarshalAs(UnmanagedType.LPUTF8Str)] string credential,
                ref XrFutureEXT outFuture, ref XrResult outResult);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrGoogleCloud_setAuthComplete(
                IntPtr manager, XrFutureEXT future, ref XrResult outResult);
        }
    }
}
