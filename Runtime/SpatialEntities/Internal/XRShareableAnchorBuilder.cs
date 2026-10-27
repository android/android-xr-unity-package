// <copyright file="XRShareableAnchorBuilder.cs" company="Google LLC">
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
    using System.Collections.Generic;
    using Google.XR.Extensions.ARSubsystems;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using UnityEngine.XR.ARSubsystems;
    using UnityEngine.XR.OpenXR.NativeTypes;

    using XrSpatialEntityIdEXT = System.UInt64;

    /// <inheritdoc/>
    internal class XRShareableAnchorBuilder : SpatialTrackableBuilder<XRShareableAnchor>
    {
        private static XrSpatialComponentTypeEXT _anchorComponentType =
            XrSpatialComponentTypeEXT.Anchor;

        private HashSet<XrSpatialComponentTypeEXT> _requiredComponents =
            new HashSet<XrSpatialComponentTypeEXT>();

        private HashSet<XrSpatialComponentTypeEXT> _optionalComponents =
            new HashSet<XrSpatialComponentTypeEXT>();

        private NativeArray<XrPosef> _poseData;
        private unsafe XrSpatialComponentAnchorListEXT* _anchorListPtr = null;

        private NativeArray<XrSpatialPersistenceDataEXT> _persistenceData;

        private unsafe XrSpatialComponentPersistenceListEXT* _persistenceListPtr = null;

        private Dictionary<XrSpatialEntityIdEXT, int> _entityIdToPersistenceIndex =
            new Dictionary<XrSpatialEntityIdEXT, int>();

        // Unique fields to construct an XRShareableAnchor
        private XRShareableAnchorHosts _host;
        private SerializableGuid _shareableId;

        /// <inheritdoc/>
        public override HashSet<XrSpatialComponentTypeEXT> PrimaryComponents =>
            _requiredComponents;

        /// <inheritdoc/>
        public override HashSet<XrSpatialComponentTypeEXT> OptionalComponents =>
            _optionalComponents;

        /// <inheritdoc/>
        public override void ApplyComponents(
            IEnumerable<XrSpatialComponentTypeEXT> enabledComponents)
        {
            base.ApplyComponents(enabledComponents);

            _requiredComponents.Clear();
            _requiredComponents.Add(_anchorComponentType);

            _optionalComponents.Clear();
            foreach (var component in _enabledComponents)
            {
                if (component != _anchorComponentType)
                {
                    _optionalComponents.Add(component);
                }
            }
        }

        /// <inheritdoc/>
        public override unsafe void* CreatePrimaryQueryChainPtr(int count)
        {
            _poseData = new NativeArray<XrPosef>(count, Allocator.Temp);
            SpatialTypeUtils.UnsafeAlloc(out _anchorListPtr);
            *_anchorListPtr = new XrSpatialComponentAnchorListEXT(_poseData);

            return _anchorListPtr;
        }

        /// <inheritdoc/>
        public override unsafe void* CreateOptionalQueryChainPtr(
            XrSpatialComponentTypeEXT component, int count)
        {
            void* next = null;
            if (component == XrSpatialComponentTypeEXT.Persistence)
            {
                _persistenceData =
                    new NativeArray<XrSpatialPersistenceDataEXT>(count, Allocator.Temp);
                SpatialTypeUtils.UnsafeAlloc(out _persistenceListPtr);

                *_persistenceListPtr =
                    new XrSpatialComponentPersistenceListEXT(next, _persistenceData);
                next = _persistenceListPtr;
            }

            return next;
        }

        /// <inheritdoc/>
        public override void SetOptionalComponentByEntityId(
            XrSpatialComponentTypeEXT component, int index, XrSpatialEntityIdEXT entityId)
        {
            if (component == XrSpatialComponentTypeEXT.Persistence &&
                ValidateIndex(index, _persistenceData))
            {
                _entityIdToPersistenceIndex[entityId] = index;
            }
        }

        /// <inheritdoc/>
        public override unsafe void ResetQueryResult()
        {
            _entityIdToPersistenceIndex.Clear();
            if (_anchorListPtr != null)
            {
                UnsafeUtility.Free(_anchorListPtr, Allocator.Temp);
                _anchorListPtr = null;
            }

            if (_poseData.IsCreated)
            {
                _poseData.Dispose();
            }

            if (_persistenceListPtr != null)
            {
                UnsafeUtility.Free(_persistenceListPtr, Allocator.Temp);
                _persistenceListPtr = null;
            }

            if (_persistenceData.IsCreated)
            {
                _persistenceData.Dispose();
            }
        }

        /// <inheritdoc/>
        public override void FillPrimaryComponent(XrSpatialComponentTypeEXT component, int index)
        {
            switch ((int)component)
            {
                case (int)XrSpatialComponentTypeEXT.Anchor:
                    var pose = ValidateIndex(index, _poseData) ? _poseData[index] : default;
                    _pose = pose.ToPose();
                    break;
                default:
                    return;
            }
        }

        /// <inheritdoc/>
        public override void FillOptionalComponent(
            XrSpatialComponentTypeEXT component, XrSpatialEntityIdEXT entityId)
        {
            switch ((int)component)
            {
                case (int)XrSpatialComponentTypeEXT.Persistence:
                    if (_entityIdToPersistenceIndex.TryGetValue(entityId, out var index))
                    {
                        var persistence = _persistenceData[index];
                        if (persistence.persistState == XrSpatialPersistenceStateEXT.Loaded)
                        {
                            _shareableId = persistence.persistUuid.ToSerializableGuid();

                            // Currently, only local host is supported.
                            _host = XRShareableAnchorHosts.Local;
                        }
                    }

                    break;
                default:
                    return;
            }
        }

        /// <inheritdoc/>
        public override void FromData(
            SpatialTrackableData<XRShareableAnchor> data,
            XrSpatialEntityTrackingStateEXT state)
        {
            FillEntity(data.Context, data.Id, data.Handle);
            FillState(state);
            _pose = data.Trackable.pose;
            _host = data.Trackable.host;
            _shareableId = data.Trackable.shareableId;
        }

        /// <inheritdoc/>
        public override void ResetData()
        {
            base.ResetData();
            _host = XRShareableAnchorHosts.None;
            _shareableId = SerializableGuid.empty;
        }

        /// <inheritdoc/>
        public override XRShareableAnchor Build()
        {
            return new XRShareableAnchor(
                _trackableId,
                _pose,
                _trackingState,
                _nativePtr,
                _parentId,
                _host,
                _shareableId);
        }
    }
}
