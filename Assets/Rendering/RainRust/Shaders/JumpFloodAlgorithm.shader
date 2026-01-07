Shader "Hidden/GiLight2D/JumpFloodAlgorithm"
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
            
            #pragma vertex vert_default
            #pragma fragment frag

            sampler2D _SeedTex;
            float2    _StepSize;
            float2    _Aspect;

            // =======================================================================
            float2 frag(const fragIn i) : SV_Target
            {
                float min_dist = 1;
                float2 min_dist_uv = float2(0, 0);

                [unroll]
                for (int y = -1; y <= 1; y ++)
                {
                    [unroll]
                    for (int x = -1; x <= 1; x ++)
                    {
                        const float2 peek = tex2D(_SeedTex, i.uv + float2(x, y) * _StepSize).xy;
                        if (all(peek))
                        {
                            const float2 dir = (peek - i.uv ) * _Aspect;
                            const float dist = dot(dir, dir);
                            if (dist < min_dist)
                            {
                                min_dist = dist;
                                min_dist_uv = peek;
                            }
                        }
                    }
                }

                return min_dist_uv;
            }
            ENDHLSL
        }
    }
}