// <copyright file="SpatialContainerBuildHook.cs" company="Google LLC">
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

namespace Google.XR.Extensions.Samples.SpatialContainer.Editor
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Xml;
    using Google.XR.Extensions;
    using UnityEditor;
    using UnityEditor.Android;
    using UnityEngine;
    using UnityEngine.XR.OpenXR;

    /// <summary>
    /// Intercepts the Android build process to inject the Jetpack Compose XR
    /// Launcher and necessary Gradle dependencies into the exported project.
    /// </summary>
    public class SpatialContainerBuildHook : IPostGenerateGradleAndroidProject
    {
        /// <summary>
        /// Gets the callback order for this post-build hook.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "UnityRules.UnityStyleRules",
            "US1109:PublicPropertiesMustBeUpperCamelCase",
            Justification = "Unity IPostGenerateGradleAndroidProject interface implementation.")]
        public int callbackOrder => 1;

        /// <summary>
        /// Callback executed after the Gradle Android project is generated.
        /// </summary>
        /// <param name="path">The path to the generated Gradle project.</param>
        public void OnPostGenerateGradleAndroidProject(string path)
        {
            OpenXRSettings settings =
                OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
            if (settings == null)
            {
                return;
            }

            XRSpatialContainerFeature spatialContainerFeature =
                settings.GetFeature<XRSpatialContainerFeature>();
            if (spatialContainerFeature == null || !spatialContainerFeature.enabled)
            {
                return;
            }

            SpatialContainerSettings spatialSettings = null;
            string[] guids = AssetDatabase.FindAssets("t:SpatialContainerSettings");
            if (guids.Length > 0)
            {
                string settingsPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                spatialSettings =
                    AssetDatabase.LoadAssetAtPath<SpatialContainerSettings>(settingsPath);
            }

            if (spatialSettings != null && !spatialSettings.InjectJxrLauncher)
            {
                Debug.Log(
                    "SpatialContainerSettings.InjectJxrLauncher is false. Skipping launcher " +
                    "injection.");
                return;
            }

            Debug.Log("SpatialContainer build detected! Injecting launcher...");

            string rootProjectPath = Path.Combine(path, "..");

            string sourceKotlinPath = FindKotlinLauncherPath();
            if (string.IsNullOrEmpty(sourceKotlinPath))
            {
                Debug.LogError("Could not find JxrLauncherActivity.kt! Aborting injection.");
                return;
            }

            string destFolder = Path.Combine(
                path, "src/main/java/com/unity3d/launcher");
            Directory.CreateDirectory(destFolder);

            string destFile = Path.Combine(destFolder, "JxrLauncherActivity.kt");
            File.Copy(sourceKotlinPath, destFile, true);

            // AGP 9.1.1 / Gradle 9.3.1 introduces android.builtInKotlin=true to
            // replace org.jetbrains.kotlin.android library.
            bool useBuiltInKotlin =
                UpdateGradleProperties(Path.Combine(rootProjectPath, "gradle.properties"));
            UpdateRootGradle(Path.Combine(rootProjectPath, "build.gradle"), useBuiltInKotlin);
            UpdateLibraryGradle(Path.Combine(path, "build.gradle"), useBuiltInKotlin);
            ModifyManifest(Path.Combine(path, "src/main/AndroidManifest.xml"));
        }

        private string FindKotlinLauncherPath()
        {
            string[] guids = AssetDatabase.FindAssets("JxrLauncherActivity");
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileName(assetPath) == "JxrLauncherActivity.kt")
                {
                    return Path.GetFullPath(assetPath);
                }
            }

            return null;
        }

        private bool UpdateGradleProperties(string propertiesPath)
        {
            if (!File.Exists(propertiesPath))
            {
                return false;
            }

            List<string> lines = File.ReadAllLines(propertiesPath).ToList();
            bool hasBuiltInKotlin = false;

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Trim().StartsWith("android.builtInKotlin="))
                {
                    lines[i] = "android.builtInKotlin=true";
                    hasBuiltInKotlin = true;
                }
            }

            if (hasBuiltInKotlin)
            {
                File.WriteAllLines(propertiesPath, lines);
            }

            return hasBuiltInKotlin;
        }

        private void UpdateRootGradle(string gradlePath, bool useBuiltInKotlin)
        {
            List<string> lines = File.ReadAllLines(gradlePath).ToList();

            // Remove existing kotlin plugins to avoid version and builtInKotlin conflicts
            lines.RemoveAll(
                l => Regex.IsMatch(l, @"id\s+'org\.jetbrains\.kotlin\.(android|plugin\.compose)'"));

            List<string> plugins = new List<string>();
            if (!useBuiltInKotlin)
            {
                plugins.Add("org.jetbrains.kotlin.android");
            }

            plugins.Add("org.jetbrains.kotlin.plugin.compose");

            int pluginsBlockIdx = lines.FindIndex(l => Regex.IsMatch(l, @"^plugins\s*\{"));

            var pluginLines = plugins.Select(p => $"    id '{p}' version '2.2.20' apply false");

            if (pluginsBlockIdx != -1)
            {
                lines.InsertRange(pluginsBlockIdx + 1, pluginLines);
            }
            else
            {
                lines.Insert(0, "}");
                lines.InsertRange(0, pluginLines);
                lines.Insert(0, "plugins {");
            }

            File.WriteAllLines(gradlePath, lines);
        }

        private void UpdateLibraryGradle(string gradlePath, bool useBuiltInKotlin)
        {
            List<string> lines = File.ReadAllLines(gradlePath).ToList();

            if (useBuiltInKotlin)
            {
                // Remove external kotlin android plugin if present
                // (builtInKotlin handles it in AGP 9+)
                lines.RemoveAll(
                    l => Regex.IsMatch(l, @"^apply plugin:\s*'org\.jetbrains\.kotlin\.android'"));
            }

            if (lines.Any(l => l.Contains("org.jetbrains.kotlin.plugin.compose")))
            {
                File.WriteAllLines(gradlePath, lines);
                return;
            }

            List<string> plugins = new List<string>();
            if (!useBuiltInKotlin)
            {
                plugins.Add("org.jetbrains.kotlin.android");
            }

            plugins.Add("org.jetbrains.kotlin.plugin.compose");

            int applyIdx = lines.FindIndex(
                l => Regex.IsMatch(l, @"^apply plugin:\s*'com\.android\.library'"));

            if (applyIdx != -1)
            {
                // Format and inject all plugins in a single pass
                lines.InsertRange(applyIdx + 1, plugins.Select(p => $"apply plugin: '{p}'"));
            }

            string[] composeDeps =
            {
                "platform('androidx.compose:compose-bom:2024.04.01')",
                "'androidx.compose.ui:ui'",
                "'androidx.compose.material3:material3'",
                "'androidx.activity:activity-compose:1.9.0'",
                "'androidx.xr.compose:compose:1.0.0-alpha16'"
            };

            int depIdx = lines.FindIndex(l => Regex.IsMatch(l, @"^dependencies\s*\{"));
            if (depIdx != -1)
            {
                // Format and inject all dependencies in a single pass
                lines.InsertRange(depIdx + 1, composeDeps.Select(d => $"    implementation {d}"));
            }

            int androidIdx = lines.FindIndex(l => Regex.IsMatch(l, @"^android\s*\{"));
            if (androidIdx != -1)
            {
                string[] buildFeatures =
                {
                    "    buildFeatures {",
                    "        compose true",
                    "    }"
                };
                lines.InsertRange(androidIdx + 1, buildFeatures);
            }

            File.WriteAllLines(gradlePath, lines);
        }

        private void ModifyManifest(string manifestPath)
        {
            if (!File.Exists(manifestPath))
            {
                return;
            }

            XmlDocument doc = new XmlDocument();
            doc.Load(manifestPath);

            XmlNamespaceManager nsMgr = new XmlNamespaceManager(doc.NameTable);
            nsMgr.AddNamespace("android", "http://schemas.android.com/apk/res/android");

            XmlNodeList apps = doc.GetElementsByTagName("application");
            if (apps.Count == 0)
            {
                return;
            }

            XmlNode appNode = apps[0];

            XmlNode unityActivity = null;
            bool hasJxrLauncher = false;

            foreach (XmlNode child in appNode.ChildNodes)
            {
                if (child.Name != "activity")
                {
                    continue;
                }

                XmlAttribute nameAttr = child.Attributes["android:name"];
                if (nameAttr == null)
                {
                    continue;
                }

                if (nameAttr.Value == "com.unity3d.player.UnityPlayerGameActivity")
                {
                    unityActivity = child;
                }
                else if (nameAttr.Value == "com.unity3d.launcher.JxrLauncherActivity")
                {
                    hasJxrLauncher = true;
                }
            }

            if (unityActivity != null)
            {
                XmlAttribute themeAttr = unityActivity.Attributes["android:theme"];
                if (themeAttr != null)
                {
                    themeAttr.Value = "@style/BaseUnityGameActivityTheme.Translucent";
                }

                XmlAttribute launchModeAttr = unityActivity.Attributes["android:launchMode"];
                if (launchModeAttr != null)
                {
                    unityActivity.Attributes.Remove(launchModeAttr);
                }

                foreach (XmlNode child in unityActivity.ChildNodes)
                {
                    if (child.Name == "property")
                    {
                        XmlAttribute nameAttr = child.Attributes["android:name"];
                        if (nameAttr != null &&
                            nameAttr.Value == "android.window.PROPERTY_XR_ACTIVITY_START_MODE")
                        {
                            XmlAttribute valueAttr = child.Attributes["android:value"];
                            if (valueAttr == null)
                            {
                                SetAndroidAttr(
                                    doc, (XmlElement)child, "value",
                                    "XR_ACTIVITY_START_MODE_FULL_SPACE_MANAGED");
                            }
                            else
                            {
                                valueAttr.Value = "XR_ACTIVITY_START_MODE_FULL_SPACE_MANAGED";
                            }
                        }
                    }
                    else if (child.Name == "intent-filter")
                    {
                        var nodesToRemove = new List<XmlNode>();
                        foreach (XmlNode filterChild in child.ChildNodes)
                        {
                            if (filterChild.Name != "category")
                            {
                                continue;
                            }

                            XmlAttribute attr = filterChild.Attributes["android:name"];
                            if (attr != null && (
                                attr.Value == "android.intent.category.LAUNCHER" ||
                                attr.Value == "android.intent.category.LEANBACK_LAUNCHER"))
                            {
                                nodesToRemove.Add(filterChild);
                            }
                        }

                        foreach (var node in nodesToRemove)
                        {
                            child.RemoveChild(node);
                        }
                    }
                }
            }

            if (!hasJxrLauncher)
            {
                XmlElement myActivity = doc.CreateElement("activity");

                SetAndroidAttr(doc, myActivity, "name",
                    "com.unity3d.launcher.JxrLauncherActivity");
                SetAndroidAttr(
                    doc, myActivity, "theme", "@style/Theme.AppCompat.Light.NoActionBar");
                SetAndroidAttr(doc, myActivity, "exported", "true");
                SetAndroidAttr(doc, myActivity, "launchMode", "singleTask");
                SetAndroidAttr(
                    doc, myActivity, "taskAffinity", "com.google.SpatialContainerLauncher");

                string config = "orientation|screenSize|screenLayout|keyboardHidden";
                SetAndroidAttr(doc, myActivity, "configChanges", config);

                XmlElement intentFilter = doc.CreateElement("intent-filter");

                XmlElement actionMain = doc.CreateElement("action");
                SetAndroidAttr(doc, actionMain, "name", "android.intent.action.MAIN");
                intentFilter.AppendChild(actionMain);

                XmlElement catLauncher = doc.CreateElement("category");
                SetAndroidAttr(doc, catLauncher, "name", "android.intent.category.LAUNCHER");
                intentFilter.AppendChild(catLauncher);

                XmlElement catLeanback = doc.CreateElement("category");
                SetAndroidAttr(
                    doc, catLeanback, "name", "android.intent.category.LEANBACK_LAUNCHER");
                intentFilter.AppendChild(catLeanback);

                myActivity.AppendChild(intentFilter);

                XmlElement propXr = doc.CreateElement("property");
                SetAndroidAttr(
                    doc, propXr, "name", "android.window.PROPERTY_XR_ACTIVITY_START_MODE");
                SetAndroidAttr(
                    doc, propXr, "value", "XR_ACTIVITY_START_MODE_FULL_SPACE_MANAGED");
                myActivity.AppendChild(propXr);

                appNode.AppendChild(myActivity);
            }

            doc.Save(manifestPath);
        }

        private void SetAndroidAttr(XmlDocument doc, XmlElement elem, string name, string val)
        {
            XmlAttribute attr = doc.CreateAttribute(
                "android", name, "http://schemas.android.com/apk/res/android");
            attr.Value = val;
            elem.Attributes.Append(attr);
        }
    }
}
