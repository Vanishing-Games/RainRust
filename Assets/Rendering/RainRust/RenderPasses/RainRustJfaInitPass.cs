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
        private RainRustLighting.RainRustLightingSettings m_Settings;

        class PassData
        {
            public TextureHandle src;
        }

        public RainRustJfaInitPass()
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
        }

        public void Setup(RainRustLighting.RainRustLightingSettings settings)
        {
            m_Settings = settings;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            RainRustContextData rainRustData = frameData.Get<RainRustContextData>();

            if (rainRustData == null || m_Settings == null)
                return;

            if (m_JfaInitMaterial == null && m_Settings.jfaInitShader != null)
            {
                m_JfaInitMaterial = CoreUtils.CreateEngineMaterial(m_Settings.jfaInitShader);
            }

            if (m_JfaInitMaterial == null)
                return;

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
