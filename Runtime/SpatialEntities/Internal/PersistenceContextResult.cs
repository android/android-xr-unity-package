// <copyright file="PersistenceContextResult.cs" company="Google LLC">
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

namespace Google.XR.Extensions.SpatialEntities.Internal
{
    using UnityEngine.XR.OpenXR.NativeTypes;

    using XrSpatialPersistenceContextEXT = System.UInt64;

    /// <summary>
    /// A wrapper for <see cref="XrCreateSpatialPersistenceContextCompletionEXT"/>
    /// returns.
    /// </summary>
    /// <seealso cref="PersistenceContextFuture"/>
    internal struct PersistenceContextResult
    {
        /// <summary>
        /// Matches <see cref="XrCreateSpatialPersistenceContextCompletionEXT.createResult"/>.
        /// </summary>
        public XrSpatialPersistenceContextResultEXT Result;

        /// <summary>
        /// Matches <see cref="XrCreateSpatialPersistenceContextCompletionEXT.persistenceContext"/>.
        /// </summary>
        public XrSpatialPersistenceContextEXT Context;

        public PersistenceContextResult(
            XrSpatialPersistenceContextResultEXT result, XrSpatialPersistenceContextEXT context)
        {
            Result = result;
            Context = context;
        }
    }
}
