// <copyright file="SpatialDataProducer.cs" company="Google LLC">
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
    using Unity.Collections.LowLevel.Unsafe;
    using UnityEngine;
    using UnityEngine.XR.ARSubsystems;
    using UnityEngine.XR.OpenXR.NativeTypes;

    using XrSpatialContextEXT = System.UInt64;
    using XrSpatialEntityEXT = System.UInt64;
    using XrSpatialEntityIdEXT = System.UInt64;
    using XrSpatialSnapshotEXT = System.UInt64;

    /// <summary>
    /// Dedicated producer for snapshot queries and maintain tracked entities
    /// which can then be consumed by individual subsystems.
    /// </summary>
    /// <typeparam name="TTrackable">
    /// The target <see cref="ITrackable"/> reported by the subsystem.
    /// </typeparam>
    /// <typeparam name="TTrackableBuilder">
    /// The spatial trackable builder of the given trackable type.
    /// </typeparam>
    /// <remarks>
    /// Must call <see cref="SpatialDataProducer{T1, T2}.Dispose()"/> to release
    /// native resources after use.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "UnityRules.UnityStyleRules", "US1300:LinesMustBe100CharactersOrShorter",
        Justification = "Constructor of generic types.")]
    internal class SpatialDataProducer<TTrackable, TTrackableBuilder>
        where TTrackable : struct, ITrackable, IEquatable<TTrackable>
        where TTrackableBuilder : SpatialTrackableBuilder<TTrackable>
    {
        private HashSet<XrSpatialEntityIdEXT> _addedEntityIds =
            new HashSet<XrSpatialEntityIdEXT>();

        private HashSet<XrSpatialEntityIdEXT> _updatedEntityIds =
            new HashSet<XrSpatialEntityIdEXT>();

        private HashSet<TrackableId> _removedTrackableIds =
            new HashSet<TrackableId>();

        private TTrackableBuilder _trackableBuilder = default;
        private Dictionary<XrSpatialEntityIdEXT, SpatialTrackableData<TTrackable>>
            _trackableDataByIds = new Dictionary<XrSpatialEntityIdEXT, SpatialTrackableData<TTrackable>>();

        private XrSpatialContextEXT _spatialContext = 0;
        private NativeArray<XrSpatialComponentTypeEXT> _enabledComponents;
        private NativeArray<XrSpatialComponentTypeEXT> _filterComponents;

        public SpatialDataProducer(TTrackableBuilder trackableBuilder)
        {
            _trackableBuilder = trackableBuilder;
        }

        /// <summary>
        /// Gets the current spatial context handle.
        /// </summary>
        public XrSpatialContextEXT SpatialContext => _spatialContext;

        /// <summary>
        /// Access all added entities.
        /// </summary>
        public HashSet<XrSpatialEntityIdEXT> Added => _addedEntityIds;

        /// <summary>
        /// Access all updated entities.
        /// </summary>
        public HashSet<XrSpatialEntityIdEXT> Updated => _updatedEntityIds;

        /// <summary>
        /// Access all removed entities by <see cref="TrackableId"/>.
        /// </summary>
        public HashSet<TrackableId> Removed => _removedTrackableIds;

        /// <summary>
        /// Applies spatial context for data producer.
        /// </summary>
        /// <param name="spatialContext">
        /// The context used to produce data. Setting to 0 will stop updates.
        /// </param>
        /// <param name="enabledComponents">
        /// The components from the configurator used to create the context.
        /// </param>
        public void UpdateContext(
            XrSpatialContextEXT spatialContext,
            HashSet<XrSpatialComponentTypeEXT> enabledComponents)
        {
            _spatialContext = spatialContext;
            _trackableBuilder.ApplyComponents(enabledComponents);

            int count = enabledComponents == null ? 0 : enabledComponents.Count;
            if (_enabledComponents.IsCreated)
            {
                _enabledComponents.Dispose();
            }

            if (count > 0)
            {
                _enabledComponents = new NativeArray<XrSpatialComponentTypeEXT>(
                    count, Allocator.Persistent);
                int i = 0;
                foreach (var component in enabledComponents)
                {
                    _enabledComponents[i++] = component;
                }
            }

            var filterComponents = _trackableBuilder.PrimaryComponents;
            int filterCount = filterComponents == null ? 0 : filterComponents.Count;
            if (_filterComponents.IsCreated)
            {
                _filterComponents.Dispose();
            }

            if (filterCount > 0)
            {
                _filterComponents = new NativeArray<XrSpatialComponentTypeEXT>(
                    filterCount, Allocator.Persistent);
                int i = 0;
                foreach (var component in filterComponents)
                {
                    _filterComponents[i++] = component;
                }
            }
        }

        /// <summary>
        /// Queries the given snapshot and updates tracked entities.
        /// </summary>
        /// <param name="snapshot">The target snapshot.</param>
        /// <param name="isSubset">
        /// Indicates if this query updates only a given subset.
        /// </param>
        /// <returns>The <see cref="XrResult"/> of this query.</returns>
        public unsafe XrResult QuerySnapshot(XrSpatialSnapshotEXT snapshot, bool isSubset = false)
        {
            // Query all entities.
            var componentCount = _filterComponents.IsCreated ? (uint)_filterComponents.Length : 0;
            var componentTypes = componentCount > 0 ?
                (XrSpatialComponentTypeEXT*)_filterComponents.GetUnsafePtr() : null;
            var queryCondition = new XrSpatialComponentDataQueryConditionEXT(
                componentCount, componentTypes);
            var queryResult = XrSpatialComponentDataQueryResultEXT.defaultValue;
            var result = OpenXRNativeApi.xrQuerySpatialComponentDataEXT(
                snapshot, queryCondition, ref queryResult);
            if (result.IsError())
            {
                Debug.LogError($"xrQuerySpatialComponentDataEXT failed with status {result}");
                return result;
            }

            int entityCount = checked((int)queryResult.entityIdCountOutput);
            if (queryResult.entityIdCountOutput == 0)
            {
                if (!isSubset)
                {
                    RemoveAllEntities();
                }
                else
                {
                    Debug.LogWarning("Cannot find any entities within the subset.");
                }

                return result;
            }

            queryResult.entityIdCapacityInput = queryResult.entityIdCountOutput;
            queryResult.entityStateCapacityInput = queryResult.entityStateCountOutput;
            var entityIds = new NativeArray<XrSpatialEntityIdEXT>(entityCount, Allocator.Temp);
            var entityStates = new NativeArray<XrSpatialEntityTrackingStateEXT>(
                entityCount, Allocator.Temp);
            queryResult.SetEntityIds(entityIds);
            queryResult.SetEntityStates(entityStates);
            queryResult.next = _trackableBuilder.CreatePrimaryQueryChainPtr(entityCount);
            result = OpenXRNativeApi.xrQuerySpatialComponentDataEXT(
                snapshot, queryCondition, ref queryResult);
            if (result.IsError())
            {
                Debug.LogError($"xrQuerySpatialComponentDataEXT failed with status {result}");
                _trackableBuilder.ResetQueryResult();
                entityIds.Dispose();
                entityStates.Dispose();
                return result;
            }

            // Query optional components.
            var optionalComponents = _trackableBuilder.OptionalComponents;
            if (optionalComponents != null && optionalComponents.Any())
            {
                foreach (var optionalComponent in optionalComponents)
                {
                    QueryOptionalComponent(snapshot, optionalComponent);
                }
            }

            for (int i = 0; i < queryResult.entityIdCountOutput; i++)
            {
                var id = queryResult.entityIds[i];
                var trackingState = queryResult.entityStates[i];
                bool newEntity = false;

                // This should only happen on discovery snapshot.
                if (!_trackableDataByIds.TryGetValue(id, out var data) &&
                    trackingState != XrSpatialEntityTrackingStateEXT.Stopped)
                {
                    var createResult = OpenXRNativeApi.xrCreateSpatialEntityFromIdEXT(
                        _spatialContext, id, out var entityHandle);
                    if (createResult.IsError())
                    {
                        Debug.LogErrorFormat(
                            "xrCreateSpatialEntityFromIdEXT(context={0:X16}, id={1:X16}) " +
                            "failed with {2}.", _spatialContext, id, createResult);
                        continue;
                    }

                    // In case _spatialContext changed while running,
                    // only use it in new entity, otherwise, use data.Context to
                    // keep TrackableId the same.
                    data = new SpatialTrackableData<TTrackable>(_spatialContext, id, entityHandle);
                    _trackableDataByIds[id] = data;
                    newEntity = true;

                    // Trackable data has not been updated yet, construct TrackableId instead.
                    AfterTrackableCreated(
                        SpatialTypeUtils.ConstructTrackableId(_spatialContext, id));
                }

                TTrackable trackable;
                switch (trackingState)
                {
                    case XrSpatialEntityTrackingStateEXT.Tracking:
                        _trackableBuilder.FillEntity(data.Context, data.Id, data.Handle);
                        _trackableBuilder.FillState(trackingState);
                        if (componentCount > 0)
                        {
                            foreach (var component in _filterComponents)
                            {
                                _trackableBuilder.FillPrimaryComponent(component, i);
                            }
                        }

                        if (optionalComponents != null)
                        {
                            foreach (var optionalComponent in optionalComponents)
                            {
                                _trackableBuilder.FillOptionalComponent(optionalComponent, data.Id);
                            }
                        }

                        trackable = _trackableBuilder.Build();
                        break;
                    case XrSpatialEntityTrackingStateEXT.Paused:
                        if (!newEntity)
                        {
                            // Updated with the last known data.
                            _trackableBuilder.FromData(data, trackingState);
                        }
                        else
                        {
                            // Components like persistence data are returned at pause state.
                            _trackableBuilder.FillEntity(data.Context, data.Id, data.Handle);
                            _trackableBuilder.FillState(trackingState);
                            if (componentCount > 0)
                            {
                                foreach (var component in _filterComponents)
                                {
                                    _trackableBuilder.FillPrimaryComponent(component, i);
                                }
                            }

                            if (optionalComponents != null)
                            {
                                foreach (var optionalComponent in optionalComponents)
                                {
                                    _trackableBuilder.FillOptionalComponent(optionalComponent, data.Id);
                                }
                            }
                        }

                        trackable = _trackableBuilder.Build();
                        break;
                    case XrSpatialEntityTrackingStateEXT.Stopped:
                        // This should only happen on update snapshot.
                        OnTrackableStopped(id);
                        continue;
                    default:
                        Debug.LogWarning($"Invalid tracking state: {trackingState}");
                        continue;
                }

                _trackableBuilder.ResetData();
                var newData = new SpatialTrackableData<TTrackable>(
                    data.Context, data.Id, data.Handle, trackable, data.HasBeenAdded);
                _trackableDataByIds[id] = newData;
                if (_trackableBuilder.TryFilterOut(trackable))
                {
                    continue;
                }

                if (trackingState == XrSpatialEntityTrackingStateEXT.Tracking &&
                    !newData.HasBeenAdded)
                {
                    // Try to report by change event in next frame.
                    _addedEntityIds.Add(id);
                }
                else if (newData.HasBeenAdded)
                {
                    _updatedEntityIds.Add(id);
                }
            }

            _trackableBuilder.ResetQueryResult();
            entityIds.Dispose();
            entityStates.Dispose();
            return result;
        }

        /// <summary>
        /// Queries optional component data and saves in trackable builder.
        /// </summary>
        /// <param name="snapshot">The target snapshot.</param>
        /// <param name="component">The component type to query.</param>
        public unsafe void QueryOptionalComponent(
            XrSpatialSnapshotEXT snapshot, XrSpatialComponentTypeEXT component)
        {
            var singleTypes = new NativeArray<XrSpatialComponentTypeEXT>(
                1, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            singleTypes[0] = component;
            var queryCondition = new XrSpatialComponentDataQueryConditionEXT(singleTypes);
            var queryResult = XrSpatialComponentDataQueryResultEXT.defaultValue;
            var xrResult = OpenXRNativeApi.xrQuerySpatialComponentDataEXT(
                snapshot, queryCondition, ref queryResult);
            if (xrResult.IsError())
            {
                Debug.LogErrorFormat(
                    "xrQuerySpatialComponentDataEXT for {0} failed with {1}",
                    component, xrResult);
                singleTypes.Dispose();
                return;
            }

            var actualCount = checked((int)queryResult.entityIdCountOutput);
            if (actualCount == 0)
            {
                singleTypes.Dispose();
                return;
            }

            queryResult.entityIdCapacityInput = queryResult.entityIdCountOutput;
            queryResult.entityStateCapacityInput = queryResult.entityStateCountOutput;
            var entityIds = new NativeArray<XrSpatialEntityIdEXT>(actualCount, Allocator.Temp);
            var entityStates = new NativeArray<XrSpatialEntityTrackingStateEXT>(
                actualCount, Allocator.Temp);
            queryResult.SetEntityIds(entityIds);
            queryResult.SetEntityStates(entityStates);
            queryResult.next =
                _trackableBuilder.CreateOptionalQueryChainPtr(component, actualCount);
            xrResult = OpenXRNativeApi.xrQuerySpatialComponentDataEXT(
                snapshot, queryCondition, ref queryResult);
            if (xrResult.IsError())
            {
                Debug.LogErrorFormat(
                    "xrQuerySpatialComponentDataEXT for {0} failed with {1}",
                    component, xrResult);
            }
            else
            {
                for (int i = 0; i < actualCount; ++i)
                {
                    _trackableBuilder.SetOptionalComponentByEntityId(
                        component, i, queryResult.entityIds[i]);
                }
            }

            singleTypes.Dispose();
            entityIds.Dispose();
            entityStates.Dispose();
        }

        /// <summary>
        /// Called by subsystem provider before getting changes.
        /// </summary>
        /// <param name="handles">
        /// The set of targeting handles to be updated.
        /// It defaults to <see langword="null"/> and updates all tracked
        /// entities along with their change status.
        /// If it's not <see langword="null"/>, each entity should have already
        /// been added by <see cref="TryAddEntity"/>.
        /// </param>
        /// <returns>The result of this update.</returns>
        public OpenXRResultStatus UpdateTrackableData(HashSet<XrSpatialEntityEXT> handles = null)
        {
            int count = handles == null ? _trackableDataByIds.Count : handles.Count;
            if (_spatialContext == 0 || !_enabledComponents.IsCreated)
            {
                return new OpenXRResultStatus(OpenXRResultStatus.StatusCode.ProviderUninitialized);
            }
            else if (count == 0)
            {
                return new OpenXRResultStatus(OpenXRResultStatus.StatusCode.UnqualifiedSuccess);
            }

            var entityHandles = new NativeArray<XrSpatialEntityEXT>(
                count, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            int i = 0;
            if (handles == null)
            {
                foreach (var data in _trackableDataByIds.Values)
                {
                    entityHandles[i++] = data.Handle;
                }
            }
            else
            {
                foreach (var handle in handles)
                {
                    entityHandles[i++] = handle;
                }
            }

            var result = OpenXRNativeApi.xrCreateSpatialUpdateSnapshotEXT(
                _spatialContext, entityHandles, _enabledComponents, out var snapshot);
            entityHandles.Dispose();
            if (result.IsError())
            {
                Debug.LogError($"xrCreateSpatialUpdateSnapshotEXT failed with status {result}");
                return result;
            }

            var queryResult = QuerySnapshot(snapshot, handles != null);

            var xrResult = OpenXRNativeApi.xrDestroySpatialSnapshotEXT(snapshot);
            if (xrResult.IsError())
            {
                Debug.LogError($"xrDestroySpatialSnapshotEXT failed with status {xrResult}");
            }

            return queryResult.ToStatus();
        }

        /// <summary>
        /// Retrieve trackable struct from tracked data.
        /// </summary>
        /// <param name="id">The spatial entity id of the trackable.</param>
        /// <returns>
        /// The last known data of the trackable or the default value if not found.
        /// </returns>
        public TTrackable GetTrackable(XrSpatialEntityIdEXT id) =>
            _trackableDataByIds.TryGetValue(id, out var data) ? data.Trackable : default;

        /// <summary>
        /// Called by subsystem provider after reporting the latest results.
        /// </summary>
        public void ClearChanges()
        {
            foreach (var id in _addedEntityIds)
            {
                if (_trackableDataByIds.TryGetValue(id, out var data))
                {
                    data.HasBeenAdded = true;
                    _trackableDataByIds[id] = data;
                }
            }

            _addedEntityIds.Clear();
            _updatedEntityIds.Clear();
            _removedTrackableIds.Clear();
        }

        /// <summary>
        /// Clean tracked entities and report all known entities as removed.
        /// </summary>
        public void RemoveAllEntities()
        {
            _addedEntityIds.Clear();
            _updatedEntityIds.Clear();
            foreach (var data in _trackableDataByIds.Values)
            {
                if (data.HasBeenAdded)
                {
                    _removedTrackableIds.Add(
                        SpatialTypeUtils.ConstructTrackableId(data.Context, data.Id));
                }

                data.DestroyHandle();
            }

            _trackableDataByIds.Clear();
        }

        /// <summary>
        /// Dispose native resources.
        /// </summary>
        public virtual void Dispose()
        {
            if (_enabledComponents.IsCreated)
            {
                _enabledComponents.Dispose();
            }

            if (_filterComponents.IsCreated)
            {
                _filterComponents.Dispose();
            }
        }

        /// <summary>
        /// Callback events invoked after xrCreateSpatialEntityFromIdEXT succeeded.
        /// </summary>
        /// <param name="trackableId">The trackable id of this new entity.</param>
        protected virtual void AfterTrackableCreated(TrackableId trackableId)
        {
        }

        /// <summary>
        /// Callback events invoked right before deleting a known entity.
        /// </summary>
        /// <param name="trackableId">The trackable to be deleted.</param>
        protected virtual void OnTrackableDelete(TrackableId trackableId)
        {
        }

        /// <summary>
        /// Attempts to add an entity which is created from another source.
        /// </summary>
        /// <param name="context">
        /// The spatial context which owns this entity.</param>
        /// <param name="id">The entity id of this entity.</param>
        /// <param name="handle">The entity handle of this entity.</param>
        /// <returns>
        /// Returns <see langword="true"/> if succeeded,
        /// otherwise, returns <see langword="false"/>.
        /// </returns>
        protected bool TryAddEntity(
            XrSpatialContextEXT context,
            XrSpatialEntityIdEXT id,
            XrSpatialEntityEXT handle)
        {
            if (_trackableDataByIds.TryGetValue(id, out var data))
            {
                return false;
            }

            // Initialized as paused state and waiting for next update.
            var initState = XrSpatialEntityTrackingStateEXT.Paused;
            data = new SpatialTrackableData<TTrackable>(context, id, handle);
            _trackableBuilder.FromData(data, initState);

            // Build trackable to apply common properties.
            var trackable = _trackableBuilder.Build();
            _trackableBuilder.ResetData();

            var newData = new SpatialTrackableData<TTrackable>(
                data.Context, data.Id, data.Handle, trackable, data.HasBeenAdded);
            _trackableDataByIds[id] = newData;
            return true;
        }

        /// <summary>
        /// Attempts to remove the entity from the tracked list. If succeeded,
        /// it will be reported as removed in next update.
        /// </summary>
        /// <param name="id">The entity id of this entity.</param>
        /// <returns>
        /// Returns <see langword="true"/> if succeeded,
        /// otherwise, returns <see langword="false"/>.
        /// </returns>
        protected bool TryRemoveEntity(XrSpatialEntityIdEXT id)
        {
            return OnTrackableStopped(id);
        }

        private bool OnTrackableStopped(XrSpatialEntityIdEXT id)
        {
            if (!_trackableDataByIds.TryGetValue(id, out var data))
            {
                return false;
            }

            data.DestroyHandle();
            OnTrackableDelete(data.Trackable.trackableId);
            _trackableDataByIds.Remove(id);
            if (!_addedEntityIds.Remove(id) && data.HasBeenAdded)
            {
                _removedTrackableIds.Add(
                    SpatialTypeUtils.ConstructTrackableId(data.Context, data.Id));
            }

            return true;
        }
    }
}
