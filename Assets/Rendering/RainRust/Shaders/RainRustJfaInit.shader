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
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Blit.hlsl"

            half4 Frag (Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                // Sample the source texture (MainRt)
                // We access the texture via the _BlitTexture provided by Blitter if using the newer API, 
                // or _MainTex if binding manually. Blitter.BlitTexture usually binds to _BlitTexture.
                // Let's support _BlitTexture as per URP 17+ standards if possible, or fallback.
                // In URP 14/15, it's often _MainTex or _BlitTexture.
                
                // Note: The prompt asks to output UV coordinates. 
                // We assume we should only output UVs where there is an object.
                // Since the instruction is brief, I'll output UVs based on input alpha/color.
                
                half4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv);
                
                // Simple threshold check. If alpha > 0, it's a seed.
                if (col.a > 0.0)
                {
                    return half4(uv.x, uv.y, 0.0, 1.0);
                }
                
                // Return a value indicating "empty". 
                // For JFA, this is often a value outside [0,1].
                return half4(-1.0, -1.0, 0.0, 0.0);
            }
            ENDHLSL
        }
    }
}
