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
            
            #pragma vertex vert_default
            #pragma fragment frag
            
            sampler2D _JfaTex;
            float     _Offset;
            float2    _Aspect;

            // =======================================================================
            float frag(fragIn i) : SV_Target
            {
                float2 sample = tex2D(_JfaTex, i.uv);
                i.uv.x   *= _Aspect;
                sample.x *= _Aspect;
                
                return distance(i.uv, sample.xy) + _Offset;
            }
            ENDHLSL
        }
    }
}