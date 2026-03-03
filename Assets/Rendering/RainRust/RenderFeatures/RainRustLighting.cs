using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace RainRust.Rendering
{
    public class RainRustLighting : ScriptableRendererFeature
    {
        /** Called when
        * - When the Scriptable Renderer Feature loads the first time.
        * - When you enable or disable the Scriptable Renderer Feature.
        * - When you change a property in the Inspector window of the Renderer Feature.
        */
        public override void Create()
        {
            // csharpier-ignore-start
            m_RainRustDrawObjectsPass = new RainRustDrawObjectsPass();
            m_JfaInitPass             = new RainRustJfaInitPass();
            m_JfaPass                 = new RainRustJfaPass();
            m_DistancePass            = new RainRustDistancePass();
            m_RainRustRayTracingPass  = new RainRustRayTracingPass();
            m_RainRustRenderingPass   = new RainRustRenderingPass();
            // csharpier-ignore-end
        }

        public override void AddRenderPasses(
            ScriptableRenderer renderer,
            ref RenderingData renderingData
        )
        {
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
