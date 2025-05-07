// <copyright file="XRMarkerDatabase.cs" company="Google LLC">
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
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.ARSubsystems;

    /// <summary>
    /// A marker database is a collection of <see cref="XRMarkerDatabaseEntry"/> which stores marker
    /// information used to configure marker tracking at runtime when
    /// <see cref="XRMarkerTrackingFeature"/> is enabled.
    /// </summary>
    /// <remarks>
    /// Marker Databases are immutable at runtime. Create and manipulate an database via Editor,
    /// then update <see cref="XRReferenceImageLibrary"/> and assign to
    /// <see cref="ARTrackedImageManager.referenceLibrary"/> for runtime configuration.
    /// </remarks>
    [CreateAssetMenu(fileName = "MarkerDatabase", menuName = "XR/Marker Database", order = 1002)]
    public class XRMarkerDatabase : ScriptableObject, IEnumerable<XRMarkerDatabaseEntry>
    {
        [SerializeField]
        private List<XRMarkerDatabaseEntry> _entries = new List<XRMarkerDatabaseEntry>();

#pragma warning disable CS0414 // Remove unread private members, used in Editor.
        [SerializeField]
        private XRReferenceImageLibrary _imageLibrary = null;
#pragma warning restore CS0414 // Remove unread private members, used in Editor.

        private List<string> _duplicateEntries = new List<string>();
        private List<XRMarkerDictionary> _conflictAllMarkers = new List<XRMarkerDictionary>();

#pragma warning disable CS0414 // Remove unread private members, used in Editor.
        private bool _initialized = false;
#pragma warning restore CS0414 // Remove unread private members, used in Editor.

        /// <summary>
        /// Gets the count of entries.
        /// </summary>
        public int Count => _entries.Count;

        /// <summary>
        /// Gets an <see cref="XRMarkerDatabaseEntry"/> from the database.
        /// </summary>
        /// <param name="index">The index of the entry .
        /// Must be between 0 and <see cref="Count"/> - 1.</param>
        /// <returns>The <see cref="XRMarkerDatabaseEntry"/> at <paramref name="index"/>.</returns>
        public XRMarkerDatabaseEntry this[int index]
        {
            get
            {
                if (Count == 0)
                {
                    throw new IndexOutOfRangeException(
                        "The reference image library is empty; cannot index into it.");
                }
                else if (index < 0 || index >= Count)
                {
                    throw new IndexOutOfRangeException(string.Format(
                        "{0} is out of range, must be between 0 and {1}", index, Count - 1));
                }
                else
                {
                    return _entries[index];
                }
            }
        }

        /// <inheritdoc/>
        public IEnumerator<XRMarkerDatabaseEntry> GetEnumerator() => _entries.GetEnumerator();

#if UNITY_EDITOR
        internal void Sort()
        {
            _entries.Sort();
        }

        internal void AddEntry(XRMarkerDatabaseEntry entry = default, bool withValidate = true)
        {
            _entries.Add(entry);
            if (withValidate)
            {
                OnValidate();
            }
        }

        internal bool FindEntry(string name, ref XRMarkerDatabaseEntry entry)
        {
            int index = _entries.FindIndex(entry => entry.Name.Equals(name));
            if (index >= 0)
            {
                entry = _entries[index];
                return true;
            }

            return false;
        }

        internal List<string> GetValidationError()
        {
            if (!_initialized)
            {
                OnValidate();
            }

            List<string> errorMessages = new List<string>();
            if (_duplicateEntries.Count > 0)
            {
                errorMessages.Add(
                    $"Found duplicated entries for {string.Join(", ", _duplicateEntries)}, " +
                    "please remove duplicated entries.");
            }

            if (_conflictAllMarkers.Count > 0)
            {
                errorMessages.Add(
                    "Found individual entries and All-Markers enabled in dictionary(s) " +
                    $"{string.Join(", ", _conflictAllMarkers)}, " +
                    "please remove either one to resolve conflicts.");
            }

            return errorMessages;
        }
#endif

        IEnumerator<XRMarkerDatabaseEntry> IEnumerable<XRMarkerDatabaseEntry>.GetEnumerator() =>
            GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private void OnValidate()
        {
            _initialized = true;
            _duplicateEntries.Clear();
            _conflictAllMarkers.Clear();
            Dictionary<XRMarkerDictionary, HashSet<int>> dictionaries =
                new Dictionary<XRMarkerDictionary, HashSet<int>>();
            foreach (var entry in _entries)
            {
                if (!dictionaries.ContainsKey(entry.Dictionary))
                {
                    dictionaries[entry.Dictionary] = new HashSet<int>();
                }

                int id = entry.AllMarkers ? -1 : (int)entry.MarkerId;
                if (dictionaries[entry.Dictionary].Contains(id))
                {
                    // Found duplicates.
                    if (!_duplicateEntries.Contains(entry.Name))
                    {
                        _duplicateEntries.Add(entry.Name);
                    }

                    continue;
                }
                else if ((entry.AllMarkers && dictionaries[entry.Dictionary].Count > 0) ||
                    (!entry.AllMarkers && dictionaries[entry.Dictionary].Contains(-1)))
                {
                    // Found conflicts with all markers.
                    if (!_conflictAllMarkers.Contains(entry.Dictionary))
                    {
                        _conflictAllMarkers.Add(entry.Dictionary);
                    }
                }

                dictionaries[entry.Dictionary].Add(id);
            }

            if (_duplicateEntries.Count > 0)
            {
                Debug.LogErrorFormat("[{0}] duplicate entries: {1}.",
                    name, string.Join(", ", _duplicateEntries));
            }

            if (_conflictAllMarkers.Count > 0)
            {
                Debug.LogErrorFormat("[{0}] conflicts in All-Markers enabled dictionary(s): {1}.",
                    name, string.Join(", ", _conflictAllMarkers));
            }
        }
    }
}
