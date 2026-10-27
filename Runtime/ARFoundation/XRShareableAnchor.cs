// <copyright file="XRShareableAnchor.cs" company="Google LLC">
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

namespace Google.XR.Extensions.ARSubsystems
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using UnityEngine;
    using UnityEngine.XR.ARSubsystems;

    /// <summary>
    /// Represents a shareable anchor trackable by the shareable anchor subsystem.
    /// </summary>
    [SuppressMessage(
        "UnityRules.UnityStyleRules",
        "US1109:PublicPropertiesMustBeUpperCamelCase",
        Justification = "Match Unity interface.")]
    [StructLayout(LayoutKind.Sequential)]
    public struct XRShareableAnchor : ITrackable, IEquatable<XRShareableAnchor>
    {
        // ITrackable properties:
        private TrackableId _trackableId;
        private Pose _pose;
        private TrackingState _state;
        private IntPtr _nativePtr;
        private TrackableId _parentId;

        // Shareable properties:
        private XRShareableAnchorHosts _host;
        private SerializableGuid _shareableId;

        /// <summary>
        /// Constructs a new <see cref="XRShareableAnchor"/>.
        /// </summary>
        /// <param name="id">
        /// The unique identifier for this shareable anchor.</param>
        /// <param name="pose">
        /// The pose of the shareable anchor in the Unity world space.</param>
        /// <param name="state">
        /// The current tracking state of the shareable anchor.</param>
        /// <param name="nativePtr">
        /// Provider-specific native pointer, or <see cref="IntPtr.Zero"/>.
        /// </param>
        /// <param name="parentId">
        /// The <see cref="TrackableId"/> of the parent of this tracked object,
        /// or <see cref="TrackableId.invalidId"/>.</param>
        /// <param name="host">The host of this shareable anchor.</param>
        /// <param name="shareableId">
        /// The shareable identifier of this anchor.</param>
        public XRShareableAnchor(
            TrackableId id,
            Pose pose,
            TrackingState state,
            IntPtr nativePtr,
            TrackableId parentId,
            XRShareableAnchorHosts host,
            SerializableGuid shareableId)
        {
            _trackableId = id;
            _pose = pose;
            _state = state;
            _nativePtr = nativePtr;
            _parentId = parentId;

            _host = host;
            _shareableId = shareableId;
        }

        /// <summary>
        /// Constructs a default anchor.
        /// </summary>
        /// <param name="id">
        /// The unique identifier for this shareable anchor.</param>
        /// <param name="pose">
        /// The pose of the shareable anchor in the Unity world space.</param>
        /// <param name="state">
        /// The current tracking state of the shareable anchor.</param>
        /// <param name="nativePtr">
        /// Provider-specific native pointer, or <see cref="IntPtr.Zero"/>.
        /// </param>
        /// <param name="parentId">
        /// The <see cref="TrackableId"/> of the parent of this tracked object,
        /// or <see cref="TrackableId.invalidId"/>.</param>
        public XRShareableAnchor(
            TrackableId id,
            Pose pose,
            TrackingState state,
            IntPtr nativePtr,
            TrackableId parentId)
            : this(
                id,
                pose,
                state,
                nativePtr,
                parentId,
                XRShareableAnchorHosts.None,
                SerializableGuid.empty)
        {
        }

        /// <summary>
        /// Gets a default-initialized <see cref="XRShareableAnchor"/>.
        /// </summary>
        public static XRShareableAnchor defaultValue =>
            new XRShareableAnchor(
                TrackableId.invalidId,
                Pose.identity,
                TrackingState.None,
                IntPtr.Zero,
                TrackableId.invalidId);

        /// <summary>
        /// Gets the unique identifier for this shareable anchor.
        /// </summary>
        public TrackableId trackableId => _trackableId;

        /// <summary>
        /// Gets the pose of the shareable anchor in the Unity world space.
        /// </summary>
        public Pose pose => _pose;

        /// <summary>
        /// Gets the current tracking state of the shareable anchor.
        /// </summary>
        public TrackingState trackingState => _state;

        /// <summary>
        /// Gets a native pointer to additional provider-specific data.
        /// May be <see cref="IntPtr.Zero"/>.
        /// </summary>
        public IntPtr nativePtr => _nativePtr;

        /// <summary>
        /// Gets the <see cref="TrackableId"/> of the parent of this tracked
        /// object. May be <see cref="TrackableId.invalidId"/>.
        /// </summary>
        public TrackableId parentId => _parentId;

        /// <summary>
        /// Gets the host of this shareable anchor.
        /// </summary>
        public XRShareableAnchorHosts host => _host;

        /// <summary>
        /// Gets the shareable ID to retrieve the anchor from other clients.
        /// </summary>
        public SerializableGuid shareableId => _shareableId;

        /// <summary>
        /// Determines whether two <see cref="XRShareableAnchor"/> instances
        /// have exactly the same property values.
        /// </summary>
        /// <param name="left">The first trackable to compare.</param>
        /// <param name="right">The second trackable to compare.</param>
        /// <returns>
        /// Returns true if all properties match. Otherwise, returns false.
        /// </returns>
        public static bool operator ==(XRShareableAnchor left, XRShareableAnchor right) =>
            left.Equals(right);

        /// <summary>
        /// Determines whether two <see cref="XRShareableAnchor"/> instances
        /// have different property values.
        /// </summary>
        /// <param name="left">The first trackable to compare.</param>
        /// <param name="right">The second trackable to compare.</param>
        /// <returns>
        /// Returns true if any property differs. Otherwise, returns false.
        /// </returns>
        public static bool operator !=(XRShareableAnchor left, XRShareableAnchor right) =>
            !(left == right);

        /// <summary>
        /// Updates host information for a default anchor.
        /// </summary>
        /// <param name="host">The host of this shareable anchor.</param>
        /// <param name="shareableId">
        /// The shareable identifier of this anchor.</param>
        public void UpdateHost(XRShareableAnchorHosts host, SerializableGuid shareableId)
        {
            if (_host != XRShareableAnchorHosts.None)
            {
#if DEBUG
                Debug.LogErrorFormat(
                    "Failed to update {0} with host {1}, it has already been hosted by {2}.",
                    _trackableId, host, _host);
#endif // DEBUG
                return;
            }

            _host = host;
            _shareableId = shareableId;
        }

        /// <summary>
        /// Indicates whether every field in this <see cref="XRShareableAnchor"/>
        /// is equal to the corresponding field in <paramref name="other"/>.
        /// </summary>
        /// <param name="other">The <see cref="XRShareableAnchor"/> to compare with.
        /// </param>
        /// <returns>
        /// Returns true if all properties match. Otherwise, returns false.
        /// </returns>
        public bool Equals(XRShareableAnchor other) =>
            trackableId.Equals(other.trackableId)
            && pose.Equals(other.pose)
            && trackingState.Equals(other.trackingState)
            && nativePtr.Equals(other.nativePtr)
            && parentId.Equals(other.parentId)
            && host.Equals(other.host)
            && shareableId.Equals(other.shareableId);

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            obj is XRShareableAnchor other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var code1 = HashCode.Combine(trackableId, pose, trackingState, nativePtr, parentId);
            var code2 = HashCode.Combine(host, shareableId);
            return HashCode.Combine(code1, code2);
        }
    }
}
