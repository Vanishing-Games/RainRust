using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace RainRust.Rendering
{
    public class RainRustLighting : ScriptableRendererFeature
    {
        [Serializable]
        public class RainRustLightingSettings
        {
            public LayerMask layerMask = -1;
        }

        public RainRustLightingSettings settings = new();

        /** Called when
        * - When the Scriptable Renderer Feature loads the first time.
        * - When you enable or disable the Scriptable Renderer Feature.
        * - When you change a property in the Inspector window of the Renderer Feature.
        */
        public override void Create()
        {
            // csharpier-ignore-start
            m_RainRustDrawObjectsPass = new RainRustDrawObjectsPass(); // 绘制所有光源, 用于后续光场计算
            m_JfaInitPass             = new RainRustJfaInitPass(); // 从光源数据获得JFA初始种子
            m_JfaPass                 = new RainRustJfaPass(); // Jump Flood Algorithm
            m_DistancePass            = new RainRustDistancePass(); // JFA -> Distance Field
            m_RainRustRayTracingPass  = new RainRustRayTracingPass(); // 通过光场与光源颜色进行屏幕空间Ray Tracing, 获得光照信息
            m_RainRustRenderingPass   = new RainRustRenderingPass(); // 渲染最终结果
            // csharpier-ignore-end
        }

        public override void AddRenderPasses(
            ScriptableRenderer renderer,
            ref RenderingData renderingData
        )
        {
            m_RainRustDrawObjectsPass.Setup(settings);
            renderer.EnqueuePass(m_RainRustDrawObjectsPass);
            renderer.EnqueuePass(m_JfaInitPass);
            renderer.EnqueuePass(m_JfaPass);
            renderer.EnqueuePass(m_DistancePass);
            renderer.EnqueuePass(m_RainRustRayTracingPass);
            renderer.EnqueuePass(m_RainRustRenderingPass);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            m_JfaInitPass?.Dispose();
            m_JfaPass?.Dispose();
            m_RainRustRenderingPass?.Dispose();
        }

        private RainRustDrawObjectsPass m_RainRustDrawObjectsPass;
        private RainRustJfaInitPass m_JfaInitPass;
        private RainRustJfaPass m_JfaPass;
        private RainRustDistancePass m_DistancePass;
        private RainRustRayTracingPass m_RainRustRayTracingPass;
        private RainRustRenderingPass m_RainRustRenderingPass;
    }
}
