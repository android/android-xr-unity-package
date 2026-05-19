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
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Google.XR.Extensions.Internal;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Custom Editor for <see cref="XRSessionFeature"/>.
    /// </summary>
    [CustomEditor(typeof(XRSessionFeature))]
    internal class XRSessionFeatureEditor : Editor
    {
        private const float _rootWidth = 200f;
        private const float _indentWidth = 150f;

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

        private const string _useSpatialApiFieldName = "_useSpatialApi";
        private const string _useSpatialApiTooltip =
            "Configure <c>{0}</c> element for <b>{1}</b> in your application's manifest file.";

        private const string _spatialApiRequiredFieldName = "_spatialApiRequired";
        private const string _spatialApiRequiredTooltip =
            "Select <b>Required</b> if your application requires OpenXR feature support.";

        private const string _spatialApiTargetVersionFieldName = "_spatialApiTargetVersion";
        private const string _spatialApiTargetVersionTooltip =
            "Configure the target API level to meet OpenXR feature requirements.";

        private static readonly GUIContent _immersiveXRLabel = new GUIContent(
            "Immersive XR", _immersiveXRTooltip);

        private static readonly GUIContent _subsamplingLabel = new GUIContent(
            "Subsampling (Vulkan)", _subsamplingTooltip);

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules",
            "SA1118:ParameterMustNotSpanMultipleLines",
            Justification = "Bypass readonly fields.")]
        private static readonly GUIContent _useSpatialApiLabel = new GUIContent(
            "Use XR Spatial API", string.Format(_useSpatialApiTooltip,
            AndroidXRManifest.Element.UsesFeature, AndroidXRManifest.XrApiSpatial));

        private static readonly GUIContent _spatialApiRequiredLabel = new GUIContent(
            "Requirement", _spatialApiRequiredTooltip);

        private static readonly GUIContent _spatialApiTargetVersionLabel = new GUIContent(
            "Target API Level", _spatialApiTargetVersionTooltip);

        private SerializedProperty _immersiveXR;
        private SerializedProperty _subsampling;
        private SerializedProperty _useSpatialApi;
        private SerializedProperty _spatialApiRequired;
        private SerializedProperty _spatialApiTargetVersion;

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            EditorGUIUtility.labelWidth = _rootWidth;

            serializedObject.Update();
            _immersiveXR.boolValue = EditorGUILayout.Toggle(
                _immersiveXRLabel, _immersiveXR.boolValue);

            _useSpatialApi.boolValue = EditorGUILayout.Toggle(
                _useSpatialApiLabel, _useSpatialApi.boolValue);
            if (_useSpatialApi.boolValue)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUIUtility.labelWidth = _indentWidth;
                    ApiRequirement requirement = _spatialApiRequired.boolValue.ToRequirement();
                    requirement = (ApiRequirement)EditorGUILayout.EnumPopup(
                        _spatialApiRequiredLabel, requirement);
                    _spatialApiRequired.boolValue = requirement.ToRequired();

                    XRSpatialSdkVersions version =
                        (XRSpatialSdkVersions)_spatialApiTargetVersion.intValue;
                    version = (XRSpatialSdkVersions)EditorGUILayout.EnumPopup(
                        _spatialApiTargetVersionLabel, version);
                    _spatialApiTargetVersion.intValue = (int)version;

                    EditorGUILayout.Separator();

                    XRSpatialSdkVersions minVersion = XRSpatialSdkVersions.XRSpatialApiLevel1;
                    List<string> featureNames = new List<string>();
                    if (!version.ValidateActiveFeatures(ref minVersion, ref featureNames))
                    {
                        EditorGUILayout.HelpBox(string.Format(
                            "Following feature(s) are not supported by {0} which " +
                            "may not work properly at runtime: {2}.\n" +
                            "Recommend to select {1} or handle unsupported cases accordingly.",
                            version.GetDisplayOption(), minVersion.GetDisplayOption(),
                            string.Join(", ", featureNames)), MessageType.Warning);

                        EditorGUILayout.Separator();
                    }
                }

                EditorGUIUtility.labelWidth = _rootWidth;
            }

            _subsampling.boolValue = EditorGUILayout.Toggle(
                _subsamplingLabel, _subsampling.boolValue);
            if (_subsampling.boolValue)
            {
                EditorGUILayout.Separator();
                EditorGUILayout.HelpBox(string.Format(
                    "{0} is deprecated and will not take effect.\n" +
                    "Please use {1} from {2} package {3} feature instead.",
                    _subsamplingLabel.text,
                    "TrySetSubsampledLayoutEnabled(bool)",
                    "OpenXR Plugin",
                    "Foveated Rendering"),
                    MessageType.Error);
            }

            serializedObject.ApplyModifiedProperties();

            EditorGUIUtility.labelWidth = 0f;
        }

        private void OnEnable()
        {
            _immersiveXR = serializedObject.FindProperty(_immersiveXRFieldName);
            _subsampling = serializedObject.FindProperty(_subsamplingFieldName);
            _useSpatialApi = serializedObject.FindProperty(_useSpatialApiFieldName);
            _spatialApiRequired = serializedObject.FindProperty(_spatialApiRequiredFieldName);
            _spatialApiTargetVersion =
                serializedObject.FindProperty(_spatialApiTargetVersionFieldName);
        }
    }
}
