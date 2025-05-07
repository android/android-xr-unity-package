// <copyright file="XRFaceConfidenceRegion.cs" company="Google LLC">
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
    /// This is an enumeration of the face confidence regions.
    /// </summary>
    public enum XRFaceConfidenceRegion
    {
        /// <summary> The confidence of the accuracy of the lower face region. </summary>
        Lower = 0,

        /// <summary> The confidence of the accuracy of the left upper face region. </summary>
        LeftUpper = 1,

        /// <summary> TThe confidence of the accuracy of the right upper face region. </summary>
        RightUpper = 2,
    }
}
