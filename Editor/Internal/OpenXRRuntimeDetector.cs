// <copyright file="OpenXRRuntimeDetector.cs" company="Google LLC">
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
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using Microsoft.Win32;
    using UnityEngine;

    internal static class OpenXRRuntimeDetector
    {
        // Unity Environment Key, defined com.unity.xr.openxr/Editor/OpenXRRuntimerSelector.cs
        private const string _selectedRuntimeEnvKey = "XR_SELECTED_RUNTIME_JSON";

        private const string _availableRuntimesRegistryKey =
            @"SOFTWARE\Khronos\OpenXR\1\AvailableRuntimes";

        private const string _xrsRuntimeFilename = "xrs_openxr.dll";

        public static bool IsXrStreamingRuntimeAvailable()
        {
            var paths = GetXrStreamingRegisteredPaths();
            return paths != null && paths.Length > 0;
        }

        public static string GetXrStreamingRuntimeDisplayName()
        {
            var paths = GetXrStreamingRegisteredPaths();
            if (paths == null || paths.Length == 0)
            {
                return XRStreamingFeature.UiName;
            }

            RuntimeConfig runtimeConfig = GetRuntimeConfig(paths[0]);
            if (runtimeConfig == null || runtimeConfig.runtime == null ||
                string.IsNullOrEmpty(runtimeConfig.runtime.name))
            {
                return XRStreamingFeature.UiName;
            }
            else
            {
                return runtimeConfig.runtime.name;
            }
        }

        public static bool IsXrStreamingRuntimeSelected()
        {
            string enValue = Environment.GetEnvironmentVariable(_selectedRuntimeEnvKey);
            return IsXrStreamingRuntime(enValue);
        }

        public static void SelectXrStreamingRuntime()
        {
            var paths = GetXrStreamingRegisteredPaths();
            if (paths == null || paths.Length == 0)
            {
                Debug.LogError("Canot find XR Streaming Runtime.");
            }
            else if (paths.Length > 1)
            {
                Debug.LogError(
                    "Failed to select XR Streaming Runtime due to multiple options are found.");
            }
            else
            {
                Environment.SetEnvironmentVariable(_selectedRuntimeEnvKey, paths[0]);
                Debug.LogFormat(
                    "Set Play Mode OpenXR Runtime to {0}.", GetXrStreamingRuntimeDisplayName());
            }
        }

        public static string[] GetXrStreamingRegisteredPaths()
        {
            string[] availableRuntimes = GetAllRegisteredRuntimes();
            if (availableRuntimes == null || availableRuntimes.Length == 0)
            {
                return null;
            }

            List<string> paths = new List<string>();
            foreach (var availableRuntime in availableRuntimes)
            {
                if (IsXrStreamingRuntime(availableRuntime))
                {
                    paths.Add(availableRuntime);
                }
            }

            return paths.ToArray();
        }

        public static string[] GetAllRegisteredRuntimes()
        {
#if UNITY_EDITOR_WIN
            // Currently only support Windows.
            RegistryKey availableKeys = Registry.LocalMachine.OpenSubKey(
                _availableRuntimesRegistryKey, false);
            if (availableKeys != null) {
                return availableKeys.GetValueNames();
            }
            else
            {
                return null;
            }
#else
            return null;
#endif
        }

        public static bool IsXrStreamingRuntime(string jsonPath)
        {
            if (string.IsNullOrEmpty(jsonPath))
            {
                return false;
            }

            RuntimeConfig config = GetRuntimeConfig(jsonPath);
            if (config != null && config.runtime != null &&
                !string.IsNullOrEmpty(config.runtime.library_path) &&
                Path.GetFileName(config.runtime.library_path).Equals(_xrsRuntimeFilename))
            {
                return true;
            }

            return false;
        }

        public static RuntimeConfig GetRuntimeConfig(string jsonPath)
        {
            if (!File.Exists(jsonPath))
            {
                return null;
            }

            return JsonUtility.FromJson<RuntimeConfig>(File.ReadAllText(jsonPath));
        }

        /// <summary>
        /// Match OpenXR runtime's config.json.
        /// </summary>
        /// <remarks>
        /// Example of XR Streaming runtime:
        /// <code>
        /// {
        ///  "file_format_version": "1.0.0",
        ///  "runtime": {
        ///   "library_path": "C:\\Program Files\\Google\\XrStreaming\\xrs_openxr.dll",
        ///   "name": "openxr_runtime"
        ///  }
        /// }
        /// </code>
        /// </remarks>
        [SuppressMessage(
            "UnityRules.UnityStyleRules", "US1103:PublicFieldsMustHaveNoPrefix",
            Justification = "JSON property.")]
        [SuppressMessage(
            "UnityRules.UnityStyleRules", "US1107:PublicFieldsMustBeUpperCamelCase",
            Justification = "JSON property.")]
        [Serializable]
        internal class RuntimeConfig
        {
            public string file_format_version;
            public RuntimeEntry runtime;

            [Serializable]
            internal class RuntimeEntry
            {
                public string library_path;
                public string name;
            }
        }
    }
}
