Shader "RainRust/RainRustLighting"
{
    Properties
    {
        [MainTexture] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        
        [Toggle(_NORMALMAP)] _EnableNormalMap("Enable Normal Map", Float) = 0.0
        [NoScaleOffset] _NormalMap("Normal Map", 2D) = "bump" {}
        
        [Header(RainRust Custom Pipeline)]
        [Toggle(_EMIT_LIGHTING)] _EmitLighting("Emit Lighting", Float) = 1.0
        [Toggle(_ACCEPT_LIGHTING)] _AcceptLighting("Accept Lighting(Toggle on cause this is useless)", Float) = 1.0
        
        [Header(URP 2D)]
        [Toggle(_USE_URP_RENDERING)] _UseURPRendering("Use URP Rendering", Float) = 1.0
        
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
            "RenderPipeline"="UniversalPipeline" 
            "PreviewType"="Plane" 
            "CanUseSpriteAtlas"="True" 
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        struct Attributes
        {
            float4 positionOS   : POSITION;
            float4 color        : COLOR;
            float2 uv           : TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct Varyings
        {
            float4 positionCS   : SV_POSITION;
            float4 color        : COLOR;
            float2 uv           : TEXCOORD0;
            half2 lightingUV    : TEXCOORD1;
            UNITY_VERTEX_OUTPUT_STEREO
        };

        sampler2D _MainTex;
        sampler2D _AlphaTex;
        sampler2D _NormalMap; 
        float4 _Color;
        float4 _RendererColor;
        float2 _Flip;

        float4 UnityPixelSnap(float4 pos)
        {
            float2 hms = _ScreenParams.xy * 0.5;
            pos.xy = round(pos.xy * hms) / hms;
            return pos;
        }

        Varyings vert(Attributes input)
        {
            Varyings output;
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

            float3 positionWS = input.positionOS.xyz;
            
            output.positionCS = TransformObjectToHClip(positionWS);
            output.uv = input.uv;
            output.color = input.color * _Color * _RendererColor;
            output.lightingUV = ComputeScreenPos(output.positionCS).xy / output.positionCS.w;

            #if PIXELSNAP_ON
            output.positionCS = UnityPixelSnap(output.positionCS);
            #endif

            return output;
        }

        float4 frag_common(Varyings input)
        {
            float4 color = tex2D(_MainTex, input.uv) * input.color;

            #if ETC1_EXTERNAL_ALPHA
            float4 alpha = tex2D(_AlphaTex, input.uv);
            color.a = alpha.r;
            #endif

            clip(color.a - 0.1);

            return color;
        }
        ENDHLSL

        // Pass 1: 用于 RainRust 光源提取
        Pass
        {
            Name "RainRustLightSource"
            Tags { "LightMode" = "RainRustLightSource" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #pragma shader_feature _EMIT_LIGHTING

            float4 frag(Varyings input) : SV_Target
            {
                #if !defined(_EMIT_LIGHTING)
                discard; 
                #endif
                return frag_common(input);
            }
            ENDHLSL
        }

        // Pass 2: 用于 RainRust 接收者渲染 (带 Stencil 标记)
        Pass
        {
            Name "RainRustLighting"
            Tags { "LightMode" = "RainRustLighting" }

            Stencil
            {
                Ref 1
                Comp Always
                Pass Replace
            }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #pragma shader_feature _ACCEPT_LIGHTING

            float4 frag(Varyings input) : SV_Target
            {
                #if !defined(_ACCEPT_LIGHTING)
                discard;
                #endif
                return frag_common(input);
            }
            ENDHLSL
        }

        // Pass 3: URP 2D 基础颜色渲染
        Pass
        {
            Name "Universal2D" 
            Tags { "LightMode" = "Universal2D" } 

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            
            #pragma shader_feature _USE_URP_RENDERING
            #pragma shader_feature _NORMALMAP

            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/ShapeLightShared.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/InputData2D.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/SurfaceData2D.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/CombinedShapeLightShared.hlsl"

            float4 frag(Varyings input) : SV_Target
            {
                #if !defined(_USE_URP_RENDERING)
                discard;
                #endif
                
                float4 color = frag_common(input);
                
                half3 normalTS = half3(0, 0, 1);
                #if defined(_NORMALMAP)
                half4 normalSample = tex2D(_NormalMap, input.uv);
                normalTS = UnpackNormal(normalSample);
                #endif

                SurfaceData2D surfaceData;
                InputData2D inputData;
                
                InitializeSurfaceData(color.rgb, color.a, 1.0, normalTS, surfaceData);
                InitializeInputData(input.uv, input.lightingUV, inputData);
                
                half4 litColor = CombinedShapeLightShared(surfaceData, inputData);
                return litColor;
            }
            ENDHLSL
        }
        
        // Pass 4: URP 2D 法线渲染
        Pass
        {
            Name "Normals"
            Tags { "LightMode" = "Normals" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment fragNormals
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #pragma shader_feature _USE_URP_RENDERING
            #pragma shader_feature _NORMALMAP

            float4 fragNormals(Varyings input) : SV_Target
            {
                #if !defined(_USE_URP_RENDERING) || !defined(_NORMALMAP)
                discard;
                #endif

                float4 baseColor = frag_common(input);
                float4 normalSample = tex2D(_NormalMap, input.uv);
                float3 normal = UnpackNormal(normalSample);
                
                // URP 2D 渲染管线要求输出的法线被压缩在 0 到 1 的颜色空间内
                float3 packedNormal = normal * 0.5 + 0.5;
                
                return float4(packedNormal, baseColor.a);
            }
            ENDHLSL
        }
    }
}
