// <copyright file="XRBodyJointSet.cs" company="Google LLC">
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

namespace Google.XR.Extensions
{
    using UnityEngine;

    /// <summary>
    /// The set of body joints to track by the platform.
    /// </summary>
    public enum XRBodyJointSet
    {
        /// <summary>
        /// The joint set covering the upper body.
        /// </summary>
        UpperBody = 0,

        /// <summary>
        /// The joint set covering the full body.
        /// </summary>
        FullBody = 1,
    }
}
