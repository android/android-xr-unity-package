// <copyright file="BodyTrackingController.cs" company="Google LLC">
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

namespace Google.XR.Extensions.Samples.BodyTracking
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;
    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.ARSubsystems;
    using UnityEngine.XR.OpenXR;

    /// <summary>
    /// An sample of body tracking with <see cref="ARHumanBodyManager"/>.
    /// </summary>
    [RequireComponent(typeof(AndroidXRPermissionUtil))]
    public class BodyTrackingController : MonoBehaviour
    {
        /// <summary>
        /// Text mesh to display debug information.
        /// </summary>
        public TextMesh DebugText;

        /// <summary>
        /// The <see cref="ARHumanBodyManager"/> component in the scene.
        /// </summary>
        public ARHumanBodyManager BodyManager;

        private List<TrackableId> _bodies = new List<TrackableId>();
        private int _added = 0;
        private int _updated = 0;
        private int _removed = 0;

        private AndroidXRPermissionUtil _permissionUtil;
        private StringBuilder _stringBuilder = new StringBuilder();

        /// <summary>
        /// Called from <see cref="ARHumanBodyManager"/> component callback.
        /// </summary>
        /// <param name="eventArgs">Body change event arguments.</param>
        public void OnBodiesChanged(ARTrackablesChangedEventArgs<ARHumanBody> eventArgs)
        {
            _added = eventArgs.added.Count;
            _updated = eventArgs.updated.Count;
            _removed = eventArgs.removed.Count;
            _bodies = eventArgs.updated.Select(body => body.trackableId).ToList();
            foreach (var body in eventArgs.added)
            {
                Debug.Log(GetBodyDebugInfo(body));
            }
        }

        private void OnEnable()
        {
            _permissionUtil = GetComponent<AndroidXRPermissionUtil>();
            if (BodyManager == null)
            {
                Debug.LogError("ARHumanBodyManager is null!");
            }
        }

        private void OnDisable()
        {
            _bodies.Clear();
        }

        private void Update()
        {
            if (!_permissionUtil.AllPermissionGranted())
            {
                return;
            }

            _stringBuilder.Clear();
            UpdateBodies();
            if (DebugText != null && _stringBuilder.Length > 0)
            {
                DebugText.text = _stringBuilder.ToString();
            }
        }

        private void UpdateBodies()
        {
            if (XRBodyTrackingFeature.IsExtensionEnabled == null)
            {
                _stringBuilder.Append($"{XRBodyTrackingFeature.UiName} is initializing.");
                return;
            }
            else if (!XRBodyTrackingFeature.IsExtensionEnabled.Value)
            {
                _stringBuilder.Append(
                    $"{XRBodyTrackingFeature.ExtensionString} is not enabled.\n");
            }
            else if (BodyManager == null)
            {
                _stringBuilder.Append("Cannot find ARHumanBodyManager.\n");
            }
            else
            {
                _stringBuilder.AppendFormat(
                    "Subsystem: {0}\n", BodyManager.subsystem.GetType().Name);
                _stringBuilder.AppendFormat(
                    $"Bodies: ({_added}, {_updated}, {_removed})\n" +
                    $"{string.Join("\n", _bodies.Select(id => id.subId1).ToArray())}\n");
            }
        }

        private string GetBodyDebugInfo(ARHumanBody body)
        {
            if (body == null)
            {
                return string.Empty;
            }

            return body.ToString();
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(
                UnityEditor.BuildTargetGroup.Android);
            var bodyFeature = settings.GetFeature<XRBodyTrackingFeature>();
            if (bodyFeature == null)
            {
                Debug.LogErrorFormat(
                    "Cannot find {0} targeting Android platform.", XRBodyTrackingFeature.UiName);
                return;
            }
            else if (!bodyFeature.enabled)
            {
                Debug.LogWarningFormat(
                    "{0} is disabled. BodyTracking sample will not work correctly.",
                    XRBodyTrackingFeature.UiName);
            }
#endif // UNITY_EDITOR
        }
    }
}
