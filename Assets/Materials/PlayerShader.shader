Shader "Unlit/PlayerShader"
{
    Properties
    {
        _ColorA ("Start Color", color) = (1, 1, 1, 1)
        _ColorB ("End Color", color) = (0, 0, 0, 1)

        _GradStart("Gradient Start", Range(0, 1)) = 0.0
        _GradEnd("Gradient End", Range(0, 1)) = 1.0
        _Gloss("Gloss", Range(0, 1)) = 1.0
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
            #include "AutoLight.cginc"
            #include "Lighting.cginc"

            #define TAU 6.283185307179586

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float3 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD0;
                float2 uv : TEXCOORD1;
                float3 wPos : TEXCOORD2;
            };

            float4 _ColorA;
            float4 _ColorB;
            float _GradStart;
            float _GradEnd;
            float _Gloss;
            uniform int damaged;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                // o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                // o.normals = v.normals;
                o.uv = v.uv;
                o.wPos = mul(unity_ObjectToWorld, v.vertex);
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
                float3 N = normalize(i.normal);
                float3 L = _WorldSpaceLightPos0.xyz;
                float diffuse = saturate(max(0.3, dot(N, L)));
                float3 diffuseLight = diffuse * _LightColor0.xyz;

                float3 halfVec = normalize(_WorldSpaceCameraPos - i.wPos);
                float specular = saturate(dot(halfVec, N));
                float specExp = exp2(_Gloss * 11) + 2;
                specular = pow(specular, specExp);
                // specular = clamp(specular, specular, 0.1);
                
                
                float xOffset = _Time.z;
                float highlight = (cos(_Time.z * 2 * TAU) + 0.5) * damaged;
                _ColorA += float4(highlight, -highlight.xx, 0);
                float4 t = lerp(_ColorA, _ColorB, frac((i.uv.x + i.uv.y + xOffset) * 2));

                
                float4 fresnel = step(0.1, (1 - dot(halfVec, N))) * _ColorB;

                
                return float4(_ColorA.xyz * diffuseLight + specular * _LightColor0 + fresnel.xyz, 1);
            }
            ENDCG
        }
    }
}

