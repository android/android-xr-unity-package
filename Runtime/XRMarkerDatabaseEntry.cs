// <copyright file="XRMarkerDatabaseEntry.cs" company="Google LLC">
//
// Copyright 2025 Google LLC
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

namespace Google.XR.Extensions
{
    using System;
    using System.Runtime.InteropServices;
    using UnityEngine;
    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.ARSubsystems;

    /// <summary>
    /// Represents an entry in an <see cref="XRMarkerDatabase"/> with the specialized information
    /// that can be converted into a marker <see cref="XRReferenceImage"/>, then used at
    /// <see cref="ARTrackedImageManager.referenceLibrary"/> for runtime configuration.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct XRMarkerDatabaseEntry :
        IEquatable<XRMarkerDatabaseEntry>, IComparable<XRMarkerDatabaseEntry>
    {
        [SerializeField]
        private XRMarkerDictionary _dictionary;

        [SerializeField]
        private bool _allMarkers;

        [SerializeField]
        private uint _markerId;

        [SerializeField]
        private float _physcialEdge;

        // Used only for runtime configuration.
        private Guid _referenceGuid;

        /// <summary>
        /// Constructs a <see cref="XRMarkerDatabaseEntry"/> which represents all markers from
        /// a given <see cref="XRMarkerDictionary"/>. It can be converted to a marker
        /// <see cref="XRReferenceImage"/> and used by
        /// <see cref="ARTrackedImageManager.referenceLibrary"/> for runtime configuration.
        /// </summary>
        /// <param name="dictionary">
        /// A predefined <see cref="XRMarkerDictionary"/> in which this entry belongs to.
        /// </param>
        public XRMarkerDatabaseEntry(XRMarkerDictionary dictionary) :
            this(dictionary, true, 0, 0f)
        {
        }

        /// <summary>
        /// Constructs a <see cref="XRMarkerDatabaseEntry"/> which contains the specialized
        /// information for the conversion with a marker <see cref="XRReferenceImage"/>.
        /// </summary>
        /// <param name="dictionary">
        /// A predefined <see cref="XRMarkerDictionary"/> in which this entry belongs to.
        /// </param>
        /// <param name="allMarkers">
        /// A bool indicating whether to track all markers from the <paramref name="dictionary"/>.
        /// </param>
        /// <param name="markerId">
        /// When <paramref name="allMarkers"/> is not set, specify the marker id from the
        /// <paramref name="dictionary"/>.</param>
        /// <param name="physicalEdge">
        /// When <paramref name="allMarkers"/> is not set, specify the physical Edge (meters).
        /// </param>
        public XRMarkerDatabaseEntry(
            XRMarkerDictionary dictionary, bool allMarkers, uint markerId, float physicalEdge)
        {
            _dictionary = dictionary;
            _allMarkers = allMarkers;
            _markerId = markerId;
            _physcialEdge = physicalEdge;
            _referenceGuid = Guid.Empty;
        }

        /// <summary>
        /// Gets the <see cref="XRMarkerDictionary"/> in which this entry belongs to.
        /// </summary>
        public XRMarkerDictionary Dictionary => _dictionary;

        /// <summary>
        /// Gets a bool indicating whether this entry reprents all markers from
        /// <see cref="Dictionary"/>.
        /// </summary>
        public bool AllMarkers => _allMarkers;

        /// <summary>
        /// Gets the marker Id.
        /// Note: It only takes effect when <see cref="AllMarkers"/> is not set.
        /// </summary>
        public uint MarkerId => _markerId;

        /// <summary>
        /// Gets the physical edege in meter.
        /// Note: it only takes effect when <see cref="AllMarkers"/> is not set.
        /// </summary>
        public float PhysicalEdge => _physcialEdge;

        /// <summary>
        /// Convert to the name of a <see cref="XRReferenceImage"/> which represents a marker
        /// reference of <see cref="XRMarkerDatabaseEntry"/>.
        /// </summary>
        public string Name => _allMarkers ? _dictionary.ToString() : $"{_dictionary}-{MarkerId}";

        /// <summary>
        /// Converts the string representation of a marker reference to an equivalent
        /// <see cref="XRMarkerDatabaseEntry"/> object.
        /// </summary>
        /// <param name="name">The name of the marker reference to use for parsing.</param>
        /// <param name="entry">
        /// When this method returns <c>true</c>, contains an <see cref="XRMarkerDatabaseEntry"/>
        /// object that represents the parsed value.</param>
        /// <returns>
        /// Returns <c>true</c> if the conversion succeeded; <c>false</c> otherwise.</returns>
        public static bool TryParse(string name, out XRMarkerDatabaseEntry entry)
        {
            return TryParse(name, 0f, out entry);
        }

        /// <summary>
        /// Converts the string representation of a marker reference to an equivalent
        /// <see cref="XRMarkerDatabaseEntry"/> object.
        /// </summary>
        /// <param name="name">The name of the marker reference to use for parsing.</param>
        /// <paramref name="edge"/>The physical edge in meters of the output.</param>
        /// <param name="entry">
        /// When this method returns <c>true</c>, contains an <see cref="XRMarkerDatabaseEntry"/>
        /// object that represents the parsed value.</param>
        /// <returns>
        /// Returns <c>true</c> if the conversion succeeded; <c>false</c> otherwise.</returns>
        public static bool TryParse(string name, float edge, out XRMarkerDatabaseEntry entry)
        {
            entry = default;
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            if (edge < 0f)
            {
                return false;
            }

            string[] substrs = name.Split("-");
            if (substrs.Length > 2)
            {
                return false;
            }

            XRMarkerDictionary dictionary;
            if (!Enum.TryParse(
                typeof(XRMarkerDictionary), substrs[0], true, out object dictionaryObject))
            {
                return false;
            }

            dictionary = (XRMarkerDictionary)dictionaryObject;
            bool allMarkers = substrs.Length == 1;
            uint markId = 0;
            if (!allMarkers)
            {
                if (!uint.TryParse(substrs[1], out markId))
                {
                    return false;
                }
            }

            entry = new XRMarkerDatabaseEntry(dictionary, allMarkers, markId, edge);
            return true;
        }

        /// <inheritdoc/>
        public override string ToString() =>
            _allMarkers ? $"{Name}: ALL" : $"{Name}, physical edge (meters): {_physcialEdge}";

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var hashCode = (int)_dictionary;
            hashCode = (hashCode * 397) ^ _allMarkers.GetHashCode();
            hashCode = (hashCode * 397) ^ _markerId.GetHashCode();
            hashCode = (hashCode * 397) ^ _physcialEdge.GetHashCode();
            return hashCode;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            (obj is XRMarkerDatabaseEntry) && Equals((XRMarkerDatabaseEntry)obj);

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">
        /// The other <see cref="XRMarkerDatabaseEntry"/> to compare against.</param>
        /// <returns><c>true</c> if the entries are equal.</returns>
        public bool Equals(XRMarkerDatabaseEntry other)
        {
            return _dictionary == other.Dictionary && _allMarkers == other.AllMarkers &&
                _markerId == other.MarkerId && _physcialEdge == other.PhysicalEdge;
        }

        /// <inheritdoc/>
        public int CompareTo(XRMarkerDatabaseEntry other)
        {
            if (Dictionary != other.Dictionary)
            {
                return Dictionary.CompareTo(other.Dictionary);
            }
            else if (AllMarkers != other.AllMarkers)
            {
                return AllMarkers.CompareTo(other.AllMarkers);
            }
            else if (MarkerId != other.MarkerId)
            {
                return MarkerId.CompareTo(other.MarkerId);
            }
            else
            {
                return PhysicalEdge.CompareTo(other.PhysicalEdge);
            }
        }

        internal void SetReference(Guid guid)
        {
            _referenceGuid = guid;
        }

        internal void ApplyEstimation()
        {
            _physcialEdge = 0f;
        }
    }
}
