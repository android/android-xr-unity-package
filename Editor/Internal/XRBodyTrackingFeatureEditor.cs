// <copyright file="XRBodyTrackingFeatureEditor.cs" company="Google LLC">
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
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Custom Editor for <see cref="XRBodyTrackingFeature"/>.
    /// </summary>
    [CustomEditor(typeof(XRBodyTrackingFeature))]
    internal class XRBodyTrackingFeatureEditor : Editor
    {
        private const string _autoCalibrationFieldName = "_autoCalibration";
        private const string _autoCalibrationTooltip =
            "Determines whether to enable automatic calibration at runtime.";

        private const string _proportionsFieldName = "_proportions";
        private const string _proportionsTooltip =
            "Optional avatar proportions for the rest pose skeleton computation. " +
            "It only takes effect when automatic calibration is disabled.";

        private static readonly GUIContent _autoCalibrationLabel =
            new GUIContent("Auto Calibration", _autoCalibrationTooltip);

        private static readonly GUIContent _proportionsLabel =
            new GUIContent("Human Body Proportions", _proportionsTooltip);

        private SerializedProperty _autoCalibration;
        private SerializedProperty _proportions;

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            EditorGUIUtility.labelWidth = 170.0f;

            serializedObject.Update();
            EditorGUILayout.PropertyField(_autoCalibration, _autoCalibrationLabel);
            if (!_autoCalibration.boolValue)
            {
                EditorGUILayout.PropertyField(_proportions, _proportionsLabel);
            }

            serializedObject.ApplyModifiedProperties();

            EditorGUIUtility.labelWidth = 0f;
        }

        private void OnEnable()
        {
            _autoCalibration = serializedObject.FindProperty(_autoCalibrationFieldName);
            _proportions = serializedObject.FindProperty(_proportionsFieldName);
        }
    }
}
