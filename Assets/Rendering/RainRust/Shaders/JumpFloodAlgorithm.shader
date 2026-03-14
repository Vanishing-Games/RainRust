Shader "Hidden/RainRust/JumpFloodAlgorithm"
{
    SubShader
    {
        Cull Off // No culling
        ZWrite Off // No depth writing
        ZTest Always // Always pass depth test

        Pass
        {
            name "JumpFlood"

            HLSLPROGRAM
            #include "Utils.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            #pragma vertex vert
            #pragma fragment frag

            // AddBlitPass 自动绑定
            TEXTURE2D(_BlitTexture);
            SAMPLER(sampler_BlitTexture);
            float2    _StepSize;
            float2    _Aspect;

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
                o.uv.y = 1.0 - o.uv.y;
#endif
                
                return o;
            }

            // =======================================================================
            float4 frag(const fragIn i) : SV_Target
            {
                float min_dist = 2e10; // 调大初始值
                float2 min_dist_uv = float2(0, 0);
                float found = 0;

                [unroll]
                for (int y = -1; y <= 1; y ++)
                {
                    [unroll]
                    for (int x = -1; x <= 1; x ++)
                    {
                        const float4 peek = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, i.uv + float2(x, y) * _StepSize);
                        // 利用 Z 通道判断是否为有效种子
                        if (peek.z > 0.5)
                        {
                            const float2 dir = (peek.xy - i.uv ) * _Aspect;
                            const float dist = dot(dir, dir);
                            if (dist < min_dist)
                            {
                                min_dist = dist;
                                min_dist_uv = peek.xy;
                                found = 1.0;
                            }
                        }
                    }
                }

                return float4(min_dist_uv, found, 1.0);
            }
            ENDHLSL
        }
    }
}