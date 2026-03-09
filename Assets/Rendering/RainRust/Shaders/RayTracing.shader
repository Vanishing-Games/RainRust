Shader "Hidden/RainRust/RayTracing"
{
    Properties 
    { 
	    SrcMode ("SrcMode", Float) = 1
	    DstMode ("DstMode", Float) = 0
    } 
    SubShader
    {
        Cull Off // No culling
        ZWrite Off // No depth writing
        ZTest Off // No depth testing

        Pass    // 0
        {
            name "RayTracing"
	        Blend [SrcMode] [DstMode]

            HLSLPROGRAM
            #include "Utils.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // =======================================================================
            
            // 随机数生成方式 shader内生成随机数, 从纹理采样随机数, 无随机
            #pragma multi_compile_local FRAGMENT_RANDOM TEXTURE_RANDOM _
            // 输出alpha通道方式 全部为1, 使用对象遮罩, 颜色归一化后最大值
            #pragma multi_compile_local ONE_ALPHA OBJECTS_MASK_ALPHA NORMALIZED_ALPHA

            #pragma vertex vert
            #pragma fragment frag

            sampler2D _ColorTex; // 存储场景颜色的纹理, 可以认为是所有光源
            sampler2D _DistTex; // 存储场景距离的纹理, 每个像素存储到最近光源的距离
            sampler2D _NoiseTex; // 存储随机数的纹理, 用于打散采样

            float4 _Aspect;
            float4 _NoiseTilingOffset;
            
            float  _Samples;
            float _Intensity;
            float _Power;

            // =======================================================================

            struct fragIn_gi
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
#if defined(FRAGMENT_RANDOM) || defined(TEXTURE_RANDOM)
                float2 noise_uv : TEXCOORD1;
#endif
            };

            // =======================================================================

            float3 trace(const float2 uv, const float2 dir) // Ray Marching
            {
                float2 uvPos = uv; // uv坐标空间的射线位置, 从uv出发沿dir方向前进

                // 采样点在光源上, 直接返回颜色
                const float4 color = tex2D(_ColorTex, uv).rgba;
                if (color.a > 0)
                    return color.rgb / color.a;
                
                uvPos += dir * tex2D(_DistTex, uvPos).rr;
                if (notUVSpace(uvPos))
                    return AMBIENT;
                
                [unroll]
                for (int n = 1; n < STEPS; n++)
                {
                    const float4 color = tex2D(_ColorTex, uvPos).rgba;
                    if (color.a > 0)
                        return color.rgb * falloff((uv - uvPos) * _Aspect.xy, _Power * color.a);

                    uvPos += dir * tex2D(_DistTex, uvPos).rr;
                    if (notUVSpace(uvPos))
                        return AMBIENT;
                }
                
                return AMBIENT;
            }

            // =======================================================================
            fragIn_gi vert(uint vertexID : SV_VertexID)
            {
                fragIn_gi o;
            
                // 生成 fullscreen triangle 的 uv
                float2 uv = float2(
                    (vertexID << 1) & 2,
                    vertexID & 2
                );
                
                o.uv = uv;
                
                // clip space position
                float2 pos = uv * 2.0 - 1.0;
                
                o.vertex = float4(pos, 0.0, 1.0);
#if UNITY_UV_STARTS_AT_TOP
                o.vertex.y = -o.vertex.y;
#endif
                
            #if defined(FRAGMENT_RANDOM) || defined(TEXTURE_RANDOM)
                o.noise_uv = uv * _NoiseTilingOffset.xy + _NoiseTilingOffset.zw;
            #endif
                
                return o;
            }

            float4 frag(fragIn_gi i) : SV_Target
            {
                float3 result = AMBIENT;

                // take random value
#if defined(FRAGMENT_RANDOM)
                const float rand = random(i.noise_uv);
#elif defined(TEXTURE_RANDOM)
                const float rand = tex2D(_NoiseTex, i.noise_uv).r * float(3.1415) * 2;
#else
                const float rand = 0;
#endif

                // emmit rays
                for (float f = 0.; f < _Samples; f++)
                {
                    const float t = (f + rand) / _Samples * float(3.1415 * 2.);
                    result += trace(i.uv, float2(cos(t), sin(t)) / _Aspect.xy);
                }

                result /= _Samples;

                // color adjustments
                result *= _Intensity;

                // alpha channel output
#if   defined(ONE_ALPHA)
                return float4(result, 1);
                
#elif defined(OBJECTS_MASK_ALPHA)
                const float mask = tex2D(_ColorTex, i.uv).a;
                return float4(result, mask);
                
#elif defined(NORMALIZED_ALPHA)
                // normalize color, alpha as opacity
                float norm = max(result.r, max(result.g, result.b));
                return float4(result / norm, norm);
#endif
            }
            ENDHLSL
        }
    }
}