// <copyright file="FoveationController.cs" company="Google LLC">
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

namespace Google.XR.Extensions.Samples.Foveation
{
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.XR.OpenXR;

    /// <summary>
    /// Used to demo the foveation API.
    /// </summary>
    public class FoveationController : MonoBehaviour
    {
        /// <summary>
        /// The Text component of the "Text Center" game object.
        /// </summary>
        public Text ProfileText;

        int _foveationProfile;
        float _timer;

        private void Update()
        {
            if (_timer <= 0)
            {
                // There are 4 levels.
                XRFoveationLevel level = (XRFoveationLevel)_foveationProfile;
                XRFoveationFeature.FBSetFoveationLevel(level, 0, false);
                ProfileText.text = "Foveation Profile\nLevel=" + level;

                _foveationProfile = (_foveationProfile + 1) % 4;
                const float profileShowDuration = 5.0f;
                _timer = profileShowDuration;
            }

            _timer -= Time.deltaTime;
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(
                UnityEditor.BuildTargetGroup.Android);
            var foveation = settings.GetFeature<XRFoveationFeature>();
            if (foveation == null)
            {
                Debug.LogErrorFormat(
                    "Cannot find {0} targeting Android platform.", XRFoveationFeature.UiName);
                return;
            }
            else if (!foveation.enabled)
            {
                Debug.LogWarningFormat(
                    "{0} is disabled. Foveation sample will not work correctly.",
                    XRFoveationFeature.UiName);
            }
#endif // UNITY_EDITOR
        }
    }
}
