// <copyright file="Controller_Builtin.shader" company="Google LLC">
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
Shader "SamsungUX/Controller_Gltf_Builtin"
{
    Properties
    {
        _BaseColor ("Base Color", 2D) = "white" {}
        _Normal ("Normal", 2D) = "bump" {}
        _AoRoughnessMetallic ("AoRoughnessMetallic", 2D) = "White" {}
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _BaseColor;
        sampler2D _Normal;
        sampler2D _AoRoughnessMetallic;

        struct Input
        {
            float2 uv_BaseColor;
        };

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            o.Albedo = tex2D(_BaseColor, IN.uv_BaseColor).rgb;
            o.Normal = UnpackNormal(tex2D(_Normal, IN.uv_BaseColor));

            float3 arm = tex2D(_AoRoughnessMetallic, IN.uv_BaseColor);
            o.Metallic = arm.b;
            o.Smoothness = 1 - arm.g;
            o.Occlusion = arm.r;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
