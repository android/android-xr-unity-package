Shader "Google XR Extensions/CubemapProjector"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
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

            samplerCUBE _cubemap;
            float4x4 _worldToCubemap;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                int sideIndex = int(i.uv.y * 6.0);
                float2 uv = float2(i.uv.x, frac(i.uv.y * 6.0)) * 2.0 - 1.0;

                float3 worldDir;
                if (sideIndex == 0)      { worldDir = float3(1.0, -uv.y, -uv.x);  }
                else if (sideIndex == 1) { worldDir = float3(-1.0, -uv.y, uv.x);  }
                else if (sideIndex == 2) { worldDir = float3(uv.x, 1.0, uv.y);    }
                else if (sideIndex == 3) { worldDir = float3(uv.x, -1.0, -uv.y);  }
                else if (sideIndex == 4) { worldDir = float3(uv.x, -uv.y, 1.0);   }
                else                     { worldDir = float3(-uv.x, -uv.y, -1.0); }

                worldDir = normalize(worldDir);

                float3 localDir = mul((float3x3)_worldToCubemap, worldDir);

                // Need to unmirror the z-mirrored cubemap
                localDir.z = -localDir.z;

                float4 col = texCUBE(_cubemap, localDir);

                return col;
            }
            ENDCG
        }
    }
}
