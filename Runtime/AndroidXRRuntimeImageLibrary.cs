// <copyright file="AndroidXRRuntimeImageLibrary.cs" company="Google LLC">
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
    using System.Text;
    using Google.XR.Extensions.Internal;
    using UnityEngine;
    using UnityEngine.XR.ARSubsystems;

    /// <summary>
    /// Constructs a <see cref="RuntimeReferenceImageLibrary"/> which stores reference images
    /// for Marker Tracking and QR Code trackng at Android XR devices.
    /// </summary>
    [SuppressMessage(
        "UnityRules.UnityStyleRules", "US1109:PublicPropertiesMustBeUpperCamelCase",
        Justification = "Override Unity interface.")]
    public class AndroidXRRuntimeImageLibrary : RuntimeReferenceImageLibrary
    {
        private static readonly string _qrCodeReferenceName = "qrcode";
        private readonly List<XRReferenceImage> _markers = new List<XRReferenceImage>();
        private readonly List<XRMarkerDatabaseEntry> _markerEntries =
            new List<XRMarkerDatabaseEntry>();

        private int _markerReferenceCount = 0;
        private int _qrCodeReferenceCount = 0;

        private int _markerReferenceIndex = -1;
        private int _qrCodeReferenceIndex = -1;

        private XRReferenceImage? _qrCodeReference = null;

        /// <summary>Constructs a <see cref="AndroidXRRuntimeImageLibrary"/> from a given
        /// <see cref="XRReferenceImageLibrary"/></summary>
        /// <param name="library">The <see cref="XRReferenceImageLibrary"/> to collect images from.
        /// </param>
        public AndroidXRRuntimeImageLibrary(XRReferenceImageLibrary library)
        {
            if (!library)
            {
                return;
            }

            foreach (var reference in library)
            {
                if (IsQrCodeReference(reference))
                {
                    // Only the first QR Code reference takes effect.
                    if (_qrCodeReference == null)
                    {
                        _qrCodeReference = reference;
                    }
                    else
                    {
                        Debug.LogWarning("Only the first QR Code reference takes effect. " +
                                         $"Ignoring XRReferenceImage <i>{reference.name}</i>.");
                    }
                }
                else if (XRMarkerDatabaseEntry.TryParse(
                    reference.name, reference.specifySize ? reference.width : 0f,
                    out XRMarkerDatabaseEntry entry))
                {
                    _markers.Add(reference);
                    entry.SetReference(reference.guid);
                    _markerEntries.Add(entry);
                }
            }

            var hasQrCode = _qrCodeReference == null;
            _qrCodeReferenceCount = hasQrCode ? 0 : 1;
            _markerReferenceCount = _markers.Count;

            _qrCodeReferenceIndex = hasQrCode ? -1 : 0;
            _markerReferenceIndex = _markerReferenceCount == 0 ? -1 : _qrCodeReferenceCount;

            if (Debug.isDebugBuild)
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.Append(
                    $"Created {GetType().Name} with {_qrCodeReferenceCount} QrCode and " +
                    $"{_markerReferenceCount} Marker references.\n");

                if (_qrCodeReference != null)
                {
                    stringBuilder.Append($"    QR Code: {_qrCodeReference.Value.name}\n");
                }

                foreach (var marker in _markerEntries)
                {
                    stringBuilder.Append($"    Marker: {marker}\n");
                }

                Debug.Log(stringBuilder.ToString());
            }
        }

        /// <inheritdoc/>
        public override int count => _qrCodeReferenceCount + _markerReferenceCount;

        /// <summary>
        /// The number of QR Code references in the library.
        /// </summary>
        public int QrCodeReferenceCount => _qrCodeReferenceCount;

        /// <summary>
        /// The number of marker references in the library.
        /// It can be used to iterate all marker reference images starting from
        /// <see cref="MarkerReferenceIndex"/>.
        /// </summary>
        public int MarkerReferenceCount => _markerReferenceCount;

        /// <summary>
        /// The index of the QR Code reference in the library, or -1 if no QR Code was added.
        /// </summary>
        public int QrCodeReferenceIndex => _qrCodeReferenceIndex;

        /// <summary>
        /// The index of the first marker reference in the library, or -1 if no marker was added.
        /// </summary>
        public int MarkerReferenceIndex => _markerReferenceIndex;

        /// <summary>
        /// The QR Code<see cref="XRReferenceImage"/>,
        /// or <c>null</c> if it's not found from the library.
        /// </summary>
        public XRReferenceImage? QRCodeReference => _qrCodeReference;

#if UNITY_EDITOR
        /// <summary>
        /// Validate QR code configuration with a given <see cref="XRReferenceImageLibrary"/>.
        /// Note: only one reference is needed for QR code tracking,
        /// where the <see cref="XRReferenceImage.name"/> starts with <c>QRCode</c>,
        /// case insensitive.
        /// </summary>
        /// <param name="imageLibrary">
        /// The <see cref="XRReferenceImageLibrary"/> with QR code reference in it.
        /// </param>
        public static void ValidateQrCode(IReferenceImageLibrary imageLibrary)
        {
            string qrCodeReference = string.Empty;
            for (int i = 0; i < imageLibrary.count; i++)
            {
                XRReferenceImage image = imageLibrary[i];
                if (IsQrCodeReference(image))
                {
                    if (string.IsNullOrEmpty(qrCodeReference))
                    {
                        qrCodeReference = image.name;
                    }
                    else
                    {
                        Debug.LogWarning(
                            "Found multiple QR Code reference images! " +
                            $"Only \"{qrCodeReference}\" will take effect.");
                    }
                }
            }

            if (string.IsNullOrEmpty(qrCodeReference))
            {
                Debug.LogError(
                    "Cannot find QR Code reference, QR Code Tracking will not do anything!");
            }
        }

        /// <summary>
        /// Validate marker configuration with a given <see cref="XRReferenceImageLibrary"/>.
        /// It may be updated through <see cref="XRMarkerDatabase"/> or manually added marker
        /// references where the <see cref="XRReferenceImage.name"/> matchs format
        /// "{<see cref="XRMarkerDictionary"/>}-{markId}".
        /// </summary>
        /// <param name="imageLibrary"></param>
        public static void ValidateMarker(IReferenceImageLibrary imageLibrary)
        {
            XRMarkerDatabase data = ScriptableObject.CreateInstance<XRMarkerDatabase>();
            data.name = "ImageLibraryValidation";
            for (int i = 0; i < imageLibrary.count; i++)
            {
                if (XRMarkerDatabaseEntry.TryParse(
                    imageLibrary[i].name, out XRMarkerDatabaseEntry entry)) {
                    data.AddEntry(entry, false);
                }
            }

            if (data.Count == 0)
            {
                Debug.LogError(
                    "Cannot find marker reference, Marker Tracking will not do anything.");
                return;
            }

            List<string> errors = data.GetValidationError();
            foreach (var error in errors)
            {
                Debug.LogError(error);
            }
        }
#endif // UNITY_EDITOR

        /// <summary>
        /// Determine if the given reference image is a QR code reference.
        /// </summary>
        /// <param name="image">The reference image with QR code name.</param>
        /// <returns>
        /// <c>true</c>, if it's a valid QR code reference.
        /// </returns>
        public static bool IsQrCodeReference(XRReferenceImage image)
        {
            return image.name.ToLower().StartsWith(_qrCodeReferenceName);
        }

        /// <summary>
        /// Determine if the given reference image is a marker reference.
        /// </summary>
        /// <param name="image">The reference image with marker name or generated from
        /// <see cref="XRMarkerDatabase"/>.</param>
        /// <returns>
        /// <c>true</c>, if it's a valid marker reference.
        /// </returns>
        public static bool IsMarkerReference(XRReferenceImage image)
        {
            return XRMarkerDatabaseEntry.TryParse(image.name, out XRMarkerDatabaseEntry entry);
        }

        internal List<XRMarkerDatabaseEntry> GetMarkerEntries() => _markerEntries;

        /// <inheritdoc/>
        protected override XRReferenceImage GetReferenceImageAt(int index)
        {
            if (_markerReferenceIndex != -1 && index >= _markerReferenceIndex)
            {
                return _markers[index - _markerReferenceIndex];
            }

            if (_qrCodeReference != null)
            {
                return _qrCodeReference.Value;
            }

            throw new ArgumentOutOfRangeException(nameof(index), index,
                $"No valid QR Code or Marker reference found for {nameof(index)}.");
        }
    }
}
