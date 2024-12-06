// <copyright file="PassthroughCameraState.cs" company="Google LLC">
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

namespace Google.XR.Extensions.Samples.Passthrough
{
    using UnityEngine;
    using UnityEngine.XR.OpenXR;

    /// <summary>
    /// Passthrough Camera state sample to demonstrate passthrough camera readiness.
    /// </summary>
    public class PassthroughCameraState : MonoBehaviour
    {
        /// <summary>
        /// Text mesh for displaying debug information.
        /// </summary>
        public TextMesh DebugText;

        private void Update()
        {
            if (XRPassthroughFeature.IsExensionEnabled == null)
            {
                DebugText.text = "XrInstance hasn't been initialized.";
                return;
            }

            if (!XRPassthroughFeature.IsExensionEnabled.Value)
            {
                DebugText.text = "XR_ANDROID_passthrough_camera_state is not enabled.";
                return;
            }

            DebugText.text = "State: " + XRPassthroughFeature.GetState();
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(
                UnityEditor.BuildTargetGroup.Android);
            var feature = settings.GetFeature<XRPassthroughFeature>();
            if (feature == null)
            {
                Debug.LogErrorFormat(
                    "Cannot find {0} targeting Android platform.", XRPassthroughFeature.UiName);
                return;
            }
            else if (!feature.enabled)
            {
                Debug.LogWarningFormat(
                    "{0} is disabled. Passthrough sample will not work properly.",
                    XRPassthroughFeature.UiName);
            }
#endif // UNITY_EDITOR
        }
    }
}
