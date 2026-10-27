// <copyright file="XRBoundingBoxBuilder.cs" company="Google LLC">
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
    using System.Linq;
    using Google.XR.Extensions.SpatialEntities.Internal;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using UnityEngine;
    using UnityEngine.XR.ARSubsystems;
    using UnityEngine.XR.OpenXR.NativeTypes;

    /// <inheritdoc/>
    internal sealed class XRBoundingBoxBuilder : SpatialTrackableBuilder<XRBoundingBox>
    {
        private static readonly XrStructureType _labelListType =
            XrStructureTypeAndroid.SpatialComponentObjectSemanticLabelList.ToExt();

        // Component native data.
        // Allocated with Allocator.Temp so it can be available for the current frame.
        // Dispose() is optional.
        private NativeArray<XrBoxf> _bound3dData;
        private unsafe XrSpatialComponentBounded3DListEXT* _bound3dListPtr = null;

        private NativeArray<XrSpatialObjectSemanticLabel> _labelData;
        private unsafe SpatialComponentList<XrSpatialObjectSemanticLabel>* _labelListPtr = null;

        // Unique fields to construct an XRBoundingBox.
        private Vector3 _size;
        private BoundingBoxClassifications _classifications;

        private HashSet<BoundingBoxClassifications> _filterMask =
            new HashSet<BoundingBoxClassifications>();

        public void UpdateActiveLabels(HashSet<XrSpatialObjectSemanticLabel> activeLabels)
        {
            _filterMask.Clear();
            if (activeLabels == null || !activeLabels.Any())
            {
                return;
            }

            foreach (var label in activeLabels)
            {
                _filterMask.Add(label.ToClassifications());
            }
        }

        /// <inheritdoc/>
        public override unsafe void* CreatePrimaryQueryChainPtr(int count)
        {
            void* next = null;
            if (_enabledComponents.Contains(XrSpatialComponentTypeEXT.Bounded3D))
            {
                _bound3dData = new NativeArray<XrBoxf>(count, Allocator.Temp);
                SpatialTypeUtils.UnsafeAlloc(out _bound3dListPtr);
                *_bound3dListPtr = new XrSpatialComponentBounded3DListEXT(next, _bound3dData);
                next = _bound3dListPtr;
            }

            if (_enabledComponents.Contains(
                XrSpatialComponentTypeAndroid.ObjectSemanticLabel.ToExt()))
            {
                _labelData = new NativeArray<XrSpatialObjectSemanticLabel>(count, Allocator.Temp);
                SpatialTypeUtils.UnsafeAlloc(out _labelListPtr);
                *_labelListPtr = new SpatialComponentList<XrSpatialObjectSemanticLabel>(
                    _labelListType, next, _labelData);
                next = _labelListPtr;
            }

            return next;
        }

        /// <inheritdoc/>
        public unsafe override void ResetQueryResult()
        {
            if (_bound3dData.IsCreated)
            {
                _bound3dData.Dispose();
            }

            if (_bound3dListPtr != null)
            {
                UnsafeUtility.Free(_bound3dListPtr, Allocator.Temp);
                _bound3dListPtr = null;
            }

            if (_labelData.IsCreated)
            {
                _labelData.Dispose();
            }

            if (_labelListPtr != null)
            {
                UnsafeUtility.Free(_labelListPtr, Allocator.Temp);
                _labelListPtr = null;
            }
        }

        /// <inheritdoc/>
        public override void FillPrimaryComponent(XrSpatialComponentTypeEXT component, int index)
        {
            switch ((int)component)
            {
                case (int)XrSpatialComponentTypeEXT.Bounded3D:
                    var bound3d = ValidateIndex(index, _bound3dData) ?
                        _bound3dData[index] : new XrBoxf();
                    _pose = bound3d.center.ToPose();
                    _size = bound3d.extents.ToVector3();
                    break;
                case (int)XrSpatialComponentTypeAndroid.ObjectSemanticLabel:
                    var label = ValidateIndex(index, _labelData) ?
                        _labelData[index] : XrSpatialObjectSemanticLabel.Uncategorized;
                    _classifications = label.ToClassifications();
                    break;
                default:
                    return;
            }
        }

        /// <inheritdoc/>
        public override void FromData(
            SpatialTrackableData<XRBoundingBox> data, XrSpatialEntityTrackingStateEXT state)
        {
            FillEntity(data.Context, data.Id, data.Handle);
            FillState(state);
            _pose = data.Trackable.pose;
            _size = data.Trackable.size;
            _classifications = data.Trackable.classifications;
        }

        /// <inheritdoc/>
        public override void ResetData()
        {
            base.ResetData();
            _size = Vector3.zero;
            _classifications = BoundingBoxClassifications.None;
        }

        /// <inheritdoc/>
        public override XRBoundingBox Build()
        {
            return new XRBoundingBox(
                _trackableId,
                _pose,
                _size,
                _trackingState,
                _classifications,
                _nativePtr);
        }

        /// <inheritdoc/>
        public override bool TryFilterOut(XRBoundingBox boundingBox)
        {
            return _filterMask.Any() && !_filterMask.Contains(boundingBox.classifications);
        }
    }
}
