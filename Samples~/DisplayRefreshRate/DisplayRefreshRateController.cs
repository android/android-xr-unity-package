// <copyright file="DisplayRefreshRateController.cs" company="Google LLC">
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

namespace Google.XR.Extensions.Samples.DisplayRefreshRate
{
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// Displays the refresh rate and cycles through refresh rates.
    /// </summary>
    public class DisplayRefreshRateController : MonoBehaviour
    {
        /// <summary>
        /// Display all of the availble refresh rates supported by the runtime.
        /// </summary>
        public TextMesh DebugTextRefreshRates;

        /// <summary>
        /// Display the current refresh rate.
        /// </summary>
        public TextMesh DebugTextCurrentRate;

        private void Start()
        {
            XRDisplayRefreshRateInfo info;
            XRDisplayRefreshRateFeature.GetDisplayRefreshRateInfo(out info);

            StartCoroutine(ChangeRefreshRate(info));

            DebugTextRefreshRates.text = "Supported Refresh Rates:";
            float[] rates = info.SupportedRefreshRates;
            for (int x = 0; x < info.SupportedRefreshRates.Length; x++)
            {
                DebugTextRefreshRates.text += rates[x].ToString();
                DebugTextRefreshRates.text +=
                    (x + 1 < info.SupportedRefreshRates.Length) ? ", " : string.Empty;
            }
        }


        IEnumerator ChangeRefreshRate(XRDisplayRefreshRateInfo info)
        {
            // Loop until the app is terminated.
            while (true)
            {
                for (int i = 0; i < info.SupportedRefreshRates.Length; i++)
                {
                    XRDisplayRefreshRateFeature.RequestDisplayRefreshRate(
                        info.SupportedRefreshRates[i]);
                    yield return new WaitForSeconds(5);
                }
            }
        }

        void Update()
        {
            XRDisplayRefreshRateInfo info;
            if (XRDisplayRefreshRateFeature.GetDisplayRefreshRateInfo(out info))
            {
                DebugTextCurrentRate.text = $"Current Refresh Rate: {info.CurrentRefreshRate}";
            }
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}
