// <copyright file="FaceTracking.cs" company="Google LLC">
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

namespace Google.XR.Extensions.Samples.FaceTracking
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.XR.OpenXR;

    /// <summary>
    /// Demonstrates the face tracking feature for avatar facial blendshapes.
    /// </summary>
    [RequireComponent(typeof(XRFaceTrackingManager), typeof(AndroidXRPermissionUtil))]
    public class FaceTracking : MonoBehaviour
    {
        /// <summary> Text mesh to display debug information over mirror. </summary>
        public TextMesh DebugTextTopCenter;

        /// <summary> Text mesh to display current frame rate. </summary>
        public TextMesh FPSCounter;

        /// <summary> Text mesh to display debug information under mirror. </summary>
        public TextMesh DebugTextBottomCenter;

        /// <summary> Text mesh to display debug information left of mirror. </summary>
        public TextMesh DebugTextLeft;

        /// <summary> Text mesh to display debug information right of mirror. </summary>
        public TextMesh DebugTextRight;

        /// <summary> Capture timestamp for the current face state. </summary>
        public TextMesh DebugTextTimeStamp;

        /// <summary> Indicates if the frame data is not random or default. </summary>
        public TextMesh DebugTextFrameValid;

        /// <summary> Indicates the confidence of the bottom face region. </summary>
        public TextMesh DebugTextConfidenceBottom;

        /// <summary> Indicates the confidence of the left face region. </summary>
        public TextMesh DebugTextConfidenceLeft;

        /// <summary> Indicates the confidence of the right face region. </summary>
        public TextMesh DebugTextConfidenceRight;

        /// <summary> An instance of the face mesh renderer. </summary>
        public SkinnedMeshRenderer SkinnedMeshRenderer;

        // The timeout threshold in seconds while waiting services to start tracking during
        // lifecycle tests.
        private const int _serviceStartTimeout = 5;

        private XRFaceTrackingManager _faceManager;
        private AndroidXRPermissionUtil _permissionUtil;
        private string[] _paramNames;

        private void Awake()
        {
            InvokeRepeating(nameof(UpdateFPS), 0, 0.5f);
            _paramNames = Enum.GetNames(typeof(XRFaceParameterIndices));
            _faceManager = GetComponent<XRFaceTrackingManager>();

            _permissionUtil = GetComponent<AndroidXRPermissionUtil>();
        }

        private void OnEnable()
        {
            StartCoroutine(CountdownToActivation());
        }

        private void OnDisable()
        {
            StopCoroutine(CountdownToActivation());
        }

        private void UpdateFPS()
        {
            FPSCounter.text = ((int)(1f / Time.unscaledDeltaTime)).ToString();
        }

        IEnumerator CountdownToActivation()
        {
            yield return new WaitUntil(() => _permissionUtil.AllPermissionGranted());

            DebugTextBottomCenter.color = Color.magenta;
            DebugTextBottomCenter.text = "Testing tracker on/off toggle";

            // Wait for face manager's update after granted permissions.
            // Otherwise, XRFaceState stays in initial value.
            yield return new WaitForSeconds(1);

            // Test toggleing off the face tracker
            _faceManager.enabled = false;
            yield return new WaitForSeconds(0.5f);
            if (_faceManager.Face.TrackingState == XRFaceTrackingStates.Paused)
            {
                DebugTextBottomCenter.color = Color.green;
                DebugTextBottomCenter.text = "Tracker toggle off test successfully completed";
            }
            else
            {
                DebugTextBottomCenter.color = Color.red;
                DebugTextBottomCenter.text = "Tracker toggle off test failed";
            }

            yield return new WaitForSeconds(1);

            // Test toggling on the face tracker
            _faceManager.enabled = true;
            int timeSpent = 0;
            while (timeSpent < _serviceStartTimeout)
            {
                yield return new WaitForSeconds(1);
                timeSpent++;
                if (_faceManager.Face.TrackingState == XRFaceTrackingStates.Tracking)
                {
                    break;
                }
            }

            if (_faceManager.Face.TrackingState == XRFaceTrackingStates.Tracking)
            {
                DebugTextBottomCenter.color = Color.green;
                DebugTextBottomCenter.text = string.Format(
                    "Tracker toggle on test successfully completed in {0}s", timeSpent);
            }
            else
            {
                DebugTextBottomCenter.color = Color.red;
                DebugTextBottomCenter.text = string.Format(
                    "Tracker toggle on test failed with timeout {0}s", timeSpent);
            }

            yield return new WaitForSeconds(1);

            DebugTextBottomCenter.color = Color.magenta;
            DebugTextBottomCenter.text = "Testing tracker on/off toggle complete";
            yield return new WaitForSeconds(1);

            // Check if the face is calibrated but wait until we start to get valid data.
            DebugTextBottomCenter.color = Color.magenta;
            DebugTextBottomCenter.text = "Checking face calibration state.";
            DebugTextBottomCenter.alignment = TextAlignment.Center;
            yield return new WaitForSeconds(2);

            DebugTextBottomCenter.text = string.Empty;
            if (_faceManager.IsFaceCalibrated())
            {
                DebugTextBottomCenter.color = Color.green;
                DebugTextBottomCenter.text = "Face Is Calibrated: True";
                yield return new WaitForSeconds(2);
            }
            else
            {
                DebugTextBottomCenter.color = Color.red;
                DebugTextBottomCenter.text = "Face Is Calibrated: False"
                + "\nFace calibration is required."
                + "\nPlease exit the app, calibrate face in Settings, and relaunch.";
                yield return null;
            }
        }

        private void Update()
        {
            if (!XRFaceTrackingFeature.IsFaceTrackingExtensionEnabled.HasValue)
            {
                DebugTextTopCenter.text = "XrInstance hasn't been initialized.";
                return;
            }
            else if (!XRFaceTrackingFeature.IsFaceTrackingExtensionEnabled.Value)
            {
                DebugTextTopCenter.text = "XR_ANDROID_face_tracking is not enabled.";
                return;
            }

            DebugTextTopCenter.text = "Tracking State: " + _faceManager.Face.TrackingState;
            DebugTextTimeStamp.text = "Capture Time: " + _faceManager.Face.Timestamp;
            DebugTextFrameValid.text = "Frame Valid: " + _faceManager.Face.IsValid;

            DebugTextConfidenceBottom.text = "0";
            DebugTextConfidenceLeft.text = "0";
            DebugTextConfidenceRight.text = "0";
            for (int x = 0; x < _faceManager.Face.ConfidenceRegions.Length; x++)
            {
                TextMesh regionText = null;
                switch (x)
                {
                    case (int)XRFaceConfidenceRegion.Lower:
                        regionText = DebugTextConfidenceBottom;
                        break;
                    case (int)XRFaceConfidenceRegion.LeftUpper:
                        regionText = DebugTextConfidenceLeft;
                        break;
                    case (int)XRFaceConfidenceRegion.RightUpper:
                        regionText = DebugTextConfidenceRight;
                        break;
                }

                if (regionText != null)
                {
                    regionText.text = _faceManager.Face.ConfidenceRegions[x].ToString("F4");
                }
            }

            DebugTextLeft.text = string.Empty;
            DebugTextRight.text = string.Empty;
            for (int x = 0; x < _faceManager.Face.Parameters.Length; x++)
            {
                if (x <= _faceManager.Face.Parameters.Length / 2)
                {
                    DebugTextLeft.text += "\n" + _paramNames[x] + ": " +
                        _faceManager.Face.Parameters[x].ToString("F4");
                }
                else
                {
                    DebugTextRight.text += "\n" + _paramNames[x] + ": " +
                        _faceManager.Face.Parameters[x].ToString("F4");
                }

                SkinnedMeshRenderer.SetBlendShapeWeight(x, _faceManager.Face.Parameters[x] * 100);
            }
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(
                UnityEditor.BuildTargetGroup.Android);
            var faceTrackingFeature = settings.GetFeature<XRFaceTrackingFeature>();
            if (faceTrackingFeature == null)
            {
                Debug.LogErrorFormat(
                    "Cannot find {0} targeting Android platform.", XRFaceTrackingFeature.UiName);
                return;
            }
            else if (!faceTrackingFeature.enabled)
            {
                Debug.LogWarningFormat(
                    "{0} is disabled. FaceTracking sample will not work correctly.",
                    XRFaceTrackingFeature.UiName);
            }
#endif // UNITY_EDITOR
        }
    }
}
