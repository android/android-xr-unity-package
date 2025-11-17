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
