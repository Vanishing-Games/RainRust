using System;
using Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;
using static UnityEngine.Rendering.RenderGraphModule.Util.RenderGraphUtils;

namespace RainRust.Rendering
{
    public class RainRustJfaPass : ScriptableRenderPass
    {
        public RainRustJfaPass()
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
        }

        class PassData
        {
            public Material material;
            public TextureHandle source;
            public Vector2 stepSize;
            public Vector2 aspect;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            // Ensure material is created
            if (m_JfaMaterial == null)
            {
                var shader = Shader.Find(k_JfaShaderName);
                if (shader == null)
                {
                    Core.Logger.LogError($"Shader not found: {k_JfaShaderName}", LogTag.Rendering);
                    return;
                }
                m_JfaMaterial = CoreUtils.CreateEngineMaterial(shader);
            }

            // Get the shared data from the previous pass
            var rainRustContextData = frameData.Get<RainRustContextData>();

            // Note: RainRustDrawObjectsPass has already swapped the ping-pong buffer after its blit.
            // So rainRustContextData.jfaRt.Previous() holds the initial seed data.

            // Resolution and JFA steps
            var desc = renderGraph.GetTextureDesc(rainRustContextData.jfaRt.Current());
            int width = desc.width;
            int height = desc.height;
            int maxDimension = Math.Max(width, height);
            int iterations = (int)Math.Ceiling(Mathf.Log(maxDimension, 2));

            Vector2 aspect = new(1f, (float)height / width); // Matches shader's _Aspect usage

            for (int i = 0; i < iterations; i++)
            {
                // JFA Step calculation: N/2, N/4, ... 1
                float step = Mathf.Pow(2, iterations - 1 - i);
                Vector2 stepSize = new(step / width, step / height);

                BlitMaterialParameters blitParams = new(
                    rainRustContextData.jfaRt.Previous(),
                    rainRustContextData.jfaRt.Current(),
                    m_JfaMaterial,
                    0
                );

                // Below isn't right//
                // m_JfaMaterial.SetVector("_StepSize", stepSize);
                // m_JfaMaterial.SetVector("_Aspect", aspect);
                // m_JfaMaterial.SetTexture("_SeedTex", rainRustContextData.jfaRt.Previous());

                renderGraph.AddBlitPass(blitParams, "RainRust JFA Step " + i);

                rainRustContextData.jfaRt.Swap();
            }
        }

        private Material m_JfaMaterial;
        private const string k_JfaShaderName = "Hidden/RainRust/JumpFloodAlgorithm";
    }
}
