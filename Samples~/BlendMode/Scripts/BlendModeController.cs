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
        /// Text mesh for displaying debug information.
        /// </summary>
        public TextMesh DebugText;

        private const float _configInterval = 3f;

        /// <summary>
        /// The <see cref="XREnvironmentBlendModeFeature"/> settings for Android platform.
        /// </summary>
        private XREnvironmentBlendModeFeature _blendFeature = null;

        /// <summary>
        /// The <see cref="XRSystemStateFeature"/> for Android platform.
        /// </summary>
        private XRSystemStateFeature _systemStateFeature = null;

        private int _currentBlendModeIndex = 0;
        private StringBuilder _stringBuilder = new StringBuilder();
        private float _configTimer = 0f;

        private void Awake()
        {
            _blendFeature = OpenXRSettings.Instance.GetFeature<XREnvironmentBlendModeFeature>();
            _systemStateFeature = OpenXRSettings.Instance.GetFeature<XRSystemStateFeature>();

            if (_blendFeature == null)
            {
                Debug.LogErrorFormat(
                    "Cannot find {0} targeting Android platform.",
                    XREnvironmentBlendModeFeature.UiName);
            }

            if (_systemStateFeature == null)
            {
                Debug.LogWarningFormat(
                    "Cannot find {0} targeting Android platform.",
                    XRSystemStateFeature.UiName);
            }
        }

        private void Update()
        {
            _stringBuilder.Clear();
            if (_blendFeature == null)
            {
                _stringBuilder.AppendFormat(
                    "Cannot find {0}.", XREnvironmentBlendModeFeature.UiName);
            }
            else if (!_blendFeature.enabled)
            {
                _stringBuilder.AppendFormat(
                    "{0} is disabled.", XREnvironmentBlendModeFeature.UiName);
            }
            else
            {
                var modes = _blendFeature.SupportedEnvironmentBlendModes;
                if ((modes?.Count ?? 0) > 0)
                {
                    _configTimer += Time.deltaTime;
                    if (_configTimer > _configInterval)
                    {
                        _configTimer = 0;
                        _currentBlendModeIndex = (_currentBlendModeIndex + 1) % modes.Count;
                    }

                    _blendFeature.RequestedEnvironmentBlendMode = modes[_currentBlendModeIndex];

                    _stringBuilder.Append(
                        $"RequestMode: {_blendFeature.RequestedEnvironmentBlendMode}\n");
                    _stringBuilder.Append($"CurrentMode: {_blendFeature.CurrentBlendMode}\n");
                }
                else
                {
                    _stringBuilder.Append("No environment blend modes supported.\n");
                    _stringBuilder.Append("Are you running on device?\n");
                }
            }

            _stringBuilder.Append("\n");
            if (_systemStateFeature == null)
            {
                _stringBuilder.AppendFormat(
                    "Cannot find {0}.", XRSystemStateFeature.UiName);
            }
            else if (!_systemStateFeature.enabled)
            {
                _stringBuilder.AppendFormat(
                    "{0} is disabled.", XRSystemStateFeature.UiName);
            }
            else
            {
                bool result = XRSystemStateFeature.TryGetSystemState(out XrSystemState systemState);
                if (result)
                {
                    _stringBuilder.Append(
                        $"Blend mode state: {systemState.BlendModeState}\n");
                    _stringBuilder.Append(
                        $"Passthrough opacity: {systemState.PassthroughOpacity}\n");
                    _stringBuilder.Append(
                        $"Input modality: {systemState.InputModalityState}");
                }
                else
                {
                    _stringBuilder.Append("Error getting system state.");
                }
            }

            DebugText.text = _stringBuilder.ToString();
        }
#if UNITY_EDITOR

        private void OnValidate()
        {
            var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(
                UnityEditor.BuildTargetGroup.Android);
            var blendFeature = settings.GetFeature<XREnvironmentBlendModeFeature>();
            if (blendFeature == null || !blendFeature.enabled)
            {
                Debug.LogErrorFormat(
                    "Cannot find {0} targeting Android platform.",
                    XREnvironmentBlendModeFeature.UiName);
            }

            var systemStateFeature = settings.GetFeature<XRSystemStateFeature>();
            if (systemStateFeature == null || !systemStateFeature.enabled)
            {
                Debug.LogWarningFormat(
                    "Cannot find {0} targeting Android platform.",
                    XRSystemStateFeature.UiName);
            }
        }
#endif // UNITY_EDITOR
    }
}
