// <copyright file="XRAnchorSpace.cs" company="Google LLC">
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

namespace Google.XR.Extensions
{
    using UnityEngine;
    using UnityEngine.XR.ARFoundation;

    /// <summary>
    /// Updates <see cref="ARAnchor"/> which created with an OpenXR Anchor Space.
    ///
    /// Note: this is only needed for anchors created by <see cref="Object.Instantiate(Object)"/>.
    /// Anchors created by AR Foundation API, e.g.
    /// <see cref="ARAnchorManager.AttachAnchor(ARPlane, Pose)"/> will be updated by
    /// <see cref="ARAnchorManager"/>.
    /// </summary>
    [RequireComponent(typeof(ARAnchor))]
    public class XRAnchorSpace : MonoBehaviour
    {
        private ARAnchor _anchor;

        private void Awake()
        {
            _anchor = GetComponent<ARAnchor>();
        }

        private void Update()
        {
            // Keep the last known pose if the anchor is not tracking.
            if (_anchor == null ||
                _anchor.trackingState != UnityEngine.XR.ARSubsystems.TrackingState.Tracking)
            {
                return;
            }

            // Update the Anchor transform to match the native data.
            transform.SetPositionAndRotation(_anchor.pose.position, _anchor.pose.rotation);
        }
    }
}
