Shader "Hidden/RainRust/Composition"
{
    Properties
    {
        _MainTex("Background", 2D) = "white" {}
        _LightingTex("Lighting", 2D) = "white" {}
        _ReceiverTex("Receiver", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "RainRustComposition"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ BLEND_ADDITIVE BLEND_ALPHABLEND BLEND_MULTIPLY
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_LightingTex);
            SAMPLER(sampler_LightingTex);
            TEXTURE2D(_ReceiverTex);
            SAMPLER(sampler_ReceiverTex);

            Varyings vert(uint vertexID : SV_VertexID)
            {
                Varyings output;
                output.uv = float2((vertexID << 1) & 2, vertexID & 2);
                output.positionCS = float4(output.uv * 2.0 - 1.0, 0.0, 1.0);

                #if UNITY_UV_STARTS_AT_TOP
                output.uv.y = 1.0 - output.uv.y;
                #endif

                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float4 background = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                float4 lighting = SAMPLE_TEXTURE2D(_LightingTex, sampler_LightingTex, input.uv);
                float4 receiver = SAMPLE_TEXTURE2D(_ReceiverTex, sampler_ReceiverTex, input.uv);

                float3 litReceiver = receiver.rgb * lighting.rgb;
                float receiverAlpha = receiver.a;

                float3 finalColor = background.rgb;

                #if defined(BLEND_ADDITIVE)
                finalColor += litReceiver;
                #elif defined(BLEND_ALPHABLEND)
                finalColor = lerp(finalColor, litReceiver, receiverAlpha);
                #elif defined(BLEND_MULTIPLY)
                finalColor *= litReceiver;
                #endif

                return float4(finalColor, background.a);
            }
            ENDHLSL
        }
    }
}
