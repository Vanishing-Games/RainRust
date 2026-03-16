Shader "RainRust/CrtEffect"
{
    Properties
    {
        _BlitTexture ("Texture", 2D) = "white" {}
        _ScanlineDensity ("Scanline Density", Range(0.0, 1000.0)) = 500.0
        _ScanlineStrength ("Scanline Strength", Range(0.0, 1.0)) = 0.1
        _BarrelDistortion ("Barrel Distortion", Range(0.0, 0.5)) = 0.1
        _VignetteStrength ("Vignette Strength", Range(0.0, 1.0)) = 0.5
        _ColorTint ("Color Tint", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" }
        ZWrite Off
        Cull Off
        ZTest Always
        Blend Off

        Pass
        {
            Name "CRT_Pass"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // Textures and Samplers
            TEXTURE2D(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

            // Shader properties
            float _ScanlineDensity;
            float _ScanlineStrength;
            float _BarrelDistortion;
            float _VignetteStrength;
            float4 _ColorTint;

            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // Procedural Fullscreen Triangle Vertex Shader
            Varyings Vert(Attributes input)
            {
                Varyings output;
                
                // Generates a triangle that covers the screen
                // VertexID 0: (-1, -1) -> UV (0, 0)
                // VertexID 1: ( 3, -1) -> UV (2, 0)
                // VertexID 2: (-1,  3) -> UV (0, 2)
                float2 uv = float2((input.vertexID << 1) & 2, input.vertexID & 2);
                output.uv = uv;
                
                // Map UV [0,2] to Clip Space [-1, 3] (starts at -1, -1)
                output.positionCS = float4(output.uv * 2.0 - 1.0, 0.0, 1.0);
                
                // Flip UVs on platforms where texture coordinates 0,0 are top-left (Direct3D-like)
                // if we are rendering directly to screen (rendering to a texture is usually bottom-left)
                #if UNITY_UV_STARTS_AT_TOP
                output.uv.y = 1.0 - output.uv.y;
                #endif

                return output;
            }

            // Function to apply barrel distortion
            float2 ApplyBarrelDistortion(float2 uv, float distortion)
            {
                float2 center = float2(0.5, 0.5);
                float2 vec = uv - center;
                float dist = dot(vec, vec);
                float f = 1.0 + dist * distortion;
                return (vec * f) + center;
            }

            // Function to apply vignette
            float ApplyVignette(float2 uv, float strength)
            {
                float2 center = float2(0.5, 0.5);
                float dist = distance(uv, center);
                float vignette = smoothstep(0.4, 1.0, dist * (1.0 + strength));
                return 1.0 - vignette * strength;
            }

            // Fragment Shader
            float4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;

                // 1. Barrel Distortion
                uv = ApplyBarrelDistortion(uv, _BarrelDistortion);

                // Check if UV is outside [0,1] range after distortion
                if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0)
                {
                    return float4(0, 0, 0, 1); // Render black outside the distorted area
                }

                // Sample the blit texture (screen content)
                float4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, uv);

                // 2. Scanlines
                float scanline = sin(uv.y * _ScanlineDensity) * 0.5 + 0.5; // Creates a wave pattern
                scanline = lerp(1.0, scanline, _ScanlineStrength); // Apply strength
                col.rgb *= scanline;

                // 3. Vignette
                float vignette = ApplyVignette(input.uv, _VignetteStrength);
                col.rgb *= vignette;

                // Apply color tint
                col.rgb *= _ColorTint.rgb;

                return col;
            }
            ENDHLSL
        }
    }
}
