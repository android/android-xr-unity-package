// <copyright file="GeospatialAnchorVisualizer.cs" company="Google LLC">
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

namespace Google.XR.Extensions.Samples.Geospatial
{
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// Visualizes a geospatial anchor with a line pointing to the origin.
    /// The color of the line and the anchor indicates the surface type of the anchor.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    [RequireComponent(typeof(MeshRenderer))]
    public class GeospatialAnchorVisualizer : MonoBehaviour
    {
        private static Material _defaultMaterial = null;
        private static Material _terrainMaterial = null;
        private static Material _rooftopMaterial = null;

        private LineRenderer _lineRenderer = null;

        private void OnEnable()
        {
            if (_lineRenderer == null)
            {
                StartCoroutine(Init());
            }
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private void Update()
        {
            if (_lineRenderer == null)
            {
                return;
            }

            _lineRenderer.SetPosition(0, transform.position);
            _lineRenderer.SetPosition(1, Vector3.zero);
        }

        private Material CreateMaterial(Color color)
        {
            Material material = new Material(Shader.Find("Sprites/Default"));
            material.color = color;
            return material;
        }

        private Material GetSharedMaterial(XRGeospatialSurfaceType surfaceType)
        {
            switch (surfaceType)
            {
                case XRGeospatialSurfaceType.Terrain:
                    if (_terrainMaterial == null)
                    {
                        _terrainMaterial = CreateMaterial(Color.green);
                    }

                    return _terrainMaterial;
                case XRGeospatialSurfaceType.Rooftop:
                    if (_rooftopMaterial == null)
                    {
                        _rooftopMaterial = CreateMaterial(Color.cyan);
                    }

                    return _rooftopMaterial;
                case XRGeospatialSurfaceType.Default:
                default:
                    if (_defaultMaterial == null)
                    {
                        _defaultMaterial = CreateMaterial(Color.orangeRed);
                    }

                    return _defaultMaterial;
            }
        }

        private IEnumerator Init()
        {
            ARShareableAnchor anchor = null;
            yield return new WaitUntil(() => (anchor = GetComponent<ARShareableAnchor>()) != null);
            yield return new WaitUntil(() => anchor.host == XRShareableAnchorHosts.Geospatial);

            Material sharedMat = GetSharedMaterial(anchor.surfaceType);

            GetComponent<MeshRenderer>().sharedMaterial = sharedMat;

            _lineRenderer = GetComponent<LineRenderer>();
            _lineRenderer.sharedMaterial = sharedMat;
            _lineRenderer.positionCount = 2;

            _lineRenderer.startWidth = 0.05f;
            _lineRenderer.endWidth = 0.05f;
        }
    }
}
