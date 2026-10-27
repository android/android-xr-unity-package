// <copyright file="XRShareableAnchorSubsystemDescriptor.cs" company="Google LLC">
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
    using System.Threading;
    using UnityEngine.SubsystemsImplementation;

    /// <summary>
    /// Indicates the capabilities supported by a provider of the
    /// <see cref="XRShareableAnchorSubsystem"/>.
    /// Provider implementations must derive from
    /// <see cref="XRShareableAnchorSubsystem.Provider"/> and may override
    /// virtual class members.
    /// </summary>
    [SuppressMessage(
        "UnityRules.UnityStyleRules", "US1109:PublicPropertiesMustBeUpperCamelCase",
        Justification = "Match Unity interface.")]
    public class XRShareableAnchorSubsystemDescriptor :
        SubsystemDescriptorWithProvider<XRShareableAnchorSubsystem,
                                        XRShareableAnchorSubsystem.Provider>
    {
        private Func<XRShareableAnchorHosts> _supportedHostsDelegate;

        XRShareableAnchorSubsystemDescriptor(Cinfo cinfo)
        {
            id = cinfo.id;
            providerType = cinfo.providerType;
            subsystemTypeOverride = cinfo.subsystemTypeOverride;
            _supportedHostsDelegate = cinfo.supportedHostsDelegate;
            supportsAsyncCancellation = cinfo.supportsAsyncCancellation;
            supportsSaveAnchor = cinfo.supportsSaveAnchor;
            supportsEraseAnchor = cinfo.supportsEraseAnchor;
            supportsLoadAnchor = cinfo.supportsLoadAnchor;
            supportsGetSavedAnchorIds = cinfo.supportsGetSavedAnchorIds;
            supportsAttachGeospatialAnchor = cinfo.supportsAttachGeospatialAnchor;
            supportsAttachSurfaceAnchor = cinfo.supportsAttachSurfaceAnchor;
        }

        /// <summary>
        /// Gets a value indicating whether the provider implementation supports
        /// cancelling async operations in progress using <see cref="CancellationToken"/>s.
        /// </summary>
        /// <value>
        /// If <see langword="false"/>, <see cref="XRShareableAnchorSubsystem"/>
        /// APIs that take a <see cref="CancellationToken"/> as input will
        /// ignore the input cancellation token.
        /// </value>
        public bool supportsAsyncCancellation { get; }

        /// <summary>
        /// Gets a value indicating whether the provider implementation supports
        /// the ability to persistently save anchors via
        /// <see cref="XRShareableAnchorSubsystem.TrySaveAnchorAsync"/>.
        /// </summary>
        public bool supportsSaveAnchor { get; }

        /// <summary>
        /// Gets a value indicating whether the provider implementation supports
        /// the ability to erase the persistent saved data associated with an
        /// anchor via
        /// <see cref="XRShareableAnchorSubsystem.TryEraseAnchorAsync"/>.
        /// </summary>
        public bool supportsEraseAnchor { get; }

        /// <summary>
        /// Gets a value indicating whether the provider implementation supports
        /// the ability to load persistently saved anchors via
        /// <see cref="XRShareableAnchorSubsystem.TryLoadAnchorAsync"/>.
        /// </summary>
        public bool supportsLoadAnchor { get; }

        /// <summary>
        /// Gets a value indicating whether the provider implementation supports
        /// the ability to get all saved persistent GUIDs via
        /// <see cref="XRShareableAnchorSubsystem.TryGetSavedAnchorIdsAsync"/>.
        /// </summary>
        public bool supportsGetSavedAnchorIds { get; }

        /// <summary>
        /// Gets a value indicating whether the provider implementation supports
        /// the ability to attach a geospatial anchor via
        /// <see cref="XRShareableAnchorSubsystem.TryAttachGeospatialAnchor"/>.
        /// </summary>
        public bool supportsAttachGeospatialAnchor { get; }

        /// <summary>
        /// Gets a value indicating whether the provider implementation supports
        /// the ability to attach a surface anchor via
        /// <see cref="XRShareableAnchorSubsystem.TryAttachSurfaceAnchorAsync"/>.
        /// </summary>
        public bool supportsAttachSurfaceAnchor { get; }

        /// <summary>
        /// Creates a new subsystem descriptor instance and registers it with
        /// the SubsystemManager.
        /// </summary>
        /// <param name="cinfo">Construction info for the descriptor.</param>
        public static void Register(Cinfo cinfo)
        {
            if (string.IsNullOrEmpty(cinfo.id))
            {
                throw new ArgumentException(
                    "Cannot create shareable anchor subsystem descriptor with invalid id",
                    nameof(cinfo));
            }

            if (cinfo.providerType == null ||
                !cinfo.providerType.IsSubclassOf(typeof(XRShareableAnchorSubsystem.Provider)))
            {
                throw new ArgumentException(
                    "Cannot create shareable anchor subsystem descriptor with invalid providerType",
                    nameof(cinfo));
            }

            if (cinfo.subsystemTypeOverride == null ||
                (cinfo.subsystemTypeOverride != typeof(XRShareableAnchorSubsystem) &&
                 !cinfo.subsystemTypeOverride.IsSubclassOf(typeof(XRShareableAnchorSubsystem))))
            {
                throw new ArgumentException(
                    "Cannot create shareable anchor subsystem descriptor with invalid " +
                    "subsystemTypeOverride", nameof(cinfo));
            }

            SubsystemDescriptorStore.RegisterDescriptor(
                new XRShareableAnchorSubsystemDescriptor(cinfo));
        }

        /// <summary>
        /// Gets the supported hosts for shareable anchors.
        /// </summary>
        /// <returns>The host types which are supported by the provider.</returns>
        public XRShareableAnchorHosts GetSupportedHosts()
        {
            return _supportedHostsDelegate == null ?
                XRShareableAnchorHosts.None : _supportedHostsDelegate.Invoke();
        }

        /// <summary>
        /// Contains the parameters necessary to construct a new
        /// <see cref="XRShareableAnchorSubsystemDescriptor"/> instance.
        /// </summary>
        [SuppressMessage(
            "UnityRules.UnityStyleRules", "US1109:PublicPropertiesMustBeUpperCamelCase",
            Justification = "Match Unity interface.")]
        public struct Cinfo : IEquatable<Cinfo>
        {
            /// <summary>
            /// Gets or sets the unique identifier of the provider implementation.
            /// No specific format is required.
            /// </summary>
            public string id { get; set; }

            /// <summary>
            /// Gets or sets the provider implementation type to use for instantiation.
            /// </summary>
            /// <value>The provider implementation type.</value>
            public Type providerType { get; set; }

            /// <summary>
            /// Gets or sets the <see cref="XRShareableAnchorSubsystem"/>-derived
            /// type to use for instantiation. The instantiated instance of this
            /// type will forward casted calls to its provider.
            /// </summary>
            /// <value>
            /// The subsystem implementation type. If <see langword="null"/>,
            /// <see cref="XRShareableAnchorSubsystem"/> will be instantiated.
            /// </value>
            public Type subsystemTypeOverride { get; set; }

            /// <summary>
            /// Gets or sets the delegate which specifies the supported hosts
            /// for shareable anchors.
            /// </summary>
            public Func<XRShareableAnchorHosts> supportedHostsDelegate { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the provider implementation
            /// supports cancelling async operations in progress using
            /// <see cref="CancellationToken"/>s.
            /// </summary>
            /// <value>
            /// If <see langword="false"/>, <see cref="XRShareableAnchorSubsystem"/>
            /// APIs that take a <see cref="CancellationToken"/> as input will
            /// ignore the input cancellation token.
            /// </value>
            public bool supportsAsyncCancellation { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the provider implementation
            /// supports the ability to persistently save anchors via
            /// <see cref="XRShareableAnchorSubsystem.TrySaveAnchorAsync"/>.
            /// </summary>
            public bool supportsSaveAnchor { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the provider implementation
            /// supports the ability to erase the persistent saved data associated
            /// with an anchor via
            /// <see cref="XRShareableAnchorSubsystem.TryEraseAnchorAsync"/>.
            /// </summary>
            public bool supportsEraseAnchor { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the provider implementation
            /// supports the ability to load persistently saved anchors via
            /// <see cref="XRShareableAnchorSubsystem.TryLoadAnchorAsync"/>.
            /// </summary>
            public bool supportsLoadAnchor { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the provider implementation
            /// supports the ability to get all saved persistent GUIDs via
            /// <see cref="XRShareableAnchorSubsystem.TryGetSavedAnchorIdsAsync"/>.
            /// </summary>
            public bool supportsGetSavedAnchorIds { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the provider implementation
            /// supports the ability to attach a geospatial anchor via
            /// <see cref="XRShareableAnchorSubsystem.TryAttachGeospatialAnchor"/>.
            /// </summary>
            public bool supportsAttachGeospatialAnchor { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the provider implementation
            /// supports the ability to attach a surface anchor via
            /// <see cref="XRShareableAnchorSubsystem.TryAttachSurfaceAnchorAsync"/>.
            /// </summary>
            public bool supportsAttachSurfaceAnchor { get; set; }

            /// <summary>
            /// Determines whether two <see cref="Cinfo"/> instances have
            /// exactly the same property values.
            /// </summary>
            /// <param name="left">The first instances to compare.</param>
            /// <param name="right">The second instances to compare.</param>
            /// <returns>
            /// Returns true if all properties match. Otherwise, returns false.
            /// </returns>
            public static bool operator ==(Cinfo left, Cinfo right) =>
                left.Equals(right);

            /// <summary>
            /// Determines whether two <see cref="Cinfo"/> instances have
            /// different property values.
            /// </summary>
            /// <param name="left">The first instances to compare.</param>
            /// <param name="right">The second instances to compare.</param>
            /// <returns>
            /// Returns true if any property differs. Otherwise, returns false.
            /// </returns>
            public static bool operator !=(Cinfo left, Cinfo right) =>
                !(left == right);

            /// <summary>
            /// Tests for equality.
            /// </summary>
            /// <param name="other">
            /// The other <see cref="Cinfo"/> to compare with.</param>
            /// <returns>
            /// Returns true if all properties match. Otherwise, returns false.
            /// </returns>
            public bool Equals(Cinfo other) =>
                string.Equals(id, other.id)
                && ReferenceEquals(providerType, other.providerType)
                && ReferenceEquals(subsystemTypeOverride, other.subsystemTypeOverride)
                && supportedHostsDelegate == other.supportedHostsDelegate
                && supportsAsyncCancellation == other.supportsAsyncCancellation
                && supportsAttachGeospatialAnchor == other.supportsAttachGeospatialAnchor
                && supportsAttachSurfaceAnchor == other.supportsAttachSurfaceAnchor
                && supportsSaveAnchor == other.supportsSaveAnchor
                && supportsEraseAnchor == other.supportsEraseAnchor
                && supportsLoadAnchor == other.supportsLoadAnchor
                && supportsGetSavedAnchorIds == other.supportsGetSavedAnchorIds;

            /// <inheritdoc/>
            public override bool Equals(object obj) =>
                obj is Cinfo other && Equals(other);

            /// <inheritdoc/>
            public override int GetHashCode()
            {
                var hashCode = new HashCode();
                hashCode.Add(id);
                hashCode.Add(providerType);
                hashCode.Add(subsystemTypeOverride);
                hashCode.Add(supportedHostsDelegate);
                hashCode.Add(supportsAsyncCancellation);
                hashCode.Add(supportsSaveAnchor);
                hashCode.Add(supportsEraseAnchor);
                hashCode.Add(supportsLoadAnchor);
                hashCode.Add(supportsGetSavedAnchorIds);
                hashCode.Add(supportsAttachGeospatialAnchor);
                hashCode.Add(supportsAttachSurfaceAnchor);
                return hashCode.ToHashCode();
            }
        }
    }
}
