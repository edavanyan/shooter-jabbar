Shader "Unlit/PlayerShader"
{
    Properties
    {
        _ColorA ("Start Color", color) = (1, 1, 1, 1)
        _ColorB ("End Color", color) = (0, 0, 0, 1)

        _GradStart("Gradient Start", Range(0, 1)) = 0.0
        _GradEnd("Gradient End", Range(0, 1)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
// Upgrade NOTE: excluded shader from DX11; has structs without semantics (struct v2f members normals)
#pragma exclude_renderers d3d11
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            #define TAU 6.283185307179586

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normals : NORMAL;
                float3 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normals : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            float4 _ColorA;
            float4 _ColorB;
            float _GradStart;
            float _GradEnd;
            uniform int moving;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                // o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normals = UnityObjectToWorldNormal(v.normals);
                // o.normals = v.normals;
                o.uv = v.uv;
                return o;
            }

            float inverseLerp(fixed a, fixed b, float v)
            {
                return (v-a)/(b-a);
            }

            float4 inverseLerp(fixed4 a, fixed4 b, float v)
            {
                return (v-a)/(b-a);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float xOffset = _Time.z;
                float t = cos((i.uv.x + xOffset / 10) * TAU * 2) * 0.5 + 0.5;
                float u = sin((i.uv.y + xOffset) * TAU * 2) * 0.5 + 0.5;
                float4 tx = inverseLerp(_ColorA, _ColorB, clamp(t, 0, 1));
                float4 ty = inverseLerp(_ColorA, _ColorB, clamp(u, 0, 1));
                return frac((i.uv.x + i.uv.y + xOffset) * 2);
            }
            ENDCG
        }
    }
}

