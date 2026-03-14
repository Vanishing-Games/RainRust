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
            internal TextureHandle lightingRtHandle;
            internal Texture noiseTextureHandle;
            internal Vector4 aspect;
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

            var desc = renderGraph.GetTextureDesc(rainRustContextData.jfaRt.OddSource());
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
                passData.material = m_RayTracingMaterial;
                passData.mainRtHandle = rainRustContextData.mainRt;
                passData.distanceRtHandle = rainRustContextData.distanceRt;
                passData.lightingRtHandle = rainRustContextData.lightingRt;
                // passData.noiseTextureHandle = frameData.Get<UniversalResourceData>()

                builder.UseTexture(passData.mainRtHandle);
                builder.UseTexture(passData.distanceRtHandle);

                builder.SetRenderAttachment(passData.lightingRtHandle, 0, AccessFlags.Write);

                passData.material.EnableKeyword("FRAGMENT_RANDOM");
                passData.material.DisableKeyword("TEXTURE_RANDOM");
                passData.material.EnableKeyword("ONE_ALPHA");
                passData.material.DisableKeyword("OBJECTS_MASK_ALPHA");
                passData.material.DisableKeyword("NORMALIZED_ALPHA");

                builder.SetRenderFunc(
                    static (RainRustRayTracingPassData data, RasterGraphContext context) =>
                    {
                        var cmd = context.cmd;

                        data.material.SetTexture("_ColorTex", data.mainRtHandle);
                        data.material.SetTexture("_DistTex", data.distanceRtHandle);
                        data.material.SetTexture("_NoiseTex", data.noiseTextureHandle);
                        data.material.SetVector("_Aspect", new Vector2(1, 0.5625f));
                        data.material.SetVector("_NoiseTilingOffset", data.noiseTilingOffset);

                        data.material.SetFloat("_Samples", 256);
                        data.material.SetFloat("_Intensity", 1);
                        data.material.SetFloat("_Falloff", 0.5f);

                        CoreUtils.DrawFullScreen(cmd, data.material);
                    }
                );
            }
        }

        private Material m_RayTracingMaterial;
        private const string k_RayTracingShaderName = "Hidden/RainRust/RayTracing";
    }
}
