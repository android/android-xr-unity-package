// <copyright file="SpatialShareableAnchorFeature.cs" company="Google LLC">
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
    using System.Collections.Generic;
    using Google.XR.Extensions.ARSubsystems;
    using Google.XR.Extensions.Internal;
    using Google.XR.Extensions.SpatialEntities.Internal;
#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.XR.OpenXR.Features;
#endif  // UNITY_EDITOR
    using UnityEngine;
    using UnityEngine.SubsystemsImplementation.Extensions;
    using UnityEngine.XR.OpenXR.NativeTypes;

    using Constants = Internal.ApiConstants;
    using SpatialShareableAnchorProvider = SpatialShareableAnchorSubsystem.SpatialEntityProvider;

    /// <summary>
    /// Enables AR Foundation shareable anchor support via OpenXR Spatial
    /// Entities extensions.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(
        UiName = UiName,
        BuildTargetGroups = new[] {
            BuildTargetGroup.Android,
        },
        Company = "Google",
        Desc = "Provides subsystem implementation for AR Foundation shareable anchor support.",
        Version = "1.0.0",
        Category = FeatureCategory.Feature,
        FeatureId = FeatureId,
        OpenxrExtensionStrings = ExtensionStrings,
        Priority = Constants.SpatialFeaturePriority)]
#endif  // UNITY_EDITOR
    public sealed class SpatialShareableAnchorFeature : AndroidXRSpatialEntityFeature, IXRSpatialSdk
    {
        /// <summary>
        /// The UI name shows on the XR Plug-in Management panel, helps users to
        /// understand validation errors and expected fixes.
        /// </summary>
        public const string UiName = "Spatial Entities (Extensions): Shareable Anchors";

        /// <summary>
        /// The feature ID string.
        /// </summary>
        public const string FeatureId =
            "com.google.xr.extensions.spatialentities.shareable_anchor";

        /// <summary>
        /// The OpenXR Extension string. Used to check if these extensions are
        /// available or enabled.
        /// </summary>
        public const string ExtensionStrings =
            Constants.ANDROID_geospatial + " " +
            Constants.ANDROID_geospatial_anchor + " " +
            Constants.EXT_future + " " +
            Constants.EXT_spatial_entity + " " +
            Constants.EXT_spatial_anchor + " " +
            Constants.EXT_spatial_persistence + " " +
            Constants.EXT_spatial_persistence_operations;

        /// <summary>
        /// Runtime permission required to enable shareable anchors.
        /// </summary>
        public static readonly AndroidXRPermission RequiredPermission =
            AndroidXRPermission.SceneUnderstandingCoarse;

        private const string _anchorExtensions =
            Constants.EXT_future + " " +
            Constants.EXT_spatial_entity + " " +
            Constants.EXT_spatial_anchor;

        private const string _persistenceExtensions =
            Constants.EXT_spatial_persistence + " " +
            Constants.EXT_spatial_persistence_operations;

        private readonly HashSet<XrSpatialPersistenceScopeEXT> _requestedScopes =
            new HashSet<XrSpatialPersistenceScopeEXT>();

        private readonly HashSet<XrSpatialComponentTypeEXT> _requestedComponents =
            new HashSet<XrSpatialComponentTypeEXT>();

        private List<XRShareableAnchorSubsystemDescriptor> _descriptors =
            new List<XRShareableAnchorSubsystemDescriptor>();

        private SpatialShareableAnchorProvider _provider = null;
        private SpatialAnchorConfigurator _anchorConfigurator = null;

        [Tooltip(
            "Indicates whether to automatically load persisted anchors when " +
            "the storage host supports discovering all persisted entities " +
            "from  discovery snapshots. Otherwise, use TryLoadAnchorAsync API " +
            "with a given shareable ID.")]
        [SerializeField]
        private bool _autoLoadPersistedAnchors = true;
#pragma warning disable CS0169 // TODO(b/540032227): Not yet implemented.

        [Tooltip("Indicates whether to enable geospatial host at runtime.")]
        [SerializeField]
        private bool _enableGeospatialHost = false;

        /// <summary>
        /// Gets if the basic anchor support is enabled.
        /// When OpenXR runtime is waiting, it returns <c>null</c>.
        /// Otherwise, it indicates whether spatial anchor and relevant
        /// extensions are available at runtime.
        /// </summary>
        public static bool? IsAnchorExtensionEnabled { get; private set; }

        /// <summary>
        /// Gets if the required extension for
        /// <see cref="XRShareableAnchorHosts.Geospatial"/> is enabled and supported.
        /// </summary>
        public static bool? IsGeospatialHostSupported { get; private set; }

#if UNITY_EDITOR
        /// <summary>
        /// Editor helper to enable or disable the geospatial host from scripts.
        /// </summary>
        public bool EnableGeospatialHost
        {
            get => _enableGeospatialHost;
            set => _enableGeospatialHost = value;
        }
#endif // UNITY_EDITOR
#pragma warning restore CS0169

        /// <summary>
        /// Gets the target version of the given feature if applicable.
        /// </summary>
        /// <returns>The target version to meet feature requirements.</returns>
        public XRSpatialSdkVersions GetTargetVersion() =>
            XRSpatialSdkVersions.XRSpatialApiLevelAuto;

        /// <summary>
        /// Callback for <see cref="SpatialShareableAnchorProvider.Start()"/>.
        /// </summary>
        internal void OnSpatialProviderStart()
        {
            if (!IsAnchorExtensionEnabled.HasValue || !IsAnchorExtensionEnabled.Value)
            {
                // Spatial anchor is not supported.
                return;
            }

            _anchorConfigurator = new SpatialAnchorConfigurator(_requestedComponents);
            StartCreatingSpatialContext(forceCreate: false, withPersistence: true);
        }

        /// <summary>
        /// Callback for <see cref="SpatialShareableAnchorProvider.Stop()"/>.
        /// </summary>
        internal void OnSpatialProviderStop()
        {
            _anchorConfigurator?.Dispose();
            _anchorConfigurator = null;
            DestroyAllSpatialContexts();
            DestroyAllPersistenceContexts();
        }

        /// <inheritdoc/>
        protected override ISpatialCapabilityConfiguration GetConfiguration() =>
            (_anchorConfigurator != null &&
             _anchorConfigurator.TryCreateConfiguration(out var config)) ? config : null;

        /// <inheritdoc/>
        protected override HashSet<XrSpatialPersistenceScopeEXT> GetScopes() => _requestedScopes;

        /// <inheritdoc/>
        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            if (!AndroidXRFeatureUtils.CheckExtensions(_anchorExtensions))
            {
                IsAnchorExtensionEnabled = false;
                IsGeospatialHostSupported = false;
                return false;
            }

            if (_enableGeospatialHost)
            {
                if (!AndroidXRFeatureUtils.CheckExtensions(Constants.ANDROID_geospatial_anchor))
                {
                    IsGeospatialHostSupported = false;
                    Debug.LogWarningFormat(
                        "Failed to enable extensions {0}. Geospatial host will not be supported.",
                        Constants.ANDROID_geospatial_anchor);
                }
                else if (!XRGeospatialApi.InitializeGeospatialAnchorHost())
                {
                    IsGeospatialHostSupported = false;
                    Debug.LogWarning(
                        "Geospatial anchor host initialization failed. Geospatial host will not " +
                        "be supported.");
                }
            }

            if (!AndroidXRFeatureUtils.CheckExtensions(_persistenceExtensions))
            {
                Debug.LogWarningFormat(
                    "Failed to enable extensions {0}. {1} cannot save, load, or erase anchors.",
                    _persistenceExtensions, UiName);
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

            _requestedScopes.Clear();
            _requestedComponents.Clear();

            if (!SpatialContextSupport.IsSupported(
                XrSpatialCapabilityEXT.Anchor, XrSpatialComponentTypeEXT.Anchor))
            {
                IsAnchorExtensionEnabled = false;
                IsGeospatialHostSupported = false;
                Debug.LogWarning($"System {xrSystem} does not support feature {UiName}.");
                return;
            }

            IsAnchorExtensionEnabled = true;
            _requestedComponents.Add(XrSpatialComponentTypeEXT.Anchor);

            if (SpatialContextSupport.IsSupported(
                XrSpatialCapabilityEXT.Anchor, XrSpatialComponentTypeEXT.Persistence))
            {
                _requestedComponents.Add(XrSpatialComponentTypeEXT.Persistence);
                if (SpatialContextSupport.IsSupported(XrSpatialPersistenceScopeEXT.LocalAnchors))
                {
                    _requestedScopes.Add(XrSpatialPersistenceScopeEXT.LocalAnchors);
                }
            }

            if (_enableGeospatialHost && !IsGeospatialHostSupported.HasValue)
            {
                IsGeospatialHostSupported = XRGeospatialFeature.IsExtensionEnabled == true;
                if (!IsGeospatialHostSupported.Value)
                {
                    Debug.LogWarningFormat(
                        "{0} is not supported. Geospatial host will not be supported.",
                        XRGeospatialFeature.UiName);
                }
            }
        }

        /// <inheritdoc/>
        protected override void OnSubsystemCreate()
        {
            CreateSubsystem<XRShareableAnchorSubsystemDescriptor, XRShareableAnchorSubsystem>(
                _descriptors, SpatialShareableAnchorSubsystem._subsystemId);

            var subsystems = new List<XRShareableAnchorSubsystem>();
            SubsystemManager.GetSubsystems(subsystems);
            foreach (var subsystem in subsystems)
            {
                if (subsystem is SpatialShareableAnchorSubsystem spatialSubsystem)
                {
                    // Register spatial context events.
                    _provider = (SpatialShareableAnchorProvider)spatialSubsystem.GetProvider();
                    _provider.SetAutoLoad(_autoLoadPersistedAnchors);
                    _onPersistenceContextCreated += _provider.OnPersistenceContextCreated;
                    _onPersistenceContextDestroy += _provider.OnPersistenceContextDestroy;
                    _onSpatialContextCreated += OnSpatialContextCreated;
                    _onSpatialContextDestroy += _provider.OnSpatialContextDestroy;
                    _onSpatialSnapshotCreated += _provider.OnDiscoverySnapshotCreated;
                    break;
                }
            }

            if (_provider == null)
            {
                Debug.LogErrorFormat("Failed to create {0}, {1} will do nothing.",
                    typeof(SpatialShareableAnchorSubsystem).Name, UiName);
            }
            else
            {
                Debug.Log($"Created {typeof(SpatialShareableAnchorSubsystem).Name}.");
            }
        }

        /// <inheritdoc/>
        protected override void OnSubsystemDestroy()
        {
            Debug.Log($"Destroy {typeof(SpatialShareableAnchorSubsystem).Name}.");
            ResetEvents();
            DestroySubsystem<SpatialShareableAnchorSubsystem>();
        }

        private void OnSpatialContextCreated(ulong context)
        {
            _provider?.OnSpatialContextCreated(context, _anchorConfigurator);
        }

#if UNITY_EDITOR
        /// <inheritdoc/>
        protected override void GetValidationChecks(List<ValidationRule> results, BuildTargetGroup targetGroup)
        {
            base.GetValidationChecks(results, targetGroup);

            if (targetGroup != BuildTargetGroup.Android)
            {
                return;
            }

            if (_enableGeospatialHost)
            {
                results.Add(AndroidXRFeatureUtils.GetFeatureDependencyRule(
                    feature: this,
                    requiredFeatureType: typeof(XRGeospatialFeature),
                    requiredFeatureName: XRGeospatialFeature.UiName,
                    buildTarget: targetGroup));
            }
        }
#endif // UNITY_EDITOR
    }
}
