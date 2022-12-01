Shader "Unlit/GroundShader"
{
    Properties
    {
        _Mask("Mask Texture", 2D) = "white" {}
        
        _Gloss("Gloss", Range(0, 1)) = 1.0
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
            BLEND SrcAlpha OneMinusSrcAlpha
            
            CGPROGRAM
// Upgrade NOTE: excluded shader from DX11; has structs without semantics (struct v2f members normals)
#pragma exclude_renderers d3d11
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"

            #define TAU 6.283185307179586

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float3 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD0;
                float2 uv : TEXCOORD1;
                float3 wPos : TEXCOORD2;
            };

            sampler2D _Mask;
            float4 _Mask_ST;
            

            float _Gloss;

            v2f vert (appdata v)
            {
                v2f o;

                o.uv = TRANSFORM_TEX(v.uv, _Mask);
                float4 mask = tex2Dlod(_Mask, float4(o.uv, 0, 0));
                v.vertex.y -= 1 - mask.x;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = v.normal;
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
                float diffuse = saturate(max(0.15, dot(N, L)));

                float3 halfVec = normalize(_WorldSpaceCameraPos - i.wPos);
                float specular = saturate(dot(halfVec, N));
                float specExp = exp2(_Gloss * 12) + 10;
                
                specular = pow(specular, specExp);
                specular = clamp(specular, 0, 0.1);
                
                float4 mask = tex2D(_Mask, i.uv);
                
                float3 col = float3(250.0 / 255.0, 208.0/255.0, 126.0/255.0);
                float t = cos((i.uv.x + _Time.y / 40) * TAU * 100) * 0.5 + 0.5;
                float u = cos((i.uv.y + _Time.x / 10) * TAU * 100) * 0.5 + 0.5;
                float tiv = sin(_Time.z * TAU) * 0.5 + 0.5;
                if (mask.x < 1 && mask.x > 0.5)
                {
                    mask.x += tiv / 10;
                    mask.x = clamp(mask.x, 0, 1);
                }
                else if (mask.x < 0.08)
                {
                    mask.x = 0;
                }
                float fresnel = 1 - dot(halfVec, N);
                return float4(col * (clamp(t + u, 0.96, 1)) * diffuse + specular, mask.x);
            }
            ENDCG
        }
    }
    Fallback "Diffuse"
}

