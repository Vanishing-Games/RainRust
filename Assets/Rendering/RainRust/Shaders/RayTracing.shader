Shader "Hidden/RainRust/RayTracing"
{
    Properties 
    { 
	    SrcMode ("SrcMode", Float) = 0
	    DstMode ("DstMode", Float) = 0
    } 
    SubShader
    {
        Cull Off // No culling
        ZWrite Off // No depth writing
        ZTest Always // Always pass depth test

        Pass    // 0
        {
            name "RayTracing"
	        Blend [SrcMode] [DstMode]

            HLSLPROGRAM
            #include "Utils.hlsl"
            
            #pragma multi_compile_local FRAGMENT_RANDOM TEXTURE_RANDOM _
            #pragma multi_compile_local ONE_ALPHA OBJECTS_MASK_ALPHA NORMALIZED_ALPHA

            #pragma vertex vert
            #pragma fragment frag

            sampler2D _ColorTex;
            sampler2D _DistTex;
            sampler2D _NoiseTex;

            float  _Samples;
            float4 _Aspect;
            float4 _Scale;
            float4 _NoiseTilingOffset;

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
            fragIn_gi vert(vertIn v)
            {
                fragIn_gi o;
                o.vertex = v.vertex * _Scale;
                o.uv = v.uv;

#if defined(FRAGMENT_RANDOM) || defined(TEXTURE_RANDOM)
                o.noise_uv = v.uv * _NoiseTilingOffset.xy + _NoiseTilingOffset.zw;
#endif
                return o;
            }

            float3 trace(const float2 uv, const float2 dir)
            {
                float2 uvPos = uv;

                // simple ray trace
                const float4 col = tex2D(_ColorTex, uv).rgba;
                if (col.a > 0)
                    return col.rgb / col.a;
                
                uvPos += dir * tex2D(_DistTex, uvPos).rr;
                if (notUVSpace(uvPos))
                    return AMBIENT;
                
                [unroll]
                for (int n = 1; n < STEPS; n++)
                {
                    const float4 col = tex2D(_ColorTex, uvPos).rgba;
                    if (col.a > 0)
                        return col.rgb * falloff((uv - uvPos) * _Aspect.xy, _Power * col.a);

                    uvPos += dir * tex2D(_DistTex, uvPos).rr;
                    if (notUVSpace(uvPos))
                        return AMBIENT;
                }
                
                return AMBIENT;
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
        
        
        Pass    // 1
        {
            name "Overlay"
            HLSLPROGRAM
            
            #include "Utils.hlsl"
            
            #pragma vertex vert_default
            #pragma fragment frag

            sampler2D _OverlayTex;
            float     _Intensity;

            // =======================================================================            
            float4 frag(fragIn i) : SV_Target
            {
                const float4 overlay = tex2D(_OverlayTex, i.uv);

                if (overlay.a != 1)
                    discard;
                
                return overlay * _Intensity;
            }
            ENDHLSL
        }
        
        Pass    // 2
        {
            name "Blit"
            HLSLPROGRAM
            
            #include "Utils.hlsl"
            
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            float4    _Scale;

            // =======================================================================
            fragIn vert(vertIn v)
            {
                fragIn o;
                o.vertex = v.vertex * _Scale;
                o.uv = v.uv;

                return o;
            }
            
            float4 frag(fragIn i) : SV_Target
            {
                return tex2D(_MainTex, i.uv);
            }
            ENDHLSL
        }
    }
}