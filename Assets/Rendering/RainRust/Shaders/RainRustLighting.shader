Shader "RainRust/RainRustLighting"
{
    Properties
    {
        [MainTexture] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        [Toggle(_ACCEPT_LIGHTING)] _AcceptLighting("Accept Lighting", Float) = 1.0
        
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

            return color;
        }
        ENDHLSL

        // Pass 1: 用于 RainRust 光照系统的自定义渲染
        Pass
        {
            Name "RainRustLighting"
            Tags { "LightMode" = "RainRustLighting" }

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
                // 如果没有勾选 Accept Lighting，则在这个 Pass 中不渲染
                #if !defined(_ACCEPT_LIGHTING)
                discard;
                #endif
                return frag_common(input);
            }
            ENDHLSL
        }

        // Pass 2: 用于 URP 标准渲染（当不接受光照时）
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
                // 如果勾选了 Accept Lighting，则在这个 Pass 中不渲染（交给上面的 Pass 处理）
                #if defined(_ACCEPT_LIGHTING)
                discard;
                #endif
                return frag_common(input);
            }
            ENDHLSL
        }
    }
}
