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

        class RainRustRenderingPassData
        {
            public Material material;
            public TextureHandle lightingRt;
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

            var desc = renderGraph.GetTextureDesc(rainRustContextData.jfaRt.Current());
            int width = desc.width;
            int height = desc.height;

            Vector2 aspect = new(1f, (float)height / width);

            // Use URP Blit shader to copy lightingRt to cameraColor
            BlitMaterialParameters blitParams = new(
                rainRustContextData.lightingRt,
                resourceData.cameraColor,
                m_RenderingMaterial,
                0 // Use Pass 0 of the standard Blit shader
            );
            renderGraph.AddBlitPass(blitParams, "Rain Rust Rendering Pass");
        }

        public void Dispose()
        {
            CoreUtils.Destroy(m_RenderingMaterial);
        }

        private Material m_RenderingMaterial;
        // Use Unity's built-in URP Blit shader instead of the custom RayTracing shader
        private const string k_RenderingShaderName = "Hidden/Universal Render Pipeline/Blit";
    }
}
