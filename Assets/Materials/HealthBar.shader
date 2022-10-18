Shader "Unlit/HealthBar"
{
    Properties
    {
        _Health("Health", range(0, 1)) = 1
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

            float _Health;
            uniform float health = 0.2;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float inverseLerp(float a, float b, float v)
            {
                return (v - a) / (b - a);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float t = saturate(inverseLerp(0.2, 0.8, health));
                
                float4 bg = float4(0.2, 0.2, 0.2, 1);
                float4 healthColor = lerp(float4(1, 0, 0, 1), float4(0, 1, 0, 1), t);
                float col = health >= i.uv.x;

                // clip(col - 0.5);
                
                return lerp(bg, healthColor, col);
            }
            ENDCG
        }
    }
}
