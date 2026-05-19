// <copyright file="AndroidXRProjectValidationRules.cs" company="Google LLC">
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
    using System.Linq;
    using Unity.XR.CoreUtils.Editor;
    using UnityEditor;
    using UnityEditor.XR.OpenXR.Features;

    internal static class AndroidXRProjectValidationRules
    {
        const string _category = "Android XR (Extensions)";
        const string _playerSettings = "<b>Project Settings > Player Settings</b>";
        const string _openxrSettings = "<b>Project Settings > XR Plug-in Management > OpenXR</b>";
        const string _androidTab = "<b>Android</b> tab";
        const string _standaloneTab = "<b>Standalone</b> tab";

        [InitializeOnLoadMethod]
        static void AddAndroidXRProjectValidationRules()
        {
            var androidXRProjectRules = new[]
            {
                new BuildValidationRule
                {
                    Category = _category,
                    Message =
                        "Android XR requires resizable windows to render pop-ups correctly " +
                        "in immersive view, such as system permission requests.",
                    IsRuleEnabled = AndroidXRBuildUtils.IsAnyAndroidXRFeatureEnabledForAndroid,
                    CheckPredicate = () => PlayerSettings.Android.resizeableActivity,
                    FixItMessage = string.Format(
                        "Go to {0}, under {1}, enable <b>Resizable Activity</b>.",
                        _playerSettings, _androidTab),
                    FixIt = () => PlayerSettings.Android.resizeableActivity = true,
                    FixItAutomatic = true,
                    Error = true,
                },
                new BuildValidationRule
                {
                    Category = _category,
                    Message =
                        "Android XR features require Application Entry Point set to GameActivity.",
                    IsRuleEnabled = AndroidXRBuildUtils.IsAnyAndroidXRFeatureEnabledForAndroid,
                    CheckPredicate = () => PlayerSettings.Android.applicationEntry ==
                        AndroidApplicationEntry.GameActivity,
                    FixItMessage = string.Format(
                        "Go to {0}, under {1}, for <b>Application Entry Point</b>" +
                        "select only <b>GameActivity</b>.",
                        _playerSettings, _androidTab),
                    FixItAutomatic = true,
                    FixIt = () => PlayerSettings.Android.applicationEntry =
                        AndroidApplicationEntry.GameActivity,
                    Error = true,
                },
                GetSessionDependentRule(BuildTargetGroup.Android),
            };

            BuildValidator.AddRules(BuildTargetGroup.Android, androidXRProjectRules);

            const string xrsCategory = XRStreamingFeature.UiName;
            var standaloneProjectRules = new[]
            {
#if UNITY_EDITOR_WIN
                new BuildValidationRule
                {
                    Category = xrsCategory,
                    Message = string.Format(
                        "{0} feature is required for running Android XR in Editor <b>PlayMode</b>.",
                        XRStreamingFeature.UiName),
                    IsRuleEnabled = AndroidXRBuildUtils.IsAndroidXRRunningInEditor,
                    CheckPredicate = () => FeatureHelpers.GetFeatureWithIdForBuildTarget(
                        BuildTargetGroup.Standalone, XRStreamingFeature.FeatureId).enabled,
                    FixItMessage = string.Format(
                        "Go to {0}, under {1}, select <b>{2}</b>.",
                        _openxrSettings, _standaloneTab, XRStreamingFeature.UiName),
                    FixIt = () => FeatureHelpers.GetFeatureWithIdForBuildTarget(
                        BuildTargetGroup.Standalone, XRStreamingFeature.FeatureId).enabled = true,
                    Error = true,
                },
                new BuildValidationRule
                {
                    Category = xrsCategory,
                    Message = string.Format(
                        "XR Streaming Runtime is not available. " +
                        "Please follow instructions to install XR Streaming SDK."),
                    IsRuleEnabled = AndroidXRBuildUtils.IsAndroidXRRunningInEditor,
                    CheckPredicate = OpenXRRuntimeDetector.IsXrStreamingRuntimeAvailable,
                    Error = true,
                },
                new BuildValidationRule
                {
                    Category = xrsCategory,
                    Message = string.Format(
                        "XR Streaming Runtime is required for running Android XR in " +
                        "Editor <b>PlayMode</b>."),
                    IsRuleEnabled = () =>
                    {
                        return AndroidXRBuildUtils.IsAndroidXRRunningInEditor()
                            && OpenXRRuntimeDetector.IsXrStreamingRuntimeAvailable();
                    },
                    CheckPredicate = OpenXRRuntimeDetector.IsXrStreamingRuntimeSelected,
                    FixItMessage = string.Format(
                        "Go to {0}, under {1}, for <b>Play Mode OpenXR Runtime</b>, " +
                        " select <b>{2}</b>.",
                        _openxrSettings, _standaloneTab,
                        OpenXRRuntimeDetector.GetXrStreamingRuntimeDisplayName()),
                    FixIt = OpenXRRuntimeDetector.SelectXrStreamingRuntime,
                    Error = true,
                },
                GetSessionDependentRule(BuildTargetGroup.Standalone),
#else
                new BuildValidationRule
                {
                    Category = xrsCategory,
                    Message = string.Format(
                        "Android XR Features under {0} is only supported by Windows Editor.",
                        _standaloneTab),
                    IsRuleEnabled = () =>
                    {
                        return AndroidXRBuildUtils.IsAnyAndroidXRFeatureEnabledForBuildTarget(
                            BuildTargetGroup.Standalone);
                    },
                    CheckPredicate = () =>
                    {
                        return !AndroidXRBuildUtils.IsAnyAndroidXRFeatureEnabledForBuildTarget(
                            BuildTargetGroup.Standalone);
                    },
                    FixItMessage = string.Format(
                        "Go to {0}, under {1}, disable all features from <b>{2}</b> feature group.",
                        _openxrSettings, _standaloneTab, AndroidXRFeatureSet._uiName),
                    FixIt = () =>
                    {
                        var buildTarget = BuildTargetGroup.Standalone;
                        var featureSets =
                            OpenXRFeatureSetManager.FeatureSetsForBuildTarget(buildTarget);
                        foreach (var featureSet in featureSets)
                        {
                            if (featureSet.featureSetId.Equals(AndroidXRFeatureSet._featureSetId))
                            {
                                featureSet.isEnabled = false;
                                break;
                            }
                        }

                        var featureIds = OpenXRFeatureSetManager.GetFeatureSetWithId(
                            buildTarget, AndroidXRFeatureSet._featureSetId).featureIds;
                        var features = FeatureHelpers.GetFeaturesWithIdsForBuildTarget(
                            buildTarget, featureIds);
                        features.ToList().ForEach(feature => feature.enabled = false);
                    },
                    Error = false,
                },
#endif
            };

            BuildValidator.AddRules(BuildTargetGroup.Standalone, standaloneProjectRules);
        }

        static BuildValidationRule GetSessionDependentRule(BuildTargetGroup buildTarget)
        {
            return new BuildValidationRule
            {
                Category = _category,
                Message = string.Format(
                        "Android XR (Extensions) features use {0} for OpenXR lifecycle management.",
                        XRSessionFeature.UiName),
                IsRuleEnabled = () =>
                    AndroidXRBuildUtils.IsAnySessionDependentEnabled(buildTarget),
                CheckPredicate = () => FeatureHelpers.GetFeatureWithIdForBuildTarget(
                    buildTarget, XRSessionFeature.FeatureId).enabled,
                FixItMessage = string.Format(
                        "Go to {0}, under <b>{1}</b> tab, select <b>{2}</b>.",
                        _openxrSettings, buildTarget, XRSessionFeature.UiName),
                FixIt = () => FeatureHelpers.GetFeatureWithIdForBuildTarget(
                    buildTarget, XRSessionFeature.FeatureId).enabled = true,
                Error = true,
            };
        }
    }
}
