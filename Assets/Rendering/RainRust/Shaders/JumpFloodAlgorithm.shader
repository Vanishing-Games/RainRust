Shader "Hidden/RainRust/JumpFloodAlgorithm"
{
    SubShader
    {
        Cull Off // 不剔除
        ZWrite Off // 不写入深度
        ZTest Always // 总是通过深度测试

        Pass
        {
            Name "JumpFlood"

            HLSLPROGRAM
            #include "Utils.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            #pragma vertex Vert
            #pragma fragment Frag

            // =======================================================================

            // AddBlitPass 自动绑定
            TEXTURE2D(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

            float2 _StepSize;
            float2 _Aspect;

            // =======================================================================

            FragInput Vert(uint vertexID : SV_VertexID)
            {
                FragInput o;
            
                // 生成全屏三角形 UV
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

            float4 Frag(const FragInput i) : SV_Target
            {
                float minDist = 2e10; // 最小平方距离
                float2 minDistUV = float2(0, 0);
                float found = 0;

                [unroll]
                for (int y = -1; y <= 1; y++)
                {
                    [unroll]
                    for (int x = -1; x <= 1; x++)
                    {
                        const float2 offset = float2(x, y) * _StepSize;
                        const float4 peek = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, i.uv + offset);

                        // 利用 Z 通道判断是否为有效种子
                        if (peek.z > 0.5)
                        {
                            const float2 dir = (peek.xy - i.uv) * _Aspect;
                            const float dist = dot(dir, dir);
                            if (dist < minDist)
                            {
                                minDist = dist;
                                minDistUV = peek.xy;
                                found = 1.0;
                            }
                        }
                    }
                }

                return float4(minDistUV, found, 1.0);
            }
            ENDHLSL
        }
    }
}
