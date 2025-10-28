Shader "SamsungUX/Controller_Builtin"
{
    Properties
    {        
        _BaseColor ("Base Color", 2D) = "white" {}
        _Normal ("Normal", 2D) = "bump" {}
        _Roughness ("Roughness", 2D) = "White" {}
        _Metallic ("Metallic", 2D) = "Black" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _BaseColor;
        sampler2D _Normal;
        sampler2D _Roughness;
        sampler2D _Metallic;

        struct Input
        {
            float2 uv_BaseColor; 
        };        
        
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            o.Albedo = tex2D (_BaseColor, IN.uv_BaseColor).rgb;
            o.Normal = UnpackNormal(tex2D(_Normal, IN.uv_BaseColor));
            o.Metallic = tex2D (_Metallic, IN.uv_BaseColor);
            o.Smoothness = 1 - tex2D(_Roughness, IN.uv_BaseColor);            
        }
        ENDCG
    }
    FallBack "Diffuse"
}
