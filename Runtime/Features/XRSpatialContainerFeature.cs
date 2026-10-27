// <copyright file="XRSpatialContainerFeature.cs" company="Google LLC">
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
    using Google.XR.Extensions.Internal;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.XR.OpenXR;
    using UnityEngine.XR.OpenXR.Features;
    using UnityEngine.XR.OpenXR.NativeTypes;

#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.XR.OpenXR.Features;
#endif  // UNITY_EDITOR

#if UNITY_OPEN_XR_ANDROID_XR
    using ARSessionFeature = UnityEngine.XR.OpenXR.Features.Android.ARSessionFeature;
#endif

    using EventData = Internal.XRSpatialContainerEventData;

    /// <summary>
    /// This feature provides access to the <c>XR_EXT_spatial_container_self_rendering</c>
    /// extension. This feature allows the application content to be rendered within a single
    /// spatial container.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(
        UiName = UiName,
        BuildTargetGroups = new[] {
            BuildTargetGroup.Android,
        },
        Company = "Google",
        Desc = "Configure Spatial Container",
        Version = "1.0.0",
        Category = FeatureCategory.Feature,
        FeatureId = FeatureId,
        OpenxrExtensionStrings = ExtensionStrings, Priority = 99)]
#endif  // UNITY_EDITOR
    public class XRSpatialContainerFeature : OpenXRFeature
    {
        /// <summary>
        /// The UI name shows on the XR Plug-in Management panel, help users to understand
        /// validation errors and expected fixes.
        /// </summary>
        public const string UiName = "Spatial Container";

        /// <summary>
        /// The feature ID string.
        /// </summary>
        public const string FeatureId = "com.google.xr.extensions.spatial_container";

        /// <summary>
        /// The required OpenXR extension.
        /// </summary>
        public const string ExtensionStrings =
            "XR_EXT_spatial_container XR_EXT_spatial_container_self_rendering";

        /// <summary>
        /// Runtime permission required to enable scene understanding coarse.
        /// </summary>
        public static readonly AndroidXRPermission RequiredPermission =
            AndroidXRPermission.SceneUnderstandingCoarse;

        internal static bool? _extensionEnabled = null;

        internal static ulong _spatialContainer = 0;

        private static SpatialContainerChangedDelegate _spatialContainerChangedDelegate =
            OnSpatialContainerChangedCallback;

        private static bool _extensionAvailable = false;
        private static bool _extensionSupported = false;

        [Tooltip("The suggested bounds of the spatial container.")]
        [SerializeField]
        private Vector3 _suggestedBounds = new Vector3(2.0f, 2.0f, 2.0f);

        private bool _eventsSubscribed = false;
        private int _currentBoundsMode = EventData.BoundsModeBounded;

        /// <summary>
        /// Invoked when the spatial container is closed.
        /// </summary>
        public static event Action OnSpatialContainerClosed;

        /// <summary>
        /// Invoked when the spatial container bounds have changed.
        /// </summary>
        public static event Action<Vector3, bool> OnSpatialContainerBoundsChanged;

        /// <summary>
        /// Invoked when a spatial container state has changed.
        /// </summary>
        public static event Action<XRSpatialContainerStateFlags, bool>
            OnSpatialContainerStateChanged;

        /// <summary>
        /// Invoked when a spatial container visibility request is denied.
        /// </summary>
        public static event Action OnSpatialContainerVisibilityRequestDenied;

        /// <summary>
        /// Invoked when a spatial container bounds mode request is denied.
        /// </summary>
        public static event Action OnSpatialContainerBoundsModeRequestDenied;

        /// <summary>
        /// Gets if the required OpenXR extension is enabled.
        /// When OpenXR runtime is waiting, it returns <c>null</c>. Otherwise, it indicates whether
        /// the <c>XR_EXT_spatial_container_self_rendering</c> extension is available and supported
        /// on current device.
        /// </summary>
        public static bool? IsExtensionEnabled => _extensionEnabled;

        /// <summary>
        /// Gets the suggested bounds.
        /// </summary>
        public Vector3 SuggestedBounds => _suggestedBounds;

        /// <summary>
        /// Tries to get the state of the spatial container.
        /// </summary>
        /// <param name="outState">The state of the spatial container.</param>
        /// <returns>True if the state was retrieved successfully, false otherwise.</returns>
        public static bool TryGetState(out XRSpatialContainerStateFlags outState)
        {
            outState = default;
            if (!_extensionEnabled.HasValue || !_extensionEnabled.Value)
            {
                Debug.LogWarning("Spatial Container extension is not enabled.");
                return false;
            }

            return XRSpatialContainerApi.TryGetState(out outState);
        }

        /// <summary>
        /// Tries to get the bounds of the spatial container.
        /// </summary>
        /// <param name="outBounds">The dimensions of the spatial container bounding box.</param>
        /// <returns>True if the bounds were retrieved successfully, false otherwise.</returns>
        public static bool TryGetBounds(out Vector3 outBounds)
        {
            outBounds = Vector3.zero;
            if (!_extensionEnabled.HasValue || !_extensionEnabled.Value)
            {
                Debug.LogWarning("Spatial Container extension is not enabled.");
                return false;
            }

            return XRSpatialContainerApi.TryGetBounds(ref outBounds);
        }

        /// <summary>
        /// Tries to request the bounds mode of the spatial container. By default, the spatial
        /// container starts in bounded mode.
        /// </summary>
        /// <param name="immersive">Whether to request immersive bounds mode. If false, requests
        /// bounded mode.</param>
        /// <returns>True if the bounds mode was requested successfully, false otherwise.</returns>
        public static bool TryRequestBoundsMode(bool immersive)
        {
            if (!_extensionEnabled.HasValue || !_extensionEnabled.Value)
            {
                Debug.LogWarning("Spatial Container extension is not enabled.");
                return false;
            }

            return XRSpatialContainerApi.TryRequestBoundsMode(immersive);
        }

        /// <summary>
        /// Tries to request the visibility of the spatial container. By default, the spatial
        /// container starts in visible mode.
        /// </summary>
        /// <param name="visible">Whether to request the container to be visible.</param>
        /// <returns>True if the visibility was requested successfully, false otherwise.</returns>
        public static bool TryRequestVisibility(bool visible)
        {
            if (!_extensionEnabled.HasValue || !_extensionEnabled.Value)
            {
                Debug.LogWarning("Spatial Container extension is not enabled.");
                return false;
            }

            return XRSpatialContainerApi.TryRequestVisibility(visible);
        }

        /// <inheritdoc/>
        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            if (!base.OnInstanceCreate(xrInstance))
            {
                return false;
            }

            _extensionAvailable = true;
            string[] extensions = ExtensionStrings.Split();
            foreach (string extension in extensions)
            {
                if (string.IsNullOrEmpty(extension))
                {
                    // ExtensionStrings.Split() creates an array of size one where the first element
                    // is an empty string if the input string is empty.
                    continue;
                }

                bool extensionEnabled = OpenXRRuntime.IsExtensionEnabled(extension);
                if (!extensionEnabled)
                {
                    Debug.LogErrorFormat(
                        "{0} is not supported by current runtime, failed to enable {1}.",
                        extension, UiName);
                    _extensionAvailable = false;
                    break;
                }
            }

            if (!_extensionAvailable)
            {
                _extensionEnabled = false;
            }

            bool result = _extensionAvailable &&
                   XRInstanceManagerApi.Register(ApiXrFeature.SpatialContainer);
            if (result)
            {
                Vector3 suggestedBounds = SuggestedBounds;
                XRSpatialContainerApi.Configure(
                    ref suggestedBounds, _spatialContainerChangedDelegate);
            }

            return result;
        }

        /// <inheritdoc/>
        protected override void OnInstanceDestroy(ulong xrInstance)
        {
            if (_extensionEnabled.HasValue && _extensionEnabled.Value)
            {
                ToggleSpatialContainerEvents(false);
            }

            XRInstanceManagerApi.Unregister(ApiXrFeature.SpatialContainer);
        }

        /// <inheritdoc/>
        protected override void OnSystemChange(ulong xrSystem)
        {
            if (_extensionAvailable && XRInstanceManagerApi.CheckSystemSupport(
                ApiXrFeature.SpatialContainer, ref _extensionSupported))
            {
                _extensionEnabled = _extensionSupported;
                if (!_extensionSupported)
                {
                    Debug.LogWarningFormat(
                        "System {0} does not support feature {1}.", xrSystem, UiName);
                }
                else
                {
                    ToggleSpatialContainerEvents(true);
                }
            }
        }

        [AOT.MonoPInvokeCallback(typeof(SpatialContainerChangedDelegate))]
        private static void OnSpatialContainerChangedCallback(ulong spatialContainer)
        {
            _spatialContainer = spatialContainer;
        }

        private void ToggleSpatialContainerEvents(bool subscribe)
        {
            if (subscribe == _eventsSubscribed)
            {
                return;
            }

            unsafe
            {
                XrStructureType closed = EventData.SpatialContainerClosedEXT;
                XrStructureType boundsChanged = EventData.SpatialContainerBoundsChangedEXT;
                XrStructureType visibleChanged =
                    EventData.SpatialContainerVisibleChangedEXT;
                XrStructureType visibleRequestDenied =
                    EventData.SpatialContainerVisibleRequestDeniedEXT;
                XrStructureType interactableChanged =
                    EventData.SpatialContainerInteractableChangedEXT;
                XrStructureType boundsModeRequestDenied =
                    EventData.SpatialContainerBoundsModeRequestDeniedEXT;

                if (subscribe)
                {
                    PollEventRouter.TrySubscribeToEventType(closed, OnSpatialContainerClosedEvent);
                    PollEventRouter.TrySubscribeToEventType(
                        boundsChanged, OnSpatialContainerBoundsChangedEvent);
                    PollEventRouter.TrySubscribeToEventType(
                        visibleChanged, OnSpatialContainerVisibleChangedEvent);
                    PollEventRouter.TrySubscribeToEventType(
                        visibleRequestDenied, OnSpatialContainerVisibleRequestDeniedEvent);
                    PollEventRouter.TrySubscribeToEventType(
                        interactableChanged, OnSpatialContainerInteractableChangedEvent);
                    PollEventRouter.TrySubscribeToEventType(
                        boundsModeRequestDenied, OnSpatialContainerBoundsModeRequestDeniedEvent);
                }
                else
                {
                    PollEventRouter.TryUnsubscribeFromEventType(
                        closed, OnSpatialContainerClosedEvent);
                    PollEventRouter.TryUnsubscribeFromEventType(
                        boundsChanged, OnSpatialContainerBoundsChangedEvent);
                    PollEventRouter.TryUnsubscribeFromEventType(
                        visibleChanged, OnSpatialContainerVisibleChangedEvent);
                    PollEventRouter.TryUnsubscribeFromEventType(
                        visibleRequestDenied, OnSpatialContainerVisibleRequestDeniedEvent);
                    PollEventRouter.TryUnsubscribeFromEventType(
                        interactableChanged, OnSpatialContainerInteractableChangedEvent);
                    PollEventRouter.TryUnsubscribeFromEventType(
                        boundsModeRequestDenied, OnSpatialContainerBoundsModeRequestDeniedEvent);
                }

                _eventsSubscribed = subscribe;
            }
        }

        private unsafe void OnSpatialContainerClosedEvent(XrEventDataBaseHeader* baseHeader)
        {
            var eventData = *(EventData.XrEventDataSpatialContainerClosedEXT*)baseHeader;
            if (eventData.SpatialContainer == _spatialContainer)
            {
                OnSpatialContainerClosed?.Invoke();
            }
        }

        private unsafe void OnSpatialContainerBoundsChangedEvent(XrEventDataBaseHeader* baseHeader)
        {
            var eventData = *(EventData.XrEventDataSpatialContainerBoundsChangedEXT*)baseHeader;
            if (eventData.SpatialContainer == _spatialContainer)
            {
                OnSpatialContainerBoundsChanged?.Invoke(
                    eventData.Bounds, eventData.InfiniteBounds != 0);
                if (eventData.BoundsMode == EventData.BoundsModeBounded ||
                    eventData.BoundsMode == EventData.BoundsModeImmersive)
                {
                    if (eventData.BoundsMode != _currentBoundsMode)
                    {
                        _currentBoundsMode = eventData.BoundsMode;
                        OnSpatialContainerStateChanged?.Invoke(
                            XRSpatialContainerStateFlags.Immersive,
                            eventData.BoundsMode == EventData.BoundsModeImmersive);
                    }
                }
                else
                {
                    Debug.LogError(
                        "Unknown XrSpatialContainerBoundsModeEXT: " + eventData.BoundsMode);
                }
            }
        }

        private unsafe void OnSpatialContainerVisibleChangedEvent(
            XrEventDataBaseHeader* baseHeader)
        {
            var eventData =
                *(EventData.XrEventDataSpatialContainerVisibleChangedEXT*)baseHeader;
            if (eventData.SpatialContainer == _spatialContainer)
            {
                OnSpatialContainerStateChanged?.Invoke(
                    XRSpatialContainerStateFlags.Visible,
                    eventData.Visible != 0);
            }
        }

        private unsafe void OnSpatialContainerVisibleRequestDeniedEvent(
            XrEventDataBaseHeader* baseHeader)
        {
            var eventData =
                *(EventData.XrEventDataSpatialContainerVisibleRequestDeniedEXT*)baseHeader;
            if (eventData.SpatialContainer == _spatialContainer)
            {
                OnSpatialContainerVisibilityRequestDenied?.Invoke();
            }
        }

        private unsafe void OnSpatialContainerInteractableChangedEvent(
            XrEventDataBaseHeader* baseHeader)
        {
            var eventData =
                *(EventData.XrEventDataSpatialContainerInteractableChangedEXT*)baseHeader;
            if (eventData.SpatialContainer == _spatialContainer)
            {
                OnSpatialContainerStateChanged?.Invoke(
                    XRSpatialContainerStateFlags.Interactive,
                    eventData.Interactable != 0);
            }
        }

        private unsafe void OnSpatialContainerBoundsModeRequestDeniedEvent(
            XrEventDataBaseHeader* baseHeader)
        {
            var eventData =
                *(EventData.XrEventDataSpatialContainerBoundsModeRequestDeniedEXT*)baseHeader;
            if (eventData.SpatialContainer == _spatialContainer)
            {
                OnSpatialContainerBoundsModeRequestDenied?.Invoke();
            }
        }

#if UNITY_EDITOR
        /// <inheritdoc/>
        protected override void GetValidationChecks(
            List<ValidationRule> results, BuildTargetGroup targetGroup)
        {
            if (targetGroup != BuildTargetGroup.Android)
            {
                return;
            }

            var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);

#if UNITY_OPEN_XR_ANDROID_XR
            results.Add(new ValidationRule(this)
            {
                message = "android.window.PROPERTY_XR_ACTIVITY_START_MODE will be set to " +
                    "<b>XR_ACTIVITY_START_MODE_FULL_SPACE_UNMANAGED</b> by Unity OpenXR " +
                    "AndroidXR Package which may cause transition artifacts.",
                checkPredicate = () =>
                {
                    return false;
                },
                error = false,
            });
#endif

            var extSessionFeature = settings.GetFeature<XRSessionFeature>();
            results.Add(new ValidationRule(this)
            {
                message = "Immersive mode cannot be used with this feature.",
                checkPredicate = () =>
                {
                    if (settings == null)
                    {
                        return false;
                    }

                    return !(extSessionFeature != null && extSessionFeature.ImmersiveXR);
                },
                fixItMessage = "Disable Immersive Mode.",
                fixIt = () =>
                {
                    if (settings == null)
                    {
                        Debug.LogWarningFormat(
                            "Autofix failed with missing OpenXRSettings on {0} platform.",
                            targetGroup);
                        return;
                    }

                    if (extSessionFeature == null)
                    {
                        Debug.LogWarningFormat(
                            "Autofix failed with missing {1} feature on {0} platform.",
                            targetGroup, XRSessionFeature.UiName);
                        return;
                    }

                    extSessionFeature.ImmersiveXR = false;
                },
                error = true
            });

            results.Add(new ValidationRule(this)
            {
                message = "The suggested bounds dimensions must be positive.",
                checkPredicate = () =>
                {
                    bool result =
                        _suggestedBounds.x > 0 && _suggestedBounds.y > 0 && _suggestedBounds.z > 0;
                    return result;
                },
                fixItMessage = "Make bounds absolute.",
                fixIt = () =>
                {
                    _suggestedBounds = new Vector3(
                        _suggestedBounds.x == 0f ? 1f : Mathf.Abs(_suggestedBounds.x),
                        _suggestedBounds.y == 0f ? 1f : Mathf.Abs(_suggestedBounds.y),
                        _suggestedBounds.z == 0f ? 1f : Mathf.Abs(_suggestedBounds.z));
                },
                error = true
            });
        }
#endif // UNITY_EDITOR
    }
}
