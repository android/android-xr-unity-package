// <copyright file="ObjectTrackingController.cs" company="Google LLC">
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

namespace Google.XR.Extensions.Samples.ObjectTracking
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;
    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.ARSubsystems;
    using UnityEngine.XR.OpenXR;

    /// <summary>
    /// An sample of object tracking with <see cref="ARTrackedObjectManager"/>
    /// </summary>
    [RequireComponent(typeof(AndroidXRPermissionUtil))]
    public class ObjectTrackingController : MonoBehaviour
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
        /// The <see cref="ARTrackedObjectManager"/> component in the scene.
        /// </summary>
        public ARTrackedObjectManager ObjectManager;

        private List<TrackableId> _planes = new List<TrackableId>();
        private int _planeAdded = 0;
        private int _planeUpdated = 0;
        private int _planeRemoved = 0;

        private List<TrackableId> _objects = new List<TrackableId>();
        private int _objectAdded = 0;
        private int _objectUpdated = 0;
        private int _objectRemoved = 0;

        private AndroidXRPermissionUtil _permissionUtil;
        private StringBuilder _stringBuilder = new StringBuilder();

        /// <summary>
        /// Called from AR Plane Manager component callback.
        /// </summary>
        /// <param name="eventArgs">Plane change event arguments.</param>
        public void OnPlanesChanged(ARTrackablesChangedEventArgs<ARPlane> eventArgs)
        {
            _planeAdded = eventArgs.added.Count;
            _planeUpdated = eventArgs.updated.Count;
            _planeRemoved = eventArgs.removed.Count;
            _planes = eventArgs.updated.Select(plane => plane.trackableId).ToList();
        }

        /// <summary>
        /// Called from AR Tracked Object Manager component callback.
        /// </summary>
        /// <param name="eventArgs">Object change event arguments.</param>
        public void OnObjectsChanged(ARTrackablesChangedEventArgs<ARTrackedObject> eventArgs)
        {
            foreach (ARTrackedObject trackedObject in eventArgs.added)
            {
                Debug.LogFormat("Adding tracked object at {0}\n{1}",
                    Time.frameCount,
                    GetObjectDebugInfo(trackedObject));

                // TODO - b/322517976: temp solution to set reference GUIDs.
                // Remove once the formal support is settled by AR Foundation upgrade.
                if (trackedObject.referenceObject != null)
                {
                    Debug.LogFormat("Reference Object {0} - {1}",
                        trackedObject.referenceObject.name, trackedObject.referenceObject.guid);
                }
            }

            _objectAdded = eventArgs.added.Count;
            _objectUpdated = eventArgs.updated.Count;
            _objectRemoved = eventArgs.removed.Count;
            _objects = eventArgs.updated.Select(
                trackedObject => trackedObject.trackableId).ToList();
        }

        private void OnEnable()
        {
            _permissionUtil = GetComponent<AndroidXRPermissionUtil>();
            if (PlaneManager == null)
            {
                Debug.LogError("ARPlaneManager is null!");
            }

            if (ObjectManager == null)
            {
                Debug.LogError("ARTrackedObjectManager is null!");
            }
        }

        private void OnDisable()
        {
            _objects.Clear();
            _planes.Clear();
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
            UpdateObjects();
            if (_stringBuilder.Length > 0)
            {
                DebugText.text = _stringBuilder.ToString();
            }
        }

        private void UpdatePlanes()
        {
            if (XRTrackableFeature.IsExtensionEnabled == null)
            {
                return;
            }
            else if (!XRTrackableFeature.IsExtensionEnabled.Value)
            {
                _stringBuilder.Append("XR_ANDROID_trackables is not enabled.\n");
            }
            else if (PlaneManager == null || PlaneManager.subsystem == null)
            {
                _stringBuilder.Append("Cannot find ARPlaneManager.\n");
            }
            else
            {
                _stringBuilder.AppendFormat(
                    $"Planes: ({_planeAdded}, {_planeUpdated}, {_planeRemoved})\n" +
                    $"{string.Join("\n", _planes.Select(id => id.subId1).ToArray())}\n");
            }
        }

        private void UpdateObjects()
        {
            if (XRObjectTrackingFeature.IsExtensionEnabled == null)
            {
                return;
            }
            else if (!XRObjectTrackingFeature.IsExtensionEnabled.Value)
            {
                _stringBuilder.Append("XR_ANDROID_trackables_object is not enabled.\n");
            }
            else if (ObjectManager == null || ObjectManager.subsystem == null)
            {
                _stringBuilder.Append("Cannnot find ARTrackedObjectManager.\n");
            }
            else
            {
                _stringBuilder.Append($"{ObjectManager.subsystem.GetType()}\n");
                _stringBuilder.AppendFormat(
                    $"Objects: ({_objectAdded}, {_objectUpdated}, {_objectRemoved})\n" +
                    $"{string.Join("\n", _objects.Select(id => id.subId1).ToArray())}\n");
            }
        }

        private string GetObjectDebugInfo(ARTrackedObject trackable)
        {
            if (trackable == null)
            {
                return string.Empty;
            }

            return string.Format(
                $"Object: {trackable.trackableId.subId1}-{trackable.trackableId.subId2}\n" +
                $"  NativePtr: {trackable.nativePtr}\n" +
                $"  Tracking: {trackable.trackingState}\n" +
                $"  Pose: {trackable.transform.position}-{trackable.transform.rotation}\n" +
                $"  Extents: {trackable.GetExtents()}\n" +
                $"  Label: {trackable.GetObjectLabel()}");
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(
                UnityEditor.BuildTargetGroup.Android);
            var trackableFeature = settings.GetFeature<XRTrackableFeature>();
            if (trackableFeature == null)
            {
                Debug.LogErrorFormat(
                    "Cannot find {0} targeting Android platform.", XRTrackableFeature.UiName);
                return;
            }
            else if (!trackableFeature.enabled)
            {
                Debug.LogWarningFormat(
                    "{0} is disabled. ObjectTracking sample will not work correctly.",
                    XRTrackableFeature.UiName);
            }

            var objectTracking = settings.GetFeature<XRObjectTrackingFeature>();
            if (objectTracking == null)
            {
                Debug.LogErrorFormat(
                    "Cannot find {0} targeting Android platform.", XRObjectTrackingFeature.UiName);
                return;
            }
            else if (!objectTracking.enabled)
            {
                Debug.LogWarningFormat(
                    "{0} is disabled. ObjectTracking sample will not work correctly.",
                    XRObjectTrackingFeature.UiName);
            }
#endif // UNITY_EDITOR
        }
    }
}
