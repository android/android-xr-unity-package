// <copyright file="XRStreamingFeatureEditor.cs" company="Google LLC">
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
    using Google.XR.Extensions.Internal;
    using UnityEditor;
    using UnityEngine;

    using XRStreamingClient = XRStreamingFeature.XRStreamingClient;

    /// <summary>
    /// Custom Editor for <see cref="XRStreamingFeature"/>.
    /// </summary>
    [CustomEditor(typeof(XRStreamingFeature))]
    public class XRStreamingFeatureEditor : Editor
    {
        private const string _clientFieldName = "_client";
        private const string _clientToolTip =
            "The target client of Android XR Streaming service.";

        private const string _autoStartFieldName = "_autoStartOnPlay";
        private const string _autoStartTooltip =
            "Indicate whether to auto-start client activity on entering play mode.";

        private const string _autoStopFieldName = "_autoStopOnExit";
        private const string _autoStopTooltip =
            "Indicate whether to auto-stop client activity on existing play mode.";

        private static readonly GUIContent _clientLabel = new GUIContent(
            "Streaming Client", _clientToolTip);

        private static readonly GUIContent _autoStartLabel = new GUIContent(
            "Auto Start On Play", _autoStartTooltip);

        private static readonly GUIContent _autoStopLabel = new GUIContent(
            "Auto Stop On Exit", _autoStopTooltip);

        private static readonly GUIContent _launchEmulatorButton = new GUIContent(
            "Launch Emulator");

        private SerializedProperty _client;
        private SerializedProperty _autoStart;
        private SerializedProperty _autoStop;

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            _client.enumValueIndex = EditorGUILayout.Popup(
                _clientLabel, _client.enumValueIndex,
                _client.enumDisplayNames);

            XRStreamingClient selectedClient = (XRStreamingClient)_client.enumValueIndex;
            EditorGUILayout.Foldout(true, string.Format("{0} Setings", selectedClient));
            int labelIndent = EditorGUI.indentLevel * 20;
            if (selectedClient == XRStreamingClient.Device)
            {
                // Configure PlayToDevice.
                GUILayout.BeginHorizontal();
                GUILayout.Space(labelIndent);
                _autoStart.boolValue = EditorGUILayout.Toggle(
                    _autoStartLabel, _autoStart.boolValue);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(labelIndent);
                _autoStop.boolValue = EditorGUILayout.Toggle(
                    _autoStopLabel, _autoStop.boolValue);
                GUILayout.EndHorizontal();
            }
            else if (selectedClient == XRStreamingClient.Emulator)
            {
                // Configure PlayToEmulator.
                GUILayout.BeginHorizontal();
                GUILayout.Space(labelIndent);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(_launchEmulatorButton, GUILayout.Width(150)))
                {
                    TryLaunchEmulator();
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                EditorGUILayout.HelpBox("Emulator is not yet available.", MessageType.Error);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            _client = serializedObject.FindProperty(_clientFieldName);
            _autoStart = serializedObject.FindProperty(_autoStartFieldName);
            _autoStop = serializedObject.FindProperty(_autoStopFieldName);
        }

        private void TryLaunchEmulator()
        {
            Debug.LogError("Emulator is not yet available.");
        }
    }
}
