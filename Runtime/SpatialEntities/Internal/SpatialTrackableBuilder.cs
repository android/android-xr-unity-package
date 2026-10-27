// <copyright file="SpatialTrackableBuilder.cs" company="Google LLC">
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
    using UnityEngine;
    using UnityEngine.XR.ARSubsystems;
    using UnityEngine.XR.OpenXR.NativeTypes;

    using XrSpatialContextEXT = System.UInt64;
    using XrSpatialEntityEXT = System.UInt64;
    using XrSpatialEntityIdEXT = System.UInt64;

    /// <summary>
    /// Store query results needed to construct a trackable object.
    /// </summary>
    /// <typeparam name="TTrackable">The target <see cref="ITrackable"/> type.
    /// </typeparam>
    internal abstract class SpatialTrackableBuilder<TTrackable>
        where TTrackable : struct, ITrackable, IEquatable<TTrackable>
    {
        /// <summary>
        /// Components enabled by the given spatial context and appended to
        /// snapshot creations.
        /// </summary>
        protected HashSet<XrSpatialComponentTypeEXT> _enabledComponents =
            new HashSet<XrSpatialComponentTypeEXT>();

        // Spatial entity info.
        protected XrSpatialContextEXT _contextHandle;
        protected XrSpatialEntityIdEXT _entityId;
        protected XrSpatialEntityEXT _entityHandle;
        protected XrSpatialEntityTrackingStateEXT _entityState;

        // Common fields from ITrackable.
        protected TrackableId _trackableId;
        protected Pose _pose;
        protected TrackingState _trackingState;
        protected IntPtr _nativePtr;
        protected TrackableId _parentId;

        /// <summary>
        /// Gets a list of required components which can represent the trackable
        /// type. Defaults to all enabled components.
        /// It matches the data lists constructed by
        /// <see cref="CreatePrimaryQueryChainPtr"/>.
        /// </summary>
        public virtual HashSet<XrSpatialComponentTypeEXT> PrimaryComponents => _enabledComponents;

        /// <summary>
        /// Gets a list of optional components which might be provided by the
        /// trackable type.
        /// It's used to iterate <see cref="CreateOptionalQueryChainPtr"/> and
        /// fill in optional data.
        /// </summary>
        public virtual HashSet<XrSpatialComponentTypeEXT> OptionalComponents => null;

        /// <summary>
        /// Applies components for snapshot queries. It will then be used to
        /// construct <see cref="CreatePrimaryQueryChainPtr"/> and
        /// <see cref="CreateOptionalQueryChainPtr"/>
        /// </summary>
        /// <param name="enabledComponents">
        /// The enabled components for snapshot queries.</param>
        public virtual void ApplyComponents(
            IEnumerable<XrSpatialComponentTypeEXT> enabledComponents)
        {
            _enabledComponents.Clear();
            if (enabledComponents == null || !enabledComponents.Any())
            {
                return;
            }

            foreach (var component in enabledComponents)
            {
                _enabledComponents.Add(component);
            }
        }

        /// <summary>
        /// Used to query required components, called once per snapshot.
        /// Entities without these components will be filtered out.
        /// If it's null, query result will only return <see cref="XrSpatialEntityIdEXT"/>
        /// and <see cref="XrSpatialEntityTrackingStateEXT"/>.
        /// </summary>
        /// <param name="count">The count of entities.</param>
        /// <returns>The pointer to the allocated component list structure.</returns>
        public virtual unsafe void* CreatePrimaryQueryChainPtr(int count) => null;

        /// <summary>
        /// Used to query optional components. It might be called multiple times
        /// per snapshot, depending on the amount of optional components.
        /// </summary>
        /// <param name="component">The component type.</param>
        /// <param name="count">The count of entities.</param>
        /// <returns>The pointer to the allocated component list structure.</returns>
        public virtual unsafe void* CreateOptionalQueryChainPtr(
            XrSpatialComponentTypeEXT component, int count) => null;

        /// <summary>
        /// Sets the optional component data with the associated entity id.
        /// </summary>
        /// <param name="component">The component type.</param>
        /// <param name="index">The index from the optional query result.</param>
        /// <param name="entityId">The associated entity id.</param>
        public virtual void SetOptionalComponentByEntityId(
            XrSpatialComponentTypeEXT component, int index, XrSpatialEntityIdEXT entityId)
        {
        }

        /// <summary>
        /// Optional for clearing query result's native resources.
        /// </summary>
        public virtual void ResetQueryResult()
        {
        }

        /// <summary>
        /// Fill builder with basic spatial entity data,
        /// and assign <see cref="ITrackable.trackableId"/>.
        /// </summary>
        /// <param name="context">The context which owns this entity.</param>
        /// <param name="entityId">The spatial entity id.</param>
        /// <param name="entity">The spatial entity handle.</param>
        public void FillEntity(
            XrSpatialContextEXT context, XrSpatialEntityIdEXT entityId, XrSpatialEntityEXT entity)
        {
            _contextHandle = context;
            _entityId = entityId;
            _entityHandle = entity;

            _trackableId = SpatialTypeUtils.ConstructTrackableId(context, entityId);
            _nativePtr = (IntPtr)_entityHandle;
        }

        /// <summary>
        /// Fill builder with spatial tracking state,
        /// and assign <see cref="ITrackable.trackingState"/>.
        /// </summary>
        /// <param name="state">The current tracking state.</param>
        public void FillState(XrSpatialEntityTrackingStateEXT state)
        {
            _entityState = state;
            _trackingState = _entityState.ToTrackingState();
        }

        /// <summary>
        /// Filling builder with primary component data from the query result.
        /// </summary>
        /// <param name="component">
        /// The component type from <see cref="PrimaryComponents"/>.</param>
        /// <param name="index">
        /// The index of the entity from the query result.</param>
        public virtual void FillPrimaryComponent(
            XrSpatialComponentTypeEXT component, int index)
        {
        }

        /// <summary>
        /// Filling builder with optional component data from the query result.
        /// </summary>
        /// <param name="component">
        /// The component type from <see cref="OptionalComponents"/>.</param>
        /// <param name="entityId">
        /// The entity ID associated with this component.</param>
        public virtual void FillOptionalComponent(
            XrSpatialComponentTypeEXT component, XrSpatialEntityIdEXT entityId)
        {
        }

        /// <summary>
        /// Fill builder data from a known trackable and update tracking state only.
        /// Expect to be used on paused state.
        /// </summary>
        /// <param name="data">The last known trackable data.</param>
        /// <param name="state">The current tracking state.</param>
        public abstract void FromData(
            SpatialTrackableData<TTrackable> data, XrSpatialEntityTrackingStateEXT state);

        /// <summary>
        /// Optional for resetting all known data.
        /// </summary>
        public virtual void ResetData()
        {
            _contextHandle = 0;
            _entityId = 0;
            _entityHandle = 0;
            _entityState = XrSpatialEntityTrackingStateEXT.Stopped;

            _trackableId = TrackableId.invalidId;
            _pose = Pose.identity;
            _trackingState = TrackingState.None;
            _nativePtr = IntPtr.Zero;
            _parentId = TrackableId.invalidId;
        }

        /// <summary>
        /// Output a trackable struct with current data.
        /// </summary>
        /// <returns>A trackable instance.</returns>
        public abstract TTrackable Build();

        /// <summary>
        /// Optional for individual builders if the given trackable should be filtered.
        /// </summary>
        /// <param name="trackable">The trackable pending for filtering.</param>
        /// <returns>
        /// True, excluded from change list and continue tracking internally.
        /// </returns>
        public virtual bool TryFilterOut(TTrackable trackable)
        {
            return false;
        }

        /// <summary>
        /// Validate index for the given native array.
        /// </summary>
        /// <typeparam name="T">The element type in the native array.</typeparam>
        /// <param name="index">The index of the target entity.</param>
        /// <param name="array">The native array filled by query result.</param>
        /// <returns>True if it's a valid index.</returns>
        protected bool ValidateIndex<T>(int index, NativeArray<T> array) where T : struct
        {
            return array.IsCreated && index >= 0 && index < array.Length;
        }
    }
}
