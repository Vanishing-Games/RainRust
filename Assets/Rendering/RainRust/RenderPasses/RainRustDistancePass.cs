using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace RainRust.Rendering
{
    public class RainRustDistancePass : ScriptableRenderPass
    {
        public RainRustDistancePass()
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
        }

        class RainRustDistancePassData
        {
            public Material material;
            public TextureHandle jfaTex;
            public Vector2 offset;
            public Vector2 aspect;
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
                var builder = renderGraph.AddRasterRenderPass<RainRustDistancePassData>(
                    "RainRust Distance Pass",
                    out var passData
                )
            )
            {
                builder.AllowPassCulling(false);

                TextureHandle jfaResult = rainRustContextData.jfaRt.Previous(); // Final JFA result after all iterations

                passData.material = m_DistanceMaterial;
                passData.jfaTex = jfaResult;
                passData.offset = Vector2.zero;
                passData.aspect = aspect;

                builder.UseTexture(jfaResult, AccessFlags.Read);
                builder.SetRenderAttachment(rainRustContextData.distanceRt, 0, AccessFlags.Write);

                builder.SetRenderFunc(
                    static (RainRustDistancePassData data, RasterGraphContext context) =>
                    {
                        data.material.SetVector("_Offset", data.offset);
                        data.material.SetVector("_Aspect", data.aspect);
                        data.material.SetTexture("_JfaTex", data.jfaTex);

                        // Full screen blit
                        Blitter.BlitTexture(
                            context.cmd,
                            data.jfaTex,
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
