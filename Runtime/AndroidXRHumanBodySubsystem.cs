// <copyright file="AndroidXRHumanBodySubsystem.cs" company="Google LLC">
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
    /// The Android XR implementation of the <c><see cref="XRHumanBodySubsystem"/></c> so it can
    /// work seamlessly with <c><see cref="ARHumanBodyManager"/></c>.
    /// </summary>
    [Preserve]
    public sealed class AndroidXRHumanBodySubsystem : XRHumanBodySubsystem
    {
        /// <summary>
        /// Gets or sets a value indicating whether to use automatic calibration at runtime.
        /// When automatic calibration is disabled, <see cref="ProportionCalibrationRequested"/>
        /// will take effect.
        /// </summary>
        public bool AutoCalibrationRequested
        {
            get
            {
                AndroidXRProvider androidXRProvider = provider as AndroidXRProvider;
                return androidXRProvider.AutoCalibrationRequested;
            }

            set
            {
                AndroidXRProvider androidXRProvider = provider as AndroidXRProvider;
                androidXRProvider.AutoCalibrationRequested = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="XRHumanBodyProportions"/> for rest pose skeleton
        /// computation. It only takes effect when <see cref="AutoCalibrationRequested"/> is
        /// disabled.
        /// </summary>
        public XRHumanBodyProportions ProportionCalibrationRequested
        {
            get
            {
                AndroidXRProvider androidXRProvider = provider as AndroidXRProvider;
                return androidXRProvider.ProportionCalibrationRequested;
            }

            set
            {
                AndroidXRProvider androidXRProvider = provider as AndroidXRProvider;
                androidXRProvider.ProportionCalibrationRequested = value;
            }
        }

        internal static string _id => AndroidXRProvider._id;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterDescriptor()
        {
#pragma warning disable CS0162 // Unreachable code detected: used at runtime.
            if (!ApiConstants.AndroidPlatform)
            {
                return;
            }

            XRHumanBodySubsystemDescriptor.Cinfo cinfo =
                new XRHumanBodySubsystemDescriptor.Cinfo()
                {
                    id = AndroidXRProvider._id,
                    providerType = typeof(AndroidXRHumanBodySubsystem.AndroidXRProvider),
                    subsystemTypeOverride = typeof(AndroidXRHumanBodySubsystem),
                    supportsHumanBody3D = true,
                };

            XRHumanBodySubsystemDescriptor.Register(cinfo);
#pragma warning restore CS0162
        }

        [SuppressMessage(
            "UnityRules.UnityStyleRules", "US1109:PublicPropertiesMustBeUpperCamelCase",
            Justification = "Override Unity interface.")]
        class AndroidXRProvider : Provider
        {
            internal static readonly string _id = "AndroidXR-BodyTracking";

            private bool _isActive = false;
            private bool _autoCalibrationRequested = false;
            private XRHumanBodyProportions _proportionRequested = null;
            private bool _pose3DRequested = false;
            private bool _pose3DEnabled = false;

            /// <inheritdoc/>
            public override bool pose3DRequested
            {
                get => _pose3DRequested;
                set
                {
                    if (_pose3DRequested != value)
                    {
                        _pose3DRequested = value;
                        ApplyRequest();
                    }
                }
            }

            /// <inheritdoc/>
            public override bool pose3DEnabled => _pose3DEnabled;

            public bool AutoCalibrationRequested
            {
                get => _autoCalibrationRequested;
                set
                {
                    if (_autoCalibrationRequested != value)
                    {
                        _autoCalibrationRequested = value;
                        if (_autoCalibrationRequested)
                        {
                            _proportionRequested = null;
                        }

                        ApplyRequest();
                    }
                }
            }

            public XRHumanBodyProportions ProportionCalibrationRequested
            {
                get => _proportionRequested;
                set
                {
                    if (_proportionRequested == null || !_proportionRequested.Equals(value))
                    {
                        _proportionRequested = value;
                        if (_proportionRequested != null)
                        {
                            _autoCalibrationRequested = false;
                        }

                        ApplyRequest();
                    }
                }
            }

            /// <inheritdoc/>
            public override void Start()
            {
                // Managed through ARHumanBodyManager's OnEnable() callback.
                Debug.Log($"{ApiConstants.LogTag}: Start AndroidXRHumanBodySubsystem.");

                _isActive = true;
                ApplyRequest();
            }

            /// <inheritdoc/>
            public override void Stop()
            {
                // Managed through ARHumanBodyManager's OnDisable() callback.
                Debug.Log($"{ApiConstants.LogTag}: Stop AndroidXRHumanBodySubsystem.");

                _isActive = false;
                ApplyRequest();
            }

            /// <inheritdoc/>
            public override void Destroy()
            {
                // Managed through OpenXRFeature.OnSubsystemDestroy event.
                Debug.Log($"{ApiConstants.LogTag}: Destroy AndroidXRHumanBodySubsystem.");
            }

            /// <inheritdoc/>
            public unsafe override TrackableChanges<XRHumanBody> GetChanges(
                XRHumanBody defaultHumanBody, Allocator allocator)
            {
                IntPtr added = IntPtr.Zero;
                IntPtr updated = IntPtr.Zero;
                IntPtr removed = IntPtr.Zero;
                uint addedCount = 0;
                uint updatedCount = 0;
                uint removedCount = 0;
                uint elementSize = 0;
                IntPtr bodyChanges = XRBodyTrackingApi.AcquireChanges(
                    ref added, ref addedCount, ref updated, ref updatedCount, ref removed,
                    ref removedCount, ref elementSize);
                if (bodyChanges == IntPtr.Zero)
                {
                    return new TrackableChanges<XRHumanBody>(0, 0, 0, allocator);
                }

                try
                {
                    return new TrackableChanges<XRHumanBody>(
                        added.ToPointer(), (int)addedCount, updated.ToPointer(), (int)updatedCount,
                        removed.ToPointer(), (int)removedCount, defaultHumanBody, (int)elementSize,
                        allocator);
                }
                finally
                {
                    XRBodyTrackingApi.ReleaseChanges(bodyChanges);
                }
            }

            /// <inheritdoc/>
            public override void GetSkeleton(
                TrackableId trackableId, Allocator allocator,
                ref NativeArray<XRHumanBodyJoint> skeleton)
            {
                skeleton = new NativeArray<XRHumanBodyJoint>(
                    XRAvatarSkeletonJointIDUtility.JointCount(), allocator);
                XRBodyTrackingApi.GetSkeleton(trackableId, skeleton);
            }

            private void ApplyRequest()
            {
                if (_isActive)
                {
                    Debug.LogFormat(
                        "Apply Request: pose3DRequested={0}, AutoCalibrationRequested={1}, " +
                        "ProportionCalibrationRequested={2}",
                        _pose3DRequested, _autoCalibrationRequested,
                        _proportionRequested == null ? "null" : _proportionRequested.ToString());
                    XRBodyTrackingApi.SetTracking(_pose3DRequested);
                    _pose3DEnabled = _pose3DRequested;
                    if (_autoCalibrationRequested || _proportionRequested != null)
                    {
                        XRBodyTrackingApi.SetCalibration(
                            _autoCalibrationRequested, _proportionRequested);
                    }
                }
                else
                {
                    XRBodyTrackingApi.SetTracking(false);
                    _pose3DEnabled = false;
                }
            }
        }
    }
}
