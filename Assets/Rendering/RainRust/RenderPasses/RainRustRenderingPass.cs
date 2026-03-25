using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;
using static UnityEngine.Rendering.RenderGraphModule.Util.RenderGraphUtils;

namespace RainRust.Rendering
{
    public class RainRustRenderingPass : ScriptableRenderPass
    {
        public RainRustRenderingPass()
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
        }

        public void Setup(RainRustLighting.RainRustLightingSettings settings)
        {
            m_Settings = settings;
        }

        class RainRustRenderingPassData
        {
            public Material material;
            public TextureHandle lightingRt;
            public TextureHandle receiverRt;
            public TextureHandle receiverDepthRt;
            public TextureHandle cameraColor;
            public RainRustLighting.BlendMode receiverBlendMode;
            public RainRustLighting.BlendMode lightingBlendMode;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            // Ensure material is created
            if (m_RenderingMaterial == null)
            {
                var shader = Shader.Find(k_RenderingShaderName);
                if (shader == null)
                {
                    Debug.LogError($"Shader not found: {k_RenderingShaderName}");
                    return;
                }
                m_RenderingMaterial = CoreUtils.CreateEngineMaterial(shader);
            }

            var rainRustContextData = frameData.Get<RainRustContextData>();
            var resourceData = frameData.Get<UniversalResourceData>();

            var cameraColor = resourceData.cameraColor;
            var desc = renderGraph.GetTextureDesc(cameraColor);
            desc.name = "RainRust Composition Temp";
            desc.clearBuffer = false;
            TextureHandle tempColor = renderGraph.CreateTexture(desc);

            using (
                var builder = renderGraph.AddRasterRenderPass<RainRustRenderingPassData>(
                    "Rain Rust Rendering Pass",
                    out var passData
                )
            )
            {
                passData.material = m_RenderingMaterial;
                passData.lightingRt = rainRustContextData.lightingRt;
                passData.receiverRt = rainRustContextData.receiverRt;
                passData.receiverDepthRt = rainRustContextData.mainDepthRt;
                passData.cameraColor = cameraColor;
                passData.receiverBlendMode =
                    m_Settings != null ? m_Settings.receiverBlendMode : RainRustLighting.BlendMode.AlphaBlend;
                passData.lightingBlendMode =
                    m_Settings != null ? m_Settings.lightingBlendMode : RainRustLighting.BlendMode.Additive;

                builder.UseTexture(passData.lightingRt, AccessFlags.Read);
                builder.UseTexture(passData.receiverRt, AccessFlags.Read);
                builder.UseTexture(passData.receiverDepthRt, AccessFlags.Read);
                builder.UseTexture(passData.cameraColor, AccessFlags.Read);
                builder.SetRenderAttachment(tempColor, 0, AccessFlags.Write);

                builder.SetRenderFunc(
                    static (RainRustRenderingPassData data, RasterGraphContext context) =>
                    {
                        var cmd = context.cmd;

                        data.material.SetTexture("_MainTex", data.cameraColor);
                        data.material.SetTexture("_LightingTex", data.lightingRt);
                        data.material.SetTexture("_ReceiverTex", data.receiverRt);
                        data.material.SetTexture("_ReceiverDepthTex", data.receiverDepthRt);

                        // Clear keywords
                        data.material.DisableKeyword("RECEIVER_BLEND_ADDITIVE");
                        data.material.DisableKeyword("RECEIVER_BLEND_ALPHABLEND");
                        data.material.DisableKeyword("RECEIVER_BLEND_MULTIPLY");
                        data.material.DisableKeyword("RECEIVER_BLEND_SCREEN");
                        data.material.DisableKeyword("RECEIVER_BLEND_OVERLAY");

                        data.material.DisableKeyword("LIGHTING_BLEND_ADDITIVE");
                        data.material.DisableKeyword("LIGHTING_BLEND_ALPHABLEND");
                        data.material.DisableKeyword("LIGHTING_BLEND_MULTIPLY");
                        data.material.DisableKeyword("LIGHTING_BLEND_SCREEN");
                        data.material.DisableKeyword("LIGHTING_BLEND_OVERLAY");

                        // Set receiver blend keywords
                        switch (data.receiverBlendMode)
                        {
                            case RainRustLighting.BlendMode.Additive: data.material.EnableKeyword("RECEIVER_BLEND_ADDITIVE"); break;
                            case RainRustLighting.BlendMode.AlphaBlend: data.material.EnableKeyword("RECEIVER_BLEND_ALPHABLEND"); break;
                            case RainRustLighting.BlendMode.Multiply: data.material.EnableKeyword("RECEIVER_BLEND_MULTIPLY"); break;
                            case RainRustLighting.BlendMode.Screen: data.material.EnableKeyword("RECEIVER_BLEND_SCREEN"); break;
                            case RainRustLighting.BlendMode.Overlay: data.material.EnableKeyword("RECEIVER_BLEND_OVERLAY"); break;
                        }

                        // Set lighting blend keywords
                        switch (data.lightingBlendMode)
                        {
                            case RainRustLighting.BlendMode.Additive: data.material.EnableKeyword("LIGHTING_BLEND_ADDITIVE"); break;
                            case RainRustLighting.BlendMode.AlphaBlend: data.material.EnableKeyword("LIGHTING_BLEND_ALPHABLEND"); break;
                            case RainRustLighting.BlendMode.Multiply: data.material.EnableKeyword("LIGHTING_BLEND_MULTIPLY"); break;
                            case RainRustLighting.BlendMode.Screen: data.material.EnableKeyword("LIGHTING_BLEND_SCREEN"); break;
                            case RainRustLighting.BlendMode.Overlay: data.material.EnableKeyword("LIGHTING_BLEND_OVERLAY"); break;
                        }

                        // Use a full-screen draw
                        cmd.DrawProcedural(
                            Matrix4x4.identity,
                            data.material,
                            0,
                            MeshTopology.Triangles,
                            3
                        );
                    }
                );
            }

            // Ensure blit material is available
            if (m_BlitMaterial == null)
            {
                var blitShader = Shader.Find("Hidden/Universal Render Pipeline/Blit");
                if (blitShader != null)
                {
                    m_BlitMaterial = CoreUtils.CreateEngineMaterial(blitShader);
                }
            }

            // Blit temp back to cameraColor
            if (m_BlitMaterial != null)
            {
                renderGraph.AddBlitPass(
                    new BlitMaterialParameters(tempColor, cameraColor, m_BlitMaterial, 0),
                    "Rain Rust Blit Back"
                );
            }
        }

        public void Dispose()
        {
            CoreUtils.Destroy(m_RenderingMaterial);
            CoreUtils.Destroy(m_BlitMaterial);
        }

        private Material m_RenderingMaterial;
        private Material m_BlitMaterial;
        private RainRustLighting.RainRustLightingSettings m_Settings;

        private const string k_RenderingShaderName = "Hidden/RainRust/Composition";
    }
}
