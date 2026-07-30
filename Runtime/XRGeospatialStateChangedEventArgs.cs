// <copyright file="XRGeospatialStateChangedEventArgs.cs" company="Google LLC">
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
    using System;

    /// <summary>
    /// Container for <see cref="XRGeospatialState"/> event arguments.
    /// </summary>
    public struct XRGeospatialStateChangedEventArgs :
        IEquatable<XRGeospatialStateChangedEventArgs>
    {
        /// <summary>
        /// Constructs event arguments with previous and current states.
        /// </summary>
        /// <param name="prev">The previous state.</param>
        /// <param name="curr">The current state.</param>
        public XRGeospatialStateChangedEventArgs(
            XRGeospatialState prev, XRGeospatialState curr)
        {
            PrevState = prev;
            CurrState = curr;
        }

        /// <summary>
        /// Gets the previous <see cref="XRGeospatialState"/>.
        /// </summary>
        public XRGeospatialState PrevState { get; private set; }

        /// <summary>
        /// Gets the current <see cref="XRGeospatialState"/>.
        /// </summary>
        public XRGeospatialState CurrState { get; private set; }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(PrevState.GetHashCode(), CurrState.GetHashCode());
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is XRGeospatialStateChangedEventArgs other && Equals(other);
        }

        /// <inheritdoc/>
        public bool Equals(XRGeospatialStateChangedEventArgs other)
        {
            return PrevState == other.PrevState && CurrState == other.CurrState;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"({PrevState}->{CurrState})";
        }
    }
}
