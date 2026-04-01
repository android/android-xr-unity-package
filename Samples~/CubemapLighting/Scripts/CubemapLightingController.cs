// <copyright file="CubemapLightingController.cs" company="Google LLC">
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

namespace Google.XR.Extensions.Samples.CubemapLighting
{
    using System.Collections;
    using Unity.XR.CoreUtils;
    using UnityEngine;
    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.OpenXR;
    using UnityEngine.XR.OpenXR.Features;

    /// <summary>
    /// Used to test the cubemap light estimation extension.
    /// </summary>
    [RequireComponent(typeof(AndroidXRPermissionUtil))]
    [RequireComponent(typeof(AREnvironmentProbeManager))]
    [RequireComponent(typeof(XROrigin))]
    public class CubemapLightingController : MonoBehaviour
    {
        private AndroidXRPermissionUtil _permissionUtil;
        private AREnvironmentProbeManager _probeManager;
        private XROrigin _xrOrigin;
        private bool _initialized = false;

        private Transform _origin => _xrOrigin.Origin ? _xrOrigin.Origin.transform : transform;

        private void Awake()
        {
            _permissionUtil = GetComponent<AndroidXRPermissionUtil>();
            _probeManager = GetComponent<AREnvironmentProbeManager>();
            _xrOrigin = GetComponent<XROrigin>();
        }

        private void Start()
        {
            OpenXRFeature feature =
                OpenXRSettings.Instance.GetFeature<XRCubemapLightEstimationFeature>();
            if (feature == null || feature.enabled == false)
            {
                Debug.LogError("XRCubemapLightEstimationFeature feature is not enabled.");
                enabled = false;
                return;
            }
        }

        private void OnEnable()
        {
            if (!_initialized)
            {
                StartCoroutine(Init());
            }
        }

        IEnumerator Init()
        {
            _probeManager.enabled = false;
            yield return new WaitUntil(_permissionUtil.AllPermissionGranted);
            _probeManager.enabled = true;
            yield return new WaitUntil(
                () => XRCubemapLightEstimationFeature.IsExtensionEnabled.HasValue);
            if (!XRCubemapLightEstimationFeature.IsExtensionEnabled.Value)
            {
                Debug.LogErrorFormat("{0} is not supported on the device.",
                    XRCubemapLightEstimationFeature.UiName);
                enabled = false;
                yield break;
            }

            yield return new WaitUntil(() => _probeManager.subsystem != null);

            if (_probeManager.subsystem is AndroidXREnvironmentProbeSubsystem subsys)
            {
                _initialized = true;
            }
            else
            {
                Debug.LogError("AndroidXREnvironmentProbeSubsystem is not available.");
                enabled = false;
            }
        }
    }
}
