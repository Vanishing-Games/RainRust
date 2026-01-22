using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace RainRust.Rendering
{
    public class RainRustJfaInitPass : ScriptableRenderPass
    {
        private Material m_JfaInitMaterial;
        private const string k_ShaderName = "Hidden/RainRust/JfaInit";

        class PassData
        {
            public TextureHandle src;
        }

        public RainRustJfaInitPass()
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            RainRustContextData rainRustData = frameData.Get<RainRustContextData>();

            if (rainRustData == null)
                return;

            TextureHandle source = rainRustData.mainRt;
            TextureHandle destination = rainRustData.jfaRt.Current();

            if (m_JfaInitMaterial == null)
            {
                var shader = Shader.Find(k_ShaderName);
                if (shader != null)
                    m_JfaInitMaterial = CoreUtils.CreateEngineMaterial(shader);
            }

            using (
                var builder = renderGraph.AddRasterRenderPass<PassData>(
                    "RainRust JFA Init",
                    out var passData
                )
            )
            {
                builder.AllowPassCulling(false);

                passData.src = source;
                builder.UseTexture(source, AccessFlags.Read);
                builder.SetRenderAttachment(destination, 0);

                var material = m_JfaInitMaterial;

                builder.SetRenderFunc(
                    (PassData data, RasterGraphContext context) =>
                    {
                        if (material != null)
                        {
                            Blitter.BlitTexture(
                                context.cmd,
                                data.src,
                                new Vector4(1, 1, 0, 0),
                                material,
                                0
                            );
                        }
                        else
                        {
                            Blitter.BlitTexture(
                                context.cmd,
                                data.src,
                                new Vector4(1, 1, 0, 0),
                                0,
                                false
                            );
                        }
                    }
                );
            }

            // Swap after initialization so next pass reads from the initialized texture
            rainRustData.jfaRt.Swap();
        }

        public void Dispose()
        {
            CoreUtils.Destroy(m_JfaInitMaterial);
        }
    }
}
