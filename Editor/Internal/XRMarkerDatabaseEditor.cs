// <copyright file="XRMarkerDatabaseEditor.cs" company="Google LLC">
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

namespace Google.XR.Extensions.Editor.Internal
{
    using System.Collections.Generic;
    using System.IO;
    using Google.XR.Extensions;
    using UnityEditor;
    using UnityEditor.XR.ARSubsystems;
    using UnityEngine;
    using UnityEngine.XR.ARSubsystems;

    [CustomEditor(typeof(XRMarkerDatabase))]
    internal class XRMarkerDatabaseEditor : Editor
    {
        private SerializedProperty _entries;
        private SerializedProperty _imageLibrary;
        private int _pendingRemoval = -1;
        private bool _actionSuceed = true;
        private string _actionMessage = string.Empty;

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            _pendingRemoval = -1;
            for (int i = 0; i < _entries.arraySize; ++i)
            {
                DrawEntryAt(i);
                EditorGUILayout.Separator();
                EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);
            }

            if (_pendingRemoval > -1)
            {
                _entries.DeleteArrayElementAtIndex(_pendingRemoval);
            }

            if (UIContent.OnAddButtonClick())
            {
                Undo.RecordObject(target, "Add marker entry");
                (target as XRMarkerDatabase).AddEntry(
                    new XRMarkerDatabaseEntry(XRMarkerDictionary.ArUco4x4_50, false, 0, 0.1f));
                EditorUtility.SetDirty(target);
                _actionSuceed = true;
                _actionMessage = string.Empty;
            }

            EditorGUILayout.PropertyField(_imageLibrary, UIContent.ReferenceLibraryField);
            serializedObject.ApplyModifiedProperties();

            List<string> errorMessages = (target as XRMarkerDatabase).GetValidationError();
            EditorGUI.BeginDisabledGroup(errorMessages.Count > 0);
            if (_imageLibrary.objectReferenceValue != null)
            {
                if (UIContent.OnUpdateButtonClick())
                {
                    _actionSuceed = TryUpdateReferenceLibrary();
                }
            }
            else
            {
                if (UIContent.OnCreateButtonClick())
                {
                    _actionSuceed = TryCreateReferenceLibrary();
                }
            }

            EditorGUI.EndDisabledGroup();

            foreach (string error in errorMessages)
            {
                EditorGUILayout.HelpBox(error, MessageType.Error);
            }

            if (!string.IsNullOrEmpty(_actionMessage))
            {
                EditorGUILayout.HelpBox(
                    _actionMessage, _actionSuceed ? MessageType.Info : MessageType.Error);
            }
        }

        private void OnEnable()
        {
            _entries = serializedObject.FindProperty("_entries");
            _imageLibrary = serializedObject.FindProperty("_imageLibrary");
            _pendingRemoval = -1;
            _actionSuceed = true;
            _actionMessage = string.Empty;
        }

        void DrawEntryAt(int index)
        {
            var entryProperty = _entries.GetArrayElementAtIndex(index);
            var dictionaryProperty = entryProperty.FindPropertyRelative("_dictionary");
            var allMarkersProperty = entryProperty.FindPropertyRelative("_allMarkers");
            var markerIdProperty = entryProperty.FindPropertyRelative("_markerId");
            var edgeProperty = entryProperty.FindPropertyRelative("_physcialEdge");

            // Receive remove action.
            using (new EditorGUILayout.HorizontalScope())
            {
                _pendingRemoval = UIContent.OnRemoveButtonClick() ? index : _pendingRemoval;
            }

            XRMarkerDatabase database = target as XRMarkerDatabase;
            XRMarkerDatabaseEntry entry = database[index];
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField(UIContent.NameLabel, entry.Name);
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(dictionaryProperty, UIContent.DictionaryField);
            EditorGUILayout.PropertyField(allMarkersProperty, UIContent.AllMarkersField);
            if (!allMarkersProperty.boolValue)
            {
                EditorGUILayout.PropertyField(markerIdProperty, UIContent.MarkerIdField);
                EditorGUILayout.PropertyField(edgeProperty, UIContent.PhysicalEdgeField);
                if (markerIdProperty.intValue < 0)
                {
                    markerIdProperty.intValue = 0;
                }

                if (edgeProperty.floatValue < 0f)
                {
                    edgeProperty.floatValue = 0f;
                }
            }

            if (allMarkersProperty.boolValue || edgeProperty.floatValue == 0f)
            {
                DrawHelpBoxZeroEdge(allMarkersProperty.boolValue);
            }

            if (EditorGUI.EndChangeCheck())
            {
                _actionSuceed = true;
                _actionMessage = string.Empty;
            }
        }

        void DrawHelpBoxZeroEdge(bool allMarkers)
        {
            EditorGUILayout.HelpBox(string.Format(
                "Physical edge is not specified in this entry, expecting size estimation at " +
                "runtime. Note: not all runtimes support estimation. Alternatively, you can " +
                "{0}specify an edge bigger than zero and select {1} from [{2}] feature.",
                allMarkers ? $"unselect {UIContent.AllMarkersField.text} and " : string.Empty,
                UIContent.PreferEstimationLabel, XRMarkerTrackingFeature.UiName),
                MessageType.Warning);
        }

        private bool TryCreateReferenceLibrary()
        {
            // Create a reference library under the same folder.
            XRMarkerDatabase database = target as XRMarkerDatabase;
            string folder = Path.GetDirectoryName(AssetDatabase.GetAssetPath(database));
            string filename = "MarkerReferenceLibrary";
            int count = AssetDatabase.FindAssets(filename).Length;
            if (count > 0)
            {
                filename = $"{filename}{count}";
            }

            XRReferenceImageLibrary imageLibrary = CreateInstance<XRReferenceImageLibrary>();
            string fullpath = $"{folder}/{filename}.asset";
            AssetDatabase.CreateAsset(imageLibrary, fullpath);
            AssetDatabase.SaveAssets();
            _imageLibrary.objectReferenceValue = imageLibrary;
            serializedObject.ApplyModifiedProperties();

            TryUpdateReferenceLibrary();
            _actionMessage = $"Created and update {fullpath}.";
            return true;
        }

        private bool TryUpdateReferenceLibrary()
        {
            XRMarkerDatabase database = target as XRMarkerDatabase;
            database.Sort();

            XRReferenceImageLibrary library =
                _imageLibrary.objectReferenceValue as XRReferenceImageLibrary;

            // Remove existing marker references.
            for (int i = 0; i < library.count;)
            {
                if (XRMarkerDatabaseEntry.TryParse(
                    library[i].name, out XRMarkerDatabaseEntry entry))
                {
                    library.RemoveAt(i);
                    continue;
                }
                else
                {
                    i++;
                }
            }

            if (database.Count == 0)
            {
                _actionMessage =
                    $"Removed marker references from {AssetDatabase.GetAssetPath(library)}.";
                return true;
            }

            // Append new references.
            foreach (var entry in database)
            {
                library.Add();
                int index = library.count - 1;
                library.SetName(index, entry.Name);
                if (!entry.AllMarkers && entry.PhysicalEdge > 0)
                {
                    library.SetSpecifySize(index, true);
                    library.SetSize(index, new Vector2(entry.PhysicalEdge, entry.PhysicalEdge));
                }
            }

            AssetDatabase.SaveAssets();
            _actionMessage = $"Updated {AssetDatabase.GetAssetPath(library)}.";
            return true;
        }

        static class UIContent
        {
            public const string PreferEstimationLabel = "Prefer Estimation";

            public static readonly GUIContent NameLabel = new GUIContent(
                "Name", "The name of the reference image.");

            public static readonly GUIContent DictionaryField = new GUIContent(
                "Dictionary", "The predefined dictionary");

            public static readonly GUIContent AllMarkersField = new GUIContent(
                "All Markers", "Indicate if tracking all markers from the dictionary.");

            public static readonly GUIContent MarkerIdField = new GUIContent(
                "Marker ID", "The Id of a single marker from the dictionary.");

            public static readonly GUIContent PhysicalEdgeField = new GUIContent(
                "Physical Edge (meters)", "The edge of the physical markers, in meters.");

            public static readonly GUIContent AddButton = new GUIContent(
                "Add Entry", "Add a new marker entry into the database.");

            public static readonly GUIContent RemoveButton = new GUIContent(
                "Remove Entry", "Remove the entry from the database.");

            public static readonly GUIContent ReferenceLibraryField = new GUIContent(
                "Reference Library",
                "A reference library to store marker references.");

            public static readonly GUIContent CreateButton = new GUIContent(
                "Create Reference Library",
                "Create a reference library based on marker entries from this database.");

            public static readonly GUIContent UpdateButton = new GUIContent(
                "Update Reference Library",
                "Remove existing marker references and append new entries.");

            public static bool OnAddButtonClick() => GUILayout.Button(AddButton);

            public static bool OnRemoveButtonClick() => GUILayout.Button(RemoveButton);

            public static bool OnCreateButtonClick() => GUILayout.Button(CreateButton);

            public static bool OnUpdateButtonClick() => GUILayout.Button(UpdateButton);
        }
    }
}
