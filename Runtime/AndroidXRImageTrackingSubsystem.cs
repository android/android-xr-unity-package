// <copyright file="AndroidXRImageTrackingSubsystem.cs" company="Google LLC">
//
// Copyright 2025 Google LLC
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
    /// The Android XR implementation of the <see cref="XRImageTrackingSubsystem"/> so it can work
    /// seamlessly with <see cref="ARTrackedImageManager"/>.
    /// </summary>
    [Preserve]
    public class AndroidXRImageTrackingSubsystem : XRImageTrackingSubsystem
    {
        internal static string _id => AndroidXRProvider._id;

        /// <summary>
        /// Try to get the decoded data from a QR code image.
        /// </summary>
        /// <param name="qrCode">
        /// The <see cref="TrackableId"/> which represents a QR Code <see cref="ARTrackedImage"/>.
        /// </param>
        /// <param name="decodedData">The decoded QR code data.</param>
        /// <returns><c>true</c>, if it succeed to get QR Code data and
        /// <paramref name="decodedData"/> contains valid data.</returns>
        public bool TryGetQrCodeData(TrackableId qrCode, out string decodedData)
        {
            AndroidXRProvider androidXRProvider = provider as AndroidXRProvider;
            decodedData = androidXRProvider.GetQrCodeData(qrCode);
            return !string.IsNullOrEmpty(decodedData);
        }

        /// <summary>
        /// Try to get the <see cref="XRMarkerDictionary"/> and the id from the dictionary of a
        /// marker image.
        /// </summary>
        /// <param name="marker">The <see cref="TrackableId"/> which represents a marker
        /// <see cref="ARTrackedImage"/>.</param>
        /// <param name="dictionary">The <see cref="XRMarkerDictionary"/> it belongs to.</param>
        /// <param name="markId">The id from the <paramref name="dictionary"/>.</param>
        /// <returns><c>True</c> if it gets marker data successfully.</returns>
        public bool TryGetMarkeData(
            TrackableId marker, out XRMarkerDictionary dictionary, out int markId)
        {
            XRMarkerDictionary md = XRMarkerDictionary.ArUco4x4_50;
            int id = -1;
            if (XRTrackableApi.GetMarkerData(marker, ref md, ref id))
            {
                dictionary = md;
                markId = id;
                return true;
            }
            else
            {
                dictionary = XRMarkerDictionary.ArUco4x4_50;
                markId = -1;
                return false;
            }
        }

        internal void SetPreferEstimation(ApiXrTrackableType type, bool prefer)
        {
            AndroidXRProvider androidXRProvider = provider as AndroidXRProvider;
            androidXRProvider.SetPreferEstimation(type, prefer);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterDescriptor()
        {
#pragma warning disable CS0162 // Unreachable code detected: used at runtime.
            if (!ApiConstants.AndroidPlatform)
            {
                return;
            }

            XRImageTrackingSubsystemDescriptor.Cinfo cinfo =
                new XRImageTrackingSubsystemDescriptor.Cinfo()
                {
                    id = AndroidXRProvider._id,
                    providerType = typeof(AndroidXRImageTrackingSubsystem.AndroidXRProvider),
                    subsystemTypeOverride = typeof(AndroidXRImageTrackingSubsystem),
                    supportsMovingImages = true,
                    requiresPhysicalImageDimensions = false,
                    supportsMutableLibrary = false,
                    supportsImageValidation = false,
                };

            XRImageTrackingSubsystemDescriptor.Register(cinfo);
#pragma warning restore CS0162
        }

        [SuppressMessage(
            "UnityRules.UnityStyleRules", "US1109:PublicPropertiesMustBeUpperCamelCase",
            Justification = "Override Unity interface.")]
        class AndroidXRProvider : Provider
        {
            internal static readonly string _id = "AndroidXR-ImageTracking";

            private bool _isActive = false;

            private bool _isQrCodeActive = false;
            private bool _qrCodeSupportEstimation = false;
            private uint _qrCodeMaxCount = 0;

            private bool _isMarkerActive = false;
            private bool _markerSupportEstimation = false;
            private uint _markerMaxCount = 0;

            private int _currentMaxMovingImages = 0;
            private int _requestedMaxMovingImages = 0;
            private bool _preferQrCodeEstimation = false;
            private bool _preferMarkerEstimation = false;
            private AndroidXRRuntimeImageLibrary _androidXRLibrary = null;

            private Dictionary<TrackableId, string> _qrCodeData =
                new Dictionary<TrackableId, string>();

            /// <inheritdoc/>
            public override int requestedMaxNumberOfMovingImages
            {
                get { return _requestedMaxMovingImages; }
                set { _requestedMaxMovingImages = value; }
            }

            /// <inheritdoc/>
            public override int currentMaxNumberOfMovingImages
            {
                get
                {
                    if (_currentMaxMovingImages == 0)
                    {
                        if (_qrCodeMaxCount == 0)
                        {
                            XRTrackableApi.TryGetQrCodeProperties(
                                ref _qrCodeMaxCount, ref _qrCodeSupportEstimation);
                        }

                        if (_markerMaxCount == 0)
                        {
                            XRTrackableApi.TryGetMarkerProperties(
                                ref _markerMaxCount, ref _markerSupportEstimation);
                        }

                        _currentMaxMovingImages += _isQrCodeActive ? (int)_qrCodeMaxCount : 0;
                        _currentMaxMovingImages += _isMarkerActive ? (int)_markerMaxCount : 0;
                    }

                    return _currentMaxMovingImages;
                }
            }

            /// <inheritdoc/>
            public override RuntimeReferenceImageLibrary imageLibrary
            {
                set
                {
                    switch (value)
                    {
                        case null:
                            _androidXRLibrary = null;
                            return;
                        case AndroidXRRuntimeImageLibrary runtimeLibrary:
                            Debug.Log("Setting AndroidXRRuntimeImageLibrary.");
                            _androidXRLibrary = runtimeLibrary;
                            TryEnableQrCodeTracking();
                            TryEnableMarkerTracking();
                            return;
                        default:
                            throw new ArgumentException(
                                $"{nameof(XRImageTrackingSubsystem)} " +
                                $"{nameof(imageLibrary)} setter was given an invalid " +
                                $"{nameof(RuntimeReferenceImageLibrary)}. " +
                                $"Use {nameof(CreateRuntimeLibrary)} to generate " +
                                $"a valid runtime library.");
                    }
                }
            }

            /// <inheritdoc/>
            public override RuntimeReferenceImageLibrary CreateRuntimeLibrary(
                XRReferenceImageLibrary serializedLibrary)
            {
                return new AndroidXRRuntimeImageLibrary(serializedLibrary);
            }

            /// <inheritdoc/>
            public override void Start()
            {
                // Managed through ARTrackedImageManager's OnEnable() callback.
                Debug.Log($"{ApiConstants.LogTag}: Start AndroidXRImageTrackingSubsystem.");
                _isActive = true;

                XRTrackableApi.TryGetQrCodeProperties(
                    ref _qrCodeMaxCount, ref _qrCodeSupportEstimation);
                Debug.LogFormat("QR Code System Properties: maxCount={0}, supportEstimation={1}",
                    _qrCodeMaxCount, _qrCodeSupportEstimation);
                TryEnableQrCodeTracking();

                XRTrackableApi.TryGetMarkerProperties(
                    ref _markerMaxCount, ref _markerSupportEstimation);
                Debug.LogFormat("Marker System Properties: maxCount={0}, supportEstimation={1}",
                    _markerMaxCount, _markerSupportEstimation);
                TryEnableMarkerTracking();
            }

            /// <inheritdoc/>
            public override void Stop()
            {
                // Managed through ARTrackedImageManager's OnDisable() callback.
                Debug.Log($"{ApiConstants.LogTag}: Stop AndroidXRImageTrackingSubsystem.");
                _isActive = false;

                XRTrackableApi.SetTracking(ApiXrTrackableType.QrCode, false);
                XRTrackableApi.SetTracking(ApiXrTrackableType.Marker, false);
                _qrCodeData.Clear();
            }

            /// <inheritdoc/>
            public override void Destroy()
            {
                // Managed through OpenXRFeature.OnSubsystemDestroy event.
                Debug.Log($"{ApiConstants.LogTag}: Destroy AndroidXRImageTrackingSubsystem.");
            }

            /// <inheritdoc/>
            public unsafe override TrackableChanges<XRTrackedImage> GetChanges(
                XRTrackedImage defaultTrackedImage, Allocator allocator)
            {
                IntPtr added = IntPtr.Zero;
                IntPtr updated = IntPtr.Zero;
                IntPtr removed = IntPtr.Zero;
                uint addedCount = 0;
                uint updatedCount = 0;
                uint removedCount = 0;
                uint elementSize = 0;
                IntPtr imageChanges = XRTrackableApi.AcquireImageChanges(
                    ref added, ref addedCount, ref updated, ref updatedCount, ref removed,
                    ref removedCount, ref elementSize);
                if (imageChanges == IntPtr.Zero)
                {
                    return new TrackableChanges<XRTrackedImage>(0, 0, 0, allocator);
                }

                try
                {
                    var changes = new TrackableChanges<XRTrackedImage>(
                       added.ToPointer(), (int)addedCount,
                       updated.ToPointer(), (int)updatedCount,
                       removed.ToPointer(), (int)removedCount,
                       defaultTrackedImage, (int)elementSize, allocator);

                    // Clear cached data in case the same trackable id is used by another instance.
                    foreach (var trackableId in changes.removed)
                    {
                        _qrCodeData.Remove(trackableId);
                    }

                    return changes;
                }
                finally
                {
                    XRTrackableApi.ReleaseImageChanges(imageChanges);
                }
            }

            internal void SetPreferEstimation(ApiXrTrackableType type, bool prefer)
            {
                switch (type)
                {
                    case ApiXrTrackableType.QrCode:
                        _isQrCodeActive = true;
                        _preferQrCodeEstimation = prefer;
                        return;
                    case ApiXrTrackableType.Marker:
                        _isMarkerActive = true;
                        _preferMarkerEstimation = prefer;
                        return;
                    default:
                        return;
                }
            }

            internal string GetQrCodeData(TrackableId qrCode)
            {
                if (!_qrCodeData.TryGetValue(qrCode, out string data))
                {
                    data = XRTrackableApi.GetQrCodeData(qrCode);
                    if (!string.IsNullOrEmpty(data))
                    {
                        _qrCodeData[qrCode] = data;
                    }
                }

                return data;
            }

            private void TryEnableQrCodeTracking()
            {
                if (!_isActive || !_isQrCodeActive || _androidXRLibrary == null ||
                    _androidXRLibrary.QRCodeReference == null)
                {
                    Debug.LogWarning("QR Code tracking is not ready.");
                    return;
                }

                XRReferenceImage reference = _androidXRLibrary.QRCodeReference.Value;
                float qrCodeEdge = reference.specifySize ? reference.width : 0;
                if (qrCodeEdge > 0 && _preferQrCodeEstimation && _qrCodeSupportEstimation)
                {
                    qrCodeEdge = 0;
                }

                XRTrackableApi.ConfigureQrCode(
                    reference.guid, qrCodeEdge, _requestedMaxMovingImages == 0);
                XRTrackableApi.SetTracking(ApiXrTrackableType.QrCode, true);
            }

            private void TryEnableMarkerTracking()
            {
                if (!_isActive || !_isMarkerActive || _androidXRLibrary == null ||
                    _androidXRLibrary.MarkerReferenceCount == 0)
                {
                    Debug.LogWarning("Marker tracking is not ready.");
                    return;
                }

                List<XRMarkerDatabaseEntry> entries = new List<XRMarkerDatabaseEntry>();
                if (_preferMarkerEstimation && _markerSupportEstimation)
                {
                    foreach (var entry in _androidXRLibrary.GetMarkerEntries())
                    {
                        XRMarkerDatabaseEntry newEntry = entry;
                        newEntry.ApplyEstimation();
                        entries.Add(newEntry);
                    }
                }
                else
                {
                    entries = _androidXRLibrary.GetMarkerEntries();
                }

                XRTrackableApi.ConfigureMarker(_requestedMaxMovingImages == 0, entries);
                XRTrackableApi.SetTracking(ApiXrTrackableType.Marker, true);
            }
        }
    }
}
