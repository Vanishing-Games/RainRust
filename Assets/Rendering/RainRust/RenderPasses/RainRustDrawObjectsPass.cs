using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace RainRust.Rendering
{
    public class RainRustDrawObjectsPass : ScriptableRenderPass
    {
        public RainRustDrawObjectsPass()
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
        }

        public void Setup(RainRustLighting.RainRustLightingSettings settings)
        {
            m_Settings = settings;
        }

        class RainRustDrawObjectsPassData
        {
            public UniversalCameraData cameraData;
            public UniversalRenderingData renderingData;
            public UniversalLightData lightData;
            public UniversalResourceData resourceData;
            public TextureHandle rrMainRt;
            public TextureHandle rrMainDepthRt;
            public RendererListHandle rendererListHandle;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            // 1. Get datas needed for the pass
            var cameraData = frameData.Get<UniversalCameraData>();
            var renderingData = frameData.Get<UniversalRenderingData>();
            var lightData = frameData.Get<UniversalLightData>();
            var resourceData = frameData.Get<UniversalResourceData>();

            // 2. Create the texture handles
            CreateRenderTextureHandles(
                renderGraph,
                cameraData,
                out TextureHandle mainRtHandle,
                out TextureHandle mainDepthRtHandle,
                out TextureHandle jfaFirstRtHandle,
                out TextureHandle jfaSecondRtHandle,
                out TextureHandle distanceRtHandle,
                out TextureHandle lightingRtHandle
            );

            // Store the texture handle in the context data so subsequent passes can use it
            RainRustContextData rainRustData = frameData.Create<RainRustContextData>();
            rainRustData.mainRt = mainRtHandle;
            rainRustData.jfaRt = new TextureHandlePingPong(jfaFirstRtHandle, jfaSecondRtHandle);
            rainRustData.distanceRt = distanceRtHandle;
            rainRustData.lightingRt = lightingRtHandle;

            // 3. Update keywords and other shader params
            SetupKeywordsAndParameters(ref m_CurrentRrSettings, ref cameraData);

            // 4. Record the render pass
            using (
                var builder = renderGraph.AddRasterRenderPass<RainRustDrawObjectsPassData>(
                    "RainRust Draw Objects Pass",
                    out var passData
                )
            )
            {
                builder.AllowPassCulling(false);

                // 1. Fill in the pass data
                InitDrawObjectsPassData(
                    cameraData,
                    renderingData,
                    lightData,
                    resourceData,
                    ref passData
                );
                passData.rrMainRt = mainRtHandle;
                passData.rrMainDepthRt = mainDepthRtHandle;

                // 2. Setup render targets
                builder.SetRenderAttachment(mainRtHandle, 0, AccessFlags.Write);
                builder.SetRenderAttachmentDepth(mainDepthRtHandle, AccessFlags.Write);

                // 3. Declare input textures
                // builder.UseTexture(passData.rrMainRt, AccessFlags.ReadWrite); // this is used by attachment

                // 4. Get renderer lists
                InitRendererLists(ref passData, renderGraph);

                // 5. Use renderer list
                builder.UseRendererList(passData.rendererListHandle);

                // 6. Setup render function
                builder.SetRenderFunc(
                    static (RainRustDrawObjectsPassData data, RasterGraphContext context) =>
                        ExecutePass(data, context)
                );
            }
        }

        private void InitRendererLists(
            ref RainRustDrawObjectsPassData passData,
            RenderGraph renderGraph
        )
        {
            SortingCriteria sortingCriteria = passData.cameraData.defaultOpaqueSortFlags;
            // Use RenderQueueRange.all to include both Opaque and Transparent objects
            // Use configured LayerMask for filtering
            LayerMask layerMask = m_Settings != null ? m_Settings.layerMask : (LayerMask)(-1);
            FilteringSettings filteringSettings = new(RenderQueueRange.all, layerMask);

            DrawingSettings drawingSettings = RenderingUtils.CreateDrawingSettings(
                new ShaderTagId("UniversalForward"),
                passData.renderingData,
                passData.cameraData,
                passData.lightData,
                sortingCriteria
            );

            // Add other common URP tags to catch more shaders
            drawingSettings.SetShaderPassName(1, new ShaderTagId("UniversalForwardOnly"));
            drawingSettings.SetShaderPassName(2, new ShaderTagId("SRPDefaultUnlit"));
            drawingSettings.SetShaderPassName(3, new ShaderTagId("Lit"));

            passData.rendererListHandle = renderGraph.CreateRendererList(
                new RendererListParams(
                    passData.renderingData.cullResults,
                    drawingSettings,
                    filteringSettings
                )
            );
        }

        private void InitDrawObjectsPassData(
            UniversalCameraData cameraData,
            UniversalRenderingData renderingData,
            UniversalLightData lightData,
            UniversalResourceData resourceData,
            ref RainRustDrawObjectsPassData passData
        )
        {
            // Initialize pass data here
            passData.cameraData = cameraData;
            passData.renderingData = renderingData;
            passData.lightData = lightData;
            passData.resourceData = resourceData;
        }

        private void SetupKeywordsAndParameters(
            ref RainRustRenderSettings m_CurrentRrSettings,
            ref UniversalCameraData cameraData
        ) { }

        private void CreateRenderTextureHandles(
            RenderGraph renderGraph,
            UniversalCameraData cameraData,
            out TextureHandle mainRtHandle,
            out TextureHandle mainDepthRtHandle,
            out TextureHandle jfaFirstRtHandle,
            out TextureHandle jfaSecondRtHandle,
            out TextureHandle distanceRtHandle,
            out TextureHandle lightingRtHandle
        )
        {
            var stack = VolumeManager.instance.stack.GetComponent<RainRustVolume>();
            float scalar = stack.resolutionScalar.value;

            RenderTextureDescriptor textureDescriptor;
            {
                textureDescriptor = cameraData.cameraTargetDescriptor;
                textureDescriptor.width = Mathf.Max(1, (int)(textureDescriptor.width * scalar));
                textureDescriptor.height = Mathf.Max(1, (int)(textureDescriptor.height * scalar));
                textureDescriptor.colorFormat = RenderTextureFormat.ARGB32;
                textureDescriptor.depthStencilFormat = GraphicsFormat.None;
                textureDescriptor.msaaSamples = 1;
                textureDescriptor.useMipMap = false;
            }

            mainRtHandle = UniversalRenderer.CreateRenderGraphTexture(
                renderGraph,
                textureDescriptor,
                "RainRust Main Texture",
                true,
                FilterMode.Bilinear,
                TextureWrapMode.Clamp
            );

            RenderTextureDescriptor depthDescriptor = textureDescriptor;
            depthDescriptor.colorFormat = RenderTextureFormat.Depth;
            depthDescriptor.depthStencilFormat = GraphicsFormat.D24_UNorm_S8_UInt;

            mainDepthRtHandle = UniversalRenderer.CreateRenderGraphTexture(
                renderGraph,
                depthDescriptor,
                "RainRust Main Depth Texture",
                true,
                FilterMode.Point,
                TextureWrapMode.Clamp
            );

            textureDescriptor.colorFormat = RenderTextureFormat.ARGBFloat;
            jfaFirstRtHandle = UniversalRenderer.CreateRenderGraphTexture(
                renderGraph,
                textureDescriptor,
                "RainRust Jfa Texture 0",
                true,
                FilterMode.Bilinear,
                TextureWrapMode.Clamp
            );
            jfaSecondRtHandle = UniversalRenderer.CreateRenderGraphTexture(
                renderGraph,
                textureDescriptor,
                "RainRust Jfa Texture 1",
                true,
                FilterMode.Bilinear,
                TextureWrapMode.Clamp
            );
            distanceRtHandle = UniversalRenderer.CreateRenderGraphTexture(
                renderGraph,
                textureDescriptor,
                "RainRust Distance Texture",
                true,
                FilterMode.Bilinear,
                TextureWrapMode.Clamp
            );
            lightingRtHandle = UniversalRenderer.CreateRenderGraphTexture(
                renderGraph,
                textureDescriptor,
                "RainRust Lighting Texture",
                true,
                FilterMode.Bilinear,
                TextureWrapMode.Clamp
            );
        }

        private static void ExecutePass(
            RainRustDrawObjectsPassData data,
            RasterGraphContext context
        )
        {
            var cmd = context.cmd;

            // Draw the objects in the list
            cmd.DrawRendererList(data.rendererListHandle);
        }

        private static readonly ProfilingSampler sDrawObjectsProfilingSampler = new(
            "RainRust Draw Objects Pass"
        );
        private static readonly ShaderTagId sShaderTagId = new("UniversalForward");
        private RainRustRenderSettings m_CurrentRrSettings;
        private RainRustLighting.RainRustLightingSettings m_Settings;
    }

    internal class RainRustRenderSettings { }
}
