// <copyright file="AndroidXRPermissionUtil.cs" company="Google LLC">
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
    using System.Linq;
    using UnityEngine;
    using UnityEngine.Android;

    /// <summary>
    /// Utility component to help manage runtime permission requests.
    /// </summary>
    public class AndroidXRPermissionUtil : MonoBehaviour
    {
        /// <summary>
        /// A list of <c><see cref="AndroidXRPermissions"/></c> to request at runtime.
        /// </summary>
        public List<AndroidXRPermission> AndroidXRPermissions = new List<AndroidXRPermission>();

        /// <summary>
        /// A list of general Android permissions to request at runtime.
        /// </summary>
        public List<string> GenernalAndroidPermissions = new List<string>();

        /// <summary>
        /// The rationale of requesting Android permissions.
        /// </summary>
        public string PermissionRationale = "Required to enable Android XR feature at runtime.";

        private bool _initialized = false;
        private Dictionary<string, bool> _requiredPermissions = new Dictionary<string, bool>();

        /// <summary>
        /// Checks if all permissions are granted at runtime.
        /// </summary>
        /// <returns>A bool to indicate that all permissions are granted at runtime.</returns>
        public bool AllPermissionGranted()
        {
            return _initialized && !_requiredPermissions.ContainsValue(false);
        }

        /// <summary>
        /// Checks if the given permission is granted at runtime.
        /// </summary>
        /// <param name="permission">The permission to check.</param>
        /// <returns>A bool to indicate that the permission is granted at runtime.</returns>
        public bool IsPerimssionGranted(string permission)
        {
            return _requiredPermissions.ContainsKey(permission) && _requiredPermissions[permission];
        }

        private void PermissionCallbacksPermissionGranted(string permissionName)
        {
            _requiredPermissions[permissionName] = true;
            Debug.Log($"Permission Granted: {permissionName}");
        }

        private void PermissionCallbacksPermissionDenied(string permissionName)
        {
            _requiredPermissions[permissionName] = false;
            Debug.Log($"Permission Denied: {permissionName}");
        }

        private void OnEnable()
        {
            foreach (var permission in AndroidXRPermissions)
            {
                _requiredPermissions[permission.ToPermissionString()] = false;
            }

            foreach (var permissionString in GenernalAndroidPermissions)
            {
                _requiredPermissions[permissionString] = false;
            }

            Debug.LogFormat("Android permissions needed at runtime:\n{0}",
                string.Join("\n", _requiredPermissions.Keys));
            _initialized = true;
            PermissionRequest();
        }

        private void OnDisable()
        {
            _requiredPermissions.Clear();
            _initialized = false;
        }

        private void PermissionRequest()
        {
            var permissions = new List<string>();
            foreach (string permission in _requiredPermissions.Keys.ToList())
            {
                if (!Permission.HasUserAuthorizedPermission(permission))
                {
                    _requiredPermissions[permission] = false;
                    if (Permission.ShouldShowRequestPermissionRationale(permission))
                    {
                        string rationale = string.IsNullOrEmpty(PermissionRationale) ?
                            "Show a message to the user with the rationale." : PermissionRationale;
                        Debug.Log(rationale);
                    }

                    permissions.Add(permission);
                }
                else
                {
                    _requiredPermissions[permission] = true;
                }
            }

            if (permissions.Count == 0)
            {
                return;
            }

            Debug.LogFormat("Android permissions requested at runtime:\n{0}",
                string.Join("\n", permissions));
            var callbacks = new PermissionCallbacks();
            callbacks.PermissionDenied += PermissionCallbacksPermissionDenied;
            callbacks.PermissionGranted += PermissionCallbacksPermissionGranted;
            Permission.RequestUserPermissions(permissions.ToArray(), callbacks);
        }

        private void OnValidate()
        {
            foreach (var permission in GenernalAndroidPermissions)
            {
                string[] strs = permission.Split(".");
                if (strs.Length != 3 || strs[0] != "android" || strs[1] != "permission")
                {
                    Debug.LogError($"Invalid permission string: {permission}");
                }
            }
        }
    }
}
