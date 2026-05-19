// <copyright file="ImageTrackingController.cs" company="Google LLC">
//
// Copyright 2025 Google LLC
// Copyright Qualcomm Technologies, Inc. and/or its affiliates. All rights reserved.
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

namespace Google.XR.Extensions.Samples.ImageTracking
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;
    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.ARSubsystems;
    using UnityEngine.XR.OpenXR;

    /// <summary>
    /// An sample of image tracking with <see cref="ARTrackedImageManager"/>
    /// </summary>
    [RequireComponent(typeof(AndroidXRPermissionUtil))]
    public class ImageTrackingController : MonoBehaviour
    {
        /// <summary>
        /// Text mesh to display debug information.
        /// </summary>
        public TextMesh DebugText;

        /// <summary>
        /// The <see cref="ARTrackedImageManager"/> component in the scene.
        /// </summary>
        public ARTrackedImageManager ImageManager;

        private List<TrackableId> _images = new List<TrackableId>();
        private int _imageAdded = 0;
        private int _imageUpdated = 0;
        private int _imageRemoved = 0;

        private AndroidXRPermissionUtil _permissionUtil;
        private StringBuilder _stringBuilder = new StringBuilder();
        private string _imageTrackingMsg = "configuration pending";

        /// <summary>
        /// Called from AR Tracked Image Manager component callback.
        /// </summary>
        /// <param name="eventArgs">Image change event arguments.</param>
        public void OnImageChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
        {
            foreach (ARTrackedImage trackedImage in eventArgs.added)
            {
                Debug.LogFormat("Adding tracked image at {0}\n{1}",
                    Time.frameCount,
                    GetImageDebugInfo(trackedImage));
            }

            _imageAdded = eventArgs.added.Count;
            _imageUpdated = eventArgs.updated.Count;
            _imageRemoved = eventArgs.removed.Count;
            _images = eventArgs.updated.Select(image => image.trackableId).ToList();
        }

        private void Awake()
        {
            // Ensure manager is disabled for start
            // and waiting for runtime permissions.
            if (ImageManager != null)
            {
                ImageManager.enabled = false;
            }
        }

        private void OnEnable()
        {
            _permissionUtil = GetComponent<AndroidXRPermissionUtil>();
            if (ImageManager == null)
            {
                Debug.LogError("ARTrackedImageManager is null!");
                return;
            }

            if (ImageManager.subsystem is AndroidXRImageTrackingSubsystem subsystem)
            {
                subsystem.OnImageTrackingConfigured += OnImageTrackingConfigured;
                subsystem.OnImageTrackingLost += OnImageTrackingLost;
            }

            StartCoroutine(WaitForPermissions());
        }

        private void OnDisable()
        {
            _images.Clear();
            StopAllCoroutines();

            if (ImageManager != null
                && ImageManager.subsystem is AndroidXRImageTrackingSubsystem subsystem)
            {
                subsystem.OnImageTrackingConfigured -= OnImageTrackingConfigured;
                subsystem.OnImageTrackingLost -= OnImageTrackingLost;
            }
        }

        private IEnumerator WaitForPermissions()
        {
            yield return new WaitUntil(() => _permissionUtil.AllPermissionGranted());

            if (ImageManager != null)
            {
                // Enable manager after granted permissions to ensure underlying
                // tracker can be created successfully.
                ImageManager.enabled = true;
            }
        }

        // Update is called once per frame
        private void Update()
        {
            if (!_permissionUtil.AllPermissionGranted())
            {
                return;
            }

            _stringBuilder.Clear();
            UpdateImages();
            if (_stringBuilder.Length > 0)
            {
                DebugText.text = _stringBuilder.ToString();
            }
        }

        private void UpdateImages()
        {
            var isQrCodeExtensionEnabled =
                XRQrCodeTrackingFeature.IsExtensionEnabled.GetValueOrDefault();
            if (!isQrCodeExtensionEnabled)
            {
                _stringBuilder.Append("XR_ANDROID_trackables_qr_code is not enabled.\n");
            }

            var isMarkerExtensionEnabled =
                XRMarkerTrackingFeature.IsExtensionEnabled.GetValueOrDefault();
            if (!isMarkerExtensionEnabled)
            {
                _stringBuilder.Append("XR_ANDROID_trackables_marker is not enabled.\n");
            }

            var isImageExtensionEnabled =
                XRImageTrackingFeature.IsExtensionEnabled.GetValueOrDefault();
            if (!isImageExtensionEnabled)
            {
                _stringBuilder.Append(
                    "XR_ANDROID_trackables_image or XR_EXT_future is not enabled.\n");
            }

            if (!isQrCodeExtensionEnabled && !isMarkerExtensionEnabled && !isImageExtensionEnabled)
            {
                // no image tracking feature is enabled.
                return;
            }

            if (ImageManager == null || ImageManager.subsystem == null)
            {
                _stringBuilder.Append("Cannot find ARTrackedImageManager.\n");
                return;
            }

            _stringBuilder.Append($"{ImageManager.subsystem.GetType()}\n");
            if (isImageExtensionEnabled)
            {
                _stringBuilder.Append($"Image Tracking: {_imageTrackingMsg} \n");
            }

            _stringBuilder.AppendFormat(
                $"Images: ({_imageAdded}, {_imageUpdated}, {_imageRemoved})\n" +
                $"{string.Join("\n", _images.Select(id => id.subId1).ToArray())}\n");
        }

        private string GetImageDebugInfo(ARTrackedImage trackable)
        {
            if (trackable == null)
            {
                return string.Empty;
            }

            string reference =
                trackable.referenceImage == null ? "null" : trackable.referenceImage.name;
            string data = "No data.";
            if (trackable.IsQrCode())
            {
                trackable.TryGetQrCodeData(out data);
            }
            else if (trackable.IsMarker())
            {
                trackable.TryGetMarkerData(out XRMarkerDictionary dictionary, out int id);
                data = $"dictionary={dictionary}, id={id}";
            }
            else
            {
                data = $"image-name={trackable.referenceImage.name}";
            }

            return string.Format(
                $"Image: {trackable.trackableId.subId1}-{trackable.trackableId.subId2}\n" +
                $"  Reference: {reference}\n" +
                $"  NativePtr: {trackable.nativePtr}\n" +
                $"  Tracking: {trackable.trackingState}\n" +
                $"  Pose: {trackable.transform.position}-{trackable.transform.rotation}\n" +
                $"  Size: {trackable.size}\n" +
                $"  Data: {data}");
        }

        private void OnImageTrackingConfigured(bool success)
        {
            if (success)
            {
                Debug.Log("Image tracking configuration completed successfully.");
                _imageTrackingMsg = "configuration successful";
            }
            else
            {
                // A pending configuration may be canceled when the ARTrackedImageManager
                // is disabled or when a new configuration process is started.
                Debug.Log("Image tracking configuration failed or was canceled.");
                _imageTrackingMsg = "configuration failed";
            }
        }

        private void OnImageTrackingLost()
        {
            // This event is raised if image tracking encounters an internal failure,
            // causing it to stop and invalidating the current configuration.
            // Restoring image tracking requires a reconfiguration,
            // e.g., by restarting the subsystem.
            if (!ImageManager.enabled)
            {
                // This event may be raised while the ARTrackedImageManager is disabled.
                // Image tracking will be reconfigured when the ARTrackedImageManager is re-enabled.
                _imageTrackingMsg = "tracking disabled";
                return;
            }

            Debug.Log("Image tracking lost. Restarting the subsystem to " +
                      "reconfigure image tracking and resume tracking.");
            _imageTrackingMsg = "tracking lost - restarting";

            // Disable ARTrackedImageManager, which will stop the subsystem.
            ImageManager.enabled = false;

            // Enable ARTrackedImageManager, which will restart the subsystem
            // and reconfigure image tracking.
            ImageManager.enabled = true;
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(
                UnityEditor.BuildTargetGroup.Android);

            var qrCodeFeature = settings.GetFeature<XRQrCodeTrackingFeature>();
            if (qrCodeFeature == null)
            {
                Debug.LogErrorFormat(
                    "Cannot find {0} targeting Android platform.", XRQrCodeTrackingFeature.UiName);
            }
            else if (!qrCodeFeature.enabled)
            {
                Debug.LogWarningFormat(
                    "{0} is disabled. ImageTracking sample will not detect QR Code.",
                    XRQrCodeTrackingFeature.UiName);
            }

            var markerFeature = settings.GetFeature<XRMarkerTrackingFeature>();
            if (markerFeature == null)
            {
                Debug.LogErrorFormat(
                    "Cannot find {0} targeting Android platform.", XRMarkerTrackingFeature.UiName);
            }
            else if (!markerFeature.enabled)
            {
                Debug.LogWarningFormat(
                    "{0} is disabled. ImageTracking sample will not detect markers.",
                    XRMarkerTrackingFeature.UiName);
            }

            var imageFeature = settings.GetFeature<XRImageTrackingFeature>();
            if (imageFeature == null)
            {
                Debug.LogErrorFormat(
                    "Cannot find {0} targeting Android platform.", XRImageTrackingFeature.UiName);
            }
            else if (!imageFeature.enabled)
            {
                Debug.LogWarningFormat(
                    "{0} is disabled. ImageTracking sample will not detect images.",
                    XRImageTrackingFeature.UiName);
            }
#endif // UNITY_EDITOR
        }
    }
}
