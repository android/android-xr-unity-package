// <copyright file="SpatialBoundingBoxSubsystem.cs" company="Google LLC">
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
    using Google.XR.Extensions.SpatialEntities.Internal;
    using Unity.Collections;
    using UnityEngine;
    using UnityEngine.Scripting;
    using UnityEngine.XR.ARSubsystems;
    using UnityEngine.XR.OpenXR;

    using XrSpatialContextEXT = System.UInt64;
    using XrSpatialSnapshotEXT = System.UInt64;

    /// <summary>
    /// The spatial entity implementation of the <see cref="XRBoundingBoxSubsystem"/>.
    /// </summary>
    [Preserve]
    public sealed class SpatialBoundingBoxSubsystem : XRBoundingBoxSubsystem
    {
        internal const string _subsystemId = "AndroidXR-SpatialEntity-BoundingBox";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterDescriptor()
        {
            XRBoundingBoxSubsystemDescriptor.Cinfo cinfo =
                new XRBoundingBoxSubsystemDescriptor.Cinfo()
                {
                    id = _subsystemId,
                    providerType = typeof(SpatialEntityProvider),
                    subsystemTypeOverride = typeof(SpatialBoundingBoxSubsystem),
                    supportsClassification = true
                };

            XRBoundingBoxSubsystemDescriptor.Register(cinfo);
        }

        internal class SpatialEntityProvider : Provider
        {
            private XRBoundingBoxBuilder _trackableBuilder = new XRBoundingBoxBuilder();
            private SpatialBoundingBoxProducer _dataProducer = null;

            public void OnSpatialContextCreated(
                XrSpatialContextEXT context, SpatialObjectConfigurator configurator)
            {
                _trackableBuilder.UpdateActiveLabels(configurator.ActiveLabels);
                _dataProducer.UpdateContext(context, configurator.EnabledComponents);
            }

            public void OnSpatialContextDestroy(XrSpatialContextEXT context)
            {
                _trackableBuilder.UpdateActiveLabels(null);
                _dataProducer.UpdateContext(0, null);
                _dataProducer.RemoveAllEntities();
            }

            public void OnDiscoverySnapshotCreated(
                XrSpatialContextEXT context, XrSpatialSnapshotEXT snapshot)
            {
                _dataProducer.QuerySnapshot(snapshot);
            }

            public override TrackableChanges<XRBoundingBox> GetChanges(
                XRBoundingBox defaultXRBoundingBox, Allocator allocator)
            {
                _dataProducer.UpdateTrackableData();
                var added = new NativeArray<XRBoundingBox>(
                    _dataProducer.Added.Count, allocator, NativeArrayOptions.UninitializedMemory);
                var updated = new NativeArray<XRBoundingBox>(
                    _dataProducer.Updated.Count, allocator, NativeArrayOptions.UninitializedMemory);
                var removed = new NativeArray<TrackableId>(
                    _dataProducer.Removed.Count, allocator, NativeArrayOptions.UninitializedMemory);

                int i = 0;
                foreach (var id in _dataProducer.Added)
                {
                    added[i++] = _dataProducer.GetTrackable(id);
                }

                i = 0;
                foreach (var id in _dataProducer.Updated)
                {
                    updated[i++] = _dataProducer.GetTrackable(id);
                }

                i = 0;
                foreach (var id in _dataProducer.Removed)
                {
                    removed[i++] = id;
                }

                _dataProducer.ClearChanges();

                return new TrackableChanges<XRBoundingBox>(added, updated, removed);
            }

            public override void Start()
            {
                // Try to create spatial context if not yet presents.
                // It's used to wait and grant runtime permissions.
                var feature = OpenXRSettings.Instance.GetFeature<SpatialBoundingBoxFeature>();
                if (feature != null)
                {
                    feature.OnSpatialSubsystemStart();
                }
            }

            public override void Stop()
            {
                var feature = OpenXRSettings.Instance.GetFeature<SpatialBoundingBoxFeature>();
                if (feature != null)
                {
                    feature.OnSpatialSubsystemStop();
                }
            }

            public override void Destroy()
            {
                // Native resources should have been reset by Stop().
                // Calling RemoveAllEntities() again for safety.
                _dataProducer.RemoveAllEntities();
                _dataProducer.Dispose();
            }

            protected override bool TryInitialize()
            {
                // Triggered by SubsystemDescriptorWithProvider.CreateProvider()
                // that ensures a valid provider for spatial context callbacks.
                _dataProducer = new SpatialBoundingBoxProducer(_trackableBuilder);
                return true;
            }
        }

        /// <inheritdoc/>
        private sealed class SpatialBoundingBoxProducer :
            SpatialDataProducer<XRBoundingBox, XRBoundingBoxBuilder>
        {
            // Inherited class to simplify constructor.
            public SpatialBoundingBoxProducer(XRBoundingBoxBuilder builder) : base(builder)
            {
            }

            public SpatialBoundingBoxProducer() : this(new XRBoundingBoxBuilder())
            {
            }
        }
    }
}
