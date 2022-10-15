Shader "Unlit/SpawnVFX"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags 
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }
        LOD 100

        Pass
        {
            CULL Off
            ZWRITE Off
            BLEND ONE ONE
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            #define TAU 6.283185307179586
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };
            
            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.normal = v.normal;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float xOffset = cos(i.uv.x * TAU * 8) * 0.01;
                float t = cos(((i.uv.y + xOffset) + _Time.z / 2) * TAU * 2) * 0.5 + 0.5;
                t *= 1 - i.uv.y; 
                return _Color * t * (abs(i.normal.y) < 0.999);
            }
            ENDCG
        }
    }
}
