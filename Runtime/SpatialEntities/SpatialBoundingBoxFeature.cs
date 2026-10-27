// <copyright file="SpatialBoundingBoxFeature.cs" company="Google LLC">
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

namespace Google.XR.Extensions.SpatialEntities
{
    using System;
    using System.Collections.Generic;
    using Google.XR.Extensions;
    using Google.XR.Extensions.Internal;
    using Google.XR.Extensions.SpatialEntities.Internal;
    using UnityEngine;
    using UnityEngine.SubsystemsImplementation.Extensions;
    using UnityEngine.XR.ARSubsystems;
    using UnityEngine.XR.OpenXR.Features;
    using UnityEngine.XR.OpenXR.NativeTypes;

    using Constants = Internal.ApiConstants;
    using SpatialBoundingBoxProvider = SpatialBoundingBoxSubsystem.SpatialEntityProvider;

#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.XR.OpenXR.Features;
#endif  // UNITY_EDITOR

#if UNITY_OPENXR_ANDROIDXR
    using AXRConstants = Extensions.Internal.ApiConstants;
    using ARBoundingBoxFeature = UnityEngine.XR.OpenXR.Features.Android.ARBoundingBoxFeature;
#endif  // UNITY_OPENXR_ANDROIDXR

    /// <summary>
    /// Spatial entity feature to enable AR Foundation Bounding Box support.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(
        UiName = UiName,
        BuildTargetGroups = new[] {
            BuildTargetGroup.Android,
        },
        Company = "Google",
        Desc = "Provides subsystem implementation for AR Foundation bounding box support.",
        Version = "1.0.0",
        Category = FeatureCategory.Feature,
        FeatureId = FeatureId,
        OpenxrExtensionStrings = ExtensionStrings,
        Priority = Constants.SpatialFeaturePriority)]
#endif  // UNITY_EDITOR
    public sealed class SpatialBoundingBoxFeature : AndroidXRSpatialEntityFeature, IXRSpatialSdk
    {
        /// <summary>
        /// The UI name shows on the XR Plug-in Management panel, it helps users to understand
        /// validation errors and expected fixes.
        /// </summary>
        public const string UiName = "Spatial Entities (Extensions): Bounding Box";

        /// <summary>
        /// The feature ID string.
        /// </summary>
        public const string FeatureId = "com.google.xr.extensions.spatialentities.boundingbox";

        /// <summary>
        /// The required OpenXR extensions.
        /// </summary>
        public const string ExtensionStrings =
            Constants.EXT_future + " " +
            Constants.EXT_spatial_entity + " " +
            Constants.ANDROID_spatial_object_tracking;

        /// <summary>
        /// Runtime permission required to enable spatial bounding box.
        /// </summary>
        public static readonly AndroidXRPermission RequiredPermission =
            AndroidXRPermission.SceneUnderstandingCoarse;

        private static readonly XrSpatialCapabilityEXT _capability =
            XrSpatialCapabilityAndroid.Object.ToExt();

        private static readonly XrSpatialComponentTypeEXT _requiredComponent =
            XrSpatialComponentTypeEXT.Bounded3D;

        private static readonly XrSpatialComponentTypeEXT _classificationComponent =
            XrSpatialComponentTypeAndroid.ObjectSemanticLabel.ToExt();

        private static readonly XrSpatialComponentTypeEXT[] _requestedComponents =
        {
            _requiredComponent,
            _classificationComponent,
        };

        private static bool? _extensionEnabled = null;

        private List<XRBoundingBoxSubsystemDescriptor> _descriptors =
            new List<XRBoundingBoxSubsystemDescriptor>();

        private SpatialBoundingBoxProvider _provider = null;
        private SpatialObjectConfigurator _objectConfigurator = null;

        [SerializeField]
        private SpatialObjectDetectionModes _requestedModes = 0;

        /// <summary>
        /// Gets if the required OpenXR extension is enabled.
        /// </summary>
        public static bool? IsExtensionEnabled => _extensionEnabled;

        /// <inheritdoc/>
        public XRSpatialSdkVersions GetTargetVersion() =>
            XRSpatialSdkVersions.XRSpatialApiLevel3;

        /// <summary>
        /// Callback for <see cref="SpatialBoundingBoxProvider.Start()"/>.
        /// </summary>
        internal void OnSpatialSubsystemStart()
        {
            if (!_extensionEnabled.HasValue || !_extensionEnabled.Value)
            {
                // Spatial object is not supported.
                return;
            }

            _objectConfigurator = new SpatialObjectConfigurator(
                _capability, _requestedComponents, _requestedModes.ToLabels());
            StartCreatingSpatialContext();
        }

        /// <summary>
        /// Callback for <see cref="SpatialBoundingBoxProvider.Stop()"/>.
        /// </summary>
        internal void OnSpatialSubsystemStop()
        {
            _objectConfigurator?.Dispose();
            _objectConfigurator = null;
            DestroyAllSpatialContexts();
        }

        /// <inheritdoc/>
        protected override ISpatialCapabilityConfiguration GetConfiguration() =>
            (_objectConfigurator != null &&
             _objectConfigurator.TryCreateConfiguration(out var config)) ? config : null;

        /// <inheritdoc/>
        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            if (!AndroidXRFeatureUtils.CheckExtensions(ExtensionStrings))
            {
                _extensionEnabled = false;
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        protected override void OnSystemChange(ulong xrSystem)
        {
            if (!SpatialContextSupport.Initialized)
            {
                return;
            }

            _extensionEnabled = SpatialContextSupport.IsSupported(
                _capability, _requiredComponent);
            if (!_extensionEnabled.Value)
            {
                Debug.LogWarning($"System {xrSystem} does not support feature {UiName}.");
            }
        }

        /// <inheritdoc/>
        protected override void OnSubsystemCreate()
        {
            CreateSubsystem<XRBoundingBoxSubsystemDescriptor, XRBoundingBoxSubsystem>(
                _descriptors, SpatialBoundingBoxSubsystem._subsystemId);

            var subsystems = new List<XRBoundingBoxSubsystem>();
            SubsystemManager.GetSubsystems(subsystems);
            foreach (var s in subsystems)
            {
                if (s is SpatialBoundingBoxSubsystem spatialSubsystem)
                {
                    _provider = (SpatialBoundingBoxProvider)spatialSubsystem.GetProvider();
                    _onSpatialContextCreated += OnSpatialContextCreated;
                    _onSpatialContextDestroy += _provider.OnSpatialContextDestroy;
                    _onSpatialSnapshotCreated += _provider.OnDiscoverySnapshotCreated;
                    break;
                }
            }

            if (_provider == null)
            {
                Debug.LogErrorFormat("Failed to create {0}, {1} will do nothing.",
                    typeof(SpatialBoundingBoxSubsystem).Name, UiName);
            }
            else
            {
                Debug.Log($"Created {typeof(SpatialBoundingBoxSubsystem).Name}.");
            }
        }

        /// <inheritdoc/>
        protected override void OnSubsystemDestroy()
        {
            ResetEvents();

            _provider = null;
            DestroySubsystem<XRBoundingBoxSubsystem>();
        }

#if UNITY_EDITOR
        /// <inheritdoc/>
        protected override void GetValidationChecks(
            List<ValidationRule> rules, BuildTargetGroup targetGroup)
        {
            if (targetGroup != BuildTargetGroup.Android)
            {
                return;
            }
#if UNITY_OPENXR_ANDROIDXR
            rules.Add(AndroidXRFeatureUtils.GetSubsystemConflictRule(
                feature: this,
                featureName: UiName,
                conflictFeatureType: typeof(ARBoundingBoxFeature),
                conflictFeatureName: AXRConstants.UnityARBoundingBoxFeature,
                subsystemName: typeof(XRBoundingBoxSubsystem).Name,
                buildTarget: targetGroup));
#endif // UNITY_OPENXR_ANDROIDXR
        }
#endif // UNITY_EDITOR

        private void OnSpatialContextCreated(ulong context)
        {
            _provider?.OnSpatialContextCreated(context, _objectConfigurator);
        }
    }
}
