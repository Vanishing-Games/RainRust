using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace RainRust.Rendering
{
    public class RainRustJfaPass : ScriptableRenderPass
    {
        private Material m_JfaMaterial;
        private const string k_JfaShaderName = "Hidden/GiLight2D/JumpFloodAlgorithm";

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
                    Debug.LogError($"Shader not found: {k_JfaShaderName}");
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

            Vector2 aspect = new Vector2(1f, (float)height / width); // Matches shader's _Aspect usage

            for (int i = 0; i < iterations; i++)
            {
                // JFA Step calculation: N/2, N/4, ... 1
                float step = Mathf.Pow(2, iterations - 1 - i);
                Vector2 stepSize = new Vector2(step / width, step / height);

                using (
                    var builder = renderGraph.AddRasterRenderPass<PassData>(
                        "RainRust JFA Step " + i,
                        out var passData
                    )
                )
                {
                    builder.AllowPassCulling(false);

                    // Let's get the handles manually to be explicit
                    TextureHandle source = rainRustContextData.jfaRt.Previous();
                    TextureHandle destination = rainRustContextData.jfaRt.Current();
                    rainRustContextData.jfaRt.Swap();

                    passData.material = m_JfaMaterial;
                    passData.source = source;
                    passData.stepSize = stepSize;
                    passData.aspect = aspect;

                    builder.UseTexture(source, AccessFlags.Read);
                    builder.SetRenderAttachment(destination, 0, AccessFlags.Write);

                    builder.SetRenderFunc(
                        static (PassData data, RasterGraphContext context) =>
                        {
                            data.material.SetVector("_StepSize", data.stepSize);
                            data.material.SetVector("_Aspect", data.aspect);
                            // _SeedTex is handled by Blitter.BlitTexture typically passing source as _BlitTexture or similar,
                            // but our shader uses _SeedTex. We need to manually set it or use property block.
                            // Ideally the shader should use _BlitTexture if using Blitter.
                            // However, let's try setting it directly on material or via cmd.
                            // Since we are using a specific material property _SeedTex:
                            data.material.SetTexture("_SeedTex", data.source);

                            // Full screen blit
                            Blitter.BlitTexture(
                                context.cmd,
                                data.source,
                                new Vector4(1, 1, 0, 0),
                                data.material,
                                0
                            );
                        }
                    );
                }
            }
        }
    }
}
