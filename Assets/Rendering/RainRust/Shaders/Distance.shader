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
            #include "Utils.hlsl" // 确保你的 fragIn 结构体定义在此处
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma vertex vert
            #pragma fragment frag
            
            TEXTURE2D(_BlitTexture);
            SAMPLER(sampler_BlitTexture);
            
            float     _Offset;
            // 修正：如果只用来修正单轴，通常定义为 float 即可。如果需要 XY 独立修正，保留 float2，但在下方对应修改。
            // 这里假设 _Aspect.x 存放的是 AspectRatio (Width/Height)
            float2    _Aspect; 

            fragIn vert(uint vertexID : SV_VertexID)
            {
                fragIn o;
            
                // 生成全屏三角形 UV
                float2 uv = float2(
                    (vertexID << 1) & 2,
                    vertexID & 2
                );
                
                o.uv = uv;
                o.vertex = float4(uv * 2.0 - 1.0, 0.0, 1.0);
                
                // 修正：正确的 UV 翻转逻辑
#if UNITY_UV_STARTS_AT_TOP
                o.uv.y = 1.0 - o.uv.y;
#endif
                
                return o;
            }

            float frag(fragIn i) : SV_Target
            {
                // 修正：显式提取 .xy，避免隐式转换警告，并更改变量名防冲突
                float2 sampledUV = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, i.uv).xy;
                
                // 修正：提取 _Aspect 的正确分量 (假设只修正 x 轴)
                i.uv.x     *= _Aspect.x;
                sampledUV.x *= _Aspect.x;
                
                return distance(i.uv, sampledUV) + _Offset;
            }
            ENDHLSL
        }
    }
}