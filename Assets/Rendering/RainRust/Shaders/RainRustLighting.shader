Shader "RainRust/RainRustLighting"
{
    Properties
    {
        [MainTexture] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        [Toggle(_EMIT_LIGHTING)] _EmitLighting("Emit Lighting", Float) = 1.0
        [Toggle(_ACCEPT_LIGHTING)] _AcceptLighting("Accept Lighting(Toggle on cause this is useless)", Float) = 1.0
        
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
            "RenderPipeline"="UniversalPipeline" 
            "PreviewType"="Plane" 
            "CanUseSpriteAtlas"="True" 
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        struct Attributes
        {
            float4 positionOS   : POSITION;
            float4 color        : COLOR;
            float2 uv           : TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct Varyings
        {
            float4 positionCS   : SV_POSITION;
            float4 color        : COLOR;
            float2 uv           : TEXCOORD0;
            UNITY_VERTEX_OUTPUT_STEREO
        };

        // 纹理定义建议使用 URP 的宏，但为了兼容你的旧逻辑保留 sampler2D
        sampler2D _MainTex;
        sampler2D _AlphaTex;
        float4 _Color;
        float4 _RendererColor;
        float2 _Flip;

        // --- 修复：手动实现 URP 缺失的 PixelSnap 函数 ---
        float4 UnityPixelSnap(float4 pos)
        {
            float2 hms = _ScreenParams.xy * 0.5;
            pos.xy = round(pos.xy * hms) / hms;
            return pos;
        }

        Varyings vert(Attributes input)
        {
            Varyings output;
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

            // 处理翻转逻辑（SpriteRenderer 常用）
            float3 positionWS = input.positionOS.xyz;
            
            output.positionCS = TransformObjectToHClip(positionWS);
            output.uv = input.uv;
            output.color = input.color * _Color * _RendererColor;

            #if PIXELSNAP_ON
            output.positionCS = UnityPixelSnap(output.positionCS);
            #endif

            return output;
        }

        float4 frag_common(Varyings input) : SV_Target
        {
            float4 color = tex2D(_MainTex, input.uv) * input.color;

            #if ETC1_EXTERNAL_ALPHA
            float4 alpha = tex2D(_AlphaTex, input.uv);
            color.a = alpha.r;
            #endif

            // 丢弃过透明的像素，确保光照边缘干净
            clip(color.a - 0.1);

            return color;
        }
        ENDHLSL

        // Pass 1: 用于 RainRust 光源提取
        Pass
        {
            Name "RainRustLightSource"
            Tags { "LightMode" = "RainRustLightSource" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite On
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #pragma shader_feature _EMIT_LIGHTING

            float4 frag(Varyings input) : SV_Target
            {
                #if !defined(_EMIT_LIGHTING)
                discard; 
                #endif
                return frag_common(input);
            }
            ENDHLSL
        }

        // Pass 2: 用于 RainRust 接收者渲染 (带 Stencil 标记)
        Pass
        {
            Name "RainRustLighting"
            Tags { "LightMode" = "RainRustLighting" }

            Stencil
            {
                Ref 1
                Comp Always
                Pass Replace
            }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite On
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #pragma shader_feature _ACCEPT_LIGHTING

            float4 frag(Varyings input) : SV_Target
            {
                #if !defined(_ACCEPT_LIGHTING)
                discard;
                #endif
                return frag_common(input);
            }
            ENDHLSL
        }

        // Pass 3: URP 2D 默认渲染（当不使用 RainRust 特性时显示）
        Pass
        {
            Name "Universal2D" 
            Tags { "LightMode" = "Universal2D" } 

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #pragma shader_feature _ACCEPT_LIGHTING

            float4 frag(Varyings input) : SV_Target
            {
                // 如果开启了接受光照，逻辑上它应该由 RainRust 的渲染管线控制
                // 在默认 URP 渲染中我们 discard 它
                #if defined(_ACCEPT_LIGHTING)
                discard;
                #endif
                return frag_common(input);
            }
            ENDHLSL
        }
    }
}