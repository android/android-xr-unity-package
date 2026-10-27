// <copyright file="GeospatialAnchorPlacer.cs" company="Google LLC">
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
    using Google.XR.Extensions;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.XR.ARSubsystems;

    /// <summary>
    /// Places geospatial anchors in front of the camera when a confirm action is triggered.
    /// Cycles through surface types on each placement.
    /// </summary>
    [RequireComponent(typeof(GeospatialController))]
    public class GeospatialAnchorPlacer : MonoBehaviour
    {
        private const float _valueThreshold = 0.95f;
        private const float _clickCooldown = 0.5f;
        private const float _placementDistance = 0.5f;

        private static readonly XRGeospatialSurfaceType[] _surfaceTypes =
            (XRGeospatialSurfaceType[])System.Enum.GetValues(typeof(XRGeospatialSurfaceType));

        private GeospatialController _geospatialController = null;
        private XRGeospatialManager _geospatialManager = null;

        private InputAction _confirmButtonAction = null;
        private InputAction _confirmValueAction = null;
        private double _clickSinceStartup = 0f;

        private int _surfaceTypeIndex = 0;

        private void Awake()
        {
            _geospatialController = GetComponent<GeospatialController>();
            _geospatialManager = GetComponent<XRGeospatialManager>();

            _confirmButtonAction = new InputAction("ConfirmButton", InputActionType.Button);
            _confirmButtonAction.AddBinding("<XRController>{RightHand}/{PrimaryButton}");

            _confirmValueAction = new InputAction("ConfirmValue", InputActionType.Value);
            _confirmValueAction.AddBinding("<HandInteraction>{RightHand}/pinchValue");
        }

        private void OnEnable()
        {
            if (_confirmButtonAction != null)
            {
                _confirmButtonAction.Enable();
                _confirmButtonAction.performed += OnConfirmButtonActionPerformed;
            }

            if (_confirmValueAction != null)
            {
                _confirmValueAction.Enable();
                _confirmValueAction.performed += OnConfirmValueActionPerformed;
            }
        }

        private void OnDisable()
        {
            if (_confirmButtonAction != null)
            {
                _confirmButtonAction.performed -= OnConfirmButtonActionPerformed;
                _confirmButtonAction.Disable();
            }

            if (_confirmValueAction != null)
            {
                _confirmValueAction.performed -= OnConfirmValueActionPerformed;
                _confirmValueAction.Disable();
            }
        }

        private void OnDestroy()
        {
            _confirmButtonAction?.Dispose();
            _confirmValueAction?.Dispose();
        }

        private void OnConfirmButtonActionPerformed(InputAction.CallbackContext ctx)
        {
            ProcessConfirmAction(ctx, checkValueThreshold: false);
        }

        private void OnConfirmValueActionPerformed(InputAction.CallbackContext ctx)
        {
            ProcessConfirmAction(ctx, checkValueThreshold: true);
        }

        private void ProcessConfirmAction(InputAction.CallbackContext ctx, bool checkValueThreshold)
        {
            if (ctx.time - _clickSinceStartup < _clickCooldown)
            {
                return;
            }

            if (checkValueThreshold && ctx.ReadValue<float>() <= _valueThreshold)
            {
                return;
            }

            _clickSinceStartup = ctx.time;
            TryPlaceAnchorAtCurrentLocation();
        }

        private void TryPlaceAnchorAtCurrentLocation()
        {
            XRGeospatialPose pose;
            if (_geospatialController.TryGetCurrentLocation(out pose))
            {
                PlaceAnchor(pose);
            }
            else
            {
                Debug.LogWarning("Failed to get current location from GeospatialController");
            }
        }

        private async void PlaceAnchor(XRGeospatialPose pose)
        {
            try
            {
                Result<ARShareableAnchor> anchorResult;
                XRGeospatialSurfaceType typeToPlace = _surfaceTypes[_surfaceTypeIndex];

                // Immediately cycle the surface type so that rapid clicks during the await
                // correctly queue up the next type in the array.
                CycleToNextSurfaceType();

                if (typeToPlace == XRGeospatialSurfaceType.Default)
                {
                    Debug.LogFormat("Attaching default geospatial anchor at {0}...", pose);

                    anchorResult = _geospatialManager.TryAttachGeospatialAnchor(pose);
                }
                else
                {
                    pose.Altitude = 0;
                    Debug.LogFormat(
                        "Attaching surface anchor {0} at {1}...", typeToPlace, pose);

                    anchorResult = await _geospatialManager.TryAttachSurfaceAnchorAsync(
                        typeToPlace, pose);
                }

                if (anchorResult.status.IsSuccess())
                {
                    Debug.LogFormat(
                        "Successfully placed {0} geospatial anchor: {1}",
                        typeToPlace, anchorResult.value.trackableId);
                }
                else
                {
                    Debug.LogErrorFormat(
                        "Failed to place {0} geospatial anchor. Status: {1}",
                        typeToPlace, anchorResult.status);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private void CycleToNextSurfaceType()
        {
            _surfaceTypeIndex = (_surfaceTypeIndex + 1) % _surfaceTypes.Length;
            Debug.LogFormat(
                "Next anchor placed will be of surface type: {0}",
                _surfaceTypes[_surfaceTypeIndex]);
        }
    }
}
