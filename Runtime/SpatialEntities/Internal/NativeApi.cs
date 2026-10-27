// <copyright file="NativeApi.cs" company="Google LLC">
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
    using System;
    using System.Linq;
    using System.Runtime.InteropServices;
    using Google.XR.Extensions.Internal;
    using UnityEngine.XR.ARSubsystems;
    using UnityEngine.XR.OpenXR.NativeTypes;

    using CoreConstant = Extensions.Internal.ApiConstants;
    using XrFutureEXT = System.UInt64;
    using XrSpatialContextEXT = System.UInt64;
    using XrSpatialEntityIdEXT = System.UInt64;

    /// <summary>
    /// Groups spatial entities into a single type to match Unity features.
    /// Mostly covers all supported configurations and extend
    /// <see cref="TrackableType"/> with wider scope.
    /// </summary>
    internal enum XRSpatialEntityType : int
    {
        None = 0,
        Plane = 1,
        Anchor = 2,
        QrCode = 3,
        MicroQrCode = 4,
        ArucoMarker = 5,
        AprilTag = 6,
        Object = 7,
    }

    internal class NativeApi
    {
        public static bool TryInit(XRSpatialEntityType type)
        {
            return ExternalApi.XrSpatialEntity_tryInit(XRInstanceManagerApi.GetIntPtr(), type);
        }

        public static void Deinit(XRSpatialEntityType type)
        {
            ExternalApi.XrSpatialEntity_deinit(XRInstanceManagerApi.GetIntPtr(), type);
        }

        public static bool TryGetContext(
            XRSpatialEntityType type, out XrSpatialContextEXT context)
        {
            context = 0;
            return ExternalApi.XrSpatialEntity_tryGetContext(
                XRInstanceManagerApi.GetIntPtr(), type, ref context);
        }

        public static void InjectAnchorConfiguration(
            XrSpatialComponentTypeEXT[] requestedComponents)
        {
            const XrStructureType anchorConfigurationType =
                XrStructureType.SpatialCapabilityConfigurationAnchorEXT;
            if (requestedComponents == null)
            {
                // Reset previous injections:
                var emptyArray = new XrSpatialComponentTypeEXT[0];
                ExternalApi.XrSpatialEntity_injectComponents(
                    XRInstanceManagerApi.GetIntPtr(), anchorConfigurationType, emptyArray, 0);
            }
            else
            {
                ExternalApi.XrSpatialEntity_injectComponents(
                    XRInstanceManagerApi.GetIntPtr(), anchorConfigurationType,
                    requestedComponents, (uint)requestedComponents.Count());
            }
        }

        private struct ExternalApi
        {
            [DllImport(CoreConstant.OpenXRAndroidApi)]
            public static extern bool XrSpatialEntity_tryInit(
                IntPtr manager, XRSpatialEntityType type);

            [DllImport(CoreConstant.OpenXRAndroidApi)]
            public static extern void XrSpatialEntity_deinit(
                IntPtr manager, XRSpatialEntityType type);

            [DllImport(CoreConstant.OpenXRAndroidApi)]
            public static extern bool XrSpatialEntity_tryGetContext(
                IntPtr manager, XRSpatialEntityType type, ref XrSpatialContextEXT context);

            [DllImport(CoreConstant.OpenXRAndroidApi)]
            public static extern void XrSpatialEntity_injectComponents(
                IntPtr manager, XrStructureType configuration_type,
                XrSpatialComponentTypeEXT[] component_types, uint component_count);
        }
    }
}
