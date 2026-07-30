// <copyright file="XRGoogleCloudFeature.cs" company="Google LLC">
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
    using System.Runtime.InteropServices;
    using Google.XR.Extensions.Internal;
    using UnityEngine;
    using UnityEngine.XR.OpenXR.Features;
    using UnityEngine.XR.OpenXR.NativeTypes;

#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.XR.OpenXR.Features;
#endif

    /// <summary>
    /// This <c><see cref="OpenXRInteractionFeature"/></c> configures Android XR
    /// extensions <c>XR_ANDROID_google_cloud_auth</c> at runtime and provides
    /// <see cref="XRGoogleCloudManager"/> to manage Google Cloud API usages.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = UiName,
        BuildTargetGroups = new[] {
            BuildTargetGroup.Android,
        },
        Company = "Google",
        OpenxrExtensionStrings = ExtensionStrings,
        Desc = "Provides Google Cloud Service access at OpenXR runtimes.",
        Version = "1.0.0",
        Category = FeatureCategory.Feature,
        FeatureId = FeatureId,
        Priority = 96)]
#endif
    public sealed class XRGoogleCloudFeature : OpenXRFeature, IXRSpatialSdk
    {
        /// <summary>
        /// The UI name shows on the XR Plug-in Management panel, it helps users
        /// to understand validation errors and expected fixes.
        /// </summary>
        public const string UiName = "Android XR: Google Cloud";

        /// <summary>
        /// The feature ID string.
        /// </summary>
        public const string FeatureId = "com.google.xr.extensions.google_cloud";

        /// <summary>
        /// The OpenXR Extension string. Used to check if this extensions is
        /// available or enabled.
        /// </summary>
        public const string ExtensionStrings =
            "XR_EXT_future " +
            "XR_ANDROID_google_cloud_auth ";

        /// <summary>
        /// The event fired when authentication errors have been triggered by
        /// other Google Cloud API calls.
        /// </summary>
        /// <remarks>
        /// If it's caused by <see cref="XRGoogleCloudAuthError.AuthError"/>,
        /// you may call <see cref="TrySetAuthenticationAsync"/> manually with a
        /// valid configuration, and access Google Cloud APIs after succeed.
        /// </remarks>
        public static Action<XRGoogleCloudAuthResult> OnAuthResultReceived;

        internal static bool? _extensionEnabled = null;

        [SerializeField]
        private XRAuthenticationStrategy _strategy = XRAuthenticationStrategy.Keyless;

        [SerializeField]
        private string _credential = string.Empty;

        private bool _sessionReady = false;

        private CloudAuthResultCallback _authCallback;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void CloudAuthResultCallback(int xrResult, int authError);

        /// <summary>
        /// Gets if the required OpenXR extension is enabled.
        /// When OpenXR runtime is waiting, it returns <c>null</c>.
        /// Otherwise, it indicates whether the <c>XR_ANDROID_google_cloud_auth</c>
        /// extension is available on current device.
        /// </summary>
        public static bool? IsExtensionEnabled => _extensionEnabled;

#if UNITY_EDITOR
        /// <summary>
        /// Editor helper to set automatic strategy from scripts.
        /// </summary>
        /// <param name="strategy">The strategy to try on session start.
        /// </param>
        /// <param name="credential">The credential to be used on session start.
        /// </param>
        /// <returns>
        /// Returns true if it succeeded; otherwise, returns false.
        /// </returns>
        public bool TrySetStrategy(XRAuthenticationStrategy strategy, string credential)
        {
            if (_sessionReady)
            {
                return false;
            }

            _strategy = strategy;
            _credential = credential;
            return true;
        }
#endif // UNITY_EDITOR

        /// <summary>
        /// Attempts to set authentication through an async operation.
        /// </summary>
        /// <param name="strategy">
        /// The target strategy to be used for authentication.
        /// It must be a valid one other than
        /// <see cref="XRAuthenticationStrategy.None"/>.
        /// </param>
        /// <param name="credential">
        /// The credentials passing for the given <paramref name="strategy"/>.
        /// Ignored for <see cref="XRAuthenticationStrategy.Keyless"/>.
        /// Invalid credential will result in
        /// <see cref="XRGoogleCloudAuthError.AuthError"/> from all calls to
        /// functions that depend on Cloud authorization.
        /// </param>
        /// <returns>
        /// The result of the async operation.
        /// Use <see cref="OpenXRResultStatus.IsSuccess()"/> to confirm if succeed.
        /// When failed with <see cref="OpenXRResultStatus.StatusCode.PlatformError"/>,
        /// check <see cref="OpenXRResultStatus.nativeStatusCode"/> for the
        /// failure reason.
        /// It might be <see cref="XrResultAndroid.KeylessNotSetup"/>,
        /// <see cref="XrResultAndroid.KeylessAuthFailed"/> or common OpenXR
        /// errors from <see cref="XrResult"/>.
        /// </returns>
        public async Awaitable<OpenXRResultStatus> TrySetAuthenticationAsync(
            XRAuthenticationStrategy strategy, string credential = "")
        {
            if (!IsExtensionEnabled.HasValue)
            {
                return new OpenXRResultStatus(
                    OpenXRResultStatus.StatusCode.ProviderUninitialized);
            }
            else if (!IsExtensionEnabled.Value)
            {
                return new OpenXRResultStatus(
                    OpenXRResultStatus.StatusCode.Unsupported);
            }
            else if (!_sessionReady)
            {
                return new OpenXRResultStatus(
                    OpenXRResultStatus.StatusCode.ProviderNotStarted);
            }

            var result = XRGoogleCloudAuthFuture.TrySetAuth(
                strategy, credential, out XRGoogleCloudAuthFuture authFuture);
            if (result.IsError())
            {
                Debug.LogErrorFormat(
                    "Failed to create XRGoogleCloudAuthFuture with {0}", result);
                return result;
            }

            Debug.Log("Waiting for XRGoogleCloudAuthFuture...");
            var status = new OpenXRResultStatus(OpenXRResultStatus.StatusCode.UnknownError);
            try
            {
                var authResult = await authFuture.TryGetResultAsync();
                status = authResult.status.ToOpenXRStatus();
                if (authResult.status.IsError())
                {
                    Debug.LogErrorFormat(
                        "XRGoogleCloudAuthFuture failed with {0}", authResult.status);
                }
                else
                {
                    Debug.LogFormat(
                        "XRGoogleCloudAuthFuture completed for {0}", strategy);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("TrySetAuthenticationAsync is canceled.");
            }
            catch (Exception e)
            {
                Debug.LogException(e, this);
            }

            authFuture?.Dispose();
            return status;
        }

        /// <inheritdoc/>
        public XRSpatialSdkVersions GetTargetVersion()
        {
            return XRSpatialSdkVersions.XRSpatialApiLevel3;
        }

        /// <inheritdoc/>
        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            _extensionEnabled = AndroidXRFeatureUtils.CheckExtensions(ExtensionStrings) &&
                XRInstanceManagerApi.Register(ApiXrFeature.GoogleCloud);
            if (_extensionEnabled.Value)
            {
                _authCallback = new CloudAuthResultCallback(CloudAuthResultTrampoline);
                ExternalApi.XrGoogleCloud_registerAuthResultCallback(
                    XRInstanceManagerApi.GetIntPtr(), _authCallback);
            }

            return _extensionEnabled.Value;
        }

        /// <inheritdoc/>
        protected override void OnInstanceDestroy(ulong xrInstance)
        {
            XRInstanceManagerApi.Unregister(ApiXrFeature.GoogleCloud);
        }

        /// <inheritdoc/>
        protected override void OnSessionCreate(ulong xrSession)
        {
            _sessionReady = true;
            TrySetAuthAndForget();
        }

        /// <inheritdoc/>
        protected override void OnSessionDestroy(ulong xrSession)
        {
            _sessionReady = false;
        }

        [AOT.MonoPInvokeCallback(typeof(CloudAuthResultCallback))]
        private static void CloudAuthResultTrampoline(int xrResult, int authError)
        {
            var status = new OpenXRResultStatus((XrResult)xrResult);
            var error = (XRGoogleCloudAuthError)authError;
            var authResult = new XRGoogleCloudAuthResult(status, error);
            OnAuthResultReceived?.Invoke(authResult);
        }

        private async void TrySetAuthAndForget()
        {
            if (_strategy == XRAuthenticationStrategy.None)
            {
                // Auto-setting is disabled.
                return;
            }
            else if (_strategy != XRAuthenticationStrategy.Keyless &&
                !XRGoogleCloudAuthFuture.ValidateCredential(_credential))
            {
                Debug.LogError(
                    "Invalid credential, skipped auto-setting.");
                return;
            }

            try
            {
                Debug.LogFormat(
                    "Attempts to set authentication with strategy {0}", _strategy);
                var result = await TrySetAuthenticationAsync(_strategy, _credential);
                if (result.IsError())
                {
                    Debug.LogErrorFormat("Auto-setting failed with {0}.", result);
                }
                else
                {
                    Debug.Log("Auto-setting succeed.");
                }
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("Auto-setting has been canceled");
            }
            catch (Exception e)
            {
                Debug.LogException(e, this);
            }
        }

        private struct ExternalApi
        {
            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void XrGoogleCloud_registerAuthResultCallback(
                IntPtr manager, CloudAuthResultCallback callback);
        }
    }
}
