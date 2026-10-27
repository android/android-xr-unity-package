// <copyright file="SpatialEntitiesController.cs" company="Google LLC">
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

namespace Google.XR.Extensions.Samples.SpatialEntities
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Google.XR.Extensions.SpatialEntities;
    using UnityEngine;
    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.ARSubsystems;
    using UnityEngine.XR.OpenXR;
    using UnityEngine.XR.OpenXR.Features;

    /// <summary>
    /// Used to test spatial entity features at runtime.
    /// </summary>
    [RequireComponent(typeof(AndroidXRPermissionUtil))]
    [RequireComponent(typeof(ARSession))]
    [RequireComponent(typeof(ARAnchorManager))]
    [RequireComponent(typeof(ARPlaneManager))]
    [RequireComponent(typeof(ARBoundingBoxManager))]
    public class SpatialEntitiesController : MonoBehaviour
    {
        /// <summary>
        /// Text mesh for displaying debug information.
        /// </summary>
        public TextMesh DebugText;

        private AndroidXRPermissionUtil _permissionUtil = null;
        private ARSession _session = null;
        private ARAnchorManager _anchorManager = null;
        private ARAnchor _spatialAnchor = null;
        private ARPlaneManager _planeManager = null;
        private TrackableChanges _planeChanges = default;
        private SpatialBoundingBoxFeature _boundingBoxFeature = null;
        private ARBoundingBoxManager _boundingBoxManager = null;
        private TrackableChanges _boxChanges = default;
        private bool _permissionGranted = false;
        private StringBuilder _stringBuilder = new StringBuilder();

        /// <summary>
        /// Called from <see cref="ARAnchorManager"/> component callback.
        /// </summary>
        /// <param name="eventArgs"><see cref="ARAnchor"/> change event arguments.</param>
        public void OnAnchorChanged(ARTrackablesChangedEventArgs<ARAnchor> eventArgs)
        {
            foreach (ARAnchor anchor in eventArgs.added)
            {
                Debug.Log("OnAnchorChanged: Added " + anchor.trackableId);
                Debug.Log(DebugAnchor(anchor));
            }

            foreach (var pair in eventArgs.removed)
            {
                Debug.Log("OnAnchorChanged: Removed " + pair.Key);
            }
        }

        /// <summary>
        /// Called from <see cref="ARPlaneManager"/> component callback.
        /// </summary>
        /// <param name="eventArgs"><see cref="ARPlane"/> change event arguments.</param>
        public void OnPlaneChanged(ARTrackablesChangedEventArgs<ARPlane> eventArgs)
        {
            _planeChanges.AddedCount = eventArgs.added.Count;
            _planeChanges.UpdatedCount = eventArgs.updated.Count;
            _planeChanges.RemovedCount = eventArgs.removed.Count;
            foreach (ARPlane plane in eventArgs.added)
            {
                Debug.Log("OnPlaneChanged: Added " + plane.trackableId);
                Debug.Log(DebugPlane(plane));
            }

            foreach (var pair in eventArgs.removed)
            {
                Debug.Log("OnPlaneChanged: Removed " + pair.Key);
            }

            _planeChanges.Trackables?.Clear();
            _planeChanges.Trackables = eventArgs.updated.Select(x => x.trackableId).ToList();
        }

        /// <summary>
        /// Called from <see cref="ARBoundingBoxManager"/> component callback.
        /// </summary>
        /// <param name="eventArgs"><see cref="ARBoundingBox"/> change event arguments.</param>
        public void OnBoundingBoxChanged(ARTrackablesChangedEventArgs<ARBoundingBox> eventArgs)
        {
            _boxChanges.AddedCount = eventArgs.added.Count;
            _boxChanges.UpdatedCount = eventArgs.updated.Count;
            _boxChanges.RemovedCount = eventArgs.removed.Count;
            foreach (ARBoundingBox boundingBox in eventArgs.added)
            {
                Debug.Log("OnBoundingBoxChanged: Added " + boundingBox.trackableId);
                Debug.Log(DebugBoundingBox(boundingBox));
            }

            foreach (var pair in eventArgs.removed)
            {
                Debug.Log("OnBoundingBoxChanged: Removed " + pair.Key);
            }

            _boxChanges.Trackables?.Clear();
            _boxChanges.Trackables = eventArgs.updated.Select(x => x.trackableId).ToList();
        }

        private void Awake()
        {
            _boundingBoxFeature =
                OpenXRSettings.Instance.GetFeature<SpatialBoundingBoxFeature>();
            _session = GetComponent<ARSession>();
            _anchorManager = GetComponent<ARAnchorManager>();
            _anchorManager.enabled = false;
            _planeManager = GetComponent<ARPlaneManager>();
            _planeManager.enabled = false;
            _boundingBoxManager = GetComponent<ARBoundingBoxManager>();
            _boundingBoxManager.enabled = false;
            _permissionUtil = GetComponent<AndroidXRPermissionUtil>();
        }

        private void OnEnable()
        {
            _anchorManager.trackablesChanged.AddListener(OnAnchorChanged);
            _planeManager.trackablesChanged.AddListener(OnPlaneChanged);
            _boundingBoxManager.trackablesChanged.AddListener(OnBoundingBoxChanged);
            StartCoroutine(WaitForPermissions());
        }

        private void OnDisable()
        {
            _anchorManager.trackablesChanged.RemoveListener(OnAnchorChanged);
            _planeManager.trackablesChanged.RemoveListener(OnPlaneChanged);
            _boundingBoxManager.trackablesChanged.RemoveListener(OnBoundingBoxChanged);
            StopAllCoroutines();

            TryDeleteAnchor();
        }

        private void Update()
        {
            if (!_permissionGranted)
            {
                DebugText.text = "Waiting for permissions.";
                return;
            }

            _stringBuilder.Clear();
            DebugCapabilities();

            _stringBuilder.Append($"ARSessionState: {ARSession.state}\n");
            SampleDebugUtils.CheckSubsystemReady(
                typeof(XRSessionSubsystem), _session.subsystem, _stringBuilder);
            if (SampleDebugUtils.CheckSubsystemReady(
                typeof(XRAnchorSubsystem), _anchorManager.subsystem, _stringBuilder))
            {
                TryCreateAndDebugAnchor();
            }

            if (SampleDebugUtils.CheckSubsystemReady(
                    typeof(XRPlaneSubsystem), _planeManager.subsystem, _stringBuilder))
            {
                _planeChanges.AppendDebugInfo(_stringBuilder);
            }

            if (SampleDebugUtils.CheckSubsystemReady(
                    typeof(XRBoundingBoxSubsystem), _boundingBoxManager.subsystem, _stringBuilder))
            {
                _boxChanges.AppendDebugInfo(_stringBuilder);
            }

            if (_stringBuilder.Length > 0)
            {
                DebugText.text = _stringBuilder.ToString();
            }
        }

        private void TryCreateAndDebugAnchor()
        {
            if (_spatialAnchor == null)
            {
                float distance = 0.5f;
                Vector3 spatialPos = Camera.main.transform.position +
                    Camera.main.transform.forward * distance;
                GameObject spatialAnchorGo = new GameObject("SpatialAnchor");
                spatialAnchorGo.transform.position = spatialPos;
                _spatialAnchor = spatialAnchorGo.AddComponent<ARAnchor>();
            }

            if (_spatialAnchor != null)
            {
                _stringBuilder.AppendFormat(" | SpatialAnchor: {0}\n",
                    _spatialAnchor.trackableId);
            }
        }

        private void TryDeleteAnchor()
        {
            if (_spatialAnchor != null)
            {
                Destroy(_spatialAnchor.gameObject);
                _spatialAnchor = null;
            }
        }

        private IEnumerator WaitForPermissions()
        {
            _anchorManager.enabled = false;
            _planeManager.enabled = false;
            _boundingBoxManager.enabled = false;

            yield return new WaitUntil(() => _permissionUtil.AllPermissionGranted());

            // Enable after granted permissions and restart subsystems.
            _anchorManager.enabled = true;
            _planeManager.enabled = true;
            _boundingBoxManager.enabled = true;

            _permissionGranted = true;
        }

        private void DebugCapabilities()
        {
            SampleDebugUtils.CheckFeatureAvailability(_boundingBoxFeature,
                SpatialBoundingBoxFeature.IsExtensionEnabled, _stringBuilder);
        }

        private string DebugAnchor(ARAnchor anchor)
        {
            if (anchor == null)
            {
                return string.Empty;
            }

            return
                $"Anchor ({anchor.trackableId})\n" +
                $" Tracking: {anchor.trackingState}\n" +
                $" NativePtr: {anchor.nativePtr}\n" +
                $" Pose: {anchor.pose.rotation}, {anchor.pose.position}";
        }

        private string DebugPlane(ARPlane plane)
        {
            if (plane == null)
            {
                return string.Empty;
            }

            return
                $"Plane ({plane.trackableId})\n" +
                $" Tracking: {plane.trackingState}\n" +
                $" NativePtr: {plane.nativePtr}\n" +
                $" Center: {plane.center}\n" +
                $" Normal: {plane.normal}\n" +
                $" Extents: {plane.extents}\n" +
                $" SubsumedBy: {(plane.subsumedBy ? plane.subsumedBy.nativePtr : "null")}\n" +
                $" Alignment: {plane.alignment}\n" +
                $" Classification: {plane.classifications}";
        }

        private string DebugBoundingBox(ARBoundingBox bb)
        {
            if (bb == null)
            {
                return string.Empty;
            }

            return $"BoundingBox ({bb.trackableId})\n" +
                   $" Tracking: {bb.trackingState}\n" +
                   $" NativePtr: {bb.nativePtr}\n" +
                   $" Pose: {bb.pose.position}, {bb.pose.rotation}\n" +
                   $" Size: {bb.size}\n" +
                   $" Classifications: {bb.classifications}";
        }

        private void OnValidate()
        {
#if !UNITY_SPATIAL_ENTITIES
            Debug.LogWarning(
                "Unity OpenXR Spatial Entities 6.6.0+ is missing from this project.\n" +
                "Anchors and plane tracking will be disabled.");
#endif // !UNITY_SPATIAL_ENTITIES
        }

        internal struct TrackableChanges
        {
            public int AddedCount;
            public int UpdatedCount;
            public int RemovedCount;

            public List<TrackableId> Trackables;

            public void AppendDebugInfo(StringBuilder builder, int maxCount = 5)
            {
                builder.AppendFormat(" | Added ({0}), Updated ({1}), Removed ({2})\n",
                    AddedCount, UpdatedCount, RemovedCount);
                int count = Trackables == null ? 0 : Trackables.Count;
                if (count > 0 && count <= maxCount)
                {
                    var trackablesInfo = string.Join("\n | ", Trackables);
                    builder.AppendFormat(" | {0}\n", trackablesInfo);
                }
            }
        }
    }
}
