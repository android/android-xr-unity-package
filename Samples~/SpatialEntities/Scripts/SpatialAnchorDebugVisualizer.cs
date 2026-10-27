// <copyright file="SpatialAnchorDebugVisualizer.cs" company="Google LLC">
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
    using System.Text;
    using Google.XR.Extensions.SpatialEntities;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.ARSubsystems;

    /// <summary>
    /// Visualizer used to debug a spatial anchor.
    /// </summary>
    [RequireComponent(typeof(ARAnchor))]
    public class SpatialAnchorDebugVisualizer : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Indicates whether to disable debug view.")]
        private bool _hideDebugView = false;

        [SerializeField]
        [Tooltip("The UI canvas containing all debug elements.")]
        private Canvas _debugCanvas = null;

        [SerializeField]
        [Tooltip("The UI text showing spatial anchor debug information.")]
        private Text _debugText = null;

        private ARAnchor _anchor;
        private StringBuilder _stringBuilder = new StringBuilder();

        private void Awake()
        {
            if (_anchor == null)
            {
                _anchor = GetComponent<ARAnchor>();
            }

            if (_hideDebugView && _debugCanvas != null)
            {
                _debugCanvas.enabled = false;
            }
        }

        private void Update()
        {
            if (_hideDebugView || _debugCanvas == null || _debugText == null)
            {
                return;
            }

            _stringBuilder.Clear();

            _stringBuilder.AppendFormat(
                "TrackingState: {0}\n", _anchor.trackingState);
            _stringBuilder.AppendFormat(
                "TrackableId: {0}-{1}\n", _anchor.trackableId.subId1, _anchor.trackableId.subId2);

            _debugText.text = _stringBuilder.ToString();
        }
    }
}
