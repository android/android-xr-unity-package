// <copyright file="ApiXrPosef.cs" company="Google LLC">
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

namespace Google.XR.Extensions.Internal
{
    using System.Runtime.InteropServices;
    using UnityEngine;

    /// <summary>
    /// Had to make this struct because XrPosef and UnityEngine.Pose reverse the order of the
    /// orientation and position.  Need an intermediate step to put them in the right order.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct ApiXrPosef
    {
        internal Quaternion _orientation;
        internal Vector3 _position;

        private static readonly Matrix4x4 _unityWorldToOXRWorld =
            Matrix4x4.Scale(new Vector3(1, 1, -1));

        private static readonly Matrix4x4 _unityWorldToOXRWorldInverse =
                                            _unityWorldToOXRWorld.inverse;

        public ApiXrPosef(Matrix4x4 unityWorld_T_unityLocal)
        {
            Matrix4x4 oxrWorld_T_oxrLocal =
                _unityWorldToOXRWorldInverse * unityWorld_T_unityLocal * _unityWorldToOXRWorld;
            _position = oxrWorld_T_oxrLocal.GetPosition();
            _orientation = oxrWorld_T_oxrLocal.rotation;
        }

        public static ApiXrPosef FromTransform(Transform transform)
        {
            Matrix4x4 unityWorld =
                Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Matrix4x4 oxrWorld = _unityWorldToOXRWorldInverse * unityWorld * _unityWorldToOXRWorld;

            ApiXrPosef pose = new ApiXrPosef();
            pose._position = oxrWorld.GetColumn(3);
            pose._orientation =
                Quaternion.LookRotation(oxrWorld.GetColumn(2), oxrWorld.GetColumn(1));
            return pose;
        }

        public Pose GetUnityPose()
        {
            Matrix4x4 oxrWorld_T_oxrLocal = Matrix4x4.TRS(_position, _orientation, Vector3.one);
            Matrix4x4 unityWorld_T_unityLocal =
                _unityWorldToOXRWorld * oxrWorld_T_oxrLocal * _unityWorldToOXRWorldInverse;

            Vector3 position = unityWorld_T_unityLocal.GetPosition();
            Quaternion rotation = unityWorld_T_unityLocal.rotation;
            return new Pose(position, rotation);
        }

        public override string ToString()
        {
            return string.Format("orientation: {0}, position: {1}", _orientation, _position);
        }
    }
}
