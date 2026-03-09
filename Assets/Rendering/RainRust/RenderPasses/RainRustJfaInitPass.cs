using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;
using static UnityEngine.Rendering.RenderGraphModule.Util.RenderGraphUtils;

namespace RainRust.Rendering
{
    public class RainRustJfaInitPass : ScriptableRenderPass
    {
        private Material m_JfaInitMaterial;
        private const string k_ShaderName = "Hidden/RainRust/JfaUvSeed";

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

            if (m_JfaInitMaterial == null)
            {
                var shader = Shader.Find(k_ShaderName);
                if (shader != null)
                    m_JfaInitMaterial = CoreUtils.CreateEngineMaterial(shader);
            }

            BlitMaterialParameters blitParams = new(
                rainRustData.mainRt,
                rainRustData.jfaRt.Current(),
                m_JfaInitMaterial,
                0
            );

            renderGraph.AddBlitPass(blitParams, passName: "RainRust JFA Init");

            // Swap after initialization so next pass reads from the initialized texture
            rainRustData.jfaRt.Swap();
        }

        public void Dispose()
        {
            CoreUtils.Destroy(m_JfaInitMaterial);
        }
    }
}
