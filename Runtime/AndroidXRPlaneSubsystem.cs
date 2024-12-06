// <copyright file="AndroidXRPlaneSubsystem.cs" company="Google LLC">
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
    using System.Diagnostics.CodeAnalysis;
    using Google.XR.Extensions.Internal;
    using Unity.Collections;
    using Unity.Jobs;
    using UnityEngine;
    using UnityEngine.Scripting;
    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.ARSubsystems;

    /// <summary>
    /// The Android XR implementation of the <see cref="XRPlaneSubsystem"/> so it can work
    /// seamlessly with <see cref="ARPlaneManager"/>.
    /// </summary>
    [Preserve]
    public sealed class AndroidXRPlaneSubsystem : XRPlaneSubsystem
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

            XRPlaneSubsystemDescriptor.Cinfo cinfo = new XRPlaneSubsystemDescriptor.Cinfo
            {
                id = AndroidXRProvider._id,
                providerType = typeof(AndroidXRPlaneSubsystem.AndroidXRProvider),
                subsystemTypeOverride = typeof(AndroidXRPlaneSubsystem),
                supportsHorizontalPlaneDetection = true,
                supportsVerticalPlaneDetection = true,
                supportsArbitraryPlaneDetection = false,
                supportsBoundaryVertices = true
            };

            XRPlaneSubsystemDescriptor.Register(cinfo);
#pragma warning restore CS0162
        }

        [SuppressMessage(
            "UnityRules.UnityStyleRules", "US1109:PublicPropertiesMustBeUpperCamelCase",
            Justification = "Override Unity interface.")]
        class AndroidXRProvider : Provider
        {
            internal static readonly string _id = "AndroidXR-Plane";

            /// <inheritdoc/>
            public override PlaneDetectionMode requestedPlaneDetectionMode
            {
                get => XRTrackableApi.GetPlaneDetectionMode();
                set => XRTrackableApi.SetPlaneDetectionMode(value);
            }

            /// <inheritdoc/>
            public override void Start()
            {
                XRTrackableApi.SetTracking(ApiXrTrackableType.Plane, true);
            }

            /// <inheritdoc/>
            public override void Stop()
            {
                XRTrackableApi.SetTracking(ApiXrTrackableType.Plane, false);
            }

            /// <inheritdoc/>
            public override void GetBoundary(
                TrackableId trackableId, Allocator allocator, ref NativeArray<Vector2> boundary)
            {
                uint size = XRTrackableApi.GetBoundaySize(trackableId);
                CreateOrResizeNativeArrayIfNecessary((int)size, allocator, ref boundary);
                if (XRTrackableApi.AcquireBoundary(trackableId, boundary))
                {
                    // Flip handedness and winding order
                    // Unity expects a clockwise winding order for mesh vertices:
                    // https://docs.unity3d.com/2022.3/Documentation/Manual/AnatomyofaMesh.html,
                    // and converts the handedness of the vertices from right-handed coordinate
                    // system (OpenXR) to left-handed system (Unity).
                    var flipHandednessHandle = new FlipBoundaryHandednessJob
                    {
                        Vertices = boundary
                    }.Schedule((int)size, 1);

                    new FlipBoundaryWindingJob
                    {
                        Vertices = boundary
                    }.Schedule(flipHandednessHandle).Complete();
                }
                else
                {
                    boundary.Dispose();
                }
            }

            /// <inheritdoc/>
            public unsafe override TrackableChanges<BoundedPlane> GetChanges(
                BoundedPlane defaultPlane, Allocator allocator)
            {
                IntPtr added = IntPtr.Zero;
                IntPtr updated = IntPtr.Zero;
                IntPtr removed = IntPtr.Zero;
                uint addedCount = 0;
                uint updatedCount = 0;
                uint removedCount = 0;
                uint elementSize = 0;
                IntPtr planeChanges = XRTrackableApi.AcquirePlaneChanges(
                    ref added, ref addedCount, ref updated, ref updatedCount, ref removed,
                    ref removedCount, ref elementSize);

                if (planeChanges == IntPtr.Zero)
                {
                    return new TrackableChanges<BoundedPlane>(0, 0, 0, allocator);
                }

                try
                {
                    return new TrackableChanges<BoundedPlane>(
                        added.ToPointer(), (int)addedCount,
                        updated.ToPointer(), (int)updatedCount,
                        removed.ToPointer(), (int)removedCount,
                        defaultPlane, (int)elementSize, allocator);
                }
                finally
                {
                    XRTrackableApi.ReleasePlaneChanges(planeChanges);
                }
            }

            /// <inheritdoc/>
            public override void Destroy()
            {
                // Plane tracker has been destroy in Stop().
            }

            struct FlipBoundaryWindingJob : IJob
            {
                /// <summary>
                /// Vertices array to flip the winding order.
                /// </summary>
                public NativeArray<Vector2> Vertices;

                public void Execute()
                {
                    var half = Vertices.Length / 2;
                    for (int i = 0; i < half; ++i)
                    {
                        var j = Vertices.Length - 1 - i;
                        var temp = Vertices[j];
                        Vertices[j] = Vertices[i];
                        Vertices[i] = temp;
                    }
                }
            }

            struct FlipBoundaryHandednessJob : IJobParallelFor
            {
                /// <summary>
                /// Vertices array to flip the handedness.
                /// </summary>
                public NativeArray<Vector2> Vertices;

                public void Execute(int index)
                {
                    Vertices[index] = new Vector2(
                         Vertices[index].x,
                        -Vertices[index].y);
                }
            }
        }
    }
}
