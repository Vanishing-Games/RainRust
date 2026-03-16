Shader "Hidden/RainRust/Distance"
{
    SubShader
    {
        Cull Off 
        ZWrite Off 
        ZTest Always 

        Pass
        {
            Name "Distance"

            HLSLPROGRAM
            #include "Utils.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma vertex Vert
            #pragma fragment Frag
            
            // =======================================================================

            TEXTURE2D(_BlitTexture);
            SAMPLER(sampler_BlitTexture);
            
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

            float Frag(FragInput i) : SV_Target
            {
                float4 jfaData = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, i.uv);
                float2 sampledUV = jfaData.xy;

                // 计算到最近种子的距离
                float2 diff = (sampledUV - i.uv) * _Aspect;
                return length(diff);
            }
            ENDHLSL
        }
    }
}
