// <copyright file="GeospatialController.cs" company="Google LLC">
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

namespace Google.XR.Extensions.Samples.Geospatial
{
    using System;
    using System.Collections;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Google.XR.Extensions;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.XR.OpenXR;
    using UnityEngine.XR.OpenXR.NativeTypes;

    /// <summary>
    /// Demonstrates geospatial features at runtime.
    /// </summary>
    [RequireComponent(typeof(XRGeospatialManager))]
    [RequireComponent(typeof(AndroidXRPermissionUtil))]
    public class GeospatialController : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField]
        private TextMesh _debugText = null;

        [SerializeField]
        private Canvas _bannerCanvas = null;

        [SerializeField]
        private Text _bannerText = null;

        [SerializeField]
        [Tooltip("The time interval of notification banner in seconds.")]
        [Range(0.5f, 10.0f)]
        private float _bannerDisplayDuration = 3.0f;

        [Header("Settings")]
        [SerializeField]
        [Tooltip(
            "Maximal attempts for manually setting authentication. " +
            "Use 0 to rely on auto-setting from OpenXR Feature configuration.")]
        [Range(0, 10)]
        private uint _maxAttemptCount = 3;

        [SerializeField]
        [Tooltip("The delay in seconds of retrying authentication after failures.")]
        [Range(0.5f, 5f)]
        private float _retryDelay = 3;

        [SerializeField]
        [Tooltip("Toggle VPS availability checks. A running location service is required.")]
        private bool _checkVPS = true;

        [SerializeField]
        [Tooltip("The time interval in seconds. Only take effect when CheckVPS is selected.")]
        [Range(0.5f, 10.0f)]
        private float _vpsCheckInterval = 3.0f;

        [SerializeField]
        [Tooltip("Indicates whether to locate geospatial pose when possible.")]
        private bool _locateGeospatialPose = true;

        [SerializeField]
        [Tooltip("The game object to demonstrate Geospatial Pose to Unity pose transition.")]
        private EarthIndicator _earthIndicator = null;

        [SerializeField]
        [Tooltip("The game object to demonstrate Unity pose to Geospatial Pose transition.")]
        private OriginIndicator _originIndicator = null;

        float _bannerTimer = 0f;
        private StringBuilder _stringBuilder = new StringBuilder();
        private AndroidXRPermissionUtil _permissionUtil = null;
        private XRGoogleCloudFeature _googleCloudFeature = null;
        private XRGoogleCloudAuthResult _cloudAuthResult;
        private CloudAuthStatus _cloudAuthStatus = CloudAuthStatus.Unknown;
        private XRGeospatialFeature _geospatialFeature = null;
        private XRGeospatialManager _geospatialManager = null;
        private string _vpsStatus = string.Empty;
        private CancellationTokenSource _cts = null;
        private XRGeospatialPose? _currentGeospatialPose = null;

        private enum CloudAuthStatus
        {
            AuthError = -3,
            Failed = -2,
            Unsupported = -1,
            Unknown = 0,
            Initializing = 1,
            Completed = 2,
            Canceled = 3,
        }

        /// <summary>
        /// Returns the current valid Geospatial Pose if available.
        /// </summary>
        /// <param name="pose">The current Geospatial Pose.</param>
        /// <returns>True if the pose is valid, false otherwise.</returns
        public bool TryGetCurrentLocation(out XRGeospatialPose pose)
        {
            pose = default;
            if (_currentGeospatialPose.HasValue)
            {
                pose = _currentGeospatialPose.Value;
                return true;
            }

            return false;
        }

        private void Awake()
        {
            _permissionUtil = GetComponent<AndroidXRPermissionUtil>();
            _geospatialManager = GetComponent<XRGeospatialManager>();
            _geospatialManager.enabled = false;

            if (GetComponent<ARShareableAnchorManager>() == null)
            {
                gameObject.AddComponent<ARShareableAnchorManager>();
            }

            GetComponent<ARShareableAnchorManager>().shareableAnchorPrefab =
                Resources.Load<GameObject>("GeospatialTestAnchor");

            if (GetComponent<GeospatialAnchorPlacer>() == null)
            {
                gameObject.AddComponent<GeospatialAnchorPlacer>();
            }

            _geospatialFeature = OpenXRSettings.Instance.GetFeature<XRGeospatialFeature>();
            _googleCloudFeature = OpenXRSettings.Instance.GetFeature<XRGoogleCloudFeature>();

            if (_bannerCanvas == null || _bannerText == null)
            {
                Debug.LogError("Banner is null. Check device logs for runtime API results.");
            }
            else
            {
                _bannerCanvas.gameObject.SetActive(false);
            }

            if (_debugText == null)
            {
                Debug.LogError("DebugText is null. Check device log for debug information.");
            }
        }

        private void OnEnable()
        {
            if (_cts == null)
            {
                _cts = new CancellationTokenSource();
            }

            if (_earthIndicator != null)
            {
                _earthIndicator.gameObject.SetActive(_locateGeospatialPose);
            }

            if (_originIndicator != null)
            {
                _originIndicator.gameObject.SetActive(_locateGeospatialPose);
            }

            XRGoogleCloudFeature.OnAuthResultReceived += OnAuthResultReceived;
            XRGeospatialManager.OnStateChanged += OnGeospatialStateChanged;
            StartCoroutine(WaitForSetup());
            StartCoroutine(UpdateVpsStatus());
        }

        private void OnDisable()
        {
            if (_earthIndicator != null)
            {
                _earthIndicator.gameObject.SetActive(false);
            }

            if (_originIndicator != null)
            {
                _originIndicator.gameObject.SetActive(false);
            }

            StopAllCoroutines();
            CancelAllTasks();

            XRGoogleCloudFeature.OnAuthResultReceived -= OnAuthResultReceived;
            XRGeospatialManager.OnStateChanged -= OnGeospatialStateChanged;
            _geospatialManager.enabled = false;
            Input.location.Stop();
        }

        private void Update()
        {
            HideBannerIfExpired();
            if (!_permissionUtil.AllPermissionGranted())
            {
                if (_debugText != null)
                {
                    _debugText.text = "Waiting for runtime permissions.";
                }

                return;
            }

            _stringBuilder.Clear();
            SampleDebugUtils.CheckFeatureAvailability(
                _googleCloudFeature, XRGoogleCloudFeature.IsExtensionEnabled, _stringBuilder);
            SampleDebugUtils.CheckFeatureAvailability(
                _geospatialFeature, XRGeospatialFeature.IsExtensionEnabled, _stringBuilder);

            _stringBuilder.AppendFormat("Authentication: {0}\n", _cloudAuthStatus);
            if (_cloudAuthStatus == CloudAuthStatus.AuthError ||
                _cloudAuthStatus == CloudAuthStatus.Failed)
            {
                _stringBuilder.AppendFormat(" | CloudAuthResult: {0}\n", _cloudAuthResult);
            }

            _stringBuilder.AppendFormat("Geospatial: {0}\n", XRGeospatialManager.State);
            if (XRGeospatialManager.State == XRGeospatialState.InitializationFailed)
            {
                _stringBuilder.AppendFormat(" | InitializationResult: {0}\n",
                    XRGeospatialManager.InitializationResult);
            }

            // It may stay in Initializing status even after granted permissions.
            // Check your network connection and outdoor/indoor environment to
            // ensure GPS signal can be received by the device.
            _stringBuilder.AppendFormat(
                "LocationService: {0}\n", Input.location.status);
            if (_checkVPS)
            {
                _stringBuilder.AppendFormat(" | VPS Status: {0}\n", _vpsStatus);
            }

            if (_locateGeospatialPose && _geospatialManager.Running &&
                Input.location.status == LocationServiceStatus.Running)
            {
                if (_earthIndicator != null)
                {
                    _stringBuilder.AppendFormat(" | EarthIndicator: {0}\n | {1}\n",
                        _earthIndicator.PoseResult.LocationFlags,
                        _earthIndicator.PoseResult.Pose.ToString("F2"));
                }

                if (_originIndicator != null)
                {
                    _stringBuilder.AppendFormat(" | OriginIndicator: {0}\n | {1}\n",
                        _originIndicator.PoseResult.LocationFlags,
                        _originIndicator.PoseResult.Pose.ToString("F2"));
                }
            }

            if (_debugText != null)
            {
                _debugText.text = _stringBuilder.ToString();
            }
        }

        private IEnumerator WaitForSetup()
        {
            yield return new WaitUntil(_permissionUtil.AllPermissionGranted);

            // Start location service for GPS access.
            if (!Input.location.isEnabledByUser)
            {
                Debug.LogError("GPS is disabled in the device's global settings.");
            }
            else
            {
                Input.location.Start();
            }

            // Try to set authentication manually.
            if (_maxAttemptCount > 0)
            {
                var asyncTask = TrySetAuthWithRetry(_maxAttemptCount, _cts.Token);
                while (!asyncTask.IsCompleted)
                {
                    yield return null;
                }

                if (asyncTask.IsFaulted)
                {
                    Debug.LogErrorFormat(
                        "TrySetAuthWithRetry task failed: {0}.", asyncTask.Exception);
                }
            }

            if (_cloudAuthStatus == CloudAuthStatus.Completed ||
                _cloudAuthStatus == CloudAuthStatus.Unknown)
            {
                // Delay for banner message to be visible.
                yield return new WaitForSeconds(_bannerDisplayDuration);

                // Start geospatial with authentication granted,
                // or try to start it with authentication auto-setting.
                _geospatialManager.enabled = true;
            }
        }

        private IEnumerator UpdateVpsStatus()
        {
            while (_checkVPS)
            {
                var asyncTask = CheckVPSAvailability(_cts.Token);
                while (!asyncTask.IsCompleted)
                {
                    yield return null;
                }

                if (asyncTask.IsFaulted)
                {
                    Debug.LogErrorFormat(
                        "CheckVPSAvailability task failed: {0}.", asyncTask.Exception);
                }

                yield return new WaitForSeconds(_vpsCheckInterval);
            }
        }

        private void HideBannerIfExpired()
        {
            if (_bannerCanvas == null || !_bannerCanvas.gameObject.activeSelf)
            {
                return;
            }

            _bannerTimer += Time.deltaTime;
            if (_bannerTimer >= _bannerDisplayDuration)
            {
                _bannerCanvas.gameObject.SetActive(false);
            }
        }

        private void PushNotification(string notification)
        {
            if (_bannerCanvas == null || _bannerText == null)
            {
                return;
            }

            _bannerTimer = 0f;
            _bannerText.text = notification;
            _bannerCanvas.gameObject.SetActive(true);
        }

        private async Task CheckVPSAvailability(CancellationToken token)
        {
            if (!Input.location.isEnabledByUser)
            {
                _vpsStatus = "GPS is disabled by User.";
                return;
            }
            else if (Input.location.status != LocationServiceStatus.Running)
            {
                _vpsStatus = "LocationService not running.";
                return;
            }

            var location = Input.location.lastData;
            try
            {
                _currentGeospatialPose = null;
                var result = await _geospatialManager.TryCheckVPSAvailabilityAsync(
                    location.latitude, location.longitude);
                token.ThrowIfCancellationRequested();
                if (result.status.nativeStatusCode ==
                    (int)XrResultAndroid.GeospatialCoordinateInvalid)
                {
                    _vpsStatus = string.Format("({0:F4}, {1:F4}) - Invalid Coordinate",
                        location.latitude, location.longitude);
                }
                else if (result.status.IsError())
                {
                    // Convert to OpenXR status for readable native error codes.
                    _vpsStatus = result.status.ToOpenXRStatus().ToString();
                }
                else
                {
                    _currentGeospatialPose = new XRGeospatialPose(location);
                    _vpsStatus = string.Format("({0:F4}, {1:F4}) - {2}",
                        location.latitude, location.longitude, result.value);
                }
            }
            catch (OperationCanceledException)
            {
                _vpsStatus = "OperationCanceledException";
                Debug.LogError("CheckVPSAvailability has been canceled.");
            }
            catch (Exception e)
            {
                _vpsStatus = e.GetType().Name;
                Debug.LogException(e, this);
            }
        }

        private async Task TrySetAuthWithRetry(uint maxCount, CancellationToken token)
        {
            // Example of manually setting Keyless authentication.
            // Ensure you have set up authentication properly at Google Cloud
            // Console for the current project. Then replace following arguments
            // if other strategy is in use.
            // Otherwise, even TrySetAuthenticationAsync succeeds below, you
            // will then receive XRGoogleCloudAuthError.AuthError while
            // accessing Google Cloud API via geospatial manager.
            var strategy = XRAuthenticationStrategy.Keyless;
            var credential = string.Empty;

            _cloudAuthStatus = CloudAuthStatus.Initializing;
            int delayInMS = (int)(_retryDelay * 1000);
            uint attempts = 0;
            while (attempts < maxCount)
            {
                try
                {
                    if (attempts > 0)
                    {
                        await Task.Delay(delayInMS);
                        token.ThrowIfCancellationRequested();
                        Debug.LogFormat(
                            "Retrying authentication, attempt {0}/{1}...", attempts, maxCount - 1);
                    }

                    Debug.LogFormat(
                        "Attempts to set authentication with strategy {0}...", strategy);
                    var result = await _googleCloudFeature.TrySetAuthenticationAsync(
                        strategy, credential);
                    token.ThrowIfCancellationRequested();
                    if (result.IsSuccess())
                    {
                        _cloudAuthStatus = CloudAuthStatus.Completed;
                        Debug.Log("TrySetAuthenticationAsync succeeded.");
                        PushNotification("TrySetAuthenticationAsync:\n" + result);
                        break;
                    }
                    else if (result.IsUnsupported())
                    {
                        _cloudAuthStatus = CloudAuthStatus.Unsupported;
                        Debug.Log("TrySetAuthenticationAsync unsupported.");
                        PushNotification("TrySetAuthenticationAsync:\n" + result);
                        break;
                    }
                    else if (result.IsKeylessError())
                    {
                        _cloudAuthStatus = CloudAuthStatus.AuthError;
                        Debug.Log("TrySetAuthenticationAsync failed with keyless error.");
                        PushNotification("TrySetAuthenticationAsync:\n" +
                            result.nativeStatusCode.GetDisplayName());
                        break;
                    }

                    // Try again later for common failures.
                    _cloudAuthResult = new XRGoogleCloudAuthResult(result);
                    Debug.LogErrorFormat("TrySetAuthenticationAsync failed with {0}.", result);
                    PushNotification("TrySetAuthenticationAsync:\n" + result);
                    attempts++;
                }
                catch (OperationCanceledException)
                {
                    _cloudAuthStatus = CloudAuthStatus.Canceled;
                    Debug.LogWarning("TrySetAuthenticationAsync has been canceled");
                    PushNotification("TrySetAuthenticationAsync:\nCanceled");
                    break;
                }
                catch (Exception e)
                {
                    // Try again for unknown issues.
                    OpenXRResultStatus status =
                        new OpenXRResultStatus(OpenXRResultStatus.StatusCode.UnknownError);
                    _cloudAuthResult = new XRGoogleCloudAuthResult(status);
                    Debug.LogException(e, this);
                    PushNotification("TrySetAuthenticationAsync:\n" + e.GetType().Name);
                    attempts++;
                }
            }

            if (attempts >= maxCount)
            {
                _cloudAuthStatus = CloudAuthStatus.Failed;
                Debug.LogErrorFormat(
                    "Failed to complete cloud authentication after {0} attempts.", maxCount);
                PushNotification("Cloud Auth Failed:\nMax attempts exceeded.");
            }
        }

        private void OnAuthResultReceived(XRGoogleCloudAuthResult result)
        {
            // Update internal status or retry Cloud API later.
            PushNotification("OnAuthResultReceived:\n" + result);
            if (result.CloudAuthError != XRGoogleCloudAuthError.None)
            {
                _cloudAuthStatus = CloudAuthStatus.AuthError;
                _cloudAuthResult = result;
            }
        }

        private void OnGeospatialStateChanged(XRGeospatialStateChangedEventArgs eventArgs)
        {
            bool isInitFailed = eventArgs.CurrState == XRGeospatialState.InitializationFailed;
            if (isInitFailed && XRGeospatialManager.InitializationResult.IsCloudAuthError())
            {
                _cloudAuthStatus = CloudAuthStatus.AuthError;
                _cloudAuthResult = new XRGoogleCloudAuthResult(XRGoogleCloudAuthError.AuthError);
            }
            else if (eventArgs.CurrState >= XRGeospatialState.Running &&
                _cloudAuthStatus == CloudAuthStatus.Unknown)
            {
                // For authentication auto-setting, update status based on completed result.
                _cloudAuthStatus = CloudAuthStatus.Completed;
            }

            string message = string.Format("OnGeospatialStateChanged:\n{0}{1}", eventArgs,
                isInitFailed ? "\n" + XRGeospatialManager.InitializationResult : string.Empty);
            PushNotification(message);
        }

        private void CancelAllTasks()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
        }
    }
}
