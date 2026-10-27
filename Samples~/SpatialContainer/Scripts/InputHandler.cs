// <copyright file="InputHandler.cs" company="Google LLC">
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

namespace Google.XR.Extensions.Samples.SpatialContainer
{
    using System;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.InputSystem;

    /// <summary>
    /// Used to test input with spatial container extension.
    /// </summary>
    public class InputHandler : MonoBehaviour
    {
        /// <summary>
        /// The animator of the bugdroid.
        /// </summary>
        public Animator BugdroidAnimator;

        /// <summary>
        /// The container transform which contains the content.
        /// </summary>
        public Transform Container;

        private InputAction _pinchAction = null;

        private bool _immersiveCoroutineRunning = false;

        [SerializeField]
        private float _immersiveDurationSeconds = 5f;
        [SerializeField]
        private Material _bugdroidMaterial = null;
        [SerializeField]
        private Color _interactiveColor = new Color(0.07763652f, 0.5660378f, 0.05606973f);
        [SerializeField]
        private Color _nonInteractiveColor = new Color(0.7830188f, 0.3267004f, 0.1957547f);

        /// <summary>
        /// Triggers the "wave" animation on the Bugdroid.
        /// </summary>
        public void BugdroidWave()
        {
            AnimatorStateInfo stateInfo = BugdroidAnimator.GetCurrentAnimatorStateInfo(0);

            // Avoid triggering while already in wave animation
            if (!stateInfo.IsName("wave") && !BugdroidAnimator.IsInTransition(0))
            {
                Debug.Log("Triggering Bugdroid Wave");
                BugdroidAnimator.SetTrigger("wave");
            }
        }

        /// <summary>
        /// Switches spatial container to immersive mode for 5 seconds.
        /// </summary>
        public void BugdroidImmersive()
        {
            if (_immersiveCoroutineRunning)
            {
                Debug.LogWarning("Immersive coroutine already running, skipping");
                return;
            }

            _immersiveCoroutineRunning = true;
            StartCoroutine(ImmersiveBoundedCycle());
        }

        /// <summary>
        /// Toggles visibility state of the spatial container.
        /// </summary>
        public void BugdroidToggleVisibility()
        {
            Debug.Log("Begining Bugdroid visibility toggle");
            if (XRSpatialContainerFeature.TryGetState(out XRSpatialContainerStateFlags state))
            {
                bool targetState = !state.HasFlag(XRSpatialContainerStateFlags.Visible);
                if (!XRSpatialContainerFeature.TryRequestVisibility(targetState))
                {
                    Debug.LogError("Failed to request visibilty to " + targetState);
                }
            }
            else
            {
                Debug.LogError("Failed to get spatial spatial container state");
            }
        }

        IEnumerator ImmersiveBoundedCycle()
        {
            Debug.Log("Begining Bugdroid bounded <-> immersive transition");
            if (!XRSpatialContainerFeature.TryRequestBoundsMode(true))
            {
                Debug.LogError("Failed to request immersive mode");
                _immersiveCoroutineRunning = false;
                yield break;
            }

            yield return new WaitForSeconds(_immersiveDurationSeconds);
            if (!XRSpatialContainerFeature.TryRequestBoundsMode(false))
            {
                Debug.LogError("Failed to request bounded mode");
            }

            _immersiveCoroutineRunning = false;
        }

        private void OnEnable()
        {
            if (BugdroidAnimator == null)
            {
                Debug.LogError("BugdroidAnimator not set");
                enabled = false;
                return;
            }

            if (_pinchAction == null)
            {
                _pinchAction = new InputAction();
                _pinchAction.AddBinding("<HandInteraction>{LeftHand}/pinchValue");
            }

            if (_pinchAction != null)
            {
                _pinchAction.Enable();
                _pinchAction.started += OnPinchStarted;
            }

            XRSpatialContainerFeature.OnSpatialContainerBoundsChanged +=
                OnSpatialContainerBoundsChanged;
            XRSpatialContainerFeature.OnSpatialContainerStateChanged +=
                OnSpatialContainerStateChanged;

            if (XRSpatialContainerFeature.TryGetBounds(out Vector3 initBounds))
            {
                OnSpatialContainerBoundsChanged(initBounds, false);
            }
            else
            {
                Debug.LogError("Failed to get initial spatial container bounds");
            }

            if (XRSpatialContainerFeature.TryGetState(out XRSpatialContainerStateFlags initState))
            {
                _bugdroidMaterial.color =
                    initState.HasFlag(XRSpatialContainerStateFlags.Interactive)
                        ? _interactiveColor : _nonInteractiveColor;
            }
            else
            {
                Debug.LogError("Failed to get initial spatial container state");
            }
        }

        private void OnDisable()
        {
            if (_pinchAction != null)
            {
                _pinchAction.started -= OnPinchStarted;
                _pinchAction.Disable();
            }

            XRSpatialContainerFeature.OnSpatialContainerBoundsChanged -=
                OnSpatialContainerBoundsChanged;
            XRSpatialContainerFeature.OnSpatialContainerStateChanged -=
                OnSpatialContainerStateChanged;
        }

        private void OnPinchStarted(InputAction.CallbackContext context)
        {
            Debug.Log("Calling BugdroidWave from Pinch Action");
            BugdroidWave();
        }

        private void OnSpatialContainerBoundsChanged(Vector3 bounds, bool infiniteBounds)
        {
            if (Container != null && !infiniteBounds)
            {
                Container.localScale = bounds;
            }
            else
            {
                Debug.LogWarning("Infinite bounds detected, not scaling container");
            }
        }

        private void OnSpatialContainerStateChanged(
            XRSpatialContainerStateFlags flag, bool state)
        {
            string stateType = string.Empty;
            if (flag == XRSpatialContainerStateFlags.Visible)
            {
                stateType = "Visibility";
            }
            else if (flag == XRSpatialContainerStateFlags.Interactive)
            {
                stateType = "Interactability";
                _bugdroidMaterial.color = state ? _interactiveColor : _nonInteractiveColor;
            }
            else if (flag == XRSpatialContainerStateFlags.Immersive)
            {
                stateType = "Immersive";
            }

            Debug.LogFormat("Spatial Container {0} state changed to {1}", stateType, state);
        }
    }
}
