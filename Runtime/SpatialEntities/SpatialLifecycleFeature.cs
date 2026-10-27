// <copyright file="SpatialLifecycleFeature.cs" company="Google LLC">
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
    using Google.XR.Extensions.Internal;
    using Google.XR.Extensions.SpatialEntities.Internal;
    using UnityEngine;
    using UnityEngine.XR.OpenXR;
    using UnityEngine.XR.OpenXR.Features;
    using UnityEngine.XR.OpenXR.NativeTypes;

    using Constants = Internal.ApiConstants;
    using XrSpatialContextEXT = System.UInt64;

#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.XR.OpenXR.Features;
#endif  // UNITY_EDITOR

    /// <summary>
    /// Provides lifecycle management for extended Spatial Entities features.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(
        Hidden = true, // Hidden from Editor UI and enabled by dependent features.
        UiName = UiName,
        BuildTargetGroups = new[] {
            BuildTargetGroup.Android,
        },
        Company = "Google",
        Desc = "Provides lifecycle management for extended Spatial Entities features.",
        Version = "1.0.0",
        Category = FeatureCategory.Feature,
        FeatureId = FeatureId,
        OpenxrExtensionStrings = ExtensionStrings,
        Priority = Constants.LifecyclePriority)]
#endif  // UNITY_EDITOR
    public sealed class SpatialLifecycleFeature : OpenXRFeature
    {
        /// <summary>
        /// The UI name shows on the XR Plug-in Management panel, it helps users to understand
        /// validation errors and expected fixes.
        /// </summary>
        public const string UiName = "Spatial Entities (Extensions): Lifecycle";

        /// <summary>
        /// The feature ID string.
        /// </summary>
        public const string FeatureId = "com.google.xr.extensions.spatialentities.lifecycle";

        /// <summary>
        /// The required OpenXR extensions.
        /// </summary>
        public const string ExtensionStrings =
            Constants.EXT_future + " " +
            Constants.EXT_spatial_entity + " " +
            Constants.EXT_spatial_persistence + " " +
            Constants.EXT_spatial_persistence_operations;

        private const XrStructureType _discoveryEvent =
            XrStructureType.EventDataSpatialDiscoveryRecommendedEXT;

        private static SpatialLifecycleFeature _instance;

        private bool _discoveryEventSubscribed = false;
        private Dictionary<XrSpatialContextEXT, DiscoveryRecommendedCallback> _discoveryCallbacks =
            new Dictionary<XrSpatialContextEXT, DiscoveryRecommendedCallback>();

        private ulong _xrInstance = 0;
        private ulong _xrSystem = 0;

        internal delegate void DiscoveryRecommendedCallback(XrSpatialContextEXT context);

        internal static void RegisterDiscoveryCallback(
            XrSpatialContextEXT context, DiscoveryRecommendedCallback callback) =>
            _instance?._discoveryCallbacks.Add(context, callback);

        internal static void UnregisterDiscoveryCallback(XrSpatialContextEXT context) =>
            _instance?._discoveryCallbacks.Remove(context);

        /// <summary>
        /// Registers callbacks for instance poll events. Called by or after
        /// <see cref="OpenXRFeature.OnInstanceCreate(ulong)"/>, unregistered by
        /// <see cref="UnregisterEventCallback(XrStructureType, XrPollEventCallback)"/>.
        /// For <see cref="XrStructureType.EventDataSpatialDiscoveryRecommendedEXT"/>, use
        /// <see cref="RegisterDiscoveryCallback(XrSpatialContextEXT, DiscoveryRecommendedCallback)"/>
        /// instead.
        /// </summary>
        /// <param name="eventType">The target event type.</param>
        /// <param name="callback">The callback to be triggered on received.</param>
        internal static void RegisterEventCallback(
            XrStructureType eventType, XrPollEventCallback callback) =>
            _instance?.TogglePollEvent(true, eventType, callback);

        /// <summary>
        /// Unregisters callbacks which have been registered via
        /// <see cref="RegisterEventCallback(XrStructureType, XrPollEventCallback)"/>.
        /// Called by or before <see cref="OpenXRFeature.OnInstanceDestroy(ulong)"/>.
        /// For <see cref="XrStructureType.EventDataSpatialDiscoveryRecommendedEXT"/>, use
        /// <see cref="UnregisterDiscoveryCallback(XrSpatialContextEXT)"/> instead.
        /// </summary>
        /// <param name="eventType">The target event type.</param>
        /// <param name="callback">The callback to be triggered on received.</param>
        internal static void UnregisterEventCallback(
            XrStructureType eventType, XrPollEventCallback callback) =>
            _instance?.TogglePollEvent(false, eventType, callback);

        /// <inheritdoc/>
        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            _instance = this;

            // Extensions status does not affect the usage.
            // Global resources will then be initialized based on runtime support.
            _xrInstance = xrInstance;

            // Meanwhile, native context is only available when spatial entity
            // extension is supported.
            if (AndroidXRFeatureUtils.CheckExtensions(Constants.EXT_spatial_entity) &&
                NativeContextInNeed())
            {
                // Register native feature to support extended features.
                XRInstanceManagerApi.Register(ApiXrFeature.SpatialEntity);
            }

            return true;
        }

        /// <inheritdoc/>
        protected override void OnInstanceDestroy(ulong xrInstance)
        {
            _xrInstance = 0;
            SpatialContextSupport.Reset();

            unsafe
            {
                TogglePollEvent(false, _discoveryEvent, OnDiscoveryRecommended);
            }

            // Unregister native feature and release underlying resources.
            XRInstanceManagerApi.Unregister(ApiXrFeature.SpatialEntity);
        }

        /// <inheritdoc/>
        protected override void OnSystemChange(ulong xrSystem)
        {
            _xrSystem = xrSystem;
            SpatialContextSupport.Initialize(_xrInstance, _xrSystem);

            if (SpatialContextSupport.IsExtensionEnabled())
            {
                unsafe
                {
                    TogglePollEvent(true, _discoveryEvent, OnDiscoveryRecommended);
                }
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Called whenever a ScriptableObject is first created.
        /// </summary>
        protected override void Awake()
        {
            EditorApplication.delayCall += RefreshEnabledState;
        }

        internal static void RefreshEnabledState()
        {
            var spatialEntityFeatures =
                TypeCache.GetTypesDerivedFrom<AndroidXRSpatialEntityFeature>(Constants.Assembly);

            // Refresh all supported targets.
            RefreshEnabledState(BuildTargetGroup.Android, spatialEntityFeatures);
        }

        private static void RefreshEnabledState(
            BuildTargetGroup buildTarget, TypeCache.TypeCollection spatialEntityFeatures)
        {
            var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(buildTarget);
            if (settings == null)
            {
                return;
            }

            var lifecycleFeature = settings.GetFeature<SpatialLifecycleFeature>();
            if (lifecycleFeature == null)
            {
                return;
            }

            foreach (var featureType in spatialEntityFeatures)
            {
                var feature = settings.GetFeature(featureType);
                if (feature != null && feature.enabled)
                {
                    if (!lifecycleFeature.enabled)
                    {
                        Debug.Log($"Enabling {UiName}.");
                    }

                    lifecycleFeature.enabled = true;
                    return;
                }
            }

            if (lifecycleFeature.enabled)
            {
                Debug.Log($"Disabling {UiName}.");
            }

            lifecycleFeature.enabled = false;
        }
#endif // UNITY_EDITOR

        private bool NativeContextInNeed()
        {
            var features = OpenXRSettings.Instance.GetFeatures<AndroidXRSpatialEntityFeature>();
            foreach (var feature in features)
            {
                var axrSpatialEntity = feature as AndroidXRSpatialEntityFeature;
                if (axrSpatialEntity != null &&
                    axrSpatialEntity.enabled && axrSpatialEntity._useNativeContext)
                {
                    return true;
                }
            }

            return false;
        }

        private void TogglePollEvent(
            bool subscribe, XrStructureType eventType, XrPollEventCallback callback)
        {
            if (eventType == _discoveryEvent && subscribe == _discoveryEventSubscribed)
            {
                return;
            }

            unsafe
            {
                var result = subscribe ?
                    PollEventRouter.TrySubscribeToEventType(eventType, callback) :
                    PollEventRouter.TryUnsubscribeFromEventType(eventType, callback);
                if (!result)
                {
                    Debug.LogErrorFormat("{0} failed on {1}.",
                        subscribe ? "TrySubscribeToEventType" : "TryUnsubscribeFromEventType",
                        eventType);
                }
                else if (eventType == _discoveryEvent)
                {
                    _discoveryEventSubscribed = subscribe;
                }
            }
        }

        private unsafe void OnDiscoveryRecommended(XrEventDataBaseHeader* baseHeader)
        {
            var eventData = *(XrEventDataSpatialDiscoveryRecommendedEXT*)baseHeader;
            if (eventData.type != _discoveryEvent)
            {
                return;
            }

            if (_discoveryCallbacks.TryGetValue(
                eventData.spatialContext, out var callback))
            {
                callback.Invoke(eventData.spatialContext);
            }
        }
    }
}
