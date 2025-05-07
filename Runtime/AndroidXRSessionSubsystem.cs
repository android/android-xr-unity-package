// <copyright file="AndroidXRSessionSubsystem.cs" company="Google LLC">
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
    using System.Diagnostics.CodeAnalysis;
    using Google.XR.Extensions.Internal;
    using UnityEngine;
    using UnityEngine.Scripting;
    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.ARSubsystems;

    /// <summary>
    /// The Android XR implementation of the <c><see cref="XRSessionSubsystem"/></c> so it can work
    /// seamlessly with <c><see cref="ARSession"/></c>.
    /// </summary>
    [Preserve]
    public sealed class AndroidXRSessionSubsystem : XRSessionSubsystem
    {
        internal static string _id => AndroidXRProvider._id;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterDescriptor()
        {
#pragma warning disable CS0162 // Unreachable code detected: used at runtime.
            if (!ApiConstants.AndroidPlatform)
            {
                return;
            }

            XRSessionSubsystemDescriptor.Cinfo cinfo = new XRSessionSubsystemDescriptor.Cinfo
            {
                id = AndroidXRProvider._id,
                providerType = typeof(AndroidXRSessionSubsystem.AndroidXRProvider),
                subsystemTypeOverride = typeof(AndroidXRSessionSubsystem),
                supportsInstall = false,
                supportsMatchFrameRate = false,
            };

            XRSessionSubsystemDescriptor.Register(cinfo);
#pragma warning restore CS0162
        }

        [SuppressMessage(
            "UnityRules.UnityStyleRules", "US1109:PublicPropertiesMustBeUpperCamelCase",
            Justification = "Override Unity interface.")]
        class AndroidXRProvider : Provider
        {
            internal static readonly string _id = "AndroidXR-Session";

            private bool _isValid = true;
            private bool _isActive = false;

            /// <inheritdoc/>
            public override TrackingState trackingState => GetTrackingState();

            /// <inheritdoc/>
            public override void Start()
            {
                // Managed through ARSession's OnEnable() callback.
                Debug.Log($"{ApiConstants.LogTag}: Start AndroidXRSessionSubsystem.");
                _isActive = true;
            }

            /// <inheritdoc/>
            public override void Stop()
            {
                // Managed through ARSession's OnDisable() callback.
                Debug.Log($"{ApiConstants.LogTag}: Stop AndroidXRSessionSubsystem.");
                _isActive = false;
            }

            /// <inheritdoc/>
            public override void Destroy()
            {
                // Managed through OpenXRFeature.OnSubsystemDestroy event.
                Debug.Log($"{ApiConstants.LogTag}: Destroy AndroidXRSessionSubsystem.");
                _isValid = false;
            }

            /// <inheritdoc/>
            public override void Reset()
            {
                _isValid = true;
                _isActive = false;
            }

            /// <inheritdoc/>
            public override Promise<SessionAvailability> GetAvailabilityAsync()
            {
                // OpenXR runtime doesn't require any additional installation.
                // Always return Supported and Installed.
                return Promise<SessionAvailability>.CreateResolvedPromise(
                    SessionAvailability.Supported | SessionAvailability.Installed);
            }

            private TrackingState GetTrackingState()
            {
                if (!_isValid)
                {
                    return TrackingState.None;
                }
                else if (!_isActive)
                {
                    return TrackingState.Limited;
                }
                else
                {
                    return TrackingState.Tracking;
                }
            }
        }
    }
}
