Shader "Vanish/Sprite-Blur"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _BlurIntensity ("Blur Intensity", Range(0, 10)) = 1.0
        _BlurMode ("Blur Mode (0=Gaussian,1=Kawase)", Range(0, 1.0)) = 0.0
        _Color ("Tint", Color) = (1,1,1,1)

        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Cull Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _MainTex_TexelSize;
            float4 _Color;
            float _BlurIntensity;
            float _BlurMode;

            Varyings Vert(Attributes input)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                o.uv = input.uv;
                o.color = input.color * _Color;
                return o;
            }

            // ===== Gaussian Blur =====
            float4 GaussianBlur(float2 uv)
            {
                float2 texel = _MainTex_TexelSize.xy;
                float blur = _BlurIntensity;

                float2 offsets[9] = {
                    float2(-1, -1), float2(0, -1), float2(1, -1),
                    float2(-1,  0), float2(0,  0), float2(1,  0),
                    float2(-1,  1), float2(0,  1), float2(1,  1)
                };

                float weights[9] = {
                    0.075, 0.125, 0.075,
                    0.125, 0.200, 0.125,
                    0.075, 0.125, 0.075
                };

                float4 col = 0;

                for(int i = 0; i < 9; i++)
                {
                    float2 sampleUV = uv + offsets[i] * texel * blur;
                    col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, sampleUV) * weights[i];
                }

                return col;
            }

            // ===== Kawase Blur =====
            float4 KawaseBlur(float2 uv)
            {
                float2 texel = _MainTex_TexelSize.xy;
                float offset = _BlurIntensity;

                float2 o = texel * offset;

                float4 col = 0;

                // 4-tap Kawase
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(-o.x, -o.y));
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2( o.x, -o.y));
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(-o.x,  o.y));
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2( o.x,  o.y));

                col *= 0.25;

                return col;
            }

            float4 Frag(Varyings i) : SV_Target
            {
                float4 col;

                if (_BlurMode < 0.5)
                {
                    col = GaussianBlur(i.uv);
                }
                else
                {
                    col = KawaseBlur(i.uv);
                }

                col *= i.color;
                col.rgb *= col.a; // premultiply

                return col;
            }

            ENDHLSL
        }
    }
}