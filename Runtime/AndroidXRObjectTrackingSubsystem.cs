// <copyright file="AndroidXRObjectTrackingSubsystem.cs" company="Google LLC">
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
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Google.XR.Extensions.Internal;
    using Unity.Collections;
    using UnityEngine;
    using UnityEngine.Scripting;
    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.ARSubsystems;

    /// <summary>
    /// The Android XR implementation of the <c><see cref="XRObjectTrackingSubsystem"/></c> so it
    /// can work seamlessly with <c><see cref="ARTrackedObjectManager"/></c>.
    /// </summary>
    [Preserve]
    public sealed class AndroidXRObjectTrackingSubsystem : XRObjectTrackingSubsystem
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

            XRObjectTrackingSubsystemDescriptor.Cinfo cinfo =
                new XRObjectTrackingSubsystemDescriptor.Cinfo()
                {
                    id = AndroidXRProvider._id,
                    providerType = typeof(AndroidXRObjectTrackingSubsystem.AndroidXRProvider),
                    subsystemTypeOverride = typeof(AndroidXRObjectTrackingSubsystem)
                };

            XRObjectTrackingSubsystemDescriptor.Register(cinfo);
#pragma warning restore CS0162
        }

        [SuppressMessage(
            "UnityRules.UnityStyleRules", "US1109:PublicPropertiesMustBeUpperCamelCase",
            Justification = "Override Unity interface.")]
        class AndroidXRProvider : Provider
        {
            internal static readonly string _id = "AndroidXR-ObjectTracking";

            private Dictionary<XRObjectLabel, Guid> _referenceGuids =
                new Dictionary<XRObjectLabel, Guid>();

            /// <inheritdoc/>
            public override XRReferenceObjectLibrary library
            {
                set
                {
                    _referenceGuids.Clear();
                    if (value != null)
                    {
                        foreach (XRReferenceObject referenceObject in value)
                        {
                            XRObjectLabel label = XRObjectLabel.Unknown;
                            switch (referenceObject.name)
                            {
                                case "Unknown":
                                {
                                    label = XRObjectLabel.Unknown;
                                    break;
                                }

                                case "Keyboard":
                                {
                                    label = XRObjectLabel.Keyboard;
                                    break;
                                }

                                case "Mouse":
                                {
                                    label = XRObjectLabel.Mouse;
                                    break;
                                }

                                case "Laptop":
                                {
                                    label = XRObjectLabel.Laptop;
                                    break;
                                }

                                default:
                                    continue;
                            }

                            _referenceGuids[label] = referenceObject.guid;
                        }
                    }

                    XRTrackableApi.UpdateObjectReferenceGuids(_referenceGuids);
                }
            }

            /// <inheritdoc/>
            public override void Start()
            {
                // Managed through ARTrackedObjectManager's OnEnable() callback.
                Debug.Log($"{ApiConstants.LogTag}: Start AndroidXRObjectTrackingSubsystem.");
                XRTrackableApi.SetTracking(ApiXrTrackableType.Object, true);
            }

            /// <inheritdoc/>
            public override void Stop()
            {
                // Managed through ARTrackedObjectManager's OnDisable() callback.
                Debug.Log($"{ApiConstants.LogTag}: Stop AndroidXRObjectTrackingSubsystem.");
                XRTrackableApi.SetTracking(ApiXrTrackableType.Object, false);
            }

            /// <inheritdoc/>
            public override void Destroy()
            {
                // Managed through OpenXRFeature.OnSubsystemDestroy event.
                Debug.Log($"{ApiConstants.LogTag}: Destroy AndroidXRObjectTrackingSubsystem.");
            }

            /// <inheritdoc/>
            public unsafe override TrackableChanges<XRTrackedObject> GetChanges(
                XRTrackedObject template, Allocator allocator)
            {
                IntPtr added = IntPtr.Zero;
                IntPtr updated = IntPtr.Zero;
                IntPtr removed = IntPtr.Zero;
                uint addedCount = 0;
                uint updatedCount = 0;
                uint removedCount = 0;
                uint elementSize = 0;
                IntPtr objectChanges = XRTrackableApi.AcquireObjectChanges(
                    ref added, ref addedCount, ref updated, ref updatedCount, ref removed,
                    ref removedCount, ref elementSize);

                if (objectChanges == IntPtr.Zero)
                {
                    return new TrackableChanges<XRTrackedObject>(0, 0, 0, allocator);
                }

                try
                {
                    return new TrackableChanges<XRTrackedObject>(
                        added.ToPointer(), (int)addedCount,
                        updated.ToPointer(), (int)updatedCount,
                        removed.ToPointer(), (int)removedCount,
                        template, (int)elementSize, allocator);
                }
                finally
                {
                    XRTrackableApi.ReleaseObjectChanges(objectChanges);
                }
            }
        }
    }
}
