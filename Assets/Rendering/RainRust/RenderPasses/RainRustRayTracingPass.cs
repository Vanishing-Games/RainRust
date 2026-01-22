using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;
using static UnityEngine.Rendering.RenderGraphModule.Util.RenderGraphUtils;

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

            BlitMaterialParameters blitParams = new(
                rainRustContextData.distanceRt,
                rainRustContextData.lightingRt,
                m_RayTracingMaterial,
                0
            );
            renderGraph.AddBlitPass(blitParams, "RainRust Ray Tracing Pass");
        }

        private Material m_RayTracingMaterial;
        private const string k_RayTracingShaderName = "Hidden/RainRust/RayTracing";
    }
}
