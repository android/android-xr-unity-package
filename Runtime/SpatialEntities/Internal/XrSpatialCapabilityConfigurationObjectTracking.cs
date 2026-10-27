// <copyright file="XrSpatialCapabilityConfigurationObjectTracking.cs" company="Google LLC">
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
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using UnityEngine.Assertions;
    using UnityEngine.XR.OpenXR.NativeTypes;

    /// <summary>
    /// Native wrapper of OpenXR struct XrSpatialCapabilityConfigurationObjectTrackingANDROID.
    /// </summary>
    [SuppressMessage(
        "UnityRules.UnityStyleRules", "US1109:PublicPropertiesMustBeUpperCamelCase",
        Justification = "Matching interface.")]
    [StructLayout(LayoutKind.Sequential)]
    internal struct XrSpatialCapabilityConfigurationObjectTracking
        : ISpatialCapabilityConfiguration
    {
        public unsafe XrSpatialCapabilityConfigurationObjectTracking(
            void* next,
            uint componentCount,
            XrSpatialComponentTypeEXT* components,
            uint semanticLabelCount,
            XrSpatialObjectSemanticLabel* semanticLabels)
        {
            Assert.IsTrue(componentCount != 0);
            Assert.IsTrue(components != null);

            type = XrStructureTypeAndroid.SpatialCapabilityConfigurationObjectTracking.ToExt();
            this.next = next;
            capability = XrSpatialCapabilityAndroid.Object.ToExt();
            enabledComponentCount = componentCount;
            enabledComponents = components;

            // When semantic label is not set,
            // the runtime will enable all supported types by default.
            activeSemanticLabelCount = semanticLabelCount;
            activeSemanticLabels = semanticLabels;
        }

        public unsafe XrSpatialCapabilityConfigurationObjectTracking(
            NativeArray<XrSpatialComponentTypeEXT> components,
            NativeArray<XrSpatialObjectSemanticLabel> semanticLabels) :
            this(
                null,
                (uint)components.Length,
                (XrSpatialComponentTypeEXT*)components.GetUnsafePtr(),
                (uint)semanticLabels.Length,
                (XrSpatialObjectSemanticLabel*)semanticLabels.GetUnsafePtr())
        {
        }

        public unsafe XrSpatialCapabilityConfigurationObjectTracking(
            NativeArray<XrSpatialComponentTypeEXT> components)
            : this(
                null,
                (uint)components.Length,
                (XrSpatialComponentTypeEXT*)components.GetUnsafePtr(),
                0,
                null)
        {
        }

        public XrStructureType type { get; }

        public unsafe void* next { get; }

        public XrSpatialCapabilityEXT capability { get; }

        public uint enabledComponentCount { get; }

        public unsafe XrSpatialComponentTypeEXT* enabledComponents { get; }

        public uint activeSemanticLabelCount { get; }

        public unsafe XrSpatialObjectSemanticLabel* activeSemanticLabels { get; }
    }
}
