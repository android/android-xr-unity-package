// <copyright file="EarthIndicator.cs" company="Google LLC">
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
    using UnityEngine;

    /// <summary>
    /// Locates earth coordinates by <see cref="XRGeospatialPose"/> and
    /// represented as Unity <see cref="Pose"/>.
    /// </summary>
    /// <remarks>
    /// Requires <see cref="Input.location"/> to be started from the main
    /// controller.
    /// </remarks>
    [RequireComponent(typeof(MeshRenderer))]
    public class EarthIndicator : MonoBehaviour
    {
        // Must match shader's color property name.
        static readonly int _colorPropertyId = Shader.PropertyToID("_BaseColor");

        [SerializeField]
        [Tooltip("The XRGeospatialManager instance in the scene.")]
        private XRGeospatialManager _geospatialManager = null;

        [SerializeField]
        [Tooltip("The latitude offset appended to the current location.")]
        [Range(-90f, 90f)]
        private double _latitudeOffset = 0;

        [SerializeField]
        [Tooltip("The longitude offset appended to the current location.")]
        [Range(-180f, 180f)]
        private double _longitudeOffset = 0;

        [SerializeField]
        [Tooltip("The altitude offset appended to the current location.")]
        private double _altitudeOffset = 0;

        [SerializeField]
        [Tooltip("Indicates whether to hide the game object when service is stopped.")]
        private bool _hiddenOnStopped = false;

        [SerializeField]
        [Tooltip("Indicates whether to update rotation.")]
        private bool _updateRotation = false;

        [SerializeField]
        [Tooltip("The color indicating the conversion is paused.")]
        private Color _pauseColor = Color.gray;

        [SerializeField]
        [Tooltip("The color indicating the conversion is resumed.")]
        private Color _resumeColor = Color.green;

        private bool _isResumed = false;
        private XRPoseResult _poseResult = default;
        private MeshRenderer _meshRenderer = null;

        /// <summary>
        /// Gets the latest pose result.
        /// </summary>
        public XRPoseResult PoseResult => _poseResult;

        private void Awake()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
            UpdateRenderer(visible: false, tracking: false, force: true);
            if (_geospatialManager == null)
            {
                Debug.LogErrorFormat(
                    "XRGeospatialManager is null. {0} will not update result at runtime.",
                    GetType().Name);
            }
        }

        private void OnEnable()
        {
            _poseResult = default;
            UpdateRenderer(false, false);
        }

        private void OnDisable()
        {
            _poseResult = default;
            UpdateRenderer(false, false);
        }

        private void Update()
        {
            bool trackerRunning = _geospatialManager != null && _geospatialManager.Running;
            if (Input.location.status != LocationServiceStatus.Running || !trackerRunning)
            {
                UpdateRenderer(!_hiddenOnStopped, false);
                return;
            }

            var geospatialPose = new XRGeospatialPose(Input.location.lastData);
            geospatialPose.Latitude =
                Math.Clamp(geospatialPose.Latitude + _latitudeOffset, -90d, 90d);
            geospatialPose.Longitude =
                Math.Clamp(geospatialPose.Longitude + _longitudeOffset, -180d, 180d);
            geospatialPose.Altitude += _altitudeOffset;
            if (_geospatialManager.TryLocate(geospatialPose, out XRPoseResult result))
            {
                _poseResult = result;
                UpdateRenderer(true, _poseResult.LocationFlags > 0);
                if (_poseResult.IsPositionValid || _poseResult.IsPositionTracked)
                {
                    transform.position = _poseResult.Pose.position;
                }

                if (_updateRotation &&
                    (_poseResult.IsRotationValid || _poseResult.IsRotationTracked))
                {
                    transform.rotation = _poseResult.Pose.rotation;
                }
            }
        }

        private void UpdateRenderer(bool visible, bool tracking, bool force = false)
        {
            _meshRenderer.enabled = visible;
            if (_isResumed == tracking && !force)
            {
                return;
            }

            Material[] materials = _meshRenderer.materials;
            materials[0].SetColor(
                _colorPropertyId, tracking ? _resumeColor : _pauseColor);
            _meshRenderer.materials = materials;
            _isResumed = tracking;
        }
    }
}
