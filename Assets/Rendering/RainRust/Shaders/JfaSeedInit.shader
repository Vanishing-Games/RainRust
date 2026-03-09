Shader "Hidden/RainRust/JfaSeedInit"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map", 2D) = "white"
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "JFAInit"

            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM

            #include "Utils.hlsl"

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // AddBlitPass 自动绑定
            TEXTURE2D(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

            fragIn vert(uint vertexID : SV_VertexID)
            {
                fragIn o;
            
                float2 uv = float2(
                    (vertexID << 1) & 2,
                    vertexID & 2
                );
                
                o.uv = uv;
                o.vertex = float4(uv * 2.0 - 1.0, 0.0, 1.0);
#if UNITY_UV_STARTS_AT_TOP
                o.vertex.y = -o.vertex.y;
#endif
                
                return o;
            }

            float4 frag(const fragIn i) : SV_Target
            {
                float alpha = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, i.uv).a;

                if (alpha > 0.001)
                {
                    return float4(i.uv, 0, 1);
                }

                return float4(0,0,0,0);
            }

            ENDHLSL
        }
    }
}