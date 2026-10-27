// <copyright file="SpatialContextSupport.cs" company="Google LLC">
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

namespace Google.XR.Extensions.SpatialEntities.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Unity.Collections;
    using UnityEngine;
    using UnityEngine.XR.OpenXR;
    using UnityEngine.XR.OpenXR.Features;
    using UnityEngine.XR.OpenXR.NativeTypes;

    /// <summary>
    /// Enumerates system capabilities supported by spatial entities.
    /// </summary>
    internal class SpatialContextSupport
    {
        private HashSet<SpatialCapability> _capabilities =
            new HashSet<SpatialCapability>();

        private HashSet<XrSpatialPersistenceScopeEXT> _persistenceScopes =
            new HashSet<XrSpatialPersistenceScopeEXT>();

        private OpenXRResultStatus _status = new OpenXRResultStatus(
            OpenXRResultStatus.StatusCode.ProviderUninitialized);

        /// <summary>
        /// Gets the global instance of spatial context support,
        /// initialized and updated in <see cref="OpenXRFeature.OnSystemChange(ulong)"/>
        /// via <see cref="SpatialLifecycleFeature"/>.
        /// <see cref="Initialized"/> is required for checking support status.
        /// </summary>
        public static SpatialContextSupport Instance { get; private set; }

        public static bool Initialized => Instance != null &&
            (Instance._status.IsSuccess() || Instance._status.IsUnsupported());

        public ulong XrInstance { get; private set; }

        public ulong XrSystemId { get; private set; }

        /// <summary>
        /// Initializes instance for the given OpenXR system.
        /// </summary>
        /// <param name="xrInstance">The OpenXR XrInstance handle.</param>
        /// <param name="xrSystem">The OpenXR XrSystemId handle.</param>
        public static void Initialize(ulong xrInstance, ulong xrSystem)
        {
            if (Instance != null &&
                Instance.XrInstance == xrInstance && Instance.XrSystemId == xrSystem)
            {
                // Repeated calls for the same system.
                return;
            }

            Instance = new SpatialContextSupport
            {
                XrInstance = xrInstance,
                XrSystemId = xrSystem,
            };

            if (Instance.TryEnumerateSupport())
            {
                Debug.Log(Instance.ToString());
            }
            else
            {
                Debug.LogError(Instance.ToString());
            }
        }

        /// <summary>
        /// Reset current instance on instance destroy.
        /// </summary>
        public static void Reset()
        {
            // In case the instance is still used by other features,
            // reset support status other than destroy instance.
            Instance = new SpatialContextSupport
            {
                XrInstance = 0,
                XrSystemId = 0,
            };
        }

        public static bool IsExtensionEnabled() =>
            Instance != null && Instance._status.IsSuccess();

        public static bool IsSupported(XrSpatialCapabilityEXT capability) =>
            Initialized && Instance._capabilities.Any(cap => cap._capability == capability);

        public static bool IsSupported(
            XrSpatialCapabilityEXT capability, XrSpatialComponentTypeEXT component) =>
            Initialized && Instance._capabilities.FirstOrDefault(
                cap => cap._capability == capability).IsSupported(component);

        public static bool IsSupported(
            XrSpatialCapabilityEXT capability, XrSpatialCapabilityFeatureEXT feature) =>
            Initialized && Instance._capabilities.FirstOrDefault(
                cap => cap._capability == capability).IsSupported(feature);

        public static bool IsSupported(XrSpatialPersistenceScopeEXT scope) =>
            Initialized && Instance._persistenceScopes.Contains(scope);

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format(
                "{0}: XrInstance ({1}), XrSystem ({2}), Initialized ({3}).\n" +
                "{4}\n",
                GetType().Name, XrInstance, XrSystemId, Initialized,
                DebugSupportStatus());
        }

        private string DebugSupportStatus()
        {
            if (_status.IsError())
            {
                return _status.ToString();
            }

            StringBuilder builder = new StringBuilder();

            builder.AppendFormat("Capabilities ({0}):\n", _capabilities.Count);
            foreach (var capability in _capabilities)
            {
                builder.AppendFormat(" | {0}\n", capability);
            }

            builder.AppendFormat("PersistenceScopes ({0}):\n", _persistenceScopes.Count);
            foreach (var scope in _persistenceScopes)
            {
                builder.AppendFormat("  | {0}\n", scope.GetDisplayName());
            }

            return builder.ToString();
        }

        private bool TryEnumerateSupport()
        {
            if (XrInstance == 0 || XrSystemId == 0)
            {
                _status = new OpenXRResultStatus(
                    OpenXRResultStatus.StatusCode.ProviderUninitialized);
                return false;
            }

            if (!OpenXRRuntime.IsExtensionEnabled(ApiConstants.EXT_spatial_entity))
            {
                _status = new OpenXRResultStatus(
                    OpenXRResultStatus.StatusCode.Unsupported);
                return false;
            }

            _status = OpenXRNativeApi.xrEnumerateSpatialCapabilitiesEXT(
                XrInstance, XrSystemId, Allocator.Temp, out var capabilities).ToStatus();
            if (_status.IsError())
            {
                return false;
            }

            var supportedCapabilities = new HashSet<SpatialCapability>();
            foreach (var cap in capabilities)
            {
                _status = OpenXRNativeApi.xrEnumerateSpatialCapabilityComponentTypesEXT(
                    XrInstance, XrSystemId, cap, Allocator.Temp, out var types).ToStatus();
                if (_status.IsError())
                {
                    return false;
                }

                _status = OpenXRNativeApi.xrEnumerateSpatialCapabilityFeaturesEXT(
                    XrInstance, XrSystemId, cap, Allocator.Temp, out var features).ToStatus();
                if (_status.IsError())
                {
                    return false;
                }

                supportedCapabilities.Add(new SpatialCapability(cap, types, features));
            }

            var supportedScopes = new HashSet<XrSpatialPersistenceScopeEXT>();
            if (OpenXRRuntime.IsExtensionEnabled(ApiConstants.EXT_spatial_persistence))
            {
                _status = OpenXRNativeApi.xrEnumerateSpatialPersistenceScopesEXT(
                    XrInstance, XrSystemId, Allocator.Temp, out var scopes).ToStatus();
                if (_status.IsError())
                {
                    return false;
                }

                foreach (var scope in scopes)
                {
                    supportedScopes.Add(scope);
                }
            }

            _capabilities = supportedCapabilities;
            _persistenceScopes = supportedScopes;
            return true;
        }

        /// <summary>
        /// A <see cref="XrSpatialCapabilityEXT"/> with system properties.
        /// </summary>
        public struct SpatialCapability : IEquatable<SpatialCapability>
        {
            internal readonly XrSpatialCapabilityEXT _capability;
            internal readonly HashSet<XrSpatialComponentTypeEXT> _components;
            internal readonly HashSet<XrSpatialCapabilityFeatureEXT> _features;

            /// <summary>
            /// Constructs with enumerated results from OpenXR runtimes.
            /// </summary>
            /// <param name="capability">
            /// A <see cref="XrSpatialCapabilityEXT"/> type enumerated by
            /// <c>xrEnumerateSpatialCapabilitiesEXT</c>.
            /// </param>
            /// <param name="components">
            /// The <see cref="XrSpatialComponentTypeEXT"/> list populated by
            /// <c>xrEnumerateSpatialCapabilityComponentTypesEXT</c>.
            /// </param>
            /// <param name="features">
            /// The <see cref="XrSpatialCapabilityFeatureEXT"/> list populated by
            /// <c>xrEnumerateSpatialCapabilityFeaturesEXT</c>.
            /// </param>
            public SpatialCapability(
                XrSpatialCapabilityEXT capability,
                IEnumerable<XrSpatialComponentTypeEXT> components,
                IEnumerable<XrSpatialCapabilityFeatureEXT> features)
            {
                _capability = capability;
                _components = new HashSet<XrSpatialComponentTypeEXT>();
                _features = new HashSet<XrSpatialCapabilityFeatureEXT>();

                foreach (var component in components)
                {
                    // Add by element to avoid duplicates.
                    _components.Add(component);
                }

                foreach (var feature in features)
                {
                    // Add by element to avoid duplicates.
                    _features.Add(feature);
                }
            }

            public bool IsSupported(XrSpatialComponentTypeEXT component) =>
                _components != null && _components.Contains(component);

            public bool IsSupported(XrSpatialCapabilityFeatureEXT feature) =>
                _features != null && _features.Contains(feature);

            public bool Equals(SpatialCapability other)
            {
                // Check value equality instead of reference.
                bool componentEq = (_components == null && other._components == null) ||
                    (_components != null && other._components != null &&
                        _components.SetEquals(other._components));
                bool featureEq = (_features == null && other._features == null) ||
                    (_features != null && other._features != null &&
                        _features.SetEquals(other._features));
                return _capability == other._capability && componentEq && featureEq;
            }

            public override string ToString()
            {
                string componentStr = (_components == null || _components.Count == 0) ?
                    "0" : string.Join(",", _components.Select(c => c.GetDisplayName()));
                string featureStr = (_features == null || _features.Count == 0) ?
                    "0" : string.Join(",", _features);
                return string.Format("{0}: Components ({1}), Features ({2})",
                    _capability.GetDisplayName(), componentStr, featureStr);
            }
        }
    }
}
