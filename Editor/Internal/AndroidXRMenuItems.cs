// <copyright file="AndroidXRMenuItems.cs" company="Google LLC">
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

namespace Google.XR.Extensions.Editor.Internal
{
    using System;
#if XR_COMPOSITION_LAYERS
    using Unity.XR.CompositionLayers;
    using Unity.XR.CompositionLayers.Services;
#endif
    using UnityEditor;
    using UnityEngine;

    internal class AndroidXRMenuItems
    {
#if XR_COMPOSITION_LAYERS
        [MenuItem("GameObject/XR/Composition Layers/Passthrough Layer", false, 20)]
        static void CreatePassthroughLayer()
        {
            // Default to a shpere.
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            SceneView lastSceneView = SceneView.lastActiveSceneView;
            Vector3 position = lastSceneView == null ? Vector3.zero : lastSceneView.pivot;
            sphere.transform.position = position;
            CreatePassthroughLayerComponents(sphere);

            var descriptor = CompositionLayerUtils.GetLayerDescriptor(
                typeof(XRPassthroughLayerData));
            sphere.name += $" {descriptor.Name}";
            Undo.RegisterCreatedObjectUndo(sphere, $"Created {sphere.name}");
            Selection.activeGameObject = sphere;
        }

        [MenuItem("Component/XR/Composition Layers/Passthrough Layer", false, 201)]
        static void CreatePassthroughLayerComponent()
        {
            GameObject gameObject = Selection.activeGameObject;
            var descriptor = CompositionLayerUtils.GetLayerDescriptor(
                typeof(XRPassthroughLayerData));
            string name = descriptor.Name;
            Undo.RecordObject(gameObject, $"Add {name} Composition Layer Component");
            CreatePassthroughLayerComponents(gameObject);
        }

        // Validate the menu item defined by the function above.
        // i.e. CreatePassthroughLayerComponent.
        [MenuItem("Component/XR/Composition Layers/Passthrough Layer", true)]
        static bool ValidateCreatePassthroughLayerComponent()
        {
            var gameObject = Selection.activeGameObject;
            if (gameObject == null)
            {
                return false;
            }

            var layer = gameObject.GetComponent(typeof(CompositionLayer));
            if (layer != null)
            {
                return false;
            }

            return true;
        }

        static CompositionLayer CreatePassthroughLayerComponents(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return null;
            }

            Type dataType = typeof(XRPassthroughLayerData);
            gameObject.SetActive(false);
            var descriptor = CompositionLayerUtils.GetLayerDescriptor(dataType);
            CompositionLayer layer = gameObject.AddComponent<CompositionLayer>();
            if (layer == null)
            {
                return null;
            }

            var layerData = CompositionLayerUtils.CreateLayerData(dataType);
            layer.ChangeLayerDataType(layerData);
            foreach (var extension in descriptor.SuggestedExtensions)
            {
                if (extension.IsSubclassOf(typeof(Component)) &&
                    gameObject.GetComponent(extension) == null)
                {
                    gameObject.AddComponent(extension);
                }
            }

            gameObject.SetActive(true);
            return layer;
        }
#endif // XR_COMPOSITION_LAYERS
    }
}
