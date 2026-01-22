using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;
using static UnityEngine.Rendering.RenderGraphModule.Util.RenderGraphUtils;

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

            BlitMaterialParameters blitParams = new(
                rainRustContextData.jfaRt.Previous(),
                rainRustContextData.distanceRt,
                m_DistanceMaterial,
                0
            );
            renderGraph.AddBlitPass(blitParams, "RainRust Distance Pass");
        }

        private Material m_DistanceMaterial;
        private const string k_DistanceShaderName = "Hidden/RainRust/Distance";
    }
}
