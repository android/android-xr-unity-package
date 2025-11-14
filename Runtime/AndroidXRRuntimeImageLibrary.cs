// <copyright file="AndroidXRRuntimeImageLibrary.cs" company="Google LLC">
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
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
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
        private readonly List<XRReferenceImage> _images = new List<XRReferenceImage>();
        private readonly List<XRMarkerDatabaseEntry> _markerEntries =
            new List<XRMarkerDatabaseEntry>();

        private int _qrCodeIndex = -1;
        private int _markerCount = 0;
        private int _markerIndex = -1;

        /// <summary>Constructs a <see cref="AndroidXRRuntimeImageLibrary"/> from a given
        /// <see cref="XRReferenceImageLibrary"/></summary>
        /// <param name="library">The <see cref="XRReferenceImageLibrary"/> to collect images from.
        /// </param>
        public AndroidXRRuntimeImageLibrary(XRReferenceImageLibrary library)
        {
            if (library != null)
            {
                List<XRReferenceImage> markerReferences = new List<XRReferenceImage>();
                foreach (var image in library)
                {
                    // Find QR Code reference and add first.
                    if (IsQrCodeReference(image))
                    {
                        // Only the first QR Code reference takes effect.
                        if (_qrCodeIndex < 0)
                        {
                            _qrCodeIndex = _images.Count;
                            _images.Add(image);
                        }
                    }
                    else if (XRMarkerDatabaseEntry.TryParse(
                        image.name, image.specifySize ? image.width : 0f,
                        out XRMarkerDatabaseEntry entry))
                    {
                        markerReferences.Add(image);
                        entry.SetReference(image.guid);
                        _markerEntries.Add(entry);
                    }
                }

                // Add marker references.
                if (markerReferences.Count > 0)
                {
                    _markerIndex = _images.Count;
                    _markerCount = markerReferences.Count;
                    _images.AddRange(markerReferences);
                }

                StringBuilder stringBuilder = new StringBuilder();
                if (_qrCodeIndex >= 0)
                {
                    if (stringBuilder.Length == 0)
                    {
                        stringBuilder.Append($"Created {this.GetType().Name} with:\n");
                    }

                    stringBuilder.Append(
                        $"    QR Code Reference: {_images[_qrCodeIndex].name}\n");
                }

                if (_markerEntries.Count > 0)
                {
                    if (stringBuilder.Length == 0)
                    {
                        stringBuilder.Append($"Created {this.GetType().Name} with:\n");
                    }

                    foreach (var entry in _markerEntries)
                    {
                        stringBuilder.Append(
                            $"    Marker reference: {entry}\n");
                    }
                }

                if (stringBuilder.Length == 0)
                {
                    stringBuilder.Append($"Created {this.GetType().Name}.");
                }

                Debug.Log(stringBuilder.ToString());
            }
            else
            {
                _qrCodeIndex = -1;
                _markerIndex = -1;
                _markerCount = 0;
                _images.Clear();
                _markerEntries.Clear();
            }
        }

        /// <inheritdoc/>
        public override int count => _images.Count;

        /// <summary>
        /// Gets the index of QR Code reference image. Or return -1 if it doesn't exist.
        /// </summary>
        public int QrCodeReferenceIndex => _qrCodeIndex;

        /// <summary>
        /// Gets QR Code reference image, or <c>null</c> if it's not found from the library.
        /// </returns>
        public XRReferenceImage? QRCodeReference
        {
            get
            {
                if (_qrCodeIndex < 0)
                {
                    return null;
                }
                else
                {
                    return _images[_qrCodeIndex];
                }
            }
        }

        /// <summary>
        /// Gets the index of the first marker reference image. Or return -1 if it doesn't exist.
        /// </summary>
        public int MarkerReferenceIndex => _markerIndex;

        /// <summary>
        /// Gets the count of marker reference images. It can be used to iterate all marker
        /// reference images starting from <see cref="MarkerReferenceIndex"/>.
        /// </summary>
        public int MarkerReferenceCount => _markerCount;

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
                    imageLibrary[i].name, out XRMarkerDatabaseEntry entry))
                {
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
            return _images[index];
        }
    }
}
