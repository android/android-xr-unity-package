// <copyright file="XRFaceTrackingManager.cs" company="Google LLC">
//
// Copyright 2024 Google LLC
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
    using System.Threading;
    using Google.XR.Extensions.Internal;
    using UnityEngine;
    using UnityEngine.Android;
    using UnityEngine.XR.OpenXR;

    /// <summary>
    /// This class provides the current eye information.
    /// </summary>
    public class XRFaceTrackingManager : MonoBehaviour
    {
        private XRFaceState _face;
        private int _parameterCount;
        private int _regionConfidencesCount;
        private bool _grantedPermission = false;
        private bool _enabledTracking = false;

        /// <summary>
        /// Gets the current value of the face parameters.
        /// </summary>
        public XRFaceState Face { get => _face; }

        /// <summary>
        /// Gets the current calibration state of the face tracker.
        /// </summary>
        /// <returns>Returns true if calibrated, false otherwise.</returns>
        public bool IsFaceCalibrated()
        {
            bool isCalibrated = false;
            return XRFaceTrackerApi.IsFaceCalibrated(ref isCalibrated) && isCalibrated;
        }

        private void Awake()
        {
            _face.Parameters = new float[0];
            _face.ConfidenceRegions = new float[0];
            _face.TrackingState = XRFaceTrackingStates.Stopped;
            _face.Timestamp = 0;
        }

        private void Start()
        {
            _parameterCount = Enum.GetNames(typeof(XRFaceParameterIndices)).Length;
            _regionConfidencesCount = Enum.GetNames(typeof(XRFaceConfidenceRegion)).Length;
        }

        private void Update()
        {
            if (!_grantedPermission)
            {
                _grantedPermission = RuntimePermissionCheck();
            }

            if (_grantedPermission && !_enabledTracking)
            {
                // Use lazy-initializer to avoid tracker creation failure for permissions denied.
                XRFaceTrackerApi.SetTrackingEnabled(true);
                _enabledTracking = true;
            }

            if (_enabledTracking)
            {
                XRFaceTrackerApi.GetXRFaceState(_parameterCount, _regionConfidencesCount,
                    ref _face);
            }
        }

        private void OnEnable()
        {
            _enabledTracking = false;
            _grantedPermission = RuntimePermissionCheck();
            if (!_grantedPermission)
            {
                Debug.LogWarningFormat("Waiting for runtime permission: {0}",
                    XRFaceTrackingFeature.RequiredPermission.ToPermissionString());
            }
        }

        private void OnDisable()
        {
            XRFaceTrackerApi.SetTrackingEnabled(false);
            _enabledTracking = false;
            if (_face.TrackingState == XRFaceTrackingStates.Tracking)
            {
                _face.TrackingState = XRFaceTrackingStates.Paused;
            }
        }

        private bool RuntimePermissionCheck()
        {
            return Permission.HasUserAuthorizedPermission(
                XRFaceTrackingFeature.RequiredPermission.ToPermissionString());
        }
    }
}
