// <copyright file="ARBoundingBoxVisualizer.cs" company="Google LLC">
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
    using System;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.ARSubsystems;

    /// <summary>
    /// A visualizer to display tracked <see cref="ARBoundingBox"/> by a cube.
    /// </summary>
    [RequireComponent(typeof(ARBoundingBox))]
    public class ARBoundingBoxVisualizer : MonoBehaviour
    {
        /// <summary>
        /// Text to display tracked object label.
        /// </summary>
        public Text Label;

        /// <summary>
        /// A threshold to render the minimal extent height.
        /// </summary>
        [Range(0.005f, 0.01f)]
        public float ExtentHeightThreshold = 0.005f;

        private ARBoundingBox _trackable;
        private MeshRenderer _renderer;

        private void Awake()
        {
            _trackable = GetComponent<ARBoundingBox>();
            _renderer = GetComponent<MeshRenderer>();
        }

        private void OnEnable()
        {
            UpdateVisibility();
        }

        private void OnDisable()
        {
            UpdateVisibility();
        }

        private void Update()
        {
            Vector3 extents = _trackable.size;
            if (extents.y < ExtentHeightThreshold)
            {
                extents.y = ExtentHeightThreshold;
            }

            transform.localScale = extents;
            if (Label != null)
            {
                Label.text = _trackable.classifications.ToString();
            }
        }

        private void UpdateVisibility()
        {
            bool visible = enabled && _trackable.trackingState >= TrackingState.Limited &&
                ARSession.state > ARSessionState.Ready;

            if (Label != null)
            {
                Label.gameObject.SetActive(visible);
            }

            if (_renderer != null)
            {
                _renderer.enabled = visible;
            }
        }
    }
}
