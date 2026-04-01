// <copyright file="CmdUtils.cs" company="Google LLC">
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

#if UNITY_EDITOR
namespace Google.XR.Extensions.Internal
{
    using System.Text;
    using UnityEditor.Android;
    using UnityEngine;

    internal static class CmdUtils
    {
        private static readonly string _xrsClientPackage =
            "com.google.vr.streaming.client";

        // LINT.IfChange
        private static readonly string _xrsClientActivity =
            "com.google.vr.streaming.client.XrStreamingActivity";
        // LINT.ThenChange(//depot/google3/java/com/google/vr/streaming/client/AndroidManifestRelay.xml)

        private static readonly string _pStartClientActivity =
            $"shell am start -n {_xrsClientPackage}/{_xrsClientActivity}";

        private static readonly string _pStopClientAcitivity =
            $"shell am force-stop {_xrsClientPackage}";

        private static readonly string _pCheckClientActivity =
            $"shell \"dumpsys activity activities | grep {_xrsClientActivity}\"";

        public static bool IsAdbAvailable()
        {
            RunCommand(GetAdbPath(), "--version", out string output, out string error);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError("adb --version:\n" + error);
                return false;
            }

            return true;
        }

        public static bool TryStartStreamingClient()
        {
            RunCommand(GetAdbPath(), _pCheckClientActivity, out string output, out string error);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogErrorFormat("adb {0}:\n{1}", _pCheckClientActivity, error);
                return false;
            }

            if (!string.IsNullOrEmpty(output))
            {
                return true;
            }

            RunCommand(GetAdbPath(), _pStartClientActivity, out output, out error);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogErrorFormat("adb {0}:\n{1}", _pStartClientActivity, error);
                return false;
            }

            Debug.Log(output);
            return true;
        }

        public static bool TryStopStreamingClient()
        {
            RunCommand(GetAdbPath(), _pStopClientAcitivity, out string output, out string error);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogErrorFormat("adb {0}:\n{1}", _pStopClientAcitivity, error);
                return false;
            }

            Debug.LogFormat("adb {0}", _pStopClientAcitivity);
            return true;
        }

        /// <summary>
        /// Run a terminal command.
        /// </summary>
        /// <param name="exeName">File name for the executable.</param>
        /// <param name="arguments">Command line arguments, space delimited.</param>
        /// <param name="output">Filled out with the result as printed to stdout.</param>
        /// <param name="error">Filled out with the result as printed to stderr.</param>
        /// <param name="hideWindow">Determine if hiding terminal windows. Defaults to <c>true</c>.
        /// </param>
        internal static void RunCommand(
            string exeName,
            string arguments,
            out string output,
            out string error,
            bool hideWindow = true)
        {
            using (var process = new System.Diagnostics.Process())
            {
                var startInfo = new System.Diagnostics.ProcessStartInfo(exeName, arguments);
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardError = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.CreateNoWindow = hideWindow;
                process.StartInfo = startInfo;

                var outputBuilder = new StringBuilder();
                var errorBuilder = new StringBuilder();
                process.OutputDataReceived += (sender, ef) => outputBuilder.AppendLine(ef.Data);
                process.ErrorDataReceived += (sender, ef) => errorBuilder.AppendLine(ef.Data);

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                process.Close();

                // Trims the output strings to make comparison easier.
                output = outputBuilder.ToString().Trim();
                error = errorBuilder.ToString().Trim();
            }
        }

        internal static string GetAdbPath()
        {
            if (string.IsNullOrEmpty(AndroidExternalToolsSettings.sdkRootPath))
            {
                // Using system evnrionement variables PATH.
                return "adb";
            }
            else
            {
                // Using Unity Preferences > External Tools > Android.
                return AndroidExternalToolsSettings.sdkRootPath + "/platform-tools/adb";
            }
        }
    }
}
#endif // UNITY_EDITOR
