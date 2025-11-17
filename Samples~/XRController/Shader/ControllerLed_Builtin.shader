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
