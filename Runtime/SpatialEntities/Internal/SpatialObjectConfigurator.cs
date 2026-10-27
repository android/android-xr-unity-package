// <copyright file="SpatialObjectConfigurator.cs" company="Google LLC">
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
    using Unity.Collections;
    using UnityEngine.XR.OpenXR.NativeTypes;

    /// <inheritdoc/>
    internal class SpatialObjectConfigurator :
        SpatialConfigurator<XrSpatialCapabilityConfigurationObjectTracking>
    {
        // Configuration fields.
        private HashSet<XrSpatialObjectSemanticLabel> _activeLabels =
            new HashSet<XrSpatialObjectSemanticLabel>();

        private NativeArray<XrSpatialObjectSemanticLabel> _activeLabelArray;

        public SpatialObjectConfigurator(
            XrSpatialCapabilityEXT capability,
            IEnumerable<XrSpatialComponentTypeEXT> requestedComponents,
            IEnumerable<XrSpatialObjectSemanticLabel> requestedLabels) :
            base(capability, requestedComponents)
        {
            if (requestedLabels != null)
            {
                foreach (var label in requestedLabels)
                {
                    if (Enum.IsDefined(typeof(XrSpatialObjectSemanticLabel), label))
                    {
                        _activeLabels.Add(label);
                    }
                }
            }

            _activeLabelArray = new NativeArray<XrSpatialObjectSemanticLabel>(
                _activeLabels.Count, Allocator.Persistent);
            int index = 0;
            foreach (var label in _activeLabels)
            {
                _activeLabelArray[index++] = label;
            }
        }

        public HashSet<XrSpatialObjectSemanticLabel> ActiveLabels => _activeLabels;

        public override bool TryCreateConfiguration(
            out XrSpatialCapabilityConfigurationObjectTracking config)
        {
            if (_enabledComponents.Count == 0)
            {
                config = default;
                return false;
            }

            // To avoid re-creation on changes of detection modes,
            // action labels are only applied to XRBoundingBoxBuilder,
            // where the spatial context always detects all supported types.
            config = new XrSpatialCapabilityConfigurationObjectTracking(_enabledComponentArray);
            return true;
        }

        public override void Dispose()
        {
            base.Dispose();
            if (_activeLabelArray.IsCreated)
            {
                _activeLabelArray.Dispose();
            }
        }
    }
}
