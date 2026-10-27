// <copyright file="SpatialBoundingBoxFeatureEditor.cs" company="Google LLC">
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

namespace Google.XR.Extensions.SpatialEntities.Editor.Internal
{
    using System;
    using Google.XR.Extensions.SpatialEntities.Internal;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(SpatialBoundingBoxFeature))]
    internal class SpatialBoundingBoxFeatureEditor : Editor
    {
        private const string _requestedModesName = "_requestedModes";
        private const string _requestedModesTooltip =
            "Requests desired classifications at runtime. " +
            "If not set, detect all supported types defined by the runtime.";

        private static readonly GUIContent _requestedModesLabel = new GUIContent(
            "Detection Modes", _requestedModesTooltip);

        private SerializedProperty _requestedModes;
        private string[] _displayOptions = { };

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            int selectedValue = _requestedModes.intValue;
            selectedValue = EditorGUILayout.MaskField(
                _requestedModesLabel, selectedValue, _displayOptions);

            // Converts Everything option from MaskField.
            SpatialObjectDetectionModes selectedOptions = selectedValue == -1 ?
                SpatialTypeUtils.ObjectDetectionModesAll :
                (SpatialObjectDetectionModes)selectedValue;
            EditorGUI.EndChangeCheck();

            _requestedModes.intValue = (int)selectedOptions;
            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            _requestedModes = serializedObject.FindProperty(_requestedModesName);
            _displayOptions = Enum.GetNames(typeof(SpatialObjectDetectionModes));
        }
    }
}
