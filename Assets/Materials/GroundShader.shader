Shader "Unlit/GroundShader"
{
    Properties
    {
        _Mask("Mask Texture", 2D) = "white" {}
        _ColorA ("Start Color", color) = (1, 1, 1, 1)
        _ColorB ("End Color", color) = (0, 0, 0, 1)

        _GradStart("Gradient Start", Range(0, 1)) = 0.0
        _GradEnd("Gradient End", Range(0, 1)) = 1.0
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

            sampler2D _Mask;
            float4 _Mask_ST;
            
            float4 _ColorA;
            float4 _ColorB;
            float _GradStart;
            float _GradEnd;

            v2f vert (appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normals = v.normals;
                o.uv = TRANSFORM_TEX(v.uv, _Mask);
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
                float4 mask = tex2D(_Mask, i.uv);
                float3 col = float3(250.0 / 255.0, 208.0/255.0, 126.0/255.0);
                float t = cos((i.uv.x + _Time.y / 40) * TAU * 100) * 0.5 + 0.5;
                float u = cos((i.uv.y + _Time.x / 10) * TAU * 100) * 0.5 + 0.5;
                float tiv = sin(_Time.z * TAU) * 0.5 + 0.5;
                if (mask.x < 1)
                {
                    mask.x += tiv / 10;
                    mask.x = clamp(mask.x, 0, 1);
                }
                return float4(col * (clamp(t + u, 0.96, 1)), mask.x);
            }
            ENDCG
        }
    }
}

