// <copyright file="ARTrackedImageVisualizer.cs" company="Google LLC">
//
// Copyright 2025 Google LLC
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
    using System;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.ARSubsystems;

    /// <summary>
    /// A visualizer to display tracked object by a cube.
    /// </summary>
    [RequireComponent(typeof(ARTrackedImage))]
    public class ARTrackedImageVisualizer : MonoBehaviour
    {
        /// <summary>
        /// Text to display tracked image reference.
        /// </summary>
        public Text Reference;

        /// <summary>
        /// Text to display additional information from the tracked image.
        /// </summary>
        public Text Info;

        /// <summary>
        /// A threshold to render the minimal extent height.
        /// </summary>
        [Range(0.005f, 0.05f)]
        public float CubeEdge = 0.01f;

        private ImageSource _source = ImageSource.Unknow;
        private bool _isDataValid = false;
        private string _qrCodeData = string.Empty;
        private XRMarkerDictionary _dictionary = XRMarkerDictionary.ArUco4x4_50;
        private int _markId = -1;
        private ARTrackedImage _trackedImage;
        private MeshRenderer _renderer;

        private enum ImageSource
        {
            Unknow,
            QrCode,
            Marker,
        }

        private void Awake()
        {
            _trackedImage = GetComponent<ARTrackedImage>();
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
            transform.localScale =
                new Vector3(_trackedImage.size.x, CubeEdge, _trackedImage.size.y);
        }

        private void UpdateVisibility()
        {
            bool visible = enabled && _trackedImage.trackingState >= TrackingState.Limited &&
                ARSession.state > ARSessionState.Ready;

            if (visible)
            {
                if (_source == ImageSource.Unknow)
                {
                    if (_trackedImage.IsQrCode())
                    {
                        _source = ImageSource.QrCode;
                    }
                    else if (_trackedImage.IsMarker())
                    {
                        _source = ImageSource.Marker;
                    }
                }

                if (_source == ImageSource.QrCode && !_isDataValid)
                {
                    _isDataValid = _trackedImage.TryGetQrCodeData(out _qrCodeData);
                }
                else if (_source == ImageSource.Marker && !_isDataValid)
                {
                    _isDataValid = _trackedImage.TryGetMarkerData(out _dictionary, out _markId);
                }
            }

            if (Reference != null)
            {
                Reference.text = _trackedImage.referenceImage == null ? "No reference" :
                    _trackedImage.referenceImage.name;
                Reference.gameObject.SetActive(visible);
            }

            if (Info != null)
            {
                Info.text = $"Image Source: {_source}";
                if (_source == ImageSource.QrCode && _isDataValid)
                {
                    Info.text += $"\n{_qrCodeData}";
                }

                if (_source == ImageSource.Marker && _isDataValid)
                {
                    Info.text += $"\nDictionary: {_dictionary}\nId: {_markId}";
                }

                Info.gameObject.SetActive(visible);
            }

            if (_renderer != null)
            {
                _renderer.enabled = visible;
            }
        }
    }
}
