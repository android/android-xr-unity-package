// <copyright file="XRAnchorFeatureEditor.cs" company="Google LLC">
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

    /// <summary>
    /// Custom Editor for <see cref="XRAnchorFeature"/>.
    /// </summary>
    [CustomEditor(typeof(XRAnchorFeature))]
    internal class XRAnchorFeatureEditor : Editor
    {
        private const string _usePersistenceFieldName = "_usePersistence";
        private const string _usePersistenceLabel = "Use Persistence";

        private SerializedProperty _usePersistence;

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            _usePersistence.boolValue = EditorGUILayout.Toggle(
                _usePersistenceLabel, _usePersistence.boolValue);

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            // FindProperty only works on fields.
            _usePersistence = serializedObject.FindProperty(_usePersistenceFieldName);
        }
    }
}
