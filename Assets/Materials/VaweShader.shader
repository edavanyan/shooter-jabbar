Shader "Unlit/VaweShader"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
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
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            
            #define TAU 6.283185307179586

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normals : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normals : TEXCOORD1;
            };

            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                // float ty = cos((v.uv.y - _Time.z * 0.1) * TAU * 5) * 2.5;
                // float tx = cos((v.uv.x - _Time.z * 0.1) * TAU * 5) * 2.5;
                // v.vertex.y = (tx * ty) * 0.1 ;
                
                float2 uvCenter = v.uv * 2 - 1;
                float center = length(uvCenter);
                float ty = cos(center * TAU * 15 - _Time.z) * 0.5;
                ty *= 1 - center;
                v.vertex.z = ty * 0.2;
                o.vertex = UnityObjectToClipPos(v.vertex);

                o.normals = v.normals;
                o.uv = v.uv;
                return o;
            }

            float4 inverseLerp(fixed4 a, fixed4 b, float v)
            {
                return (v-a)/(b-a);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uvCenter = i.uv * 2 - 1;
                float center = length(uvCenter);
                float ty = cos(center * TAU * 15 - _Time.y) * 0.5 - 0.5;
                float tx = cos((i.uv.x + ty - _Time.y) * TAU) * 0.5 - 0.5;
                fixed4 col = _Color;
                return lerp(col, float4(1, 1, 1, 1), tx);
            }
            ENDCG
        }
    }
}
