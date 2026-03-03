using System;
using System.Collections.Generic;
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

            EnsureMaterials(iterations);

            for (int i = 0; i < iterations; i++)
            {
                // JFA Step calculation: N/2, N/4, ... 1
                float step = Mathf.Pow(2, iterations - 1 - i);
                Vector2 stepSize = new(step / width, step / height);

                var material = m_JfaMaterials[i];
                if (material == null) continue;

                material.SetVector("_StepSize", stepSize);
                material.SetVector("_Aspect", aspect);
                // material.SetTexture("_SeedTex", rainRustContextData.jfaRt.Previous()); // Handled by Blit inputs usually, but SetTexture is safe if needed.

                BlitMaterialParameters blitParams = new(
                    rainRustContextData.jfaRt.Previous(),
                    rainRustContextData.jfaRt.Current(),
                    material,
                    0
                );

                renderGraph.AddBlitPass(blitParams, "RainRust JFA Step " + i);

                rainRustContextData.jfaRt.Swap();
            }
        }

        private void EnsureMaterials(int count)
        {
            if (m_JfaMaterials == null)
                m_JfaMaterials = new List<Material>();

            if (m_Shader == null)
            {
                m_Shader = Shader.Find(k_JfaShaderName);
                if (m_Shader == null)
                {
                    Core.Logger.LogError($"Shader not found: {k_JfaShaderName}", LogTag.Rendering);
                    return;
                }
            }

            while (m_JfaMaterials.Count < count)
            {
                m_JfaMaterials.Add(CoreUtils.CreateEngineMaterial(m_Shader));
            }
        }

        public void Dispose()
        {
            if (m_JfaMaterials != null)
            {
                foreach (var material in m_JfaMaterials)
                {
                    CoreUtils.Destroy(material);
                }
                m_JfaMaterials.Clear();
            }
        }

        private List<Material> m_JfaMaterials;
        private Shader m_Shader;
        private const string k_JfaShaderName = "Hidden/RainRust/JumpFloodAlgorithm";
    }
}
