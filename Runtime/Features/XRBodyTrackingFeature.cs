// <copyright file="XRBodyTrackingFeature.cs" company="Google LLC">
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
    using System.Collections.Generic;
    using Google.XR.Extensions.Internal;
    using UnityEngine;
    using UnityEngine.XR.ARSubsystems;
    using UnityEngine.XR.Management;
    using UnityEngine.XR.OpenXR;
    using UnityEngine.XR.OpenXR.Features;

#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.XR.OpenXR.Features;
#endif

    /// <summary>
    /// This <c><see cref="OpenXRInteractionFeature"/></c> configures Android XR extensions
    /// <c>XR_ANDROIDX_body_tracking</c> at runtime and provides
    /// <c><see cref="XRHumanBodySubsystem"/></c> implementation that works on Android XR platform.
    ///
    /// Note: due to the dependency on <c><see cref="XRSessionFeature"/></c>, its priority must be
    /// lower than the features so the feature registration happens after <c>XrInstanceManager</c>
    /// creation.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = UiName,
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        Company = "Google",
        OpenxrExtensionStrings = ExtensionString,
        Desc = "Enable Android XR Body Tracking at runtime. " +
            "And provide the implementation of XRHumanBodySubsystem",
        Version = "1.0.0",
        Category = FeatureCategory.Feature,
        FeatureId = FeatureId,
        Priority = 97)]
#endif
    public class XRBodyTrackingFeature : OpenXRFeature
    {
        /// <summary>
        /// The UI name shows on the XR Plug-in Management panel, help users to understand
        /// validation errors and expected fixes.
        /// </summary>
        public const string UiName = "Android XR (Extensions): Human Body (Experimental*)";

        /// <summary>
        /// The feature ID string.
        /// </summary>
        public const string FeatureId = "com.google.xr.extensions.body_tracking";

        /// <summary>
        /// The OpenXR Extension string. Used to check if this extensions is
        /// available or enabled.
        /// </summary>
        public const string ExtensionString = "XR_ANDROIDX_body_tracking";

        /// <summary>
        /// Runtime permission required to enable body tracking.
        /// </summary>
        public static readonly AndroidXRPermission RequiredPermission =
            AndroidXRPermission.BodyTracking;

        internal static bool? _extensionEnabled = null;

        [SerializeField]
        private bool _autoCalibration = false;

        [SerializeField]
        private XRHumanBodyProportions _proportions = null;

        private AndroidXRHumanBodySubsystem _subsystemInstance = null;

        /// <summary>
        /// Gets if the required OpenXR extension is enabled.
        /// When OpenXR runtime is waiting, it returns <c>null</c>. Otherwise, it indicates
        /// whether the XR_ANDROIDX_body_tracking extension is available on current device.
        /// </summary>
        public static bool? IsExtensionEnabled => _extensionEnabled;

        /// <summary>
        /// Gets or sets a value indicating whether to use automatic calibration at runtime.
        /// When automatic calibration is disabled, <see cref="HumanBodyProportions"/> will take
        /// effect.
        /// </summary>
        public bool AutoCalibration
        {
            get => _autoCalibration;
            set
            {
                _autoCalibration = value;
#if !UNITY_EDITOR
                if (_subsystemInstance != null)
                {
                    // Handle calibration change at runtime.
                    _subsystemInstance.AutoCalibrationRequested = _autoCalibration;
                }
#endif
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="XRHumanBodyProportions"/> for rest pose skeleton
        /// computation. It only takes effect when <see cref="AutoCalibration"/> is disabled.
        /// </summary>
        public XRHumanBodyProportions HumanBodyProportions
        {
            get => _proportions;
            set
            {
                _proportions = HumanBodyProportions;
#if !UNITY_EDITOR
                if (_subsystemInstance != null)
                {
                    // Handle calibration change at runtime.
                    _subsystemInstance.ProportionCalibrationRequested = _proportions;
                }
#endif
            }
        }

        /// <inheritdoc/>
        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            if (!base.OnInstanceCreate(xrInstance))
            {
                return false;
            }

            _extensionEnabled = OpenXRRuntime.IsExtensionEnabled(ExtensionString);
            if (!_extensionEnabled.Value)
            {
                return false;
            }

            return XRInstanceManagerApi.Register(ApiXrFeature.BodyTracking);
        }

        /// <inheritdoc/>
        protected override void OnInstanceDestroy(ulong xrInstance)
        {
            XRInstanceManagerApi.Unregister(ApiXrFeature.BodyTracking);
        }

        /// <inheritdoc/>
        protected override void OnSubsystemCreate()
        {
            var descriptors = new List<XRHumanBodySubsystemDescriptor>();
            SubsystemManager.GetSubsystemDescriptors(descriptors);
            if (descriptors.Count < 1)
            {
                Debug.LogWarning("Failed to find XRHumanBodySubsystemDescriptor.");
                return;
            }

            // Create custom XRHumanBodySubsystem.
            CreateSubsystem<XRHumanBodySubsystemDescriptor, XRHumanBodySubsystem>(
                descriptors, AndroidXRHumanBodySubsystem._id);

            XRLoader xrLoader = XRSessionFeature.GetXRLoader();
            if (xrLoader != null)
            {
                _subsystemInstance = xrLoader.GetLoadedSubsystem<XRHumanBodySubsystem>() as
                    AndroidXRHumanBodySubsystem;
            }
            else
            {
                Debug.LogWarning("Failed to find any active loader.");
                return;
            }

            if (_subsystemInstance == null)
            {
                Debug.LogErrorFormat(
                    "Failed to find descriptor '{0}' - Body Tracking will not do anything!",
                    AndroidXRHumanBodySubsystem._id);
                return;
            }
            else
            {
                Debug.LogFormat("Created {0}.", _subsystemInstance.GetType());
                if (_autoCalibration)
                {
                    _subsystemInstance.AutoCalibrationRequested = _autoCalibration;
                }
                else if (_proportions != null)
                {
                    _subsystemInstance.ProportionCalibrationRequested = _proportions;
                }
            }
        }

        /// <inheritdoc/>
        protected override void OnSubsystemDestroy()
        {
            if (_subsystemInstance != null)
            {
                _subsystemInstance.Destroy();
                _subsystemInstance = null;
            }
        }

#if UNITY_EDITOR
        /// <inheritdoc/>
        protected override void GetValidationChecks(
            List<ValidationRule> results, BuildTargetGroup targetGroup)
        {
            if (targetGroup != BuildTargetGroup.Android)
            {
                return;
            }

            results.Add(AndroidXRFeatureUtils.GetExperimentalFeatureValidationCheck(
                 this, UiName, targetGroup));
        }
#endif // UNITY_EDITOR
    }
}
