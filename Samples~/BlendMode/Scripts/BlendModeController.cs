// <copyright file="BlendModeController.cs" company="Google LLC">
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

namespace Google.XR.Extensions.Samples.BlendMode
{
    using System.Text;
    using UnityEngine;
    using UnityEngine.XR.OpenXR;

    /// <summary>
    /// Blend sample controller to demonstrate blend mode modification at runtime.
    /// </summary>
    public class BlendModeController : MonoBehaviour
    {
        /// <summary>
        /// The <see cref="XREnvironmentBlendModeFeature"/> settings for Android platform.
        /// </summary>
        public XREnvironmentBlendModeFeature BlendFeature = null;

        /// <summary>
        /// Text mesh for displaying debug information.
        /// </summary>
        public TextMesh DebugText;

        private const float _configInterval = 3f;

        private int _currentBlendModeIndex = 0;
        private StringBuilder _stringBuilder = new StringBuilder();
        private float _configTimer = 0f;

        private void Update()
        {
            if (BlendFeature == null)
            {
                return;
            }

            var modes = BlendFeature.SupportedEnvironmentBlendModes;
            _stringBuilder.Clear();
            if ((modes?.Count ?? 0) > 0)
            {
                _configTimer += Time.deltaTime;
                if (_configTimer > _configInterval)
                {
                    _configTimer = 0;
                    _currentBlendModeIndex = (_currentBlendModeIndex + 1) % modes.Count;
                }

                BlendFeature.RequestedEnvironmentBlendMode = modes[_currentBlendModeIndex];

                _stringBuilder.Append(
                    $"RequestMode: {BlendFeature.RequestedEnvironmentBlendMode}\n");
                _stringBuilder.Append($"CurrentMode: {BlendFeature.CurrentBlendMode}");
            }
            else
            {
                _stringBuilder.Append("No environment blend modes supported.\n");
                _stringBuilder.Append("Are you running on device?");
            }

            DebugText.text = _stringBuilder.ToString();
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (BlendFeature == null)
            {
                var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(
                    UnityEditor.BuildTargetGroup.Android);
                BlendFeature = settings.GetFeature<XREnvironmentBlendModeFeature>();
                if (BlendFeature == null)
                {
                    Debug.LogErrorFormat(
                        "Cannot find {0} targeting Android platform.",
                        XREnvironmentBlendModeFeature.UiName);
                    return;
                }
            }

            if (BlendFeature != null && !BlendFeature.enabled)
            {
                Debug.LogWarningFormat(
                    "{0} is disabled. BlendMode sample will not work correctly.",
                    XREnvironmentBlendModeFeature.UiName);
            }
#endif
        }
    }
}
