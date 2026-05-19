// <copyright file="XRStreamingFeature.cs" company="Google LLC">
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

namespace Google.XR.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.XR.OpenXR;
    using UnityEngine.XR.OpenXR.Features;

#if UNITY_EDITOR
    using Google.XR.Extensions.Internal;
    using UnityEditor;
    using UnityEditor.XR.OpenXR.Features;
#endif

    /// <summary>
    /// This <c><see cref="OpenXRInteractionFeature"/></c> provides Android XR Direct Preview
    /// support within Editor, require Android XR Streaming runtime to be preinstalled on the host.
    /// Note: it now supports Windows only.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = UiName,
        BuildTargetGroups = new[] { BuildTargetGroup.Standalone },
        Company = "Google",
        Desc = "Manage and configure Android XR Direct Preview.",
        Version = "1.0.0",
        OpenxrExtensionStrings = "",
        Category = FeatureCategory.Feature,
        FeatureId = FeatureId,
        Priority = 101)]
#endif
    public class XRStreamingFeature : OpenXRFeature
    {
        /// <summary>
        /// The UI name shows on the XR Plug-in Management panel, help users to understand
        /// validation errors and expected fixes.
        /// </summary>
        public const string UiName = "Android XR Streaming";

        /// <summary>
        /// The feature ID string.
        /// </summary>
        public const string FeatureId = "com.google.xr.extensions.streaming";

#if UNITY_EDITOR
        private static GraphicsDeviceType[] _supportedGraphicsTypes =
        {
            GraphicsDeviceType.Vulkan,
            GraphicsDeviceType.Direct3D12,
            GraphicsDeviceType.Direct3D11,
        };

        [Tooltip("The target client of Android XR Streaming service.")]
        [SerializeField]
        private XRStreamingClient _client = XRStreamingClient.Device;

        [Tooltip("Indicate whether to auto-start client activity on entering play mode.")]
        [SerializeField]
        private bool _autoStartOnPlay = true;

        [Tooltip("Indicate whether to auto-stop client activity on existing play mode.")]
        [SerializeField]
        private bool _autoStopOnExit = true;

        private bool _isAdbInstalled = false;

        /// <summary>
        /// The client of XR Stremaing.
        /// </summary>
        public enum XRStreamingClient
        {
            /// <summary>
            /// Streaming to Android XR devices in Editor Play Mode.
            /// </summary>
            Device = 0,

            /// <summary>
            /// Streaming to Android XR emulators in Editor Play Mode.
            /// </summary>
            Emulator = 1,
        }

        protected override void OnEnable()
        {
#if UNITY_EDITOR_WIN
            _isAdbInstalled = CmdUtils.IsAdbAvailable();
            EditorApplication.playModeStateChanged += OnPlayModeState;
#else
            Debug.LogWarning(
                "Play Mode configuration is only supported by Windows Editor.");
#endif
            base.OnEnable();
        }

        protected override void OnDisable()
        {
#if UNITY_EDITOR_WIN
            _isAdbInstalled = false;
            EditorApplication.playModeStateChanged -= OnPlayModeState;
#endif
            base.OnDisable();
        }

        /// <inheritdoc/>
        protected override void GetValidationChecks(
            List<ValidationRule> results, BuildTargetGroup targetGroup)
        {
            if (targetGroup != BuildTargetGroup.Standalone)
            {
                return;
            }

#if UNITY_EDITOR_WIN
            // Player Settings > Standalone > Other Settings:
            // Uncheck Auto Graphics API for Windows.
            // Select only supported APIs.
            results.Add(new ValidationRule(this)
            {
                message =
                    "Only Graphics APIs <b>Vulkan</b>, <b>Direct3D12</b> and <b>Direct3D11</b> " +
                    "are supported in Editor PlayMode.",
                checkPredicate = () =>
                {
                    // Default settings are not guaranteed.
                    if (PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64))
                    {
                        return false;
                    }

                    GraphicsDeviceType[] apis =
                        PlayerSettings.GetGraphicsAPIs(BuildTarget.StandaloneWindows64);
                    if (apis.Length == 0)
                    {
                        return false;
                    }

                    foreach (var settings in apis)
                    {
                        if (!_supportedGraphicsTypes.Contains(settings))
                        {
                            return false;
                        }
                    }

                    return true;
                },
                fixItMessage = "Uncheck Auto Graphics API and select supported types.",
                fixIt = () =>
                {
                    PlayerSettings.SetUseDefaultGraphicsAPIs(
                        BuildTarget.StandaloneWindows64, false);
                    GraphicsDeviceType[] apis = _supportedGraphicsTypes;
                    PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64, apis);
                },
                error = true,
            });

            // Player Settings > XR Plug-in Management > OpenXR > Standalone tab
            // Render Mode: Multi-pass.
            results.Add(new ValidationRule(this)
            {
                message = "<b>Multi-pass</b> Render Mode is required in Editor PlayMode.",
                checkPredicate = () =>
                {
                    var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);
                    return settings != null &&
                        settings.renderMode == OpenXRSettings.RenderMode.MultiPass;
                },
                fixItMessage = "Switch to Render Mode <b>Multi-pass</b>.",
                fixIt = () =>
                {
                    var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);
                    if (settings == null)
                    {
                        Debug.LogWarningFormat(
                            "Autofix failed with missing OpenXRSettings on {0} platform.",
                            targetGroup);
                        return;
                    }

                    settings.renderMode = OpenXRSettings.RenderMode.MultiPass;
                },
                error = true
            });

            // Player Settings > XR Plug-in Management > OpenXR > Standalone tab
            // Depth Submission Mode: Depth 24 Bit.
            results.Add(new ValidationRule(this)
            {
                message = "Only <b>Depth 24 Bit</b> is supported in Editor PlayMode.",
                checkPredicate = () =>
                {
                    var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);
                    return settings != null &&
                        settings.depthSubmissionMode ==
                            OpenXRSettings.DepthSubmissionMode.Depth24Bit;
                },
                fixItMessage = "Switch to Depth Submission Mode <b>Depth 24 Bit</b>.",
                fixIt = () =>
                {
                    var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);
                    if (settings == null)
                    {
                        Debug.LogWarningFormat(
                            "Autofix failed with missing OpenXRSettings in {0} platform.",
                            targetGroup);
                        return;
                    }

                    settings.depthSubmissionMode =
                        OpenXRSettings.DepthSubmissionMode.Depth24Bit;
                },
                error = true,
            });
#endif
        }

        private void OnPlayModeState(PlayModeStateChange state)
        {
            Debug.LogFormat("PlayModeStateChange: {0}.", state);
            if (!_isAdbInstalled)
            {
                Debug.LogWarning(
                    "ADB is not available. " +
                    "Play Mode configuration will not take effect.");
                return;
            }

            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                if (_client == XRStreamingClient.Device && _autoStartOnPlay)
                {
                    CmdUtils.TryStartStreamingClient();
                }
            }
            else if (state == PlayModeStateChange.ExitingPlayMode)
            {
                if (_client == XRStreamingClient.Device && _autoStopOnExit)
                {
                    CmdUtils.TryStopStreamingClient();
                }
            }
        }
#endif // UNITY_EDITOR
    }
}
