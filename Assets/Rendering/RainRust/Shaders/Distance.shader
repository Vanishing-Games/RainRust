Shader "Hidden/RainRust/Distance"
{
    SubShader
    {
        Cull Off // No culling
        ZWrite Off // No depth writing
        ZTest Always // Always pass depth test

        Pass
        {
            name "Distance"

            HLSLPROGRAM
            #include "Utils.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma vertex vert
            #pragma fragment frag
            
            // AddBlitPass 自动绑定
            TEXTURE2D(_BlitTexture);
            SAMPLER(sampler_BlitTexture);
            float     _Offset;
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
                
                return o;
            }

            float frag(fragIn i) : SV_Target
            {
                float2 sample = SAMPLE_TEXTURE2D(_BlitTexture,sampler_BlitTexture, i.uv);
                i.uv.x   *= _Aspect;
                sample.x *= _Aspect;
                
                return distance(i.uv, sample.xy) + _Offset;
            }
            ENDHLSL
        }
    }
}