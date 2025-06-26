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
    using System.Collections;
    using System.Collections.Generic;
    using Unity.XR.CoreUtils;
    using UnityEngine;
    using UnityEngine.XR;
    using UnityEngine.XR.OpenXR;
    using UnityEngine.XR.OpenXR.Features;

    /// <summary>
    /// Used to test the hand mesh extension.
    /// </summary>
    [RequireComponent(typeof(AndroidXRPermissionUtil))]
    [RequireComponent(typeof(XROrigin))]
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
        private HandMeshData[] _hands;

        private void Start()
        {
            OpenXRFeature feature = OpenXRSettings.Instance.GetFeature<XRHandMeshFeature>();
            if (feature == null || feature.enabled == false)
            {
                Debug.LogError("XRHandMesh feature is not enabled.");
                enabled = false;
                return;
            }

            _hands = new HandMeshData[2]
            {
                new HandMeshData { Filter = LeftHand },
                new HandMeshData { Filter = RightHand }
            };

            foreach (HandMeshData data in _hands)
            {
                // Generated meshes should be children of XROrigin
                data.Filter.transform.SetParent(transform);
            }

            StartCoroutine(InitSubsystem());
        }

        private IEnumerator InitSubsystem()
        {
            yield return new WaitUntil(PermissionUtil.AllPermissionGranted);

            List<XRMeshSubsystem> meshSubsystems = new List<XRMeshSubsystem>();
            SubsystemManager.GetSubsystems(meshSubsystems);
            if (meshSubsystems.Count != 1)
            {
                Debug.LogError("Unexpected number of mesh subsystems."
                    + "Expected 1, got {meshSubsystems.Count}.");
                enabled = false;
                yield break;
            }

            _meshSubsystem = meshSubsystems[0];
            _meshSubsystem.Stop();
            _meshSubsystem.Start();
        }

        private void Update()
        {
            if (!PermissionUtil.AllPermissionGranted())
            {
                return;
            }

            if (_meshSubsystem == null || !_meshSubsystem.running)
            {
                return;
            }

            List<MeshInfo> meshInfos = new List<MeshInfo>();
            if (_meshSubsystem.TryGetMeshInfos(meshInfos))
            {
                int index = 0;
                foreach (MeshInfo info in meshInfos)
                {
                    if (_meshSubsystem.IsSceneMeshId(info.MeshId))
                    {
                        continue;
                    }
                    if (info.ChangeState == MeshChangeState.Added
                        || info.ChangeState == MeshChangeState.Updated)
                    {
                        _hands[index].Id = info.MeshId;
                        Mesh hand = _hands[index].Filter.mesh;
                        _meshSubsystem.GenerateMeshAsync(info.MeshId, hand, null,
                            MeshVertexAttributes.Normals, OnMeshGenerated,
                            MeshGenerationOptions.ConsumeTransform);
                        index++;
                    }
                }
            }
        }

        private void OnMeshGenerated(MeshGenerationResult result)
        {
            if (result.Status == MeshGenerationStatus.Success)
            {
                foreach (HandMeshData data in _hands)
                {
                    if (result.MeshId == data.Id)
                    {
                        Transform handTransform = data.Filter.transform;

                        handTransform.localPosition = result.Position;
                        handTransform.localRotation = result.Rotation;
                        handTransform.localScale = result.Scale;

                        break;
                    }
                }
            }
        }

        private struct HandMeshData
        {
            public MeshId Id;
            public MeshFilter Filter;
        }
    }
}
