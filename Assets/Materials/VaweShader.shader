Shader "Unlit/VaweShader"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Gloss("Gloss", Range(0, 1)) = 1.0
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
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD1;
                float3 wPos : TEXCOORD2;
            };

            float4 _Color;
            float _Gloss;

            v2f vert (appdata v)
            {
                v2f o;
                
                float2 uvCenter = v.uv * 2 - 1;
                float center = length(uvCenter);
                float ty = cos((center * 5 - _Time.y / 5) * TAU) * 0.5;
                ty *= 1 - center;
                v.vertex.z = ty * 0.2;
                o.vertex = UnityObjectToClipPos(v.vertex);

                o.normal = v.normal;
                o.uv = v.uv;
                o.wPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            float4 inverseLerp(fixed4 a, fixed4 b, float v)
            {
                return (v-a)/(b-a);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 N = normalize(i.normal);
                float3 L = _WorldSpaceLightPos0.xyz;
                float diffuse = saturate(max(0.15, dot(N, L)));

                float3 halfVec = normalize(_WorldSpaceCameraPos - i.wPos);
                float specular = saturate(dot(halfVec, N));
                float specExp = exp2(_Gloss * 11) + 2;
                
                specular = pow(specular, specExp);
                
                float2 uvCenter = i.uv * 2 - 1;
                float center = length(uvCenter);
                float ty = cos(center * TAU * 15 - _Time.y) * 0.5 - 0.5;
                float tx = cos((i.uv.x + ty - _Time.y) * TAU) * 0.5 - 0.5;
                fixed4 col = _Color;
                return lerp(col, float4(1, 1, 1, 1), tx);// * diffuse + specular;
            }
            ENDCG
        }
    }
}
