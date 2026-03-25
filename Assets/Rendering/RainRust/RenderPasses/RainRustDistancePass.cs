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
        private Material m_DistanceMaterial;
        private RainRustLighting.RainRustLightingSettings m_Settings;

        public RainRustDistancePass()
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
        }

        public void Setup(RainRustLighting.RainRustLightingSettings settings)
        {
            m_Settings = settings;
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
            if (m_Settings == null)
                return;

            if (m_DistanceMaterial == null && m_Settings.distanceShader != null)
            {
                m_DistanceMaterial = CoreUtils.CreateEngineMaterial(m_Settings.distanceShader);
            }

            if (m_DistanceMaterial == null)
                return;

            var rainRustContextData = frameData.Get<RainRustContextData>();
            var cameraData = frameData.Get<UniversalCameraData>();

            float width = cameraData.cameraTargetDescriptor.width;
            float height = cameraData.cameraTargetDescriptor.height;
            Vector2 aspect = new(1f, height / width);

            m_DistanceMaterial.SetVector("_Aspect", aspect);

            BlitMaterialParameters blitParams = new(
                rainRustContextData.finalJfaRt,
                rainRustContextData.distanceRt,
                m_DistanceMaterial,
                0
            );
            renderGraph.AddBlitPass(blitParams, "RainRust Distance Pass");
        }
    }
}
