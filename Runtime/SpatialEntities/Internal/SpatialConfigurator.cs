// <copyright file="SpatialConfigurator.cs" company="Google LLC">
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
    using Unity.Collections;
    using UnityEngine.XR.OpenXR.NativeTypes;

    /// <summary>
    /// Stores all necessary native resources required to construct OpenXR
    /// configurations, and provides utilities around enabled components.
    /// </summary>
    /// <typeparam name="TConfiguration">
    /// The derived <see cref="ISpatialCapabilityConfiguration"/>
    /// </typeparam>
    /// <remarks>
    /// Must call <see cref="SpatialConfigurator{T}.Dispose()"/> to release
    /// native resources after use.
    /// </remarks>
    internal abstract class SpatialConfigurator<TConfiguration> : IDisposable
        where TConfiguration : ISpatialCapabilityConfiguration
    {
        protected HashSet<XrSpatialComponentTypeEXT> _enabledComponents =
            new HashSet<XrSpatialComponentTypeEXT>();

        protected NativeArray<XrSpatialComponentTypeEXT> _enabledComponentArray;

        public SpatialConfigurator(
            XrSpatialCapabilityEXT capability,
            IEnumerable<XrSpatialComponentTypeEXT> requestedComponents)
        {
            if (requestedComponents != null)
            {
                List<string> componentNames = new List<string>();
                foreach (var component in requestedComponents)
                {
                    if (SpatialContextSupport.IsExtensionEnabled() &&
                        SpatialContextSupport.IsSupported(capability, component))
                    {
                        _enabledComponents.Add(component);
                        componentNames.Add(component.GetDisplayName());
                    }
                }

                if (componentNames.Any())
                {
                    UnityEngine.Debug.LogFormat(
                        "Configure spatial context with components: {0}",
                        string.Join(",", componentNames));
                }
            }

            _enabledComponentArray = new NativeArray<XrSpatialComponentTypeEXT>(
                _enabledComponents.Count, Allocator.Persistent);
            int index = 0;
            foreach (var component in _enabledComponents)
            {
                _enabledComponentArray[index++] = component;
            }
        }

        public HashSet<XrSpatialComponentTypeEXT> EnabledComponents => _enabledComponents;

        public NativeArray<XrSpatialComponentTypeEXT> ComponentNativeArray =>
            _enabledComponentArray;

        public abstract bool TryCreateConfiguration(out TConfiguration config);

        public virtual void Dispose()
        {
            if (_enabledComponentArray.IsCreated)
            {
                _enabledComponentArray.Dispose();
            }
        }
    }
}
