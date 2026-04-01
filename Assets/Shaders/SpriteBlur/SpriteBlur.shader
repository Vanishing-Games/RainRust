Shader "Vanish/Sprite-Blur"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [Toggle(_NORMALMAP)] _EnableNormalMap("Enable Normal Map", Float) = 0.0
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _BlurIntensity ("Blur Intensity", Range(0, 10)) = 1.0
        _BlurMode ("Blur Mode (0=Gaussian,1=Kawase)", Range(0, 1.0)) = 0.0
        _Color ("Tint", Color) = (1,1,1,1)

        [Toggle(_USE_URP_RENDERING)] _UseURPRendering("Use URP Rendering", Float) = 1.0

        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        HLSLINCLUDE
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
            half2 lightingUV  : TEXCOORD1;
        };

        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_NormalMap);
        SAMPLER(sampler_NormalMap);

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
            o.lightingUV = ComputeScreenPos(o.positionCS).xy / o.positionCS.w;
            return o;
        }

        // ===== Gaussian Blur =====
        float4 GaussianBlur(TEXTURE2D_PARAM(tex, samp), float2 uv)
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
                col += SAMPLE_TEXTURE2D(tex, samp, sampleUV) * weights[i];
            }

            return col;
        }

        // ===== Kawase Blur =====
        float4 KawaseBlur(TEXTURE2D_PARAM(tex, samp), float2 uv)
        {
            float2 texel = _MainTex_TexelSize.xy;
            float offset = _BlurIntensity;

            float2 o = texel * offset;

            float4 col = 0;

            // 4-tap Kawase
            col += SAMPLE_TEXTURE2D(tex, samp, uv + float2(-o.x, -o.y));
            col += SAMPLE_TEXTURE2D(tex, samp, uv + float2( o.x, -o.y));
            col += SAMPLE_TEXTURE2D(tex, samp, uv + float2(-o.x,  o.y));
            col += SAMPLE_TEXTURE2D(tex, samp, uv + float2( o.x,  o.y));

            col *= 0.25;

            return col;
        }

        float4 FragCommon(TEXTURE2D_PARAM(tex, samp), float2 uv)
        {
            if (_BlurMode < 0.5)
            {
                return GaussianBlur(TEXTURE2D_ARGS(tex, samp), uv);
            }
            else
            {
                return KawaseBlur(TEXTURE2D_ARGS(tex, samp), uv);
            }
        }
        ENDHLSL

        Cull Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        // Pass 1: URP 2D 渲染
        Pass
        {
            Name "Universal2D"
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma shader_feature _USE_URP_RENDERING
            #pragma shader_feature _NORMALMAP

            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/ShapeLightShared.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/InputData2D.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/SurfaceData2D.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/CombinedShapeLightShared.hlsl"

            float4 Frag(Varyings i) : SV_Target
            {
                #if defined(_USE_URP_RENDERING)
                // 如果启用了 URP 渲染，我们在这里执行逻辑
                #else
                discard;
                #endif

                float4 col = FragCommon(TEXTURE2D_ARGS(_MainTex, sampler_MainTex), i.uv);
                col *= i.color;
                
                half3 normalTS = half3(0, 0, 1);
                #if defined(_NORMALMAP)
                float4 normalSample = FragCommon(TEXTURE2D_ARGS(_NormalMap, sampler_NormalMap), i.uv);
                normalTS = UnpackNormal(normalSample);
                #endif

                SurfaceData2D surfaceData;
                InputData2D inputData;
                
                InitializeSurfaceData(col.rgb, col.a, 1.0, normalTS, surfaceData);
                InitializeInputData(i.uv, i.lightingUV, inputData);
                
                float4 litColor = CombinedShapeLightShared(surfaceData, inputData);
                litColor.rgb *= litColor.a; // premultiply

                return litColor;
            }
            ENDHLSL
        }

        // Pass 2: URP 2D 法线渲染
        Pass
        {
            Name "Normals"
            Tags { "LightMode" = "Normals" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragNormals
            #pragma shader_feature _USE_URP_RENDERING
            #pragma shader_feature _NORMALMAP

            float4 FragNormals(Varyings i) : SV_Target
            {
                #if !defined(_USE_URP_RENDERING) || !defined(_NORMALMAP)
                discard;
                #endif

                float4 col = FragCommon(TEXTURE2D_ARGS(_MainTex, sampler_MainTex), i.uv);
                float4 normalSample = FragCommon(TEXTURE2D_ARGS(_NormalMap, sampler_NormalMap), i.uv);
                
                float3 normal = UnpackNormal(normalSample);
                float3 packedNormal = normal * 0.5 + 0.5;

                return float4(packedNormal, col.a * i.color.a);
            }
            ENDHLSL
        }

        // 保留一个默认 Pass 兼容非 URP 2D 情况（如果需要）
        Pass
        {
            Name "SpriteBlurUnlit"
            ZWrite Off
            
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            float4 Frag(Varyings i) : SV_Target
            {
                float4 col = FragCommon(TEXTURE2D_ARGS(_MainTex, sampler_MainTex), i.uv);
                col *= i.color;
                col.rgb *= col.a; // premultiply
                return col;
            }
            ENDHLSL
        }
    }
}
