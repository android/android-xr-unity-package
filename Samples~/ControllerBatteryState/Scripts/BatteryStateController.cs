// <copyright file="BatteryStateController.cs" company="Google LLC">
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
namespace Google.XR.Extensions.Samples.ControllerBatteryState
{
    using System.Text;
    using Google.XR.Extensions;
    using UnityEngine;
    using UnityEngine.XR.OpenXR;

    /// <summary>
    /// Updates the UI display with the controller battery state.
    /// </summary>
    public class BatteryStateController : MonoBehaviour
    {
        [SerializeField]
        private TextMesh _text = null;

        private XRBatteryStateDisplayFeature _batteryStateFeature = null;

        private StringBuilder _stringBuilder = new StringBuilder();

        private void Awake()
        {
            _batteryStateFeature =
                OpenXRSettings.Instance.GetFeature<XRBatteryStateDisplayFeature>();
            if (_batteryStateFeature == null)
            {
                Debug.LogWarningFormat(
                    "Cannot find {0} targeting Android platform.",
                    XRBatteryStateDisplayFeature.UiName);
                enabled = false;
            }
        }

        void Update()
        {
            _stringBuilder.Clear();
            AppendStringBuilderWithBatteryState("Left Controller", "/user/hand/left");
            _stringBuilder.Append("\n\n");
            AppendStringBuilderWithBatteryState("Right Controller", "/user/hand/right");
            _text.text = _stringBuilder.ToString();
        }

        void AppendStringBuilderWithBatteryState(string controllerLabel, string inputPath)
        {
            BatteryStatus batteryStatus;
            float batteryLevel;
            bool result = XRBatteryStateDisplayFeature.TryGetBatteryStateDisplay(
                inputPath, out batteryStatus, out batteryLevel);
            if (!result || batteryStatus == BatteryStatus.Unknown)
            {
                _stringBuilder.Append(controllerLabel + " Unknown battery status");
            }
            else
            {
                _stringBuilder.AppendFormat(
                    controllerLabel + " Battery: {0}, Status: {1}",
                    (int)(batteryLevel * 100.0f),
                    batteryStatus);
            }
        }
    }
}
