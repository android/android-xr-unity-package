// <copyright file="XRAnchorFeature.cs" company="Google LLC">
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
    using System.Collections.Generic;
    using Google.XR.Extensions.Internal;
    using UnityEngine;
    using UnityEngine.XR.ARSubsystems;
    using UnityEngine.XR.Management;
    using UnityEngine.XR.OpenXR;
    using UnityEngine.XR.OpenXR.Features;

#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.XR.OpenXR.Features;
#endif

#if UNITY_OPEN_XR_ANDROID_XR
    using ARPlaneFeature = UnityEngine.XR.OpenXR.Features.Android.ARPlaneFeature;
#endif
#if UNITY_OPEN_XR_ANDROID_XR_0_2_0
    using ARAnchorFeature = UnityEngine.XR.OpenXR.Features.Android.ARAnchorFeature;
#endif

    /// <summary>
    /// This <see cref="OpenXRInteractionFeature"/> configures new extensions
    /// <c>XR_ANDROID_trackables</c> and <c>XR_ANDROID_device_anchor_persistence</c> at runtime and
    /// provides <see cref="XRAnchorSubsystem"/> implementation that works on Android XR platform.
    ///
    /// Note: due to the dependency on <see cref="XRSessionFeature"/> and
    /// <see cref="XRTrackableFeature"/>, its priority must be lower than session and trackable
    /// features so the feature registration happens after <c>XrInstanceManager</c> and
    /// <c>XrTrackableProvider</c> creation.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = UiName,
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        Company = "Google",
        OpenxrExtensionStrings = ExtensionStrings,
        Desc = "Enable Android XR Anchors at runtime. " +
            "And provide the implementation of XRAnchorSubsystem",
        Version = "1.0.0",
        Category = FeatureCategory.Feature,
        FeatureId = FeatureId,
        Priority = 98)]
#endif
    public class XRAnchorFeature : OpenXRFeature
    {
        /// <summary>
        /// The UI name shows on the XR Plug-in Management panel, help users to understand
        /// validation errors and expected fixes.
        /// </summary>
        public const string UiName = "Android XR (Extensions): Anchor";

        /// <summary>
        /// The feature ID string.
        /// </summary>
        public const string FeatureId = "com.google.xr.extensions.anchor";

        /// <summary>
        /// The OpenXR Extension string. Used to check if this extensions is
        /// available or enabled.
        /// To enable runtime confiugration, always enabling persistence extension
        /// when it's available.
        /// </summary>
        public const string ExtensionStrings =
            "XR_ANDROID_trackables " +
            "XR_ANDROID_device_anchor_persistence";

        /// <summary>
        /// The OpenXR extensions for persistence feature. Used to check if this extensions is
        /// available or enabled.
        /// </summary>
        public const string PersistentExtensionString = "XR_ANDROID_device_anchor_persistence";

        /// <summary>
        /// Runtime permission required to enable scene understanding.
        /// </summary>
        public static readonly AndroidXRPermission RequiredPermission =
            AndroidXRPermission.SceneUnderstanding;

        internal static bool? _extensionEnabled = null;

#pragma warning disable CS0414 // Remove unread private members, used at runtime.
        private static bool? _isPersistentExtensionEnabled = null;
#pragma warning restore CS0414 // Remove unread private members, used at runtime.

        private XRAnchorSubsystem _anchorSubsystem = null;

        [SerializeField]
        private bool _usePersistence = false;

        /// <summary>
        /// Gets if the required OpenXR extension is enabled.
        /// When OpenXR runtime is waiting, it returns <c>null</c>. Otherwise, it indicates
        /// whether the XR_ANDROID_trackables extension is available on current device.
        /// </summary>
        public static bool? IsExtensionEnabled => _extensionEnabled;

        /// <summary>
        /// Gets or sets a value indicating whether to use persistence at runtime.
        /// </summary>
        public bool UsePersistence
        {
            get => _usePersistence;
            set
            {
#if UNITY_EDITOR
                _usePersistence = value;
#else
                if (!_isPersistentExtensionEnabled.HasValue || !_isPersistentExtensionEnabled.Value)
                {
                    return;
                }
                else if (XRAnchorApi.SetPersistence(value))
                {
                    _usePersistence = value;
                }
#endif
            }
        }

        /// <summary>
        /// Gets the subsystem instance that to be used in extension methods.
        /// </summary>
        internal static AndroidXRAnchorSubsystem _subsystemInstance { get; private set; }

        /// <inheritdoc/>
        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            if (!base.OnInstanceCreate(xrInstance))
            {
                return false;
            }

            string[] extensions = ExtensionStrings.Split(" ");
            _isPersistentExtensionEnabled = false;
            foreach (string extension in extensions)
            {
                _extensionEnabled = OpenXRRuntime.IsExtensionEnabled(extension);
                if (PersistentExtensionString.Equals(extension) && _extensionEnabled.Value)
                {
                    _isPersistentExtensionEnabled = true;
                }

                if (!_extensionEnabled.Value)
                {
                    return false;
                }
            }

            bool result = XRInstanceManagerApi.Register(ApiXrFeature.Anchor);
            if (_usePersistence)
            {
                // Update UsePersistence property based on runtime result.
                result &= XRAnchorApi.SetPersistence(_usePersistence);
                _usePersistence = result;
            }

            return result;
        }

        /// <inheritdoc/>
        protected override void OnInstanceDestroy(ulong xrInstance)
        {
            XRInstanceManagerApi.Unregister(ApiXrFeature.Anchor);
        }

        /// <inheritdoc/>
        protected override void OnSubsystemCreate()
        {
            var descriptors = new List<XRAnchorSubsystemDescriptor>();
            SubsystemManager.GetSubsystemDescriptors(descriptors);
            if (descriptors.Count < 1)
            {
                Debug.LogError("Failed to find XRAnchorSubsystemDescriptor.");
                return;
            }

            // Create custom XRAnchorSubsystem.
            CreateSubsystem<XRAnchorSubsystemDescriptor, XRAnchorSubsystem>(
                descriptors, AndroidXRAnchorSubsystem._id);

            XRLoader xrLoader = XRSessionFeature.GetXRLoader();
            if (xrLoader != null)
            {
                _anchorSubsystem = xrLoader.GetLoadedSubsystem<XRAnchorSubsystem>();
            }
            else
            {
                Debug.LogError("Failed to find any active loader.");
            }

            if (_anchorSubsystem == null)
            {
                Debug.LogErrorFormat(
                    "Failed to find descriptor '{0}'!",
                    AndroidXRAnchorSubsystem._id);
                return;
            }
            else
            {
                _subsystemInstance = _anchorSubsystem as AndroidXRAnchorSubsystem;
                Debug.LogFormat("Created {0}.", _anchorSubsystem.GetType());
            }
        }

        /// <inheritdoc/>
        protected override void OnSubsystemStart()
        {
            Debug.Log($"{ApiConstants.LogTag}:: Start AndroidXRAnchorSubsystem.");
            if (_anchorSubsystem != null)
            {
                _anchorSubsystem.Start();
            }
        }

        /// <inheritdoc/>
        protected override void OnSubsystemStop()
        {
            Debug.Log($"{ApiConstants.LogTag}:: Stop AndroidXRAnchorSubsystem.");
            if (_anchorSubsystem != null)
            {
                _anchorSubsystem.Stop();
            }
        }

        /// <inheritdoc/>
        protected override void OnSubsystemDestroy()
        {
            Debug.Log($"{ApiConstants.LogTag}:: Destroy AndroidXRAnchorSubsystem.");
            if (_anchorSubsystem != null)
            {
                _subsystemInstance = null;
                _anchorSubsystem.Destroy();
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

            // Attaching anchor functionality requires XRTrackableFeature.
            results.Add(new ValidationRule(this)
            {
                message = string.Format("Attaching anchor will fail when {0} is disabled.",
                   XRTrackableFeature.UiName),
                checkPredicate = () =>
                {
                    var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);
                    if (settings == null)
                    {
                        return false;
                    }

                    bool arPlaneInUse = false;
#if UNITY_OPEN_XR_ANDROID_XR
                    var arPlane = settings.GetFeature<ARPlaneFeature>();
                    arPlaneInUse = arPlane != null && arPlane.enabled;
#endif
                    var feature = settings.GetFeature<XRTrackableFeature>();
                    return (feature != null && feature.enabled) || arPlaneInUse;
                },
                fixIt = () =>
                {
                    var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);
                    if (settings == null)
                    {
                        Debug.LogWarningFormat(
                            "Autofix failed with missing OpenXRSettings on {0} platform.",
                            targetGroup);
                        return;
                    }

                    var feature = settings.GetFeature<XRTrackableFeature>();
                    if (feature == null)
                    {
                        Debug.LogWarningFormat(
                            "Autofix failed with missing {0} on {1} platform.",
                            XRTrackableFeature.UiName, targetGroup);
                        return;
                    }

                    feature.enabled = true;
                },
                error = false
            });

#if UNITY_OPEN_XR_ANDROID_XR_0_2_0
            const string arAnchorUiNanem = "AndroidXR: AR Anchor";
            results.Add(new ValidationRule(this)
            {
                // Use UI names to help users to understand the problem and suggestion.
                message = string.Format(
                    "{0} is incompatible and duplicate with {1}, " +
                    "please choose one of the Anchor subsystem implementation. " +
                    "You can use anchor persistence provided by {0} or use {1} to attach anchors.",
                    XRAnchorFeature.UiName, arAnchorUiNanem),
                checkPredicate = () =>
                {
                    var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);
                    if (settings != null)
                    {
                        var arAnchor = settings.GetFeature<ARAnchorFeature>();
                        return arAnchor == null || !arAnchor.enabled;
                    }
                    else
                    {
                        return true;
                    }
                },
                fixIt = () =>
                {
                    var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);
                    if (settings == null)
                    {
                        Debug.LogWarningFormat(
                            "Autofix failed with missing OpenXRSettings on {0} platform.",
                            targetGroup);
                        return;
                    }

                    var arAnchor = settings.GetFeature<ARAnchorFeature>();
                    if (arAnchor == null)
                    {
                        Debug.LogWarningFormat(
                            "Autofix failed with missing {0} on {1} platform.",
                            arAnchorUiNanem, targetGroup);
                        return;
                    }

                    // Prefer XRAnchorFeature over ARAnchorFeature.
                    arAnchor.enabled = false;
                },
                error = true
            });
#endif
        }
#endif // UNITY_EDITOR
    }
}
