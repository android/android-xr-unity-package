// <copyright file="XRSessionFeatureEditor.cs" company="Google LLC">
//
// Copyright 2024 Google LLC
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
    /// Custom Editor for <see cref="XRSessionFeature"/>.
    /// </summary>
    [CustomEditor(typeof(XRSessionFeature))]
    internal class XRSessionFeatureEditor : Editor
    {
        private const string _immersiveXRFieldName = "ImmersiveXR";
        private const string _immersiveXRTooltip =
            "Indicate that the activity starts as XR Immersive app, " +
            "and will be launched in full-screen mode.";

        private const string _subsamplingFieldName = "_vulkanSubsampling";
        private const string _subsamplingTooltip =
            "Enable Subsampling (Vulkan) to reduce memory bandwidth on foveated framebuffers. " +
            "This requires the <b>Foveated Rendering</b> feature to be enabled or " +
            "<b>Foveation (Legacy)</b> for legacy support. " +
            "Otherwise, the application will result in rendering faulty.";

        private static readonly GUIContent _immersiveXRLabel = new GUIContent(
            "Immersive XR", _immersiveXRTooltip);

        private static readonly GUIContent _subsamplingLabel = new GUIContent(
            "Subsampling (Vulkan)", _subsamplingTooltip);

        private SerializedProperty _immersiveXR;
        private SerializedProperty _subsampling;

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            EditorGUIUtility.labelWidth = 200.0f;

            serializedObject.Update();
            _immersiveXR.boolValue = EditorGUILayout.Toggle(
                _immersiveXRLabel, _immersiveXR.boolValue);
            _subsampling.boolValue = EditorGUILayout.Toggle(
                _subsamplingLabel, _subsampling.boolValue);
            serializedObject.ApplyModifiedProperties();

            EditorGUIUtility.labelWidth = 0f;
        }

        private void OnEnable()
        {
            _immersiveXR = serializedObject.FindProperty(_immersiveXRFieldName);
            _subsampling = serializedObject.FindProperty(_subsamplingFieldName);
        }
    }
}
