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

        public void Setup(RainRustLighting.RainRustLightingSettings settings)
        {
            m_Settings = settings;
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
            if (m_Settings == null)
                return;

            // Get the shared data from the previous pass
            var rainRustContextData = frameData.Get<RainRustContextData>();
            var cameraData = frameData.Get<UniversalCameraData>();

            if (cameraData.isPreviewCamera)
                return;

            var oddSource = rainRustContextData.jfaRt.OddSource();
            if (!oddSource.IsValid())
                return;

            // Note: RainRustDrawObjectsPass has already swapped the ping-pong buffer after its blit.
            // So rainRustContextData.jfaRt.Previous() holds the initial seed data.

            // Resolution and JFA steps
            var desc = renderGraph.GetTextureDesc(oddSource);
            int width = desc.width;
            int height = desc.height;
            int maxDimension = Math.Max(width, height);
            int iterations = (int)Math.Ceiling(Mathf.Log(maxDimension, 2));

            Vector2 aspect = new(1f, (float)height / width); // Matches shader's _Aspect usage

            EnsureMaterials(iterations);

            if (m_JfaMaterials == null || m_JfaMaterials.Count < iterations)
                return;

            for (int i = 0; i < iterations; i++)
            {
                // JFA Step calculation: N/2, N/4, ... 1
                float step = Mathf.Pow(2, iterations - 1 - i);
                Vector2 stepSize = new(step / width, step / height);

                var material = m_JfaMaterials[i];
                if (material == null)
                    continue;

                material.SetVector("_StepSize", stepSize);
                material.SetVector("_Aspect", aspect);
                // material.SetTexture("_SeedTex", rainRustContextData.jfaRt.Previous()); // Handled by Blit inputs usually, but SetTexture is safe if needed.

                var source = rainRustContextData.jfaRt.GetByStep(i);
                var destination = rainRustContextData.jfaRt.GetByStep(i + 1);

                BlitMaterialParameters blitParams = new(source, destination, material, 0);

                renderGraph.AddBlitPass(blitParams, "RainRust JFA Step " + i);
            }

            // The final result is in GetByStep(iterations)
            rainRustContextData.finalJfaRt = rainRustContextData.jfaRt.GetByStep(iterations);
        }

        private void EnsureMaterials(int count)
        {
            if (m_JfaMaterials == null)
                m_JfaMaterials = new List<Material>();

            if (m_Shader == null && m_Settings.jfaShader != null)
            {
                m_Shader = m_Settings.jfaShader;
            }

            if (m_Shader == null)
                return;

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
        private RainRustLighting.RainRustLightingSettings m_Settings;
    }
}
