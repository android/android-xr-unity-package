// <copyright file="AndroidXRManifest.cs" company="Google LLC">
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

namespace Google.XR.Extensions.Editor.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Unity.XR.Management.AndroidManifest.Editor;

    internal static class AndroidXRManifest
    {
        public static readonly string ImmersiveHmd =
            "org.khronos.openxr.intent.category.IMMERSIVE_HMD";

        public static readonly string XrActivityStartMode =
            "android.window.PROPERTY_XR_ACTIVITY_START_MODE";

        public static readonly string XrActivityStartModeFullSpaceUnmanaged =
            "XR_ACTIVITY_START_MODE_FULL_SPACE_UNMANAGED";

        public static readonly string XrActivityStartModeHomeSpace =
            "XR_ACTIVITY_START_MODE_HOME_SPACE";

        public static readonly string OpenXRNativeLibrary = "libopenxr.google.so";

        // Require minimum OpenXR version 1.1 (no patch version specified)
        // Major version bits: 0xffff0000
        // Minor version bits: 0x0000ffff
        public static readonly string XrApiOpenXR = "android.software.xr.api.openxr";
        public static readonly string XrApiOpenXRVersion = "0x00010001";

        public static readonly string XrApiSpatial = "android.software.xr.api.spatial";

        public static readonly string UseExperimentalOpenXRFeature =
            "com.android.extensions.xr.uses_experimental_openxr_feature";

        public static void AddElement(this List<ManifestElement> elements,
            List<string> path, Dictionary<string, string> attributes)
        {
            elements.Add(new ManifestElement()
            {
                ElementPath = path,
                Attributes = attributes,
            });
        }

        // <uses-permission android:name="permission" />
        public static void AddPermission(this List<ManifestElement> elements, string permission)
        {
            elements.AddElement(
                new List<string>() { Element.Manifest, Element.UsesPermission },
                new Dictionary<string, string>()
                {
                    { Attribute.Name, permission }
                });
        }

        // <uses-permission android:name="permission" />
        public static void AddPermission(
            this List<ManifestElement> elements, AndroidXRPermission permission)
        {
            elements.AddPermission(permission.ToPermissionString());
        }

        // <uses-feature android:name="xxx" android:required="true|false" android:version="xxx" />
        public static void AddFeature(
            this List<ManifestElement> elements, string name, bool required, string version = "")
        {
            Dictionary<string, string> attributes = new Dictionary<string, string>
            {
                { Attribute.Name, name },
                { Attribute.Required, required ? Attribute.True : Attribute.False }
            };
            if (!string.IsNullOrEmpty(version))
            {
                attributes.Add(Attribute.Version, version);
            }

            elements.AddElement(
                new List<string>() { Element.Manifest, Element.UsesFeature }, attributes);
        }

        // <uses-feature android:name="android.software.xr.api.openxr" android:required="true"
        //               android:version="0x10001" />
        public static void AddOpenXRApiFeature(this List<ManifestElement> elements)
        {
            elements.AddFeature(XrApiOpenXR, true, XrApiOpenXRVersion);
        }

        // <uses-feature android:name="xr.api.SPATIAL" android:required="true|false"
        //               android:version="version" />
        public static void AddSpatialApiFeature(
            this List<ManifestElement> elements, bool required, XRSpatialSdkVersions version)
        {
            if (version == XRSpatialSdkVersions.XRSpatialApiLevelAuto)
            {
                var allValues = Enum.GetValues(typeof(XRSpatialSdkVersions));
                version = (XRSpatialSdkVersions)allValues.GetValue(allValues.Length - 1);
            }

            elements.AddFeature(XrApiSpatial, required, ((int)version).ToString());
        }

        // <uses-native-library android:name="libopenxr.google.so" android:required="false" />
        public static void AddNativeLibrary(this List<ManifestElement> elements)
        {
            elements.AddElement(
                new List<string>()
                {
                    Element.Manifest, Element.Application, Element.UsesNativeLibrary
                },
                new Dictionary<string, string>
                {
                    { Attribute.Name, OpenXRNativeLibrary },
                    { Attribute.Required, Attribute.False },
                });
        }

        // <property android:name="name" android:value="value" />
        public static void AddProperty(
            this List<ManifestElement> elements, string name, string value)
        {
            elements.AddElement(
                GetActivityPath().Append(Element.Property).ToList(),
                new Dictionary<string, string>
                {
                    { Attribute.Name, name },
                    { Attribute.Value, value },
                });
        }

        // <property android:name="android.window.PROPERTY_XR_ACTIVITY_START_MODE"
        //           android:value="startMode" />
        public static void AddActivityStartMode(
            this List<ManifestElement> elements, string startMode)
        {
            elements.AddProperty(XrActivityStartMode, startMode);
        }

        // <category android:name="name" />
        public static void AddCategory(this List<ManifestElement> elements, string name)
        {
            elements.AddElement(
                GetIntentFilterPath().Append(Element.Category).ToList(),
                new Dictionary<string, string>() { { Attribute.Name, name } });
        }

        // <category android:name="org.khronos.openxr.intent.category.IMMERSIVE_HMD" />
        public static void AddImmersiveHmd(this List<ManifestElement> elements)
        {
            elements.AddCategory(ImmersiveHmd);
        }

        // <meta-data android:name="name" android:value="value" />
        public static void AddMetaData(
            this List<ManifestElement> elements, string name, string value)
        {
            elements.AddElement(
                GetApplicationPath().Append(Element.MetaData).ToList(),
                new Dictionary<string, string>
                {
                    { Attribute.Name, name },
                    { Attribute.Value, value },
                });
        }

        private static List<string> GetApplicationPath()
        {
            return new List<string> { Element.Manifest, Element.Application };
        }

        private static List<string> GetActivityPath()
        {
            return GetApplicationPath().Append(Element.Activity).ToList();
        }

        private static List<string> GetIntentFilterPath()
        {
            return GetActivityPath().Append(Element.IntentFilter).ToList();
        }

        /// <summary>
        /// Common android manifest elements.
        /// </summary>
        public static class Element
        {
            public static readonly string Manifest = "manifest";
            public static readonly string Application = "application";
            public static readonly string Activity = "activity";
            public static readonly string Property = "property";
            public static readonly string IntentFilter = "intent-filter";
            public static readonly string Category = "category";
            public static readonly string MetaData = "meta-data";
            public static readonly string UsesPermission = "uses-permission";
            public static readonly string UsesFeature = "uses-feature";
            public static readonly string UsesNativeLibrary = "uses-native-library";
        }

        /// <summary>
        /// Common android manifest attributes.
        /// </summary>
        public static class Attribute
        {
            public static readonly string Name = "name";
            public static readonly string Value = "value";
            public static readonly string Required = "required";
            public static readonly string True = "true";
            public static readonly string False = "false";
            public static readonly string Version = "version";
        }
    }
}
