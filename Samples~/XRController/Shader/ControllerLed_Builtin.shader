// <copyright file="ControllerLed_Builtin.shader" company="Google LLC">
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
Shader "SamsungUX/ControllerLed_Builtin"
{
    Properties
    {
        _Mask ("Mask", 2D) = "white" {}
        _LedColor ("LED Color", Color) = (0, 0, 0, 1)
        _LedIntensity ("LED Intensity", Range(0, 2)) = 0
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent" "RenderType"="Transparent" "IgnoreProjector" = "True"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _Mask;
            fixed3 _Mask_ST;
            fixed3 _LedColor;
            fixed _LedIntensity;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float mask = tex2D(_Mask, i.uv).a;
                float3 color = mask * _LedColor * _LedIntensity;
                float alpha = mask * min(1, _LedIntensity);
                return fixed4(color, alpha);
            }
            ENDCG
        }
    }
}
