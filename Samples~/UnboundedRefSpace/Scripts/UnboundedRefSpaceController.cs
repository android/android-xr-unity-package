// <copyright file="UnboundedRefSpaceController.cs" company="Google LLC">
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

namespace Google.XR.Extensions.Samples.UnboundedRefSpace
{
    using UnityEngine;
    using UnityEngine.XR;
    using UnityEngine.XR.Management;

    /// <summary>
    /// Sets the XRInputSubsystem tracking origin mode to Unbounded if it is supported and displays
    /// the support status to the user.
    /// </summary>
    public class UnboundedRefSpaceController : MonoBehaviour
    {
        /// <summary>
        /// Text mesh to display debug information.
        /// </summary>
        public TextMesh DebugText;

        private XRInputSubsystem _inputSubsystem;

        void Update()
        {
            if (_inputSubsystem == null)
            {
                var settingLoader = XRGeneralSettings.Instance?.Manager?.activeLoader;
                if (settingLoader)
                {
                    _inputSubsystem = settingLoader.GetLoadedSubsystem<XRInputSubsystem>();
                }
            }

            if (_inputSubsystem != null)
            {
                TrackingOriginModeFlags supportedTrackingModes =
                    _inputSubsystem.GetSupportedTrackingOriginModes();

                if (supportedTrackingModes.HasFlag(TrackingOriginModeFlags.Unbounded))
                {
                    DebugText.text = "Unbounded Reference Space is supported.";
                    if (_inputSubsystem.GetTrackingOriginMode() == TrackingOriginModeFlags.Unbounded
                        || _inputSubsystem.TrySetTrackingOriginMode(
                            TrackingOriginModeFlags.Unbounded))
                    {
                        DebugText.text +=
                            $"\nTracking Origin Mode: {_inputSubsystem.GetTrackingOriginMode()}";
                        DebugText.text +=
                            "\nRecentering via the system UI will not" +
                            "\nchange content placement when using Unbounded mode.";
                    }
                }
                else
                {
                    DebugText.text = "Unbounded Reference Space is not supported";
                }
            }
        }
    }
}
