using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

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
            if (m_DistanceMaterial == null)
            {
                var shader = Shader.Find(k_DistanceShaderName);
                if (shader == null)
                {
                    Debug.LogError($"Shader not found: {k_DistanceShaderName}");
                    return;
                }
                m_DistanceMaterial = CoreUtils.CreateEngineMaterial(shader);
            }

            var rainRustContextData = frameData.Get<RainRustContextData>();

            var desc = renderGraph.GetTextureDesc(rainRustContextData.jfaRt.Current());
            int width = desc.width;
            int height = desc.height;

            Vector2 aspect = new(1f, (float)height / width);

            using (
                var builder = renderGraph.AddRasterRenderPass<RainRustRenderingPassData>(
                    "RainRust Rendering Pass",
                    out var passData
                )
            )
            {
                builder.AllowPassCulling(false);

                TextureHandle lightingRt = rainRustContextData.lightingRt; // Final JFA result after all iterations

                passData.material = m_DistanceMaterial;
                passData.lightingRt = lightingRt;

                builder.UseTexture(lightingRt, AccessFlags.Read);
                builder.SetRenderAttachment(rainRustContextData.distanceRt, 0, AccessFlags.Write);

                builder.SetRenderFunc(
                    static (RainRustRenderingPassData data, RasterGraphContext context) =>
                    {
                        // Full screen blit
                        Blitter.BlitTexture(
                            context.cmd,
                            data.lightingRt,
                            new Vector4(1, 1, 0, 0),
                            data.material,
                            0
                        );
                    }
                );
            }
        }

        private Material m_DistanceMaterial;
        private const string k_DistanceShaderName = "Hidden/RainRust/Distance";
    }
}
