// <copyright file="XRGeospatialManager.cs" company="Google LLC">
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
    using System.Collections.Generic;
    using Google.XR.Extensions.Internal;
    using UnityEngine;
    using UnityEngine.XR.ARSubsystems;
    using UnityEngine.XR.OpenXR;
    using UnityEngine.XR.OpenXR.NativeTypes;

    using XrGeospatialTracker = System.UInt64;

    /// <summary>
    /// Controls the lifecycle and configuration for a Geospatial Tracker.
    /// There is only one active tracker. Adding multiple
    /// <see cref="XRGeospatialManager"/> components is not allowed.
    /// </summary>
    /// <remarks>
    /// You may enable or disable the <see cref="XRGeospatialManager"/>
    /// component to start or stop the tracker, the underlying resources will be
    /// released on stopped.
    /// </remarks>
    [DisallowMultipleComponent]
    [AddComponentMenu("XR/Android XR/XR Geospatial Manager")]
    public sealed class XRGeospatialManager : MonoBehaviour
    {
        /// <summary>
        /// The event fired whenever the <see cref="State"/> changes.
        /// </summary>
        public static Action<XRGeospatialStateChangedEventArgs> OnStateChanged;

        /// <summary>
        /// Surface XrGeospatialTrackerStateANDROID to Unity.
        /// </summary>
        private static XRGeospatialState _state;

        /// <summary>
        /// Represents the OpenXR native handle if it has been initialized.
        /// </summary>
        private static XrGeospatialTracker _nativeHandle = 0;

        /// <summary>
        /// Represents the initialization result of the geospatial tracker.
        /// </summary>
        private static OpenXRResultStatus _initializationResult = new OpenXRResultStatus(
            OpenXRResultStatus.StatusCode.ProviderUninitialized);

        private static bool _isSessionReady = false;
        private static bool _isManagerActive = false;
        private XRGeospatialFeature _geospatialFeature = null;
        private bool _isEventSubscribed = false;
        private List<XRVPSAvailabilityFuture> _pendingFutures = new List<XRVPSAvailabilityFuture>();

        /// <summary>
        /// Gets the state that represents the geospatial services.
        /// Used to determine the status of geospatial capability and tracker state.
        /// </summary>
        public static XRGeospatialState State
        {
            get => _state;
            internal set
            {
                var oldState = _state;
                _state = value;
                if (oldState != _state)
                {
                    OnStateChanged?.Invoke(
                        new XRGeospatialStateChangedEventArgs(oldState, _state));
                }
            }
        }

        /// <summary>
        /// The result returned from initialization.
        /// Check <see cref="OpenXRResultStatus.nativeStatusCode"/> for the failure reason.
        /// It can be <see cref="XrResult.PermissionInsufficient"/>,
        /// <see cref="XrResultAndroid.GeospatialCloudAuthFailed"/> or common
        /// OpenXR errors from <see cref="XrResult"/>.
        /// Once it's resolved, you may disable and re-enable the manager to
        /// retry initialization.
        /// </summary>
        public static OpenXRResultStatus InitializationResult => _initializationResult;

        /// <summary>
        /// Gets a value indicating whether the Geospatial tracker is running.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item><description>
        /// Returns <c>true</c> after OnEnable() has been called and the
        /// initialization succeeded.
        /// </description></item>
        /// <item><description>
        /// Returns <c>false</c> after OnDisable() is called or the
        /// initialization failed.
        /// Accessing geospatial API under this state may result in
        /// <see cref="XrResultAndroid.GeospatialTrackerNotRunning"/> error,
        /// check individual API for its requirement.
        /// </description></item>
        /// </list>
        /// </remarks>
        public bool Running => _state == XRGeospatialState.Running;

        /// <summary>
        /// Try to check the availability of VPS near a given location asynchronously.
        /// A <see cref="XRGeospatialState.Running"/> tracker is not required,
        /// as long as Geospatial is supported and enabled by the current session.
        /// </summary>
        /// <param name="latitude">The latitude of the location.</param>
        /// <param name="longitude">The longitude of the location.</param>
        /// <returns>
        /// The result of the asynchronous operation.
        /// It can fail with <see cref="XrResult.PermissionInsufficient"/>,
        /// <see cref="XrResultAndroid.GeospatialCoordinateInvalid"/>,
        /// and other common OpenXR errors from <see cref="XrResult"/>.
        /// </returns>
        public async Awaitable<Result<XRVPSAvailability>> TryCheckVPSAvailabilityAsync(
            double latitude, double longitude)
        {
            var status = new XRResultStatus(XRResultStatus.StatusCode.ProviderUninitialized);
            if (State < XRGeospatialState.Initializing)
            {
                return new Result<XRVPSAvailability>(status, XRVPSAvailability.Unknown);
            }

            var result = XRVPSAvailabilityFuture.TryCheck(
                latitude, longitude, out XRVPSAvailabilityFuture future);
            if (result.IsError())
            {
                Debug.LogErrorFormat("Failed to create XRVPSAvailabilityFuture with {0}", result);
                return new Result<XRVPSAvailability>(
                    result.ToXRResultStatus(), XRVPSAvailability.Unknown);
            }

            _pendingFutures.Add(future);
            status = new XRResultStatus(XRResultStatus.StatusCode.UnknownError);
            var vpsResult = new Result<XRVPSAvailability>(status, XRVPSAvailability.Unknown);
            try
            {
                vpsResult = await future.TryGetResultAsync();
                if (vpsResult.status.IsError())
                {
                    // Convert to OpenXRStatus for clearer messages around XrResult failures.
                    Debug.LogErrorFormat("XRVPSAvailabilityFuture failed with {0}",
                        vpsResult.status.ToOpenXRStatus());
                }
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("TryCheckVPSAvailabilityAsync is canceled.");
            }
            catch (Exception e)
            {
                Debug.LogException(e, this);
            }
            finally
            {
                _pendingFutures.Remove(future);
                future?.Dispose();
            }

            return vpsResult;
        }

        /// <summary>
        /// Try to locate the geospatial pose for the given reference frame,
        /// expressed as a pose related to Unity world space.
        /// A <see cref="XRGeospatialState.Running"/> tracker is required.
        /// </summary>
        /// <param name="pose">The pose in Unity world space to be converted.
        /// </param>
        /// <param name="result">The result of geospatial pose.</param>
        /// <returns>True if the calculation succeeded, and use
        /// <see cref="XRGeospatialPoseResult.LocationFlags"/> to determine
        /// which fields are valid.
        /// </returns>
        public bool TryLocate(Pose pose, out XRGeospatialPoseResult result)
        {
            result = default;
            if (!Running)
            {
                return false;
            }

            return XRGeospatialApi.Locate(_nativeHandle, pose, out result);
        }

        /// <summary>
        /// Try to locate the pose in Unity world space from a geospatial pose.
        /// A <see cref="XRGeospatialState.Running"/> tracker is required.
        /// </summary>
        /// <param name="geospatialPose">The geospatial pose to be converted.</param>
        /// <param name="result">The result of the pose in Unity world space.</param>
        /// <returns>True if the calculation succeeded, and use
        /// <see cref="XRPoseResult.LocationFlags"/> to determine
        /// which fields are valid.
        /// Otherwise, returns <c>false</c>. You may retry later with a different geospatial pose.
        /// </returns>
        public bool TryLocate(XRGeospatialPose geospatialPose, out XRPoseResult result)
        {
            result = default;
            if (!Running || !geospatialPose.IsCoordinateValid)
            {
                return false;
            }

            return XRGeospatialApi.Locate(_nativeHandle, geospatialPose, out result);
        }

        internal static void SetSession(ulong xrSession)
        {
            _isSessionReady = xrSession != 0;
            if (_isSessionReady && _isManagerActive)
            {
                StartTracking();
            }
            else if (!_isSessionReady)
            {
                StopTracking();
            }
        }

        private static void StartTracking()
        {
            if (_nativeHandle != 0)
            {
                return;
            }

            var result = XRGeospatialApi.Create(out _nativeHandle);
            var status = new OpenXRResultStatus(result);
            if (status.IsUnsupported())
            {
                State = XRGeospatialState.Unsupported;
                return;
            }
            else if (status.IsError())
            {
                _initializationResult = status;
                State = XRGeospatialState.InitializationFailed;
            }
            else
            {
                // Waiting for XrEventDataGeospatialTrackerStateChanged.
                State = XRGeospatialState.Initializing;
            }
        }

        private static void StopTracking()
        {
            if (_nativeHandle == 0)
            {
                return;
            }

            XRGeospatialApi.Destroy(_nativeHandle);
            _nativeHandle = 0;
            State = XRGeospatialState.Stopped;
        }

        private void OnEnable()
        {
            _geospatialFeature = OpenXRSettings.Instance.GetFeature<XRGeospatialFeature>();
            if (_geospatialFeature == null)
            {
                State = XRGeospatialState.Unsupported;
                Debug.LogErrorFormat(
                    "{0} is not available at current platform, disabling {1}.",
                    XRGeospatialFeature.UiName, GetType().Name);
                enabled = false;
                return;
            }
            else if (XRGeospatialFeature.IsExtensionEnabled == null && !_geospatialFeature.enabled)
            {
                State = XRGeospatialState.Disabled;
                Debug.LogErrorFormat(
                    "{0} is disabled at current platform, {1} will do nothing. Disabling it.",
                    XRGeospatialFeature.UiName, GetType().Name);
                enabled = false;
                return;
            }

            _isManagerActive = true;
            ToggleTrackerStateEvent(true);
            if (_isSessionReady)
            {
                StartTracking();
            }
        }

        private void OnDisable()
        {
            _isManagerActive = false;
            foreach (var future in _pendingFutures)
            {
                future.Dispose();
            }

            _pendingFutures.Clear();
            ToggleTrackerStateEvent(false);
            StopTracking();
        }

        private void ToggleTrackerStateEvent(bool subscribe)
        {
            if (subscribe == _isEventSubscribed)
            {
                return;
            }

            unsafe
            {
                const XrStructureType eventType =
                    XrEventDataGeospatialTrackerStateChanged.EventType;
                var result = subscribe ?
                    PollEventRouter.TrySubscribeToEventType(eventType, OnTrackerStateChanged) :
                    PollEventRouter.TryUnsubscribeFromEventType(eventType, OnTrackerStateChanged);
                if (!result)
                {
                    Debug.LogErrorFormat("{0} failed on {1}.",
                        subscribe ? "TrySubscribeToEventType" : "TryUnsubscribeFromEventType",
                        eventType);
                }
                else
                {
                    _isEventSubscribed = subscribe;
                    Debug.LogFormat("{0} succeeded on {1}.",
                        subscribe ? "TrySubscribeToEventType" : "TryUnsubscribeFromEventType",
                        eventType);
                }
            }
        }

        private unsafe void OnTrackerStateChanged(XrEventDataBaseHeader* baseHeader)
        {
            var eventData = *(XrEventDataGeospatialTrackerStateChanged*)baseHeader;
            if (eventData.Type != XrEventDataGeospatialTrackerStateChanged.EventType ||
                eventData.Tracker != _nativeHandle)
            {
                return;
            }

            if (eventData.State == ApiGeospatialTrackerState.InitializationFailed)
            {
                _initializationResult = new OpenXRResultStatus(
                    eventData.InitializationResult);
            }
            else
            {
                _initializationResult = new OpenXRResultStatus(
                    OpenXRResultStatus.StatusCode.UnqualifiedSuccess);
            }

            // Send state changed events after updated initialization result.
            State = eventData.State.ToXRGeospatialState();
        }
    }
}
