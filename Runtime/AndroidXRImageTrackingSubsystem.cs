// <copyright file="AndroidXRImageTrackingSubsystem.cs" company="Google LLC">
//
// Copyright 2025 Google LLC
// Copyright Qualcomm Technologies, Inc. and/or its affiliates. All rights reserved.
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
    using System.Runtime.InteropServices;
    using Google.XR.Extensions.Internal;
    using Unity.Collections;
    using UnityEngine;
    using UnityEngine.Scripting;
    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.ARSubsystems;
    using UnityEngine.XR.OpenXR.NativeTypes;

    using XrTrackableImageDatabase = System.UInt64;

    /// <summary>
    /// The Android XR implementation of the <see cref="XRImageTrackingSubsystem"/> so it can work
    /// seamlessly with <see cref="ARTrackedImageManager"/>.
    /// </summary>
    [Preserve]
    public class AndroidXRImageTrackingSubsystem : XRImageTrackingSubsystem
    {
        /// <summary>
        /// This event is invoked when asynchronous image tracking configuration completes.
        /// The <c>bool</c> parameter is <c>true</c> if configuration succeeded and the system is
        /// ready to track the configured image references, or <c>false</c> if configuration failed
        /// or was canceled.
        /// </summary>
        /// <remarks>
        /// A pending configuration may be canceled if the <see cref="ARTrackedImageManager"/> is
        /// disabled, which stops configuration altogether, or if a new configuration process is
        /// started, which cancels and replaces the previous one.
        /// </remarks>
        public event Action<bool> OnImageTrackingConfigured
        {
            add
            {
                if (provider is AndroidXRProvider androidXRProvider)
                {
                    androidXRProvider.OnImageTrackingConfiguredEvent += value;
                }
                else
                {
                    Debug.LogWarning("Failed to subscribe to the OnImageTrackingConfigured event.");
                }
            }

            remove
            {
                if (provider is AndroidXRProvider androidXRProvider)
                {
                    androidXRProvider.OnImageTrackingConfiguredEvent -= value;
                }
            }
        }

        /// <summary>
        /// This event is raised if image tracking encounters an internal failure,
        /// causing it to stop and invalidating the current configuration.
        /// In this case, restoring image tracking requires restarting the subsystem.
        /// Note: This event may be raised while the
        /// <see cref="ARTrackedImageManager"/> is disabled.
        /// </summary>
        public event Action OnImageTrackingLost
        {
            add
            {
                if (provider is AndroidXRProvider androidXRProvider)
                {
                    androidXRProvider.OnImageTrackingLostEvent += value;
                }
                else
                {
                    Debug.LogWarning("Failed to subscribe to the OnImageTrackingLost event.");
                }
            }

            remove
            {
                if (provider is AndroidXRProvider androidXRProvider)
                {
                    androidXRProvider.OnImageTrackingLostEvent -= value;
                }
            }
        }

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
        public bool TryGetMarkerData(
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

        internal void InitTracking(ApiXrTrackableType type, bool preferSizeEstimation)
        {
            AndroidXRProvider androidXRProvider = provider as AndroidXRProvider;
            androidXRProvider?.InitTracking(type, preferSizeEstimation);
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

            private bool _isImageTrackingInitialized;
            private bool _preferImageSizeEstimation;
            private bool _isImageSizeEstimationSupported;
            private uint _imageMaxTrackedCount;
            private uint _imageMaxLoadedCount;
            private XRImageDatabaseEntry[] _nativeImageDatabaseEntries;
            private ImageDatabaseFuture _imageTrackingDatabaseFuture = null;
            private XrTrackableImageDatabase _imageDatabase = 0;

            private int _requestedMaxMovingImages = 0;
            private bool _preferQrCodeEstimation = false;
            private bool _preferMarkerEstimation = false;
            private AndroidXRRuntimeImageLibrary _androidXRLibrary = null;

            // Cached reference to the last configured library to enable
            // comparison with new libraries.
            // This allows reconfiguration only when the library has changed,
            // avoiding redundant operations.
            // Allows disabling and re-enabling the ImageManager without having
            // to reconfigure image tracking each time.
            private AndroidXRRuntimeImageLibrary _cachedImageTrackingLibrary = null;

            private Dictionary<TrackableId, string> _qrCodeData =
                new Dictionary<TrackableId, string>();

            internal event Action<bool> OnImageTrackingConfiguredEvent;

            internal event Action OnImageTrackingLostEvent;

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
                    var qr = _isQrCodeActive ? (int)_qrCodeMaxCount : 0;
                    var marker = _isMarkerActive ? (int)_markerMaxCount : 0;
                    var image = _isImageTrackingInitialized ? (int)_imageMaxTrackedCount : 0;
                    return qr + marker + image;
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
                            TryEnableImageTrackingAsync();
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

                TryEnableQrCodeTracking();
                TryEnableMarkerTracking();
                TryEnableImageTrackingAsync();
            }

            /// <inheritdoc/>
            public override void Stop()
            {
                // Managed through ARTrackedImageManager's OnDisable() callback.
                Debug.Log($"{ApiConstants.LogTag}: Stop AndroidXRImageTrackingSubsystem.");
                _isActive = false;

                CancelPendingImageTrackingConfiguration();
                XRTrackableApi.SetTracking(ApiXrTrackableType.Image, false);
                DisposeNativeImageDatabaseEntries(ref _nativeImageDatabaseEntries);

                XRTrackableApi.SetTracking(ApiXrTrackableType.QrCode, false);
                _qrCodeData.Clear();

                XRTrackableApi.SetTracking(ApiXrTrackableType.Marker, false);
            }

            /// <inheritdoc/>
            public override void Destroy()
            {
                // Managed through OpenXRFeature.OnSubsystemDestroy event.
                Debug.Log($"{ApiConstants.LogTag}: Destroy AndroidXRImageTrackingSubsystem.");
                _isImageTrackingInitialized = false;
                UnsubscribeFromImageTrackingLostEvent();
                ImageDatabaseFuture.Destroy(ref _imageDatabase);
                _cachedImageTrackingLibrary = null;
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

            internal void InitTracking(ApiXrTrackableType type, bool preferSizeEstimation)
            {
                switch (type)
                {
                    case ApiXrTrackableType.QrCode:
                        _isQrCodeActive = true;
                        _preferQrCodeEstimation = preferSizeEstimation;
                        XRTrackableApi.TryGetQrCodeProperties(
                            ref _qrCodeMaxCount, ref _qrCodeSupportEstimation);
                        Debug.LogFormat("QR Code System Properties: maxCount={0}, " +
                            "supportEstimation={1}", _qrCodeMaxCount, _qrCodeSupportEstimation);
                        return;
                    case ApiXrTrackableType.Marker:
                        _isMarkerActive = true;
                        _preferMarkerEstimation = preferSizeEstimation;
                        XRTrackableApi.TryGetMarkerProperties(
                            ref _markerMaxCount, ref _markerSupportEstimation);
                        Debug.LogFormat("Marker System Properties: maxCount={0}, " +
                            "supportEstimation={1}", _markerMaxCount, _markerSupportEstimation);
                        return;
                    case ApiXrTrackableType.Image:
                        _isImageTrackingInitialized = true;
                        _preferImageSizeEstimation = preferSizeEstimation;
                        XRTrackableApi.TryGetImageTrackingProperties(
                            ref _isImageSizeEstimationSupported, ref _imageMaxTrackedCount,
                            ref _imageMaxLoadedCount);
                        Debug.LogFormat(
                            "Image System Properties: maxTrackedCount={0}, maxLoadedCount={1}, " +
                            "supportEstimation={2}", _imageMaxTrackedCount, _imageMaxLoadedCount,
                            _isImageSizeEstimationSupported);
                        SubscribeToImageTrackingLostEvent();
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

            #region Image Tracking
            private unsafe void SubscribeToImageTrackingLostEvent()
            {
                PollEventRouter.TrySubscribeToEventType(
                    (XrStructureType)XrStructureTypeAndroid.EventDataImageTrackingLostAndroid,
                    OnImageTrackingLostCallback);
            }

            private unsafe void UnsubscribeFromImageTrackingLostEvent()
            {
                PollEventRouter.TryUnsubscribeFromEventType(
                    (XrStructureType)XrStructureTypeAndroid.EventDataImageTrackingLostAndroid,
                    OnImageTrackingLostCallback);
            }

            /// <summary>
            /// Attempts to configure image tracking asynchronously using
            /// <see cref="_androidXRLibrary"/>.
            /// Image tracking is enabled if configuration succeeds.
            /// If image tracking is already configured with the same reference image library,
            /// tracking is enabled without reconfiguration.
            /// When a different library is provided, any pending configuration is canceled,
            /// image tracking is stopped, and a new asynchronous configuration is started.
            /// </summary>
            private void TryEnableImageTrackingAsync()
            {
                Debug.Log("Try enable image tracking asynchronously.");

                if (!_isActive)
                {
                    Debug.LogWarning("Failed to enable Image tracking: subsystem is not active.");
                    return;
                }

                if (!_isImageTrackingInitialized)
                {
                    Debug.LogWarning(
                        "Failed to enable Image tracking: image tracking was not initialized.");
                    return;
                }

                if (_androidXRLibrary == null || _androidXRLibrary.ImageReferenceCount == 0)
                {
                    Debug.LogWarning(
                        "Failed to enable Image tracking: invalid reference image library.");
                    return;
                }

                // Get configuration status.
                var isConfigured = _imageDatabase != 0;
                var isConfigurationPending = _imageTrackingDatabaseFuture != null;

                // (Re-)Configure image tracking if needed
                if (_androidXRLibrary.Equals(_cachedImageTrackingLibrary))
                {
                    if (isConfigurationPending)
                    {
                        Debug.Log("Image tracking configuration is already pending for " +
                                  "the provided reference images.");
                        return;
                    }

                    if (isConfigured)
                    {
                        Debug.Log("Image tracking is already configured with the provided " +
                                  "reference images.");
                        XRTrackableApi.SetTracking(ApiXrTrackableType.Image, true);
                        OnImageTrackingConfiguredEvent?.Invoke(true);
                        return;
                    }
                }

                // Prepare marshalling of reference images:
                // Copy managed pixel data to unmanaged buffers. The created entries must remain
                // allocated until configuration completes or is cancelled, after which they must
                // be disposed to free unmanaged memory.
                if (!_androidXRLibrary.TryCopyImageReferencesToNativeMemory(
                    _preferImageSizeEstimation, _isImageSizeEstimationSupported,
                    out var nativeImageDatabaseEntries))
                {
                    Debug.LogWarning("Failed to enable Image tracking: " +
                        "failed to copy pixel data of reference images.");
                    return;
                }

                // Cancel any ongoing configuration. Only one can be active at a time.
                CancelPendingImageTrackingConfiguration();

                if (isConfigured)
                {
                    // Stop image tracking until configuration succeeds.
                    XRTrackableApi.SetTracking(ApiXrTrackableType.Image, false);
                    ImageDatabaseFuture.Destroy(ref _imageDatabase);
                }

                // Keep reference for later disposal.
                SetNativeImageDatabaseEntries(nativeImageDatabaseEntries);

                _cachedImageTrackingLibrary = _androidXRLibrary;
                TryConfigureImageTrackingAsync();
            }

            private void CancelPendingImageTrackingConfiguration()
            {
                if (_imageTrackingDatabaseFuture == null)
                {
                    return;
                }

                // Cancel pending async configuration. This implicitly calls
                // CompleteImageTrackingConfiguration(false) due to OperationCanceledException.
                _imageTrackingDatabaseFuture.Dispose();
                _imageTrackingDatabaseFuture = null;
            }

            /// <summary>
            /// When configuration completes or is cancelled,
            /// <see cref="CompleteImageTrackingConfiguration"/> is called and
            /// <see cref="OnImageTrackingConfiguredEvent"/> is invoked.
            /// </summary>
            private async void TryConfigureImageTrackingAsync()
            {
                try
                {
                    var result = await ConfigureImageTrackingAsync();
                    CompleteImageTrackingConfiguration(result);
                }
                catch (OperationCanceledException)
                {
                    Debug.Log("Cancel pending Image tracking configuration.");
                    CompleteImageTrackingConfiguration(false);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    CompleteImageTrackingConfiguration(false);
                }
            }

            private void SetNativeImageDatabaseEntries(XRImageDatabaseEntry[] entries)
            {
                // Ensure any previous entries are disposed before overwriting the reference
                DisposeNativeImageDatabaseEntries(ref _nativeImageDatabaseEntries);
                _nativeImageDatabaseEntries = entries;
            }

            /// <summary>
            /// Called when image tracking configuration completes, is canceled, or has failed.
            /// Handles any necessary cleanup and enables tracking if configuration was successful.
            /// </summary>
            /// <param name="success">
            /// If <c>true</c>, image tracking configuration completed successfully;
            /// otherwise, the operation failed or was canceled.
            /// </param>
            private void CompleteImageTrackingConfiguration(bool success)
            {
                // Dispose the future handler.
                _imageTrackingDatabaseFuture?.Dispose();
                _imageTrackingDatabaseFuture = null;

                // Entries are no longer needed and can be safely disposed to free up memory
                DisposeNativeImageDatabaseEntries(ref _nativeImageDatabaseEntries);

                if (success)
                {
                    Debug.Log("Image tracking successfully configured.");

                    if (_isActive)
                    {
                        XRTrackableApi.SetTracking(ApiXrTrackableType.Image, true);
                    }

                    OnImageTrackingConfiguredEvent?.Invoke(true);
                }
                else
                {
                    Debug.Log("Image tracking configuration failed or was canceled.");
                    OnImageTrackingConfiguredEvent?.Invoke(false);
                }
            }

            private void DisposeNativeImageDatabaseEntries(
                ref XRImageDatabaseEntry[] nativeImageDatabaseEntries)
            {
                if (nativeImageDatabaseEntries == null)
                {
                    return;
                }

                foreach (var entry in nativeImageDatabaseEntries)
                {
                    entry.Dispose();
                }

                nativeImageDatabaseEntries = null;
            }

            private async Awaitable<bool> ConfigureImageTrackingAsync()
            {
                // Start asynchronous (re-)configuration.
                var result = ImageDatabaseFuture.TryCreate(
                    _nativeImageDatabaseEntries, out _imageTrackingDatabaseFuture);
                if (!result)
                {
                    Debug.LogWarning("Failed to start async image tracking configuration.");
                    return false;
                }

                Debug.Log("Started async image tracking configuration with " +
                    $"{_nativeImageDatabaseEntries.Length} images.");

                Debug.Log("Waiting for ImageDatabaseFuture.TryGetResultAsync");
                var configurationResult = await _imageTrackingDatabaseFuture.TryGetResultAsync();
                if (configurationResult.status.IsError())
                {
                    Debug.LogErrorFormat("ImageDatabaseFuture.TryGetResultAsync failed with {0}",
                        configurationResult.status);
                    return false;
                }

                _imageDatabase = configurationResult.value;
                Debug.LogFormat("ImageDatabaseFuture.TryGetResultAsync succeed with result {0}.",
                    _imageDatabase);

                return true;
            }

            private unsafe void OnImageTrackingLostCallback(XrEventDataBaseHeader* baseHeader)
            {
                if (baseHeader == null)
                {
                    return;
                }

                var eventData = *(XrEventDataImageTrackingLostANDROID*)baseHeader;
                if (eventData.type !=
                    (XrStructureType)XrStructureTypeAndroid.EventDataImageTrackingLostAndroid)
                {
                    return;
                }

                Debug.LogWarning("Image tracking has stopped due to an internal failure. " +
                                 "Images will no longer be tracked. Restoring image " +
                                 "tracking requires a restart of the subsystem.");

                XRTrackableApi.SetTracking(ApiXrTrackableType.Image, false);
                _cachedImageTrackingLibrary = null;

                // Deconfigures image tracking and destroys the native resources.
                ImageDatabaseFuture.Destroy(ref _imageDatabase);

                OnImageTrackingLostEvent?.Invoke();
            }

            [SuppressMessage(
            "UnityRules.UnityStyleRules", "US1107:PublicFieldsMustBeUpperCamelCase",
            Justification = "Matching OpenXR extensions.")]
            [StructLayout(LayoutKind.Sequential)]
            internal unsafe struct XrEventDataImageTrackingLostANDROID
            {
                public XrStructureType type;
                public void* next;
                public long time; // XrTime = int64_t
            }
            #endregion
        }
    }
}
