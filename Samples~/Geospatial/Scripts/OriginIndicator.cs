// <copyright file="OriginIndicator.cs" company="Google LLC">
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
    using UnityEngine;

    /// <summary>
    /// Locates the Unity pose relative to the Earth, represented by
    /// <see cref="XRGeospatialPose"/>.
    /// </summary>
    [RequireComponent(typeof(MeshRenderer))]
    public class OriginIndicator : MonoBehaviour
    {
        // Must match shader's color property name.
        static readonly int _colorPropertyId = Shader.PropertyToID("_BaseColor");

        [SerializeField]
        [Tooltip("The XRGeospatialManager instance in the scene.")]
        private XRGeospatialManager _geospatialManager = null;

        [SerializeField]
        [Tooltip("Indicates whether to hide the game object when service is stopped.")]
        private bool _hiddenOnStopped = false;

        [SerializeField]
        [Tooltip("The color indicating the conversion is paused.")]
        private Color _pauseColor = Color.yellow;

        [SerializeField]
        [Tooltip("The color indicating the conversion is resumed.")]
        private Color _resumeColor = Color.blue;

        private bool _isResumed = false;
        private XRGeospatialPoseResult _poseResult = default;
        private MeshRenderer _meshRenderer = null;

        /// <summary>
        /// Gets the latest pose result.
        /// </summary>
        public XRGeospatialPoseResult PoseResult => _poseResult;

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
            if (_geospatialManager == null || !_geospatialManager.Running)
            {
                UpdateRenderer(!_hiddenOnStopped, false);
                return;
            }

            Pose pose = new Pose(transform.position, transform.rotation);
            if (_geospatialManager.TryLocate(
                pose, out XRGeospatialPoseResult geospatialResult))
            {
                _poseResult = geospatialResult;
                UpdateRenderer(true, _poseResult.LocationFlags > 0);
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
