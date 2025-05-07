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
        private MeshFilter[] _hands;

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
            _hands = new MeshFilter[2] { LeftHand, RightHand };
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
                int index = 0;
                foreach (MeshInfo info in meshInfos)
                {
                    if (XRMeshSubsystemExtension.IsSceneMeshId(info.MeshId))
                    {
                        continue;
                    }
                    if (info.ChangeState == MeshChangeState.Added
                        || info.ChangeState == MeshChangeState.Updated)
                    {
                        Mesh hand = _hands[index].mesh;
                        _meshSubsystem.GenerateMeshAsync(info.MeshId, hand,
                            null, MeshVertexAttributes.Normals, result => { });
                        index++;
                    }
                }
            }
        }
    }
}
