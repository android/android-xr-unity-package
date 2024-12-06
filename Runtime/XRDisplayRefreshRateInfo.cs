// <copyright file="XRDisplayRefreshRateInfo.cs" company="Google LLC">
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

namespace Google.XR.Extensions
{
    /// <summary>
    /// Contains the supported refresh rates and the current refresh rate.
    /// </summary>
    public struct XRDisplayRefreshRateInfo
    {
        /// <summary>The display refresh rates supported by the device.</summary>
        public float[] SupportedRefreshRates;

        /// <summary>The current refresh rate.</summary>
        public float CurrentRefreshRate;
    }
}
