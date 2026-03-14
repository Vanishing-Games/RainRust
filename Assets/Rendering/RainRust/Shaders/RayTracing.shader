Shader "Hidden/RainRust/RayTracing"
{
    Properties 
    { 
	    SrcMode ("SrcMode", Float) = 1
	    DstMode ("DstMode", Float) = 0
    } 
    SubShader
    {
        Cull Off // 不剔除
        ZWrite Off // 不写入深度
        ZTest Off // 不进行深度测试

        Pass    // 0
        {
            Name "RayTracing"
	        Blend [SrcMode] [DstMode]

            HLSLPROGRAM
            #include "Utils.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // =======================================================================
            
            // 随机数生成方式: shader内生成随机数, 从纹理采样随机数, 无随机
            #pragma multi_compile_local FRAGMENT_RANDOM TEXTURE_RANDOM _
            // 输出alpha通道方式: 全部为1, 使用对象遮罩, 颜色归一化后最大值
            #pragma multi_compile_local ONE_ALPHA OBJECTS_MASK_ALPHA NORMALIZED_ALPHA

            #pragma vertex Vert
            #pragma fragment Frag

            sampler2D _ColorTex; // 场景颜色纹理
            sampler2D _DistTex; // 场景距离纹理 (SDF)
            sampler2D _NoiseTex; // 随机噪声纹理

            float2 _Aspect; // 16:9 为 (1, 0.5625)
            float4 _NoiseTilingOffset; // 噪声纹理的缩放和偏移 (tiling.x, tiling.y, offset.x, offset.y)
            
            float  _Samples; // 光线采样数
            float _Intensity; // 光照强度
            float _Falloff; // 距离衰减幂指数

            // =======================================================================

            struct FragInputGI
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
#if defined(FRAGMENT_RANDOM) || defined(TEXTURE_RANDOM)
                float2 noise_uv : TEXCOORD1;
#endif
            };

            // =======================================================================

            float3 Trace(const float2 uv, const float2 dir) // Ray Marching
            {
                float2 uvPos = uv; // 当前采样坐标

                // 若起始点已在光源上, 直接返回颜色
                const float4 color = tex2D(_ColorTex, uv).rgba;
                if (color.a > 0)
                    return color.rgb / color.a;
                
                // 步进
                uvPos += dir * tex2D(_DistTex, uvPos).rr;
                if (NotUVSpace(uvPos))
                    return AMBIENT;
                
                [unroll]
                for (int n = 1; n < STEPS; n++)
                {
                    const float4 color = tex2D(_ColorTex, uvPos).rgba;
                    if (color.a > 0)
                        return color.rgb * Falloff((uv - uvPos) * _Aspect.xy, _Falloff * color.a);

                    uvPos += dir * tex2D(_DistTex, uvPos).rr;
                    if (NotUVSpace(uvPos))
                        return AMBIENT;
                }
                
                return AMBIENT;
            }

            // =======================================================================

            FragInputGI Vert(uint vertexID : SV_VertexID)
            {
                FragInputGI o;
            
                // 生成全屏三角形 UV
                float2 uv = float2(
                    (vertexID << 1) & 2,
                    vertexID & 2
                );
                
                o.uv = uv;
                
                // 裁剪空间位置
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

            float4 Frag(FragInputGI i) : SV_Target
            {
                float3 result = AMBIENT;

                // 获取随机值
#if defined(FRAGMENT_RANDOM)
                const float rand = Random(i.noise_uv);
#elif defined(TEXTURE_RANDOM)
                const float rand = tex2D(_NoiseTex, i.noise_uv).r * float(3.1415) * 2;
#else
                const float rand = 0;
#endif

                // 发射光线
                for (float f = 0.; f < _Samples; f++)
                {
                    const float t = (f + rand) / _Samples * float(3.1415 * 2.); // 均匀分布在圆周上
                    result += Trace(i.uv, float2(cos(t), sin(t)) / _Aspect.xy);
                }

                result /= _Samples;

                // 亮度调节
                result *= _Intensity;

                // Alpha 通道处理
#if   defined(ONE_ALPHA)
                return float4(result, 1);
                
#elif defined(OBJECTS_MASK_ALPHA)
                const float mask = tex2D(_ColorTex, i.uv).a;
                return float4(result, mask);
                
#elif defined(NORMALIZED_ALPHA)
                // 颜色归一化, Alpha 作为不透明度
                float norm = max(result.r, max(result.g, result.b));
                return float4(result / norm, norm);
#endif
            }
            ENDHLSL
        }
    }
}
