// <copyright file="SpatialTrackableData.cs" company="Google LLC">
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
    using UnityEngine;
    using UnityEngine.XR.ARSubsystems;
    using UnityEngine.XR.OpenXR.NativeTypes;

    using XrSpatialContextEXT = System.UInt64;
    using XrSpatialEntityEXT = System.UInt64;
    using XrSpatialEntityIdEXT = System.UInt64;

    /// <summary>
    /// Generalized struct to store spatial entity info and last known trackable data.
    /// </summary>
    /// <typeparam name="TTrackable">
    /// The desired trackable type that represents the spatial entity.
    /// </typeparam>
    internal struct SpatialTrackableData<TTrackable> : IEquatable<SpatialTrackableData<TTrackable>>
        where TTrackable : ITrackable, IEquatable<TTrackable>
    {
        private XrSpatialContextEXT _contextHandle;
        private XrSpatialEntityIdEXT _entityId;
        private XrSpatialEntityEXT _entityHandle;
        private bool _hasBeenAdded;
        private TTrackable _trackable;

        /// <summary>
        /// Construct data from a known trackable.
        /// </summary>
        /// <param name="context">The context which owns this entity.</param>
        /// <param name="id">The spatial entity id.</param>
        /// <param name="entity">The spatial entity handle.</param>
        /// <param name="trackable">The known trackable struct.</param>
        /// <param name="hasBeenAdded">
        /// Indicates if it has been reported by change events.
        /// </param>
        public SpatialTrackableData(
            XrSpatialContextEXT context, XrSpatialEntityIdEXT id, XrSpatialEntityEXT entity,
            TTrackable trackable, bool hasBeenAdded)
        {
            _contextHandle = context;
            _entityId = id;
            _entityHandle = entity;
            _hasBeenAdded = hasBeenAdded;
            _trackable = trackable;
        }

        /// <summary>
        /// Construct data from a newly added entities which have not be filled by query data.
        /// </summary>
        /// <param name="context">The context which owns this entity.</param>
        /// <param name="id">The spatial entity id.</param>
        /// <param name="entity">The spatial entity handle.</param>
        public SpatialTrackableData(
            XrSpatialContextEXT context, XrSpatialEntityIdEXT id, XrSpatialEntityEXT entity) :
            this(context, id, entity, default, false)
        {
        }

        public XrSpatialContextEXT Context => _contextHandle;

        public XrSpatialEntityIdEXT Id => _entityId;

        public XrSpatialEntityEXT Handle => _entityHandle;

        public TTrackable Trackable => _trackable;

        /// <summary>
        /// Gets or sets a value indicating whether the current entity has been
        /// reported by AR Foundation's change events.
        /// </summary>
        public bool HasBeenAdded
        {
            get => _hasBeenAdded;
            set => _hasBeenAdded |= value;
        }

        public void DestroyHandle()
        {
            if (_entityHandle == 0)
            {
                return;
            }

            var result = OpenXRNativeApi.xrDestroySpatialEntityEXT(_entityHandle);
            if (result.IsError())
            {
                Debug.LogError(
                    $"xrDestroySpatialEntityEXT({_entityHandle}) failed with status {result}");
            }

            _entityHandle = 0;
        }

        /// <inheritdoc/>
        public bool Equals(SpatialTrackableData<TTrackable> other)
        {
            return
                Context.Equals(other.Context) &&
                Id.Equals(other.Id) &&
                Handle.Equals(other.Handle) &&
                HasBeenAdded.Equals(other.HasBeenAdded) &&
                Trackable.Equals(other.Trackable);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is SpatialTrackableData<TTrackable> other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(Context, Id, Handle, HasBeenAdded, Trackable);
        }
    }
}
