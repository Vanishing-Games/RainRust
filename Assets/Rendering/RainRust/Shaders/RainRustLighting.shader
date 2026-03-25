Shader "RainRust/RainRustLighting"
{
    Properties
    {
        [MainTexture] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        [Toggle(_ACCEPT_LIGHTING)] _AcceptLighting("Accept Lighting", Float) = 1.0
        [Toggle(_EMIT_LIGHTING)] _EmitLighting("Emit Lighting", Float) = 1.0
        
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }

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

        sampler2D _MainTex;
        sampler2D _AlphaTex;
        float4 _Color;
        float4 _RendererColor;
        float2 _Flip;

        Varyings vert(Attributes input)
        {
            Varyings output;
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

            output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
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

            // Discard transparent pixels
            clip(color.a - 0.1);

            return color;
        }
        ENDHLSL

        // Pass 1: 用于 RainRust 光源提取 (Light Sources Pass)
        // 只有当材质开启了 _EMIT_LIGHTING 时，此 Pass 才会产生输出
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
                discard; // 如果未开启发射光照，则在光源阶段不渲染
                #endif
                return frag_common(input);
            }
            ENDHLSL
        }

        // Pass 2: 用于 RainRust 接收者渲染 (Receivers Pass)
        // 只有当材质开启了 _ACCEPT_LIGHTING 时，此 Pass 才会产生输出
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
                discard; // 如果未开启接受光照，则在接收者阶段不渲染
                #endif
                return frag_common(input);
            }
            ENDHLSL
        }

        // Pass 3: 用于 URP 标准渲染（ fallback / 预览）
        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }

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
                // 如果开启了接受光照，则在 URP 默认渲染中禁用自己（交给 RainRust 特性处理渲染）
                #if defined(_ACCEPT_LIGHTING)
                discard;
                #endif
                return frag_common(input);
            }
            ENDHLSL
        }
    }
}
