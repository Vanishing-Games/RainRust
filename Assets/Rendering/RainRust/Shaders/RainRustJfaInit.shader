Shader "Hidden/RainRust/JfaInit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100

        Pass
        {
            Name "JfaInit"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #include "Utils.hlsl"

            #pragma vertex vert_default
            #pragma fragment frag

            sampler2D _MainTex;
            float2    _StepSize;
            float2    _Aspect;

            float2 frag (const fragIn input) : SV_Target
            {
                float min_dist = 1;
                float2 min_dist_uv = float2(0, 0);

                [unroll]
                for (int y = -1; y <= 1; y ++)
                {
                    [unroll]
                    for (int x = -1; x <= 1; x ++)
                    {
                        const float2 peek = tex2D(_MainTex, input.uv + float2(x, y) * _StepSize).xy;
                        if (all(peek))
                        {
                            const float2 dir = (peek - input.uv ) * _Aspect;
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
