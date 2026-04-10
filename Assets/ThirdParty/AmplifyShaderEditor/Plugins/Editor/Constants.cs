// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using System.Collections.Generic;
using UnityEngine;

namespace AmplifyShaderEditor
{
    public struct Constants
    {
        /*public readonly static string[] CustomASEStandardArgsMacros =
        {
            "#if defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(UNITY_COMPILER_HLSLCC)//ASE Args Macros",
            "#define ASE_TEXTURE2D_ARGS(textureName) Texture2D textureName, SamplerState sampler##textureName",
            "#define ASE_TEXTURE3D_ARGS(textureName) Texture3D textureName, SamplerState sampler##textureName",
            "#define ASE_TEXTURECUBE_ARGS(textureName) TextureCube textureName, SamplerState sampler##textureName",
            "#define ASE_TEXTURE2D_PARAMS(textureName) textureName, sampler##textureName",
            "#define ASE_TEXTURE3D_PARAMS(textureName) textureName, sampler##textureName",
            "#define ASE_TEXTURECUBE_PARAMS(textureName) textureName, sampler##textureName",
            "#define ASE_TEXTURE2D_ARRAY_PARAMS(textureName) textureName, sampler##textureName",
            "#else//ASE Args Macros",
            "#define ASE_TEXTURE2D_ARGS(textureName) sampler2D textureName",
            "#define ASE_TEXTURE3D_ARGS(textureName) sampler3D textureName",
            "#define ASE_TEXTURECUBE_ARGS(textureName) samplerCUBE textureName",
            "#define ASE_TEXTURE2D_PARAMS(textureName) textureName",
            "#define ASE_TEXTURE3D_PARAMS(textureName) textureName",
            "#define ASE_TEXTURECUBE_PARAMS(textureName) textureName",
            "#define ASE_TEXTURE2D_ARRAY_PARAMS(textureName) textureName",
            "#endif//ASE Args Macros\n"
        };

        public readonly static string[] CustomASEDeclararionMacros =
        {
            "#define ASE_TEXTURE2D(textureName) {0}2D(textureName)",
            "#define ASE_TEXTURE2D_ARRAY(textureName) {0}2D_ARRAY(textureName)",
            "#define ASE_TEXTURE3D(textureName) {0}3D(textureName)",
            "#define ASE_TEXTURECUBE(textureName) {0}CUBE(textureName)\n"
        };

        public readonly static string[] CustomASEStandarSamplingMacrosHelper =
        {
            "#if defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(UNITY_COMPILER_HLSLCC)//ASE Sampling Macros",
            "#else//ASE Sampling Macros",
            "#endif//ASE Sampling Macros\n"
        };*/

        /*public readonly static string[] CustomASESamplingMacros =
        {
            "#define ASE_SAMPLE_TEXTURE2D(textureName,{0}coords) {1}2D{2}(textureName,{0}coords)",
            "#define ASE_SAMPLE_TEXTURE2D_LOD(textureName, {0}coord2, lod) {1}2D{2}_LOD(textureName, {0}coord2, lod)",
            "#define ASE_SAMPLE_TEXTURE2D_BIAS(textureName,{0}coord2, bias) {1}2D{2}_BIAS(textureName,{0}coord2, bias)",
            "#define ASE_SAMPLE_TEXTURE2D_GRAD(textureName,{0}coord2, dpdx, dpdy) {1}2D{2}_GRAD(textureName,{0}coord2, dpdx, dpdy)",

            "#define ASE_SAMPLE_TEXTURE3D(textureName,{0}coord3) {1}3D{2}(textureName,{0}coord3)",
            "#define ASE_SAMPLE_TEXTURE3D_LOD(textureName,{0}coord3, lod) {1}3D{2}_LOD(textureName,{0}coord3, lod)",
            "#define ASE_SAMPLE_TEXTURE3D_BIAS(textureName,{0}coord3, bias) {1}3D{2}_BIAS(textureName,{0}coord3, bias)",
            "#define ASE_SAMPLE_TEXTURE3D_GRAD(textureName,{0}coord3, dpdx, dpdy) {1}3D{2}_GRAD(textureName,{0}coord3, dpdx, dpdy)",

            "#define ASE_SAMPLE_TEXTURECUBE(textureName,{0}coord3) {1}CUBE{2}(textureName,{0}coord3)",
            "#define ASE_SAMPLE_TEXTURECUBE_LOD(textureName,{0}coord3, lod) {1}CUBE{2}_LOD(textureName,{0}coord3, lod)",
            "#define ASE_SAMPLE_TEXTURECUBE_BIAS(textureName,{0}coord3, bias) {1}CUBE{2}_BIAS(textureName,{0}coord3, bias)\n"
        };*/

        // SRP
        /*public readonly static string[] CustomASESRPArgsMacros =
        {
            "#define ASE_TEXTURE2D_ARGS(textureName) TEXTURE2D(textureName), SAMPLER(textureName)",
            "#define ASE_TEXTURE3D_ARGS(textureName) TEXTURE3D(textureName), SAMPLER(textureName)",
            "#define ASE_TEXTURECUBE_ARGS(textureName) TEXTURECUBE(textureName), SAMPLER(textureName)",
            "#define ASE_TEXTURE2D_PARAMS(textureName) textureName, sampler##textureName",
            "#define ASE_TEXTURE3D_PARAMS(textureName) textureName, sampler##textureName",
            "#define ASE_TEXTURECUBE_PARAMS(textureName) textureName, sampler##textureName",
            "#define ASE_TEXTURE2D_ARRAY_PARAMS(textureName) textureName, sampler##textureName\n"
        };*/

        public readonly static RenderTextureFormat PreviewFormat = RenderTextureFormat.ARGBFloat;
        public static readonly int PreviewSize = 128;

        public static readonly List<string> UnityNativeInspectors = new List<string>
        {
            "Rendering.HighDefinition.LightingShaderGraphGUI",
            "Rendering.HighDefinition.HDUnlitGUI",
            "UnityEditor.Rendering.HighDefinition.HDLitGUI",
            "UnityEditor.ShaderGraph.PBRMasterGUI",
            "UnityEditor.Rendering.HighDefinition.DecalGUI",
            "UnityEditor.Rendering.HighDefinition.FabricGUI",
            "UnityEditor.Experimental.Rendering.HDPipeline.HDLitGUI",
            "Rendering.HighDefinition.DecalGUI",
            "Rendering.HighDefinition.LitShaderGraphGUI",
            "Rendering.HighDefinition.DecalShaderGraphGUI",
            "UnityEditor.ShaderGraphUnlitGUI",
            "UnityEditor.ShaderGraphLitGUI",
            "UnityEditor.Rendering.Universal.DecalShaderGraphGUI",
        };

        public static readonly Dictionary<string, string> CustomInspectorHD7To10 = new Dictionary<
            string,
            string
        >
        {
            {
                "UnityEditor.Rendering.HighDefinition.DecalGUI",
                "Rendering.HighDefinition.DecalGUI"
            },
            {
                "UnityEditor.Rendering.HighDefinition.FabricGUI",
                "Rendering.HighDefinition.LightingShaderGraphGUI"
            },
            {
                "UnityEditor.Rendering.HighDefinition.HDLitGUI",
                "Rendering.HighDefinition.LitShaderGraphGUI"
            },
            {
                "UnityEditor.Experimental.Rendering.HDPipeline.HDLitGUI",
                "Rendering.HighDefinition.LitShaderGraphGUI"
            },
        };

        public static readonly Dictionary<string, string> CustomInspectorURP10To12 = new Dictionary<
            string,
            string
        >
        {
            { "UnityEditor.ShaderGraph.PBRMasterGUI", "UnityEditor.ShaderGraphLitGUI" },
        };

        public static readonly Dictionary<string, string> CustomInspectorHDLegacyTo11 =
            new Dictionary<string, string>
            {
                {
                    "UnityEditor.Rendering.HighDefinition.DecalGUI",
                    "Rendering.HighDefinition.DecalShaderGraphGUI"
                },
                {
                    "Rendering.HighDefinition.DecalGUI",
                    "Rendering.HighDefinition.DecalShaderGraphGUI"
                },
                {
                    "UnityEditor.Rendering.HighDefinition.FabricGUI",
                    "Rendering.HighDefinition.LightingShaderGraphGUI"
                },
                {
                    "UnityEditor.Rendering.HighDefinition.HDLitGUI",
                    "Rendering.HighDefinition.LitShaderGraphGUI"
                },
                {
                    "UnityEditor.Experimental.Rendering.HDPipeline.HDLitGUI",
                    "Rendering.HighDefinition.LitShaderGraphGUI"
                },
            };

        public static readonly string CustomASEStandardSamplerParams =
            "#define ASE_TEXTURE_PARAMS(textureName) textureName\n";
        public static readonly string[] CustomASESRPTextureArrayMacros =
        {
            "#define ASE_TEXTURE2D_ARRAY_ARGS(textureName) TEXTURE2D_ARRAY_ARGS(textureName,sampler##textureName)\n",
            "#define ASE_TEXTURE2D_ARRAY_PARAM(textureName) TEXTURE2D_ARRAY_PARAM(textureName,sampler##textureName)\n",
            "#define ASE_SAMPLE_TEXTURE2D_ARRAY(textureName, coord3) textureName.Sample(sampler##textureName, coord3)",
            "#define ASE_SAMPLE_TEXTURE2D_ARRAY_LOD(textureName, coord3, lod) textureName.SampleLevel(sampler##textureName, coord3, lod)",
        };
        public static readonly string CustomASESRPSamplerParams =
            "#define ASE_TEXTURE_PARAMS(textureName) textureName, sampler##textureName\n";

        public static readonly string[] CustomSRPSamplingMacros =
        {
            "#if defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(UNITY_COMPILER_HLSLCC) || defined(SHADER_API_PSSL) || (defined(SHADER_TARGET_SURFACE_ANALYSIS) && !defined(SHADER_TARGET_SURFACE_ANALYSIS_MOJOSHADER))//3D SRP MACROS",
            "#define SAMPLE_TEXTURE3D_GRAD(textureName, samplerName, coord3, dpdx, dpdy) textureName.SampleGrad(samplerName, coord3, dpdx, dpdy)",
            "#define SAMPLE_TEXTURE3D_BIAS(textureName, samplerName, coord3, bias) textureName.SampleBias(samplerName, coord3, bias)",
            "#else//3D SRP MACROS",
            "#define SAMPLE_TEXTURE3D_GRAD(textureName, samplerName, coord3, dpdx, dpdy) SAMPLE_TEXTURE3D(textureName, samplerName, coord3)",
            "#define SAMPLE_TEXTURE3D_BIAS(textureName, samplerName, coord3, bias) SAMPLE_TEXTURE3D(textureName, samplerName, coord3)",
            "#endif//3D SRP MACROS\n",
        };

        public static readonly Dictionary<TextureType, string> TexDeclarationSRPMacros =
            new Dictionary<TextureType, string>
            {
                { TextureType.Texture2D, "TEXTURE2D({0}); SAMPLER(sampler{0});" },
                { TextureType.Texture3D, "TEXTURE3D({0}); SAMPLER(sampler{0});" },
                { TextureType.Cube, "TEXTURECUBE({0}); SAMPLER(sampler{0});" },
                { TextureType.Texture2DArray, "TEXTURE2D_ARRAY({0}); SAMPLER(sampler{0});" },
            };

        public static readonly Dictionary<TextureType, string> SamplerDeclarationSRPMacros =
            new Dictionary<TextureType, string>
            {
                { TextureType.Texture2D, "SAMPLER(sampler{0});" },
                { TextureType.Texture3D, "SAMPLER(sampler{0});" },
                { TextureType.Cube, "SAMPLER(sampler{0});" },
                { TextureType.Texture2DArray, "SAMPLER(sampler{0});" },
            };

        public static readonly Dictionary<TextureType, string> TexDeclarationNoSamplerSRPMacros =
            new Dictionary<TextureType, string>
            {
                { TextureType.Texture2D, "TEXTURE2D({0})" },
                { TextureType.Texture3D, "TEXTURE3D({0})" },
                { TextureType.Cube, "TEXTURECUBE({0})" },
                { TextureType.Texture2DArray, "TEXTURE2D_ARRAY({0})" },
            };

        public static readonly Dictionary<TextureType, string> TexSampleSRPMacros = new Dictionary<
            TextureType,
            string
        >
        {
            { TextureType.Texture2D, "SAMPLE_TEXTURE2D{0}( {1}, {2}, {3} )" },
            { TextureType.Texture3D, "SAMPLE_TEXTURE3D{0}( {1}, {2}, {3} )" },
            { TextureType.Cube, "SAMPLE_TEXTURECUBE{0}( {1}, {2}, {3} )" },
            { TextureType.Texture2DArray, "SAMPLE_TEXTURE2D_ARRAY{0}( {1}, {2}, {3} )" },
        };

        public static readonly Dictionary<TextureType, string> TexParams = new Dictionary<
            TextureType,
            string
        >
        {
            { TextureType.Texture2D, "ASE_TEXTURE2D_PARAMS({0})" },
            { TextureType.Texture3D, "ASE_TEXTURE3D_PARAMS({0})" },
            { TextureType.Cube, "ASE_TEXTURECUBE_PARAMS({0})" },
            { TextureType.Texture2DArray, "ASE_TEXTURE2D_ARRAY_PARAMS({0})" },
        };

        public static readonly Dictionary<WirePortDataType, TextureType> WireToTexture =
            new Dictionary<WirePortDataType, TextureType>
            {
                { WirePortDataType.SAMPLER1D, TextureType.Texture1D },
                { WirePortDataType.SAMPLER2D, TextureType.Texture2D },
                { WirePortDataType.SAMPLER3D, TextureType.Texture3D },
                { WirePortDataType.SAMPLERCUBE, TextureType.Cube },
                { WirePortDataType.SAMPLER2DARRAY, TextureType.Texture2DArray },
            };

        public static readonly Dictionary<TextureType, WirePortDataType> TextureToWire =
            new Dictionary<TextureType, WirePortDataType>
            {
                { TextureType.Texture1D, WirePortDataType.SAMPLER1D },
                { TextureType.Texture2D, WirePortDataType.SAMPLER2D },
                { TextureType.Texture3D, WirePortDataType.SAMPLER3D },
                { TextureType.Cube, WirePortDataType.SAMPLERCUBE },
                { TextureType.Texture2DArray, WirePortDataType.SAMPLER2DARRAY },
                { TextureType.ProceduralTexture, WirePortDataType.SAMPLER2D },
            };

        public static readonly string SamplingMacrosDirective =
            "#define ASE_USING_SAMPLING_MACROS 1";

        // STANDARD
        public readonly static string[] CustomASEStandarSamplingMacrosHelper =
        {
            "#if defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(UNITY_COMPILER_HLSLCC) || defined(SHADER_API_PSSL) || (defined(SHADER_TARGET_SURFACE_ANALYSIS) && !defined(SHADER_TARGET_SURFACE_ANALYSIS_MOJOSHADER))//ASE Sampler Macros",
            "#else//ASE Sampling Macros",
            "#endif//ASE Sampling Macros\n",
        };

        public static readonly string[] CustomASEArraySamplingMacrosRecent =
        {
            "#define UNITY_SAMPLE_TEX2DARRAY(tex,coord) tex.Sample(sampler##tex,coord)",
            "#define UNITY_SAMPLE_TEX2DARRAY_LOD(tex,coord,lod) tex.SampleLevel(sampler##tex,coord, lod)",
            "#define UNITY_SAMPLE_TEX2DARRAY_BIAS(tex,coord,bias) tex.SampleBias(sampler##tex,coord,bias)",
            "#define UNITY_SAMPLE_TEX2DARRAY_GRAD(tex,coord,ddx,ddy) tex.SampleGrad(sampler##tex,coord,ddx,ddy)",
        };

        public static readonly string[] CustomASEArraySamplingMacrosOlder =
        {
            "#define UNITY_SAMPLE_TEX2DARRAY(tex,coord) tex2DArray(tex,coord)",
            "#define UNITY_SAMPLE_TEX2DARRAY_LOD(tex,coord,lod) tex2DArraylod(tex, float4(coord,lod))",
            "#define UNITY_SAMPLE_TEX2DARRAY_BIAS(tex,coord,bias) tex2DArray(tex,coord)",
            "#define UNITY_SAMPLE_TEX2DARRAY_GRAD(tex,coord,ddx,ddy) tex2DArray(tex,coord)",
        };

        public static readonly string[] CustomASEStandarSamplingMacrosRecent =
        {
            "#define SAMPLE_TEXTURE2D(tex,samplerTex,coord) tex.Sample(samplerTex,coord)",
            "#define SAMPLE_TEXTURE2D_LOD(tex,samplerTex,coord,lod) tex.SampleLevel(samplerTex,coord, lod)",
            "#define SAMPLE_TEXTURE2D_BIAS(tex,samplerTex,coord,bias) tex.SampleBias(samplerTex,coord,bias)",
            "#define SAMPLE_TEXTURE2D_GRAD(tex,samplerTex,coord,ddx,ddy) tex.SampleGrad(samplerTex,coord,ddx,ddy)",
            "#define SAMPLE_TEXTURE3D(tex,samplerTex,coord) tex.Sample(samplerTex,coord)",
            "#define SAMPLE_TEXTURE3D_LOD(tex,samplerTex,coord,lod) tex.SampleLevel(samplerTex,coord, lod)",
            "#define SAMPLE_TEXTURE3D_BIAS(tex,samplerTex,coord,bias) tex.SampleBias(samplerTex,coord,bias)",
            "#define SAMPLE_TEXTURE3D_GRAD(tex,samplerTex,coord,ddx,ddy) tex.SampleGrad(samplerTex,coord,ddx,ddy)",
            "#define SAMPLE_TEXTURECUBE(tex,samplerTex,coord) tex.Sample(samplerTex,coord)",
            "#define SAMPLE_TEXTURECUBE_LOD(tex,samplerTex,coord,lod) tex.SampleLevel(samplerTex,coord, lod)",
            "#define SAMPLE_TEXTURECUBE_BIAS(tex,samplerTex,coord,bias) tex.SampleBias(samplerTex,coord,bias)",
            "#define SAMPLE_TEXTURECUBE_GRAD(tex,samplerTex,coord,ddx,ddy) tex.SampleGrad(samplerTex,coord,ddx,ddy)",
            "#define SAMPLE_TEXTURE2D_ARRAY(tex,samplerTex,coord) tex.Sample(samplerTex,coord)",
            "#define SAMPLE_TEXTURE2D_ARRAY_LOD(tex,samplerTex,coord,lod) tex.SampleLevel(samplerTex,coord, lod)",
            "#define SAMPLE_TEXTURE2D_ARRAY_BIAS(tex,samplerTex,coord,bias) tex.SampleBias(samplerTex,coord,bias)",
            "#define SAMPLE_TEXTURE2D_ARRAY_GRAD(tex,samplerTex,coord,ddx,ddy) tex.SampleGrad(samplerTex,coord,ddx,ddy)",
        };

        public static readonly string[] CustomASEStandarSamplingMacrosOlder =
        {
            "#define SAMPLE_TEXTURE2D(tex,samplerTex,coord) tex2D(tex,coord)",
            "#define SAMPLE_TEXTURE2D_LOD(tex,samplerTex,coord,lod) tex2Dlod(tex,float4(coord,0,lod))",
            "#define SAMPLE_TEXTURE2D_BIAS(tex,samplerTex,coord,bias) tex2Dbias(tex,float4(coord,0,bias))",
            "#define SAMPLE_TEXTURE2D_GRAD(tex,samplerTex,coord,ddx,ddy) tex2Dgrad(tex,coord,ddx,ddy)",
            "#define SAMPLE_TEXTURE3D(tex,samplerTex,coord) tex3D(tex,coord)",
            "#define SAMPLE_TEXTURE3D_LOD(tex,samplerTex,coord,lod) tex3Dlod(tex,float4(coord,lod))",
            "#define SAMPLE_TEXTURE3D_BIAS(tex,samplerTex,coord,bias) tex3D(tex,coord)",
            "#define SAMPLE_TEXTURE3D_GRAD(tex,samplerTex,coord,ddx,ddy) tex3D(tex,coord)",
            "#define SAMPLE_TEXTURECUBE(tex,samplertex,coord) texCUBE(tex,coord)",
            "#define SAMPLE_TEXTURECUBE_LOD(tex,samplertex,coord,lod) texCUBElod (tex,half4(coord,lod))",
            "#define SAMPLE_TEXTURECUBE_BIAS(tex,samplertex,coord,bias) texCUBE(tex,coord)",
            "#define SAMPLE_TEXTURECUBE_GRAD(tex,samplertex,coord,ddx,ddy) texCUBE(tex,coord)",
            "#define SAMPLE_TEXTURE2D_ARRAY(tex,samplertex,coord) tex2DArray(tex,coord)",
            "#define SAMPLE_TEXTURE2D_ARRAY_LOD(tex,samplertex,coord,lod) tex2DArraylod(tex, float4(coord,lod))",
            "#define SAMPLE_TEXTURE2D_ARRAY_BIAS(tex,samplerTex,coord,bias) tex2DArray(tex,coord)",
            "#define SAMPLE_TEXTURE2D_ARRAY_GRAD(tex,samplerTex,coord,ddx,ddy) tex2DArray(tex,coord)",
        };

        public static readonly string[] CustomArraySamplingMacros =
        {
            "#if defined(UNITY_COMPILER_HLSL2GLSL) || defined(SHADER_TARGET_SURFACE_ANALYSIS)//ASE Array Sampler Macros",
            "#define ASE_SAMPLE_TEX2DARRAY_GRAD(tex,coord,dx,dy) UNITY_SAMPLE_TEX2DARRAY (tex,coord)",
            "#else//ASE Array Sampler Macros",
            "#define ASE_SAMPLE_TEX2DARRAY_GRAD(tex,coord,dx,dy) tex.SampleGrad (sampler##tex,coord,dx,dy)",
            "#endif//ASE Array Sampler Macros\n",
        };

        public static readonly Dictionary<TextureType, string> TexDeclarationStandardMacros =
            new Dictionary<TextureType, string>
            {
                { TextureType.Texture2D, "UNITY_DECLARE_TEX2D({0});" },
                { TextureType.Texture3D, "UNITY_DECLARE_TEX3D({0});" },
                { TextureType.Cube, "UNITY_DECLARE_TEXCUBE({0});" },
                { TextureType.Texture2DArray, "UNITY_DECLARE_TEX2DARRAY({0});" },
            };

        public static readonly Dictionary<
            TextureType,
            string
        > TexDeclarationNoSamplerStandardMacros = new Dictionary<TextureType, string>
        {
            { TextureType.Texture2D, "UNITY_DECLARE_TEX2D_NOSAMPLER({0})" },
            { TextureType.Texture3D, "UNITY_DECLARE_TEX3D_NOSAMPLER({0})" },
            { TextureType.Cube, "UNITY_DECLARE_TEXCUBE_NOSAMPLER({0})" },
            { TextureType.Texture2DArray, "UNITY_DECLARE_TEX2DARRAY_NOSAMPLER({0})" },
        };

        public static readonly Dictionary<TextureType, string> TexSampleStandardMacros =
            new Dictionary<TextureType, string>
            {
                { TextureType.Texture2D, "UNITY_SAMPLE_TEX2D{0}( {1}, {3} )" },
                { TextureType.Texture3D, "UNITY_SAMPLE_TEX3D{0}( {1}, {3} )" },
                { TextureType.Cube, "UNITY_SAMPLE_TEXCUBE{0}( {1}, {3} )" },
                { TextureType.Texture2DArray, "UNITY_SAMPLE_TEX2DARRAY{0}( {1}, {3} )" },
            };

        public static readonly Dictionary<TextureType, string> TexSampleSamplerStandardMacros =
            new Dictionary<TextureType, string>
            {
                { TextureType.Texture2D, "SAMPLE_TEXTURE2D{0}( {1}, {2}, {3} )" },
                { TextureType.Texture3D, "SAMPLE_TEXTURE3D{0}( {1}, {2}, {3} )" },
                { TextureType.Cube, "SAMPLE_TEXTURECUBE{0}( {1}, {2}, {3} )" },
                { TextureType.Texture2DArray, "SAMPLE_TEXTURE2D_ARRAY{0}( {1}, {2}, {3} )" },
            };

        public static readonly Dictionary<TextureType, string> TexSampleStandard = new Dictionary<
            TextureType,
            string
        >
        {
            { TextureType.Texture2D, "tex2D{0}( {1}, {2} )" },
            { TextureType.Texture3D, "tex3D{0}( {1}, {2} )" },
            { TextureType.Cube, "texCUBE{0}( {1}, {2} )" },
            { TextureType.Texture2DArray, "tex2DArray{0}( {1}, {2} )" },
        };
        public static readonly char LineFeedSeparator = '$';
        public static readonly char SemiColonSeparator = '@';
        public static readonly string AppDataFullName = "appdata_full";
        public static readonly string CustomAppDataFullName = "appdata_full_custom";
        public static readonly string CustomAppDataFullBody =
            "\n\t\tstruct appdata_full_custom\n"
            + "\t\t{\n"
            + "\t\t\tfloat4 vertex : POSITION;\n"
            + "\t\t\tfloat4 tangent : TANGENT;\n"
            + "\t\t\tfloat3 normal : NORMAL;\n"
            + "\t\t\tfloat4 texcoord : TEXCOORD0;\n"
            + "\t\t\tfloat4 texcoord1 : TEXCOORD1;\n"
            + "\t\t\tfloat4 texcoord2 : TEXCOORD2;\n"
            + "\t\t\tfloat4 texcoord3 : TEXCOORD3;\n"
            + "\t\t\tfloat4 color : COLOR;\n"
            + "\t\t\tUNITY_VERTEX_INPUT_INSTANCE_ID\n";

        public static readonly string IncludeFormat = "#include \"{0}\"";
        public static readonly string PragmaFormat = "#pragma {0}";
        public static readonly string DefineFormat = "#define {0}";

        public static readonly string RenderTypeHelperStr = "RenderType";
        public static readonly string RenderQueueHelperStr = "Queue";
        public static readonly string DisableBatchingHelperStr = "DisableBatching";

        public static readonly string DefaultShaderName = "New Amplify Shader";

        public static readonly string UndoReplaceMasterNodeId = "Replacing Master Node";
        public static readonly string UnityLightingLib = "Lighting.cginc";
        public static readonly string UnityAutoLightLib = "AutoLight.cginc";
        public static readonly string UnityBRDFLib = "UnityStandardBRDF.cginc";
        public static readonly string LocalValueDecWithoutIdent = "{0} {1} = {2};";
        public static readonly string CustomTypeLocalValueDecWithoutIdent = "{0} {1} =({0}){2};";
        public static readonly string LocalValueDefWithoutIdent = "{0} {1} {2};";
        public static readonly string TilingOffsetFormat = "{0} * {1} + {2}";
        public static string InvalidPostProcessDatapath = "__DELETED_GUID_Trash";

        //TEMPLATES

        public static float PlusMinusButtonLayoutWidth = 15;

        public static float NodeButtonSizeX = 16;
        public static float NodeButtonSizeY = 16;
        public static float NodeButtonDeltaX = 5;
        public static float NodeButtonDeltaY = 11;

        public static readonly string SafeNormalizeInfoStr =
            "With Safe Normalize division by 0 is prevented over the normalize operation at the expense of additional instructions on shader.";
        public static readonly string ReservedPropertyNameStr =
            "Property name '{0}' is reserved and cannot be used";
        public static readonly string NumericPropertyNameStr =
            "Property name '{0}' is numeric thus cannot be used";
        public static readonly string DeprecatedMessageStr =
            "Node '{0}' is deprecated. Use node '{1}' instead.";
        public static readonly string DeprecatedNoAlternativeMessageStr =
            "Node '{0}' is deprecated and should be removed.";
        public static readonly string UndoChangePropertyTypeNodesId = "Changing Property Types";
        public static readonly string UndoChangeTypeNodesId = "Changing Nodes Types";
        public static readonly string UndoMoveNodesId = "Moving Nodes";
        public static readonly string UndoRegisterFullGrapId = "Register Graph";
        public static readonly string UndoAddNodeToCommentaryId = "Add node to Commentary";
        public static readonly string UndoRemoveNodeFromCommentaryId =
            "Remove node from Commentary";
        public static readonly string UndoCreateDynamicPortId = "Create Dynamic Port";
        public static readonly string UndoDeleteDynamicPortId = "Destroy Dynamic Port";
        public static readonly string UndoRegisterNodeId = "Register Object";
        public static readonly string UndoUnregisterNodeId = "Unregister Object";
        public static readonly string UndoCreateNodeId = "Create Object";
        public static readonly string UndoPasteNodeId = "Paste Object";
        public static readonly string UndoDeleteNodeId = "Destroy Object";
        public static readonly string UndoDeleteConnectionId = "Destroy Connection";
        public static readonly string UndoCreateConnectionId = "Create Connection";

        public static readonly float MenuDragSpeed = -0.5f;
        public static readonly string DefaultCustomInspector =
            "AmplifyShaderEditor.MaterialInspector";
        public static readonly string OldCustomInspector = "ASEMaterialInspector";
        public static readonly string ReferenceTypeStr = "Mode";
        public static readonly string AvailableReferenceStr = "Reference";
        public static readonly string InstancePostfixStr = " (Reference) ";

        public static readonly string ASEMenuName = "Amplify Shader";

        public static readonly string LodCrossFadeOption2017 = "dithercrossfade";

        public static readonly string UnityShaderVariables = "UnityShaderVariables.cginc";
        public static readonly string UnityCgLibFuncs = "UnityCG.cginc";
        public static readonly string UnityStandardUtilsLibFuncs = "UnityStandardUtils.cginc";
        public static readonly string UnityPBSLightingLib = "UnityPBSLighting.cginc";
        public static readonly string UnityDeferredLightLib = "UnityDeferredLibrary.cginc";
        public static readonly string ATSharedLibGUID = "ba242738c4be3324aa88d126f7cc19f9";
        public static readonly string CameraDepthTextureValue =
            "UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );";

        //public readonly static string CameraDepthTextureSRPVar = "TEXTURE2D(_CameraDepthTexture);";
        //public readonly static string CameraDepthTextureSRPSampler = "SAMPLER(sampler_CameraDepthTexture);";
        public readonly static string CameraDepthTextureLWEnabler = "REQUIRE_DEPTH_TEXTURE 1";

        public static readonly string CameraDepthTextureTexelSize =
            "uniform float4 _CameraDepthTexture_TexelSize;";
        public static readonly string InstanceIdMacro = "UNITY_VERTEX_INPUT_INSTANCE_ID";
        public static readonly string InstanceIdVariable = "UNITY_GET_INSTANCE_ID({0})";

        public static readonly string HelpURL =
            "http://wiki.amplify.pt/index.php?title=Unity_Products:Amplify_Shader_Editor";

        //public readonly static string NodeCommonUrl = "http://wiki.amplify.pt/index.php?title=Unity_Products:Amplify_Shader_Editor/Nodes#";
        //public readonly static string CommunityNodeCommonUrl = "http://wiki.amplify.pt/index.php?title=Unity_Products:Amplify_Shader_Editor/Community_Nodes#";
        public readonly static string NodeCommonUrl =
            "http://wiki.amplify.pt/index.php?title=Unity_Products:Amplify_Shader_Editor/";
        public static readonly string CommunityNodeCommonUrl =
            "http://wiki.amplify.pt/index.php?title=Unity_Products:Amplify_Shader_Editor/";
        public static readonly Color InfiniteLoopColor = Color.red;

        public static readonly Color DefaultCategoryColor = new Color(0.26f, 0.35f, 0.44f, 1.0f);
        public static readonly Color NodeBodyColor = new Color(1f, 1f, 1f, 1.0f);

        public static readonly Color ModeTextColor = new Color(1f, 1f, 1f, 0.25f);
        public static readonly Color ModeIconColor = new Color(1f, 1f, 1f, 0.75f);

        public static readonly Color PortTextColor = new Color(1f, 1f, 1f, 0.5f);
        public static readonly Color PortLockedTextColor = new Color(1f, 1f, 1f, 0.35f);
        public static readonly Color BoxSelectionColor = new Color(0.5f, 0.75f, 1f, 0.33f);
        public static readonly Color SpecialRegisterLocalVarSelectionColor = new Color(
            0.27f,
            0.52f,
            1.0f,
            1f
        );
        public static readonly Color SpecialGetLocalVarSelectionColor = new Color(
            0.2f,
            0.8f,
            0.4f,
            1f
        );
        public static readonly Color NodeSelectedColor = new Color(0.85f, 0.56f, 0f, 1f);
        public static readonly Color NodeDefaultColor = new Color(1f, 1f, 1f, 1f);
        public static readonly Color NodeConnectedColor = new Color(1.0f, 1f, 0.0f, 1f);
        public static readonly Color NodeErrorColor = new Color(1f, 0.5f, 0.5f, 1f);
        public static readonly string NoSpecifiedCategoryStr = "<None>";

        public static readonly int MINIMIZE_WINDOW_LOCK_SIZE = 630;

        public static readonly int FoldoutMouseId = 0; // Left Mouse Button

        public static readonly float SNAP_SQR_DIST = 200f;
        public static readonly int INVALID_NODE_ID = -1;
        public static readonly float WIRE_WIDTH = 7f;
        public static readonly float WIRE_CONTROL_POINT_DIST = 0.7f;
        public static readonly float WIRE_CONTROL_POINT_DIST_INV = 1.7f;

        public static readonly float IconsLeftRightMargin = 5f;
        public static readonly float PropertyPickerWidth = 16f;
        public static readonly float PropertyPickerHeight = 16f;
        public static readonly float PreviewExpanderWidth = 16f;
        public static readonly float PreviewExpanderHeight = 16f;
        public static readonly float TextFieldFontSize = 11f;
        public static readonly float DefaultFontSize = 14f;
        public static readonly float DefaultTitleFontSize = 12f;
        public static readonly float PropertiesTitleFontSize = 11f;
        public static readonly float MessageFontSize = 40f;
        public static readonly float SelectedObjectFontSize = 30f;

        public static readonly float PORT_X_ADJUST = 10;
        public static readonly float PORT_INITIAL_X = 10;

        public static readonly float PORT_INITIAL_Y = 41;
        public static readonly float INPUT_PORT_DELTA_Y = 7;
        public static readonly float PORT_TO_LABEL_SPACE_X = 4;

        public static readonly float NODE_HEADER_HEIGHT = 32;
        public static readonly float NODE_HEADER_EXTRA_HEIGHT = 0;
        public static readonly float NODE_HEADER_LEFTRIGHT_MARGIN = 10;

        public static readonly float MULTIPLE_SELECION_BOX_ALPHA = 0.5f;
        public static readonly float RMB_CLICK_DELTA_TIME = 0.1f;
        public static readonly float RMB_SCREEN_DIST = 10f;

        public static readonly float CAMERA_MAX_ZOOM = 2f;
        public static readonly float CAMERA_MIN_ZOOM = 1f;
        public static readonly float CAMERA_ZOOM_SPEED = 0.1f;
        public static readonly float ALT_CAMERA_ZOOM_SPEED = -0.05f;

        public static readonly object INVALID_VALUE = null;

        public static readonly float HORIZONTAL_TANGENT_SIZE = 100f;
        public static readonly float OUTSIDE_WIRE_MARGIN = 5f;

        public static readonly string SubTitleNameFormatStr = "Name( {0} )";
        public static readonly string SubTitleSpaceFormatStr = "Space( {0} )";
        public static readonly string SubTitleTypeFormatStr = "Type( {0} )";
        public static readonly string SubTitleModeFormatStr = "Mode( {0} )";
        public static readonly string SubTitleValueFormatStr = "Value( {0} )";
        public static readonly string SubTitleConstFormatStr = "Const( {0} )";
        public static readonly string SubTitleVarNameFormatStr = "Var( {0} )";
        public static readonly string SubTitleRefNameFormatStr = "Ref( {0} )";
        public static readonly string SubTitleCurrentFormatStr = "Current( {0} )";

        public static readonly string CodeWrapper = "( {0} )";
        public static readonly string InlineCodeWrapper = "{{\n{0}\n}}";

        public static readonly string NodesDumpFormat = "{0}:,{1},{2}\n";
        public static readonly string TagFormat = " \"{0}\" = \"{1}\"";

        public static readonly string LocalVarIdentation = "\t\t\t";
        public static readonly string SimpleLocalValueDec = LocalVarIdentation + "{0} {1};\n";

        public static readonly string LocalValueDec =
            LocalVarIdentation + LocalValueDecWithoutIdent + '\n';
        public static readonly string LocalValueDef = LocalVarIdentation + "{0} = {1};\n";
        public static readonly string CastHelper = "({0}).{1}";
        public static readonly string PropertyLocalVarDec = "{0} {1} = {0}({2});";
        public static readonly string[] UniformDec = { "uniform {0} {1};", "{0} {1};" };

        public static readonly string PropertyValueLabel = "Value( {0} )";
        public static readonly string ConstantsValueLabel = "Const( {0} )";

        public static readonly string PropertyFloatFormatLabel = "0.###";
        public static readonly string PropertyBigFloatFormatLabel = "0.###e+0";

        public static readonly string PropertyIntFormatLabel = "0";
        public static readonly string PropertyBigIntFormatLabel = "0e+0";

        public static readonly string PropertyVectorFormatLabel = "0.##";
        public static readonly string PropertyBigVectorFormatLabel = "0.##e+0";

        public static readonly string PropertyMatrixFormatLabel = "0.#";
        public static readonly string PropertyBigMatrixFormatLabel = "0.#e+0";

        public static readonly string NoPropertiesLabel = "No assigned properties";

        public static readonly string ValueLabel = "Value";
        public static readonly string DefaultValueLabel = "Default Value";
        public static readonly string MaterialValueLabel = "Material Value";
        public static readonly GUIContent DefaultValueLabelContent = new GUIContent(
            "Default Value"
        );
        public static readonly GUIContent MaterialValueLabelContent = new GUIContent(
            "Material Value"
        );

        public static readonly string InputVarStr = "i"; //"input";
        public static readonly string OutputVarStr = "o"; //"output";

        public static readonly string CustomLightOutputVarStr = "s";
        public static readonly string CustomLightStructStr = "Custom";

        public static readonly string VertexShaderOutputStr = "o";
        public static readonly string VertexShaderInputStr = "v"; //"vertexData";
        public static readonly string VertexDataFunc = "vertexDataFunc";

        public static readonly string VirtualCoordNameStr = "vcoord";

        public static readonly string VertexVecNameStr = "vertexVec";
        public static readonly string VertexVecDecStr = "float3 " + VertexVecNameStr;
        public static readonly string VertexVecVertStr =
            VertexShaderOutputStr + "." + VertexVecNameStr;

        public static readonly string NormalVecNameStr = "normalVec";
        public static readonly string NormalVecDecStr = "float3 " + NormalVecNameStr;
        public static readonly string NormalVecFragStr = InputVarStr + "." + NormalVecNameStr;
        public static readonly string NormalVecVertStr =
            VertexShaderOutputStr + "." + NormalVecNameStr;

        public static readonly string IncidentVecNameStr = "incidentVec";
        public static readonly string IncidentVecDecStr = "float3 " + IncidentVecNameStr;
        public static readonly string IncidentVecDefStr =
            VertexShaderOutputStr
            + "."
            + IncidentVecNameStr
            + " = normalize( "
            + VertexVecNameStr
            + " - _WorldSpaceCameraPos.xyz)";
        public static readonly string IncidentVecFragStr = InputVarStr + "." + IncidentVecNameStr;
        public static readonly string IncidentVecVertStr =
            VertexShaderOutputStr + "." + IncidentVecNameStr;
        public static readonly string WorldNormalLocalDecStr =
            "WorldNormalVector( " + Constants.InputVarStr + " , {0}( 0,0,1 ))";

        public static readonly string IsFrontFacingVariable = "ASEIsFrontFacing";
        public static readonly string IsFrontFacingInput =
            "half ASEIsFrontFacing : SV_IsFrontFacing";
        public static readonly string IsFrontFacingInputVFACE = "half ASEIsFrontFacing : VFACE";

        public static readonly string ColorVariable = "vertexColor";
        public static readonly string ColorInput = "float4 vertexColor : COLOR";

        public static readonly string NoStringValue = "None";
        public static readonly string EmptyPortValue = "  ";

        public static readonly string[] OverallInvalidChars =
        {
            "\r",
            "\n",
            "\\",
            " ",
            ".",
            ">",
            ",",
            "<",
            "\'",
            "\"",
            ";",
            ":",
            "[",
            "{",
            "]",
            "}",
            "|",
            "=",
            "+",
            "`",
            "~",
            "/",
            "?",
            "!",
            "@",
            "#",
            "$",
            "%",
            "^",
            "&",
            "*",
            "(",
            ")",
            "-",
        };
        public static readonly string[] RegisterInvalidChars =
        {
            "\r",
            "\n",
            "\\",
            ".",
            ">",
            ",",
            "<",
            "\'",
            "\"",
            ";",
            ":",
            "[",
            "{",
            "]",
            "}",
            "|",
            "=",
            "+",
            "`",
            "~",
            "?",
            "!",
            "@",
            "#",
            "$",
            "%",
            "^",
            "&",
            "*",
            "(",
            ")",
            "-",
        };
        public static readonly string[] ShaderInvalidChars = { "\r", "\n", "\\", "\'", "\"" };
        public static readonly string[] EnumInvalidChars =
        {
            "\r",
            "\n",
            "\\",
            ".",
            ">",
            ",",
            "<",
            "\'",
            "\"",
            ";",
            ":",
            "[",
            "{",
            "]",
            "}",
            "=",
            "+",
            "`",
            "~",
            "/",
            "?",
            "!",
            "@",
            "#",
            "$",
            "%",
            "^",
            "&",
            "*",
            "(",
            ")",
            "-",
        };
        public static readonly string[] AttrInvalidChars =
        {
            "\r",
            "\n",
            "\\",
            ">",
            "<",
            "\'",
            "\"",
            ";",
            ":",
            "[",
            "{",
            "]",
            "}",
            "=",
            "+",
            "`",
            "~",
            "/",
            "?",
            "!",
            "@",
            "#",
            "$",
            "%",
            "^",
            "&",
            "*",
        };
        public static readonly string[] HeaderInvalidChars =
        {
            "\r",
            "\n",
            "\\",
            ">",
            ",",
            "<",
            "\'",
            "\"",
            ";",
            ":",
            "[",
            "{",
            "]",
            "}",
            "=",
            "+",
            "`",
            "~",
            "/",
            "?",
            "!",
            "@",
            "#",
            "$",
            "%",
            "^",
            "&",
            "*",
            "(",
            ")",
            "-",
        };

        public static readonly string[] WikiInvalidChars =
        {
            "#",
            "<",
            ">",
            "[",
            "]",
            "|",
            "{",
            "}",
            "%",
            "+",
            "?",
            "\\",
            "/",
            ",",
            ";",
            ".",
        };

        public static readonly string[,] UrlReplacementStringValues =
        {
            { " = ", "Equals" },
            { " == ", "Equals" },
            { " != ", "NotEqual" },
            { " \u2260 ", "NotEqual" },
            { " > ", "Greater" },
            { " \u2265 ", "GreaterOrEqual" },
            { " >= ", "GreaterOrEqual" },
            { " < ", "Less" },
            { " \u2264 ", "LessOrEqual" },
            { " <= ", "LessOrEqual" },
            { " ", "_" },
            { "[", string.Empty },
            { "]", string.Empty },
        };

        public static readonly int UrlReplacementStringValuesLen =
            UrlReplacementStringValues.Length / 2;

        public static readonly string[,] ReplacementStringValues =
        {
            { " = ", "Equals" },
            { " == ", "Equals" },
            { " != ", "NotEqual" },
            { " \u2260 ", "NotEqual" },
            { " > ", "Greater" },
            { " \u2265 ", "GreaterOrEqual" },
            { " >= ", "GreaterOrEqual" },
            { " < ", "Less" },
            { " \u2264 ", "LessOrEqual" },
            { " <= ", "LessOrEqual" },
        };
        public static readonly int ReplacementStringValuesLen = ReplacementStringValues.Length / 2;

        public static readonly string InternalData = "INTERNAL_DATA";

        public static readonly string NoMaterialStr = "None";

        public static readonly string OptionalParametersSep = " ";

        public static readonly string NodeUndoId = "NODE_UNDO_ID";
        public static readonly string NodeCreateUndoId = "NODE_CREATE_UNDO_ID";
        public static readonly string NodeDestroyUndoId = "NODE_DESTROY_UNDO_ID";

        // Custom node tags
        //[InPortBegin:Id:Type:Name:InPortEnd]
        public readonly static string CNIP = "#IP";

        public static readonly float FLOAT_DRAW_HEIGHT_FIELD_SIZE = 16f;
        public static readonly float FLOAT_DRAW_WIDTH_FIELD_SIZE = 45f;
        public static readonly float FLOAT_WIDTH_SPACING = 3f;

        public static readonly Color LockedPortColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

        public static readonly int[] AvailableUVChannels = { 0, 1, 2, 3, 4, 5, 6, 7 };
        public static readonly string[] AvailableUVChannelsStr =
        {
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
        };
        public static readonly string AvailableUVChannelLabel = "UV Channel";

        public static readonly int[] AvailableUVSets = { 0, 1, 2, 3, 4, 5, 6, 7 };
        public static readonly string[] AvailableUVSetsStr =
        {
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
        };
        public static readonly string AvailableUVSetsLabel = "UV Set";

        public static readonly int[] AvailableUVSizes = { 2, 3, 4 };
        public static readonly string[] AvailableUVSizesStr = { "Float 2", "Float 3", "Float 4" };
        public static readonly string AvailableUVSizesLabel = "Coord Size";

        public static readonly string LineSeparator = "________________________________";

        public static readonly Vector2 CopyPasteDeltaPos = new Vector2(40, 40);

        public static readonly string[] VectorSuffixes = { ".x", ".y", ".z", ".w" };
        public static readonly string[] ColorSuffixes = { ".r", ".g", ".b", ".a" };

        public const string InternalDataLabelStr = "Internal Data";
        public const string AttributesLaberStr = "Attributes";
        public const string ParameterLabelStr = "Parameters";

        public static readonly string[] ReferenceArrayLabels = { "Object", "Reference" };

        public static readonly string[] ChannelNamesVector = { "X", "Y", "Z", "W" };
        public static readonly string[] ChannelNamesColor = { "R", "G", "B", "A" };

        public static readonly string SamplerFormat = "sampler{0}";
        public static readonly string SamplerDeclFormat = "SamplerState {0}";
        public static readonly string SamplerDeclSRPFormat = "SAMPLER({0})";
    }
}
