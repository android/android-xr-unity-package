// <copyright file="SceneMeshingController.cs" company="Google LLC">
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

namespace Google.XR.Extensions.Samples.SceneMeshing
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.XR;
    using UnityEngine.XR.OpenXR;
    using UnityEngine.XR.OpenXR.Features;

    /// <summary>
    /// Used to test the scene meshing extension.
    /// </summary>
    [RequireComponent(typeof(AndroidXRPermissionUtil))]
    public class SceneMeshingController : MonoBehaviour
    {
        /// <summary>
        /// The AndroidXRPermissionUtil component of the game object. Set via the
        /// editor.
        /// </summary>
        public AndroidXRPermissionUtil PermissionUtil;

        /// <summary>
        /// A Game Object with an empty mesh which is instantiated to be used as a
        /// template for a scene mesh.
        /// </summary>
        public GameObject SceneMeshPrefab;

        /// <summary>
        /// The XROrigin (origin of the XR Session).
        /// </summary>
        public Transform XROrigin;

        private Dictionary<MeshId, MeshFilter> _sceneMeshes;
        private uint _generatingCount = 0;
        private XRMeshSubsystem _meshSubsystem;

        private void Start()
        {
            OpenXRFeature feature = OpenXRSettings.Instance.GetFeature<XRSceneMeshingFeature>();
            if (feature == null || feature.enabled == false)
            {
                Debug.LogError("XRSceneMeshing feature is not enabled.");
                enabled = false;
                return;
            }

            List<XRMeshSubsystem> meshSubsystems = new List<XRMeshSubsystem>();
            SubsystemManager.GetSubsystems(meshSubsystems);
            if (meshSubsystems.Count != 1)
            {
                Debug.LogError("Unexpected number of mesh subsystems."
                    + "Expected 1, got " + meshSubsystems.Count);
                enabled = false;
                return;
            }

            _meshSubsystem = meshSubsystems[0];
            _sceneMeshes = new Dictionary<MeshId, MeshFilter>();
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

            if (_generatingCount > 0)
            {
                // Wait till all meshes queued for generation are generated
                return;
            }

            List<MeshInfo> meshInfos = new List<MeshInfo>();
            if (_meshSubsystem.TryGetMeshInfos(meshInfos))
            {
                foreach (MeshInfo info in meshInfos)
                {
                    if (!XRMeshSubsystemExtension.IsSceneMeshId(info.MeshId))
                    {
                        //// This might be a hand mesh id
                        continue;
                    }

                    if (info.ChangeState == MeshChangeState.Added
                        || info.ChangeState == MeshChangeState.Updated)
                    {
                        MeshFilter mf;
                        if (!_sceneMeshes.TryGetValue(info.MeshId, out mf))
                        {
                            mf = Instantiate(SceneMeshPrefab).GetComponent<MeshFilter>();
                            mf.gameObject.name = "SceneMesh" + info.MeshId;
                            mf.mesh = new Mesh();
                            _sceneMeshes.Add(info.MeshId, mf);
                        }

                        _meshSubsystem.GenerateMeshAsync(
                            meshId: info.MeshId,
                            mesh: mf.mesh,
                            meshCollider: null,
                            attributes: MeshVertexAttributes.Normals,
                            onMeshGenerationComplete: OnMeshGenerated,
                            //// This option is important to ensure triangle faces are correct
                            options: MeshGenerationOptions.ConsumeTransform);

                        _generatingCount++;
                    }
                }
            }
        }

        void DecrementGeneratingCount()
        {
            _generatingCount--;
        }

        private void OnMeshGenerated(MeshGenerationResult result)
        {
            //// Give enough time for the mesh to be displayed
            Invoke("DecrementGeneratingCount", 1.0f);
            if (_sceneMeshes.TryGetValue(result.MeshId, out MeshFilter mf))
            {
                mf.transform.localPosition = result.Position;
                mf.transform.localRotation = result.Rotation;
                mf.transform.localScale = result.Scale;
            }
        }
    }
}
