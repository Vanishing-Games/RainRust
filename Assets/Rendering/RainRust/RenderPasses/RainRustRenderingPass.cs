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
                var shader = Shader.Find(k_DistanceShaderName);
                if (shader == null)
                {
                    Debug.LogError($"Shader not found: {k_DistanceShaderName}");
                    return;
                }
                m_RenderingMaterial = CoreUtils.CreateEngineMaterial(shader);
            }

            var rainRustContextData = frameData.Get<RainRustContextData>();
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            var desc = renderGraph.GetTextureDesc(rainRustContextData.jfaRt.Current());
            int width = desc.width;
            int height = desc.height;

            Vector2 aspect = new(1f, (float)height / width);

            BlitMaterialParameters blitParams = new(
                rainRustContextData.lightingRt,
                resourceData.cameraColor,
                m_RenderingMaterial,
                0
            );
            renderGraph.AddBlitPass(blitParams, "Rain Rust Rendering Pass");
        }

        private Material m_RenderingMaterial;
        private const string k_DistanceShaderName = "Hidden/RainRust/Distance";
    }
}
