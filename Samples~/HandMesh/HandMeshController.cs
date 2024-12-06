// <copyright file="HandMeshController.cs" company="Google LLC">
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

namespace Google.XR.Extensions.Samples.HandMesh
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.XR;
    using UnityEngine.XR.OpenXR;
    using UnityEngine.XR.OpenXR.Features;

    /// <summary>
    /// Used to test the hand mesh extension.
    /// </summary>
    [RequireComponent(typeof(AndroidXRPermissionUtil))]
    public class HandMeshController : MonoBehaviour
    {
        /// <summary>
        /// The AndroidXRPermissionUtil component of the game object. Set via the
        /// editor.
        /// </summary>
        public AndroidXRPermissionUtil PermissionUtil;

        /// <summary>
        /// The mesh filter component of the left hand object in the scene.
        /// Set via the editor.
        /// </summary>
        public MeshFilter LeftHand;

        /// <summary>
        /// The mesh filter component of the right hand object in the scene.
        /// Set via the editor.
        /// </summary>
        public MeshFilter RightHand;

        private XRMeshSubsystem _meshSubsystem;

        private void Start()
        {
            OpenXRFeature feature = OpenXRSettings.Instance.GetFeature<XRHandMeshFeature>();
            if (feature == null || feature.enabled == false)
            {
                Debug.LogError("XRHandMesh feature is not enabled.");
                enabled = false;
                return;
            }

            List<XRMeshSubsystem> meshSubsystems = new List<XRMeshSubsystem>();
            SubsystemManager.GetSubsystems(meshSubsystems);
            if (meshSubsystems.Count != 1)
            {
                Debug.LogError("Unexpected number of mesh subsystems."
                    + "Expected 1, got {meshSubsystems.Count}.");
                enabled = false;
                return;
            }

            _meshSubsystem = meshSubsystems[0];
        }

        private void Update()
        {
            if (!PermissionUtil.AllPermissionGranted())
            {
                return;
            }

            if (!_meshSubsystem.running)
            {
                return;
            }

            List<MeshInfo> meshInfos = new List<MeshInfo>();
            if (_meshSubsystem.TryGetMeshInfos(meshInfos))
            {
                if (meshInfos.Count != 2)
                {
                    Debug.LogError("Unexpected number of mesh infos from hand mesh subsystem."
                        + $" Expected 2, got {meshInfos.Count}.");
                }

                if (meshInfos[0].ChangeState == MeshChangeState.Added
                    || meshInfos[0].ChangeState == MeshChangeState.Updated)
                {
                    _meshSubsystem.GenerateMeshAsync(meshInfos[0].MeshId, LeftHand.mesh,
                        null, MeshVertexAttributes.Normals, result => { });
                }

                if (meshInfos[1].ChangeState == MeshChangeState.Added
                    || meshInfos[1].ChangeState == MeshChangeState.Updated)
                {
                    _meshSubsystem.GenerateMeshAsync(meshInfos[1].MeshId, RightHand.mesh,
                        null, MeshVertexAttributes.Normals, result => { });
                }
            }
        }
    }
}
