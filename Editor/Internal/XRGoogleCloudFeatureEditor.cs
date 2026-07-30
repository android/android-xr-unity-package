// <copyright file="XRGoogleCloudFeatureEditor.cs" company="Google LLC">
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

namespace Google.XR.Extensions.Editor.Internal
{
    using Google.XR.Extensions.Internal;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Custom editor for <see cref="XRGoogleCloudFeature"/>.
    /// </summary>
    [CustomEditor(typeof(XRGoogleCloudFeature))]
    public class XRGoogleCloudFeatureEditor : Editor
    {
        private const string _strategyFieldName = "_strategy";
        private const string _strategyTooltip =
            "The strategy to configure Google Cloud authentication.";

        private const string _credentialFieldName = "_credential";
        private const string _credentialTooltip =
            "The credential passed to the authentication.";

        private static readonly GUIContent _strategyLabel = new GUIContent(
            "Authentication Strategy", _strategyTooltip);

        private static readonly GUIContent _credentialLabel = new GUIContent(
            "Credential", _credentialTooltip);

        private static readonly string _featureName = typeof(XRGoogleCloudFeature).Name;

        private SerializedProperty _strategy;
        private SerializedProperty _credential;

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            XRAuthenticationStrategy strategy =
                (XRAuthenticationStrategy)_strategy.intValue;
            strategy = (XRAuthenticationStrategy)EditorGUILayout.EnumPopup(
                _strategyLabel, strategy);
            _strategy.intValue = (int)strategy;
            if (strategy == XRAuthenticationStrategy.None)
            {
                EditorGUILayout.HelpBox(
                    "Authentication Strategy is not in use. \n" +
                    "To access Google Cloud APIs at runtime, " +
                    $"you can use {_featureName} to set authentication manually.",
                    MessageType.Warning);
            }
            else if (strategy != XRAuthenticationStrategy.Keyless)
            {
                _credential.stringValue = EditorGUILayout.TextField(
                    _credentialLabel, _credential.stringValue);
                if (string.IsNullOrEmpty(_credential.stringValue))
                {
                    EditorGUILayout.HelpBox(
                        "Credential is empty. " +
                        "Google Cloud APIs will not be available.",
                        MessageType.Warning);
                }
                else if (!XRGoogleCloudAuthFuture.ValidateCredential(_credential.stringValue))
                {
                    EditorGUILayout.HelpBox(
                        "Credential has invalid characters. " +
                        "Google Cloud APIs will not be available.",
                        MessageType.Warning);
                }
            }

            if (strategy == XRAuthenticationStrategy.AuthToken)
            {
                EditorGUILayout.HelpBox(
                    "An unexpired credential is required for Authentication Token strategy.\n" +
                    $"You can use {_featureName} to update it manually before expiration.",
                    MessageType.Info);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            _strategy = serializedObject.FindProperty(_strategyFieldName);
            _credential = serializedObject.FindProperty(_credentialFieldName);
        }
    }
}
