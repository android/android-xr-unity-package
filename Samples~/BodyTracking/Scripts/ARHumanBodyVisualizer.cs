// <copyright file="ARHumanBodyVisualizer.cs" company="Google LLC">
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

namespace Google.XR.Extensions.Samples.BodyTracking
{
    using UnityEngine;
    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.ARSubsystems;

    /// <summary>
    /// Body visualizer to render hand joints.
    /// </summary>
    [RequireComponent(typeof(ARHumanBody))]
    public class ARHumanBodyVisualizer : MonoBehaviour
    {
        /// <summary>
        /// The prefab used to instantiate body joints. If it contains <see cref="LineRenderer"/>
        /// at root, the line renderer will be used to draw bones. All <see cref="MeshRenderer"/>
        /// should be in children objects for toggling visibility.
        /// </summary>
        public GameObject JointPrefab;

        /// <summary>
        /// The width of bones used by <see cref="LineRenderer"/> from <see cref="JointPrefab"/>.
        /// </summary>
        [Range(0.01f, 0.2f)]
        public float BoneWidth = 0.05f;

        /// <summary>
        /// The color of bones used by <see cref="LineRenderer"/> from <see cref="JointPrefab"/>.
        /// </summary>
        public Color BoneColor = Color.gray;

        /// <summary>
        /// The offset of the root position.
        /// </summary>
        public Vector3 RootOffset = Vector3.zero;

        /// <summary>
        /// Whether to draw root mesh from body prefab.
        /// </summary>
        public bool DrawMesh = true;

        /// <summary>
        /// Whether to draw joints from <see cref="JointPrefab"/>.
        /// </summary>
        public bool DrawJoints = true;

        private GameObject[] _jointGOs =
            new GameObject[XRAvatarSkeletonJointIDUtility.JointCount()];

        private LineRenderer[] _bones =
            new LineRenderer[XRAvatarSkeletonJointIDUtility.JointCount()];

        private ARHumanBody _body;
        private MeshRenderer _rootMesh;

        /// <summary>
        /// Sets bone color.
        /// </summary>
        /// <param name="color">The desired color of bones.</param>
        /// <param name="force">Whether to force color update.</param>
        public void UpdateBoneColor(Color color, bool force = false)
        {
            if (!force && BoneColor == color)
            {
                return;
            }

            BoneColor = color;
            for (int index = 0; index < _bones.Length; index++)
            {
                if (_bones[index])
                {
                    _bones[index].startColor = color;
                    _bones[index].endColor = color;
                }
            }
        }

        private void Awake()
        {
            _body = GetComponent<ARHumanBody>();
            _rootMesh = GetComponent<MeshRenderer>();
            if (JointPrefab == null)
            {
                return;
            }

            // Setup joints and bones.
            for (int index = 0; index < _jointGOs.Length; index++)
            {
                // Use body prefab as joint root.
                _jointGOs[index] = Instantiate(JointPrefab, transform);
                _jointGOs[index].transform.localPosition = Vector3.zero;
                _jointGOs[index].transform.localRotation = Quaternion.identity;
                _jointGOs[index].name = XRAvatarSkeletonJointIDUtility.FromIndex(index).ToString();
                _bones[index] = _jointGOs[index].GetComponent<LineRenderer>();
                if (_bones[index])
                {
                    _bones[index].startWidth = BoneWidth;
                    _bones[index].endWidth = BoneWidth;
                    _bones[index].startColor = BoneColor;
                    _bones[index].endColor = BoneColor;
                    _bones[index].useWorldSpace = false;
                    _bones[index].positionCount = 2;
                    _bones[index].SetPosition(0, Vector3.zero);
                    _bones[index].SetPosition(1, Vector3.zero);
                }
            }
        }

        private void OnEnable()
        {
            UpdateVisibility();
        }

        private void OnDisable()
        {
            UpdateVisibility();
        }

        private void Update()
        {
            if (_body == null)
            {
                return;
            }

            // Update transform.
            transform.SetPositionAndRotation(_body.pose.position + RootOffset, _body.pose.rotation);
            for (int index = 0; index < _jointGOs.Length; index++)
            {
                if (!_jointGOs[index])
                {
                    continue;
                }

                if (!DrawJoints || index >= _body.joints.Length || !_body.joints[index].tracked)
                {
                    _jointGOs[index].SetActive(false);
                    continue;
                }

                // Use anchor pose which is returned in world space.
                _jointGOs[index].transform.SetPositionAndRotation(
                    _body.joints[index].anchorPose.position + RootOffset,
                    _body.joints[index].anchorPose.rotation);
                _jointGOs[index].SetActive(true);
            }

            for (int index = 0; index < _bones.Length; index++)
            {
                if (!_bones[index])
                {
                    continue;
                }

                if (!DrawJoints || index >= _body.joints.Length || !_body.joints[index].tracked)
                {
                    _bones[index].enabled = false;
                    continue;
                }

                _bones[index].SetPosition(0, Vector3.zero);
                if (_body.joints[index].parentIndex >= 0 &&
                    _body.joints[index].parentIndex < XRAvatarSkeletonJointIDUtility.JointCount())
                {
                    Pose pose = _body.joints[index].anchorPose;
                    Pose parent = _body.joints[_body.joints[index].parentIndex].anchorPose;
                    _bones[index].SetPosition(1,
                        Quaternion.Inverse(pose.rotation) * (parent.position - pose.position));
                }
                else
                {
                    _bones[index].SetPosition(1, Vector3.zero);
                }
            }
        }

        private void UpdateVisibility()
        {
            bool visible = enabled && _body.trackingState >= TrackingState.Limited;

            if (_rootMesh)
            {
                _rootMesh.enabled = DrawMesh && visible;
            }

            for (int index = 0; index < XRAvatarSkeletonJointIDUtility.JointCount(); index++)
            {
                if (_jointGOs[index])
                {
                    _jointGOs[index].SetActive(DrawJoints && visible);
                }

                if (_bones[index])
                {
                    _bones[index].enabled = DrawJoints && visible;
                }
            }
        }
    }
}
