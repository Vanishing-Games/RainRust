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
        private const string k_ShaderName = "Hidden/RainRust/JfaSeedInit";

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
                rainRustData.jfaRt.EvenSource(), // jfa 从 step 0 开始
                m_JfaInitMaterial,
                0
            );

            renderGraph.AddBlitPass(blitParams, passName: "RainRust JFA Init");
        }

        public void Dispose()
        {
            CoreUtils.Destroy(m_JfaInitMaterial);
        }
    }
}
