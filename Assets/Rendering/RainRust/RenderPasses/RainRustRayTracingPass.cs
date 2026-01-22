using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace RainRust.Rendering
{
    public class RainRustRayTracingPass : ScriptableRenderPass
    {
        public RainRustRayTracingPass()
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
        }

        class RainRustRayTracingPassData
        {
            internal Material material;
            internal TextureHandle mainRtHandle;
            internal TextureHandle distanceRtHandle;
            internal TextureHandle distanceTex;
            internal Texture noiseTextureHandle;
            internal Vector2 offset;
            internal Vector4 aspect;
            internal Vector4 scale;
            internal Vector4 noiseTilingOffset;
            internal int rayCount;
            internal float intensity;
            internal float power;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (m_RayTracingMaterial == null)
            {
                var shader = Shader.Find(k_RayTracingShaderName);
                if (shader == null)
                {
                    Debug.LogError($"Shader not found: {k_RayTracingShaderName}");
                    return;
                }
                m_RayTracingMaterial = CoreUtils.CreateEngineMaterial(shader);
            }

            var rainRustContextData = frameData.Get<RainRustContextData>();

            var desc = renderGraph.GetTextureDesc(rainRustContextData.jfaRt.Current());
            int width = desc.width;
            int height = desc.height;

            Vector2 aspect = new(1f, (float)height / width);

            using (
                var builder = renderGraph.AddRasterRenderPass<RainRustRayTracingPassData>(
                    "RainRust Ray Tracing Pass",
                    out var passData
                )
            )
            {
                builder.AllowPassCulling(false);

                TextureHandle disResult = rainRustContextData.distanceRt;

                passData.material = m_RayTracingMaterial;
                passData.distanceTex = disResult;
                passData.offset = Vector2.zero;
                passData.aspect = aspect;

                builder.UseTexture(disResult, AccessFlags.Read);
                builder.SetRenderAttachment(rainRustContextData.lightingRt, 0, AccessFlags.Write);

                builder.SetRenderFunc(
                    static (RainRustRayTracingPassData data, RasterGraphContext context) =>
                    {
                        data.material.SetVector("_Offset", data.offset);
                        data.material.SetVector("_Aspect", data.aspect);
                        data.material.SetTexture("_JfaTex", data.distanceTex);

                        // Full screen blit
                        Blitter.BlitTexture(
                            context.cmd,
                            data.distanceTex,
                            new Vector4(1, 1, 0, 0),
                            data.material,
                            0
                        );
                    }
                );
            }
        }

        private Material m_RayTracingMaterial;
        private const string k_RayTracingShaderName = "Hidden/RainRust/RayTracing";
    }
}
