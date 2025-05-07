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
    using Unity.XR.CoreUtils.Editor;
    using UnityEditor;
    using UnityEditor.XR.OpenXR.Features;

    internal static class AndroidXRProjectValidationRules
    {
        const string _catergory = "Android XR (Extensions)";

        [InitializeOnLoadMethod]
        static void AddAndroidXRProjectValidationRules()
        {
            var androidXRProjectRules = new[]
            {
                new BuildValidationRule
                {
                    Category = _catergory,
                    Message =
                        "Android XR reuiqres resizable windows to render pop-ups correctly " +
                        "in immersive view, such as system permission requests.",
                    IsRuleEnabled = AndroidXRBuildUtils.IsAnyAndroidXRFeatureEnabled,
                    CheckPredicate = () => PlayerSettings.Android.resizeableActivity,
                    FixItMessage =
                        "Go to <b>Project Settings > Player Settings</b>, " +
                        "selet <b>Android</b> tab, then enable <b>Resizable Activity</b>.",
                    FixIt = () => PlayerSettings.Android.resizeableActivity = true,
                    FixItAutomatic = true,
                    Error = true,
                },
                new BuildValidationRule
                {
                    Category = _catergory,
                    Message =
                        "Android XR features require Application Entry Point set to GameActivity.",
                    IsRuleEnabled = AndroidXRBuildUtils.IsAnyAndroidXRFeatureEnabled,
                    CheckPredicate = () => PlayerSettings.Android.applicationEntry ==
                        AndroidApplicationEntry.GameActivity,
                    FixItMessage =
                        "Go to <b>Project Settings > Player Settings</b>, " +
                        "selet <b>Android</b> tab, in <b>Application Entry Point</b>, " +
                        "select only <b>GameActivity</b>.",
                    FixItAutomatic = true,
                    FixIt = () => PlayerSettings.Android.applicationEntry =
                        AndroidApplicationEntry.GameActivity,
                    Error = true,
                },
                new BuildValidationRule
                {
                    Category = _catergory,
                    Message = string.Format(
                        "Android XR (Extensions) features use {0} for OpenXR lifecycle management.",
                        XRSessionFeature.UiName),
                    IsRuleEnabled = AndroidXRBuildUtils.IsAnySessionDependentEnabled,
                    CheckPredicate = () => FeatureHelpers.GetFeatureWithIdForBuildTarget(
                        BuildTargetGroup.Android, XRSessionFeature.FeatureId).enabled,
                    FixItMessage = string.Format(
                        "Go to <b>Project Settings > XR Plug-in Management > OpenXR </b>, " +
                        "selet <b>Android</b> tab, select <b>{0}</b>.",
                        XRSessionFeature.UiName),
                    FixIt = () => FeatureHelpers.GetFeatureWithIdForBuildTarget(
                        BuildTargetGroup.Android, XRSessionFeature.FeatureId).enabled = true,
                    Error = true,
                }
            };

            BuildValidator.AddRules(BuildTargetGroup.Android, androidXRProjectRules);
        }
    }
}
