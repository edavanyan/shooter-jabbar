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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                float ty = cos((v.uv.y - _Time.z / 10) * TAU * 2);
                float tx = cos((v.uv.x - _Time.z / 10) * TAU * 2);
                v.vertex.y = (tx * ty) ;
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float4 inverseLerp(fixed4 a, fixed4 b, float v)
            {
                return (v-a)/(b-a);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float ty = cos((i.uv.y) * TAU * _Time.x) * 0.5 + 0.5;
                float tx = cos((i.uv.x) * TAU + _Time.z) * 0.5 + 0.5;
                fixed4 col = _Color;
                return lerp(col, float4(1, 1, 1, 1), tx * ty * _CosTime.z);
            }
            ENDCG
        }
    }
}
