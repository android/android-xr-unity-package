// <copyright file="SpatialTypeUtils.cs" company="Google LLC">
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
    using System.Collections.Generic;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using UnityEngine;
    using UnityEngine.XR.ARSubsystems;
    using UnityEngine.XR.OpenXR.NativeTypes;

    using XrSpatialContextEXT = System.UInt64;
    using XrSpatialEntityIdEXT = System.UInt64;

    /// <summary>
    /// The native enum <c>XrStructureType</c> provided by vendor extensions.
    /// For general types, refer to <see cref="XrStructureType"/>.
    /// </summary>
    internal enum XrStructureTypeAndroid : int
    {
        //// XR_TYPE_SPATIAL_CAPABILITY_CONFIGURATION_OBJECT_TRACKING_ANDROID
        SpatialCapabilityConfigurationObjectTracking = 1000785000,
        //// XR_TYPE_SPATIAL_COMPONENT_OBJECT_SEMANTIC_LABEL_LIST_ANDROID
        SpatialComponentObjectSemanticLabelList = 1000785001,
    }

    /// <summary>
    /// The native enum <c>XrSpatialCapabilityEXT</c> provided by vendor extensions.
    /// For general types, refer to <see cref="XrSpatialCapabilityEXT"/>.
    /// </summary>
    internal enum XrSpatialCapabilityAndroid : int
    {
        //// XR_SPATIAL_CAPABILITY_OBJECT_TRACKING_ANDROID
        Object = 1000785000,
    }

    /// <summary>
    /// The native enum <c>XrSpatialComponentTypeEXT</c> provided by vendor extensions.
    /// For general types, refer to <see cref="XrSpatialComponentTypeEXT"/>.
    /// </summary>
    internal enum XrSpatialComponentTypeAndroid : int
    {
        //// XR_SPATIAL_COMPONENT_TYPE_OBJECT_SEMANTIC_LABEL_ANDROID
        ObjectSemanticLabel = 1000785000,
    }

    /// <summary>
    /// The native enum <c>XrSpatialPersistenceScopeEXT</c> provided by vendor extensions.
    /// For general types, refer to <see cref="XrSpatialPersistenceScopeEXT"/>.
    /// </summary>
    internal enum XrSpatialPersistenceScopeAndroid : int
    {
    }

    internal enum XrSpatialObjectSemanticLabel : int
    {
        //// XR_SPATIAL_OBJECT_SEMANTIC_LABEL_UNCATEGORIZED_ANDROID
        Uncategorized = 0,
        //// XR_SPATIAL_OBJECT_SEMANTIC_LABEL_KEYBOARD_ANDROID
        Keyboard = 1,
        //// XR_SPATIAL_OBJECT_SEMANTIC_LABEL_MOUSE_ANDROID
        Mouse = 2,
        //// XR_SPATIAL_OBJECT_SEMANTIC_LABEL_LAPTOP_BASE_ANDROID
        LaptopBase = 3,
    }

    /// <summary>
    /// A subset from <see cref="BoundingBoxClassifications"/> with only supported types
    /// to match Editor view dropdown options, and avoid overflow on maskfield value.
    /// </summary>
    [Flags]
    internal enum SpatialObjectDetectionModes : int
    {
        Keyboard = 1 << 0,
        Mouse = 1 << 1,
        Laptop = 1 << 2,
        Other = 1 << 3,
    }

    internal static class SpatialTypeUtils
    {
        public static SpatialObjectDetectionModes ObjectDetectionModesAll =
            SpatialObjectDetectionModes.Keyboard |
            SpatialObjectDetectionModes.Mouse |
            SpatialObjectDetectionModes.Laptop |
            SpatialObjectDetectionModes.Other;

        public static TrackingState ToTrackingState(this XrSpaceLocationFlags locationFlags)
        {
            if (locationFlags.HasFlag(XrSpaceLocationFlags.OrientationTracked) ||
                locationFlags.HasFlag(XrSpaceLocationFlags.PositionTracked))
            {
                return TrackingState.Tracking;
            }
            else if (locationFlags.HasFlag(XrSpaceLocationFlags.OrientationValid) ||
                locationFlags.HasFlag(XrSpaceLocationFlags.PositionValid))
            {
                return TrackingState.Limited;
            }
            else
            {
                return TrackingState.None;
            }
        }

        public static XrStructureType ToExt(this XrStructureTypeAndroid android) =>
            (XrStructureType)android;

        public static XrSpatialCapabilityEXT ToExt(this XrSpatialCapabilityAndroid android) =>
            (XrSpatialCapabilityEXT)android;

        public static XrSpatialComponentTypeEXT ToExt(this XrSpatialComponentTypeAndroid android) =>
            (XrSpatialComponentTypeEXT)android;

        public static XrSpatialPersistenceScopeEXT ToExt(
            this XrSpatialPersistenceScopeAndroid android) => (XrSpatialPersistenceScopeEXT)android;

        // Equivalent to the default constructor but with alias.
        public static TrackableId ConstructTrackableId(
            XrSpatialContextEXT contextHandle, XrSpatialEntityIdEXT entityId) =>
            new TrackableId(contextHandle, entityId);

        public static XrSpatialContextEXT GetContextHandle(this TrackableId trackableId) =>
            trackableId.subId1;

        public static XrSpatialEntityIdEXT GetEntityId(this TrackableId trackableId) =>
            trackableId.subId2;

        public static Vector3 ToVector3(this XrExtent3Df extents) =>
            new Vector3(extents.width, extents.height, extents.depth);

        public static Pose ToPose(this XrPosef xrPose) =>
            new Pose(xrPose.Position.AsVector3(), xrPose.Orientation.AsQuaternion());

        public static TrackingState ToTrackingState(this XrSpatialEntityTrackingStateEXT state)
        {
            switch (state)
            {
                case XrSpatialEntityTrackingStateEXT.Tracking:
                    return TrackingState.Tracking;
                case XrSpatialEntityTrackingStateEXT.Paused:
                    return TrackingState.Limited;
                case XrSpatialEntityTrackingStateEXT.Stopped:
                default:
                    return TrackingState.None;
            }
        }

        public static SerializableGuid ToSerializableGuid(this XrUuid uuid)
        {
            // Expect to be replaced by implicit operator with ARF updates.
            return new SerializableGuid(uuid.dataPart1, uuid.dataPart2);
        }

        public static bool TryGetPersistenceScope(
            this XRShareableAnchorHosts host, out XrSpatialPersistenceScopeEXT scope)
        {
            switch (host)
            {
                case XRShareableAnchorHosts.Local:
                    scope = XrSpatialPersistenceScopeEXT.LocalAnchors;
                    return true;
                default:
                    scope = XrSpatialPersistenceScopeEXT.SystemManaged;
                    return false;
            }
        }

        /// <summary>
        /// Gets display name of a given capability. It converts to
        /// <see cref="XrSpatialCapabilityAndroid"/> when possible.
        /// </summary>
        /// <param name="capability">
        /// A capability returned by the OpenXR runtime.</param>
        /// <returns>The display name of the given capability.</returns>
        public static string GetDisplayName(this XrSpatialCapabilityEXT capability)
        {
            var androidType = (XrSpatialCapabilityAndroid)capability;
            return Enum.IsDefined(typeof(XrSpatialCapabilityAndroid), androidType) ?
                androidType.ToString() : capability.ToString();
        }

        /// <summary>
        /// Gets display name of a given component. It converts to
        /// <see cref="XrSpatialComponentTypeAndroid"/> when possible.
        /// </summary>
        /// <param name="component">
        /// A component returned by the OpenXR runtime.</param>
        /// <returns>The display name of the given component.</returns>
        public static string GetDisplayName(this XrSpatialComponentTypeEXT component)
        {
            var androidType = (XrSpatialComponentTypeAndroid)component;
            return Enum.IsDefined(typeof(XrSpatialComponentTypeAndroid), androidType) ?
                androidType.ToString() : component.ToString();
        }

        /// <summary>
        /// Gets display name of a given scope. It converts to
        /// <see cref="XrSpatialPersistenceScopeAndroid"/> when possible.
        /// </summary>
        /// <param name="scope">
        /// A scope returned by the OpenXR runtime.</param>
        /// <returns>The display name of the given scope.</returns>
        public static string GetDisplayName(this XrSpatialPersistenceScopeEXT scope)
        {
            var androidType = (XrSpatialPersistenceScopeAndroid)scope;
            return Enum.IsDefined(typeof(XrSpatialPersistenceScopeAndroid), androidType) ?
                androidType.ToString() : scope.ToString();
        }

        public static BoundingBoxClassifications ToClassifications(
            this XrSpatialObjectSemanticLabel label)
        {
            switch (label)
            {
                case XrSpatialObjectSemanticLabel.Keyboard:
                    return BoundingBoxClassifications.Keyboard;
                case XrSpatialObjectSemanticLabel.Mouse:
                    return BoundingBoxClassifications.Mouse;
                case XrSpatialObjectSemanticLabel.LaptopBase:
                    return BoundingBoxClassifications.Laptop;
                case XrSpatialObjectSemanticLabel.Uncategorized:
                default:
                    return BoundingBoxClassifications.Other;
            }
        }

        public static XrSpatialObjectSemanticLabel[] ToLabels(
            this SpatialObjectDetectionModes modes)
        {
            var labels = new List<XrSpatialObjectSemanticLabel>();
            if (modes.HasFlag(SpatialObjectDetectionModes.Keyboard))
            {
                labels.Add(XrSpatialObjectSemanticLabel.Keyboard);
            }

            if (modes.HasFlag(SpatialObjectDetectionModes.Mouse))
            {
                labels.Add(XrSpatialObjectSemanticLabel.Mouse);
            }

            if (modes.HasFlag(SpatialObjectDetectionModes.Laptop))
            {
                labels.Add(XrSpatialObjectSemanticLabel.LaptopBase);
            }

            if (modes.HasFlag(SpatialObjectDetectionModes.Other))
            {
                labels.Add(XrSpatialObjectSemanticLabel.Uncategorized);
            }

            return labels.ToArray();
        }

        public static unsafe void UnsafeAlloc<T>(
            out T* ptr, bool clear = false, Allocator allocator = Allocator.Temp)
            where T : unmanaged
        {
            int size = UnsafeUtility.SizeOf<T>();
            ptr = (T*)UnsafeUtility.Malloc(size, UnsafeUtility.AlignOf<T>(), allocator);
            if (clear)
            {
                UnsafeUtility.MemClear(ptr, size);
            }
        }

        // Due to Unity XrBaseInStructure does not declare const,
        // it can be used for both XrBaseInStructure and XrBaseOutStructure types.
        public static unsafe bool TryGetStructInChain<T>(
            void* next, XrStructureType structureType, out T target)
            where T : unmanaged
        {
            while (next != null)
            {
                var baseIn = *(XrBaseInStructure*)next;
                if (baseIn.type == structureType)
                {
                    target = *(T*)next;
                    return true;
                }

                next = baseIn.next;
            }

            target = default;
            return false;
        }
    }
}
