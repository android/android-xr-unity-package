// <copyright file="TrackablesController.cs" company="Google LLC">
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

namespace Google.XR.Extensions.Samples.Trackables
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;
    using UnityEngine.Android;
    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.ARSubsystems;
    using UnityEngine.XR.OpenXR;

#if UNITY_OPEN_XR_ANDROID_XR_0_2_0
    using UnityEngine.XR.OpenXR.Features.Android;
#endif

    /// <summary>
    /// A temporary control hooking up with XRTrackableSubsystem update.
    /// </summary>
    [RequireComponent(typeof(AndroidXRPermissionUtil))]
    public class TrackablesController : MonoBehaviour
    {
        /// <summary>
        /// Text mesh to display debug information.
        /// </summary>
        public TextMesh DebugText;

        /// <summary>
        /// The <see cref="ARPlaneManager"/> component in the scene.
        /// </summary>
        public ARPlaneManager PlaneManager;

        /// <summary>
        /// The <see cref="ARAnchorManager"/> component in the scene.
        /// </summary>
        public ARAnchorManager AnchorManager;

        /// <summary>
        /// The <see cref="ARRaycastManager"/> component in the scene.
        /// </summary>
        public ARRaycastManager RaycastManager;

        /// <summary>
        /// The prefab used to instantiate an <see cref="ARAnchor"/>.
        /// </summary>
        public GameObject AnchorPrefab;

        /// <summary>
        /// The prefab used to instantiate an <see cref="ARAnchor"/> attaching to a plane.
        /// </summary>
        public GameObject PlaneAnchorPrefab;

        /// <summary>
        /// The prefab used to instantiate an <see cref="ARAnchor"/> loaded from stored UUID.
        /// </summary>
        public GameObject PersistAnchorPrefab;

        /// <summary>
        /// A switch to enable or disable depth raycast.
        /// </summary>
        public bool EnableDepthRaycast = true;

        private const string _persistentKey = "PersistentGuid";

        private Vector3 _spatialPosition = new Vector3(0f, -0.25f, 0.5f);

        private List<TrackableId> _planes = new List<TrackableId>();
        private ARPlane _plane = null;
        private int _planeAdded = 0;
        private int _planeUpdated = 0;
        private int _planeRemoved = 0;

        // Place a new anchor for persisting in current session.
        private ARAnchor _spatialAnchor = null;
        private ARAnchor _planeAnchor = null;
        private List<ARAnchor> _loadedAnchors = new List<ARAnchor>();
        private Guid _persistedId = Guid.Empty;
        private int _anchorAdded = 0;
        private int _anchorUpdated = 0;
        private int _anchorRemoved = 0;

        private XRAnchorFeature _extensionAnchorFeature = null;
        private AndroidXRPermissionUtil _permissionUtil;
        private StringBuilder _stringBuilder = new StringBuilder();

        // Loaded all persisted ids from device's local storage.
        // Due the writing operation happens while device goes to sleep,
        // current on will not be included.
        private List<Guid> _loadedPersistedIds = new List<Guid>();

        // Save one latest persisted id on app storage for loading and unpersisting in next session.
        private Guid _prefSavedGuid = Guid.Empty;

        // List of raycast hit results
        private List<ARRaycastHit> _raycastHitResults = new List<ARRaycastHit>();

        // GameObject placed at hit pose
        private GameObject _raycastObject;

        /// <summary>
        /// Called from AR Plane Manager component callback.
        /// </summary>
        /// <param name="eventArgs">Plane change event arguments.</param>
        public void OnPlanesChanged(ARTrackablesChangedEventArgs<ARPlane> eventArgs)
        {
            _planeAdded = eventArgs.added.Count;
            _planeUpdated = eventArgs.updated.Count;
            _planeRemoved = eventArgs.removed.Count;
            foreach (ARPlane plane in eventArgs.added)
            {
                Debug.Log("OnPlaneChanged: Added: " + plane.trackableId);
                Debug.Log(DebugPlane(plane));
            }

            foreach (var planePair in eventArgs.removed)
            {
                Debug.Log("OnPlaneChanged: Removed: " + planePair.Key);
            }

            if (_planeAdded > 0)
            {
                // Display the details of a plane to verify the underlying plane data.
                _plane = eventArgs.added[0];
            }

            _planes = eventArgs.updated.Select(plane => plane.trackableId).ToList();
        }

        /// <summary>
        /// Called from AR Anchor Manager component callback.
        /// </summary>
        /// <param name="eventArgs">Anchor change event arguments.</param>
        public void OnAnchorChanged(ARTrackablesChangedEventArgs<ARAnchor> eventArgs)
        {
            _anchorAdded = eventArgs.added.Count;
            _anchorUpdated = eventArgs.updated.Count;
            _anchorRemoved = eventArgs.removed.Count;
            if (_extensionAnchorFeature == null || !_extensionAnchorFeature.UsePersistence)
            {
                return;
            }

            foreach (ARAnchor anchor in eventArgs.added)
            {
                if ((_spatialAnchor != null && _spatialAnchor.trackableId == anchor.trackableId) ||
                    (_planeAnchor != null && _planeAnchor.trackableId == anchor.trackableId))
                {
                    return;
                }

                Guid guid = anchor.GetPersistentId();
                Debug.LogFormat("OnAnchorChanged: Added: {0} ({1}) - {2}",
                   anchor.trackableId, anchor.trackingState,
                   guid == Guid.Empty ? "Not persisted" : guid);
                if (!_loadedAnchors.Contains(anchor) && _spatialAnchor != null)
                {
                    Instantiate(PersistAnchorPrefab, anchor.gameObject.transform);
                    _loadedAnchors.Add(anchor);
                }
            }
        }

        private void OnEnable()
        {
            _permissionUtil = GetComponent<AndroidXRPermissionUtil>();
            _extensionAnchorFeature =
                OpenXRSettings.ActiveBuildTargetInstance.GetFeature<XRAnchorFeature>();
            if (_extensionAnchorFeature == null || !_extensionAnchorFeature.UsePersistence)
            {
                Debug.Log("Anchor Persistence is disabled.");
            }

            if (PlaneManager == null)
            {
                Debug.LogError("ARPlaneManager is null!");
            }

            if (AnchorManager != null)
            {
                LoadHistory();
                if (AnchorManager.anchorPrefab != null)
                {
                    // Reset default prefab so that we can instantiate different game objects
                    // from different anchor sources.
                    AnchorManager.anchorPrefab = null;
                }
            }
            else
            {
                Debug.LogError("ARAnchorManager is null!");
            }

            if (RaycastManager == null)
            {
                Debug.LogError("ARRaycastManager is null.");
            }

            Debug.LogFormat("Running Trackables sample with depth raycasting {0}.",
                EnableDepthRaycast ? "enabled" : "disabled");
        }

        private void OnDisable()
        {
            foreach (var anchor in _loadedAnchors)
            {
                Destroy(anchor.gameObject);
            }

            _loadedAnchors.Clear();

            if (_spatialAnchor != null)
            {
                Destroy(_spatialAnchor.gameObject);
            }

            if (_planeAnchor != null)
            {
                Destroy(_planeAnchor.gameObject);
            }

            if (PlaneManager != null)
            {
                _planes.Clear();
                if (_plane != null)
                {
                    _plane = null;
                }
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
            UpdatePlanes();
            UpdateRaycast();
            UpdateAnchors();

            if (_stringBuilder.Length > 0)
            {
                DebugText.text = _stringBuilder.ToString();
            }
        }

        private void UpdatePlanes()
        {
            if (PlaneManager == null || PlaneManager.subsystem == null)
            {
                _stringBuilder.Append("Cannot find ARPlaneManager.\n");
            }
            else if (!PlaneManager.subsystem.running)
            {
                _stringBuilder.AppendFormat("{0} is not running.\n",
                    PlaneManager.subsystem.GetType().Name);
            }
            else
            {
                _stringBuilder.AppendFormat(
                    $"Planes: ({_planeAdded}, {_planeUpdated}, {_planeRemoved})\n" +
                    $"{string.Join("\n", _planes.Select(id => id.subId1).ToArray())}\n");
            }
        }

        private void UpdateRaycast()
        {
            if (RaycastManager == null || RaycastManager.subsystem == null)
            {
                _stringBuilder.Append("Cannot find ARRaycastManager.\n");
            }
            else if (!RaycastManager.subsystem.running)
            {
                _stringBuilder.AppendFormat("{0} is not running.\n",
                    RaycastManager.subsystem.GetType().Name);
            }
            else
            {
                if (_raycastObject == null)
                {
                    _raycastObject = Instantiate(
                        RaycastManager.raycastPrefab, Vector3.zero, Quaternion.identity);
                }

                Vector3 rayDirection = Camera.main.transform.TransformDirection(Vector3.forward);
                Ray ray = new Ray(Camera.main.transform.position, rayDirection);

                // Skip plane raycasting if plane detection is not running.
                var filter = EnableDepthRaycast ? TrackableType.Depth : TrackableType.None;
                if (PlaneManager != null && PlaneManager.subsystem != null &&
                    PlaneManager.subsystem.running)
                {
                    filter |= TrackableType.PlaneWithinPolygon;
                }

                if (filter == TrackableType.None)
                {
                    return;
                }

                if (RaycastManager.Raycast(ray, _raycastHitResults, filter))
                {
                    // If there is a hit result for any trackable type other then depth, choose
                    // that, otherwise choose the first result itself.
                    int selectedIndex = 0;
                    for (int i = 0; i < _raycastHitResults.Count; ++i)
                    {
                        if (!_raycastHitResults[i].hitType.HasFlag(TrackableType.Depth))
                        {
                            selectedIndex = i;
                            break;
                        }
                    }

                    _stringBuilder.AppendFormat(
                        $"Raycast: Hit {_raycastHitResults[selectedIndex].hitType}\n");
                    _raycastObject.SetActive(true);
                    _raycastObject.transform.position =
                        _raycastHitResults[selectedIndex].pose.position;
                }
                else
                {
                    _stringBuilder.Append("Raycast: No Hit\n");
                    _raycastObject.SetActive(false);
                }
            }
        }

        private void UpdateAnchors()
        {
            if (AnchorManager == null || AnchorManager.subsystem == null)
            {
                _stringBuilder.Append("Cannot find ARAnchorManager.\n");
            }
            else if (!AnchorManager.subsystem.running)
            {
                _stringBuilder.AppendFormat("{0} is not running.\n",
                    AnchorManager.subsystem.GetType().Name);
            }
            else
            {
                _stringBuilder.AppendFormat(
                    $"Anchors: ({_anchorAdded}, {_anchorUpdated}, {_anchorRemoved})\n");
                TryPlaceAnchor();
                if (_spatialAnchor != null)
                {
                    _stringBuilder.AppendFormat(
                        $"Spatial Anchor: {0}-{1} ({2})\n",
                        _spatialAnchor.trackableId.subId1, _spatialAnchor.trackableId.subId2,
                        _spatialAnchor.trackingState);
                }

                if (_planeAnchor != null)
                {
                    _stringBuilder.AppendFormat(
                        $"Plane Anchor: {0}-{1} ({2})\n",
                        _planeAnchor.trackableId.subId1, _planeAnchor.trackableId.subId2,
                        _planeAnchor.trackingState);
                }

                if (_extensionAnchorFeature == null || !_extensionAnchorFeature.UsePersistence)
                {
                    _stringBuilder.Append("Anchor Persistence: not in use.");
                    return;
                }

                TryPersistAnchor();
                if (_persistedId != Guid.Empty)
                {
                    _stringBuilder.AppendFormat(
                        $"Latest Persisted Id: {_persistedId}\n\n");
                }

                TryGetPersistedIds();
                _stringBuilder.AppendFormat(
                    $"All Preload IDs: {_loadedPersistedIds.Count}\n");
                if (_loadedPersistedIds.Count > 0)
                {
                    _stringBuilder.AppendFormat(
                        $"{string.Join("\n", _loadedPersistedIds.Select(id => id).ToArray())}\n");
                }
            }
        }

        private void TryPlaceAnchor()
        {
            if (_spatialAnchor == null)
            {
                var instance = Instantiate(AnchorPrefab, _spatialPosition, Quaternion.identity);
                _spatialAnchor = instance.GetComponent<ARAnchor>();
                if (_spatialAnchor == null)
                {
                    _spatialAnchor = instance.AddComponent<ARAnchor>();
                }

                if (_spatialAnchor != null)
                {
                    Debug.LogFormat("Created spatial anchor: {0}-{1}",
                        _spatialAnchor.trackableId.subId1, _spatialAnchor.trackableId.subId2);
                }
            }

            if (_planeAnchor == null && _plane != null)
            {
                // It requires a valid pose, i.e. Quaternion.identity, to attach an anchor.
                Pose pose = new Pose(Vector3.zero, Quaternion.identity);
                _planeAnchor = AnchorManager.AttachAnchor(_plane, pose);
                if (_planeAnchor == null)
                {
                    Debug.LogWarning("Failed to add a plane anchor.");
                }
                else
                {
                    Debug.LogFormat("Created plane anchor: {0}-{1}",
                        _planeAnchor.trackableId.subId1, _planeAnchor.trackableId.subId2);
                    Instantiate(PlaneAnchorPrefab, _planeAnchor.transform);
                }
            }
        }

        private void TryPersistAnchor()
        {
            if (_spatialAnchor == null || _spatialAnchor.trackingState != TrackingState.Tracking ||
                _persistedId != Guid.Empty)
            {
                return;
            }

            if (AnchorManager.Persist(_spatialAnchor))
            {
                _persistedId = _spatialAnchor.GetPersistentId();
                if (_persistedId != Guid.Empty)
                {
                    StartCoroutine(WaitUntilPersisted(_persistedId));
                }
            }

            if (_prefSavedGuid != Guid.Empty && !AnchorManager.TryLoad(_prefSavedGuid))
            {
                // Clean saved data.
                Debug.Log("Failed to load previous persisted anchor: " + _prefSavedGuid);
            }

            SaveHistory(_persistedId);
        }

        private IEnumerator WaitUntilPersisted(Guid guid)
        {
            yield return AnchorManager.WaitUntilPersistState(
                guid, XRAnchorPersistStates.Persisted,
                (bool isPersisted) =>
                {
                    if (isPersisted)
                    {
                        Debug.LogFormat("Persisted anchor: {0}", guid);
                    }
                    else
                    {
                        Debug.LogFormat("Failed to persist anchor: {0}", guid);
                    }
                });
        }

        private void TryGetPersistedIds()
        {
            if (_loadedPersistedIds.Count > 0)
            {
                return;
            }

            AnchorManager.TryGetAllPersistedIds(ref _loadedPersistedIds);
        }

        private void LoadHistory()
        {
            if (PlayerPrefs.HasKey(_persistentKey))
            {
                Debug.LogFormat("Read {0}: {1}",
                    _persistentKey, PlayerPrefs.GetString(_persistentKey));
                _prefSavedGuid = new Guid(PlayerPrefs.GetString(_persistentKey));
            }
        }

        private void SaveHistory(Guid guid)
        {
            if (_prefSavedGuid != Guid.Empty)
            {
                AnchorManager.Unpersist(_prefSavedGuid);
            }

            _prefSavedGuid = guid;
            PlayerPrefs.SetString(_persistentKey, guid.ToString());
            PlayerPrefs.Save();
            Debug.LogFormat("Saved {0}: {1}",
                _persistentKey, PlayerPrefs.GetString(_persistentKey));
        }

        private string DebugPlane(ARPlane plane)
        {
            if (plane == null)
            {
                return string.Empty;
            }

            return string.Format(
                $"Plane ({plane.trackableId.subId1}, {plane.trackableId.subId2})\n" +
                $" Tracking: {plane.trackingState}\n" +
                $" NativePtr: {plane.nativePtr}\n" +
                $" Pose: {plane.center}\n" +
                $" Normal: {plane.normal}\n" +
                $" SubsumedBy: {(plane.subsumedBy != null ? plane.subsumedBy.nativePtr : null)}\n" +
                $" Alignment: {plane.alignment}\n" +
                $" Classification: {plane.classifications}");
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(
                UnityEditor.BuildTargetGroup.Android);
            var trackableFeature = settings.GetFeature<XRTrackableFeature>();
            var arPlaneInUse = false;
#if UNITY_OPEN_XR_ANDROID_XR_0_2_0
            var planeFeature = settings.GetFeature<ARPlaneFeature>();
            arPlaneInUse = planeFeature != null && planeFeature.enabled;
#endif
            if (trackableFeature != null && trackableFeature.enabled)
            {
                Debug.LogFormat("Plane subsystem provided by {0}.", XRTrackableFeature.UiName);
            }
            else if (arPlaneInUse)
            {
                Debug.Log("Plane subsystem provided by AndroidXR: AR Plane.");
            }
            else
            {
                Debug.LogErrorFormat(
                    "Cannot find a valid plane provider targeting Android platform.");
            }

            var anchorFeature = settings.GetFeature<XRAnchorFeature>();
            var arAnchorInUse = false;
#if UNITY_OPEN_XR_ANDROID_XR_0_2_0
            var arAnchorFeature = settings.GetFeature<ARAnchorFeature>();
            arAnchorInUse = arAnchorFeature != null && arAnchorFeature.enabled;
#endif
            if (anchorFeature != null && anchorFeature.enabled)
            {
                Debug.LogFormat("Anchor subsystem provided by {0}.", XRAnchorFeature.UiName);
            }
            else if (arAnchorInUse)
            {
                Debug.Log("Anchor subsystem provided by AndroidXR: AR Anchor.");
            }
            else
            {
                Debug.LogWarningFormat(
                    "Cannot find a valid anchor provider targeting Android platform.");
            }

#if UNITY_OPEN_XR_ANDROID_XR_0_2_0
            var raycastFeature = settings.GetFeature<ARRaycastFeature>();
            if (raycastFeature != null && raycastFeature.enabled) {
                Debug.Log("Raycast subsystem provided by AndroidXR: AR Raycast.");
            }
#endif
#endif // UNITY_EDITOR
        }
    }
}
