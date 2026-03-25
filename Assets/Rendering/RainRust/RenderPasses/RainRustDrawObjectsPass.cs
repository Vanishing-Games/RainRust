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
            // csharpier-ignore-start
            public UniversalCameraData    cameraData;
            public UniversalRenderingData renderingData;
            public UniversalLightData     lightData;
            public UniversalResourceData  resourceData;
            public TextureHandle          rrMainRt;
            public TextureHandle          rrMainDepthRt;
            public TextureHandle          rrScaledDepthRt;
            public TextureHandle          rrReceiverRt;
            public RendererListHandle     lightSourcesRendererList;
            public RendererListHandle     receiversRendererList;
            // csharpier-ignore-end
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
                out TextureHandle scaledDepthRtHandle,
                out TextureHandle receiverRtHandle,
                out TextureHandle jfaFirstRtHandle,
                out TextureHandle jfaSecondRtHandle,
                out TextureHandle distanceRtHandle,
                out TextureHandle lightingRtHandle
            );

            // Store the texture handle in the context data so subsequent passes can use it
            RainRustContextData rainRustData = frameData.Create<RainRustContextData>();
            rainRustData.mainRt = mainRtHandle;
            rainRustData.mainDepthRt = mainDepthRtHandle;
            rainRustData.receiverRt = receiverRtHandle;
            rainRustData.jfaRt = new TextureHandlePingPong(jfaFirstRtHandle, jfaSecondRtHandle);
            rainRustData.distanceRt = distanceRtHandle;
            rainRustData.lightingRt = lightingRtHandle;

            // 3. Update keywords and other shader params
            SetupKeywordsAndParameters(ref m_CurrentRrSettings, ref cameraData);

            // 4. Record Light Sources Pass
            using (
                var builder = renderGraph.AddRasterRenderPass<RainRustDrawObjectsPassData>(
                    "RainRust Draw Light Sources Pass",
                    out var passData
                )
            )
            {
                builder.AllowPassCulling(false);
                InitDrawObjectsPassData(
                    cameraData,
                    renderingData,
                    lightData,
                    resourceData,
                    ref passData
                );
                passData.rrMainRt = mainRtHandle;
                passData.rrScaledDepthRt = scaledDepthRtHandle;

                builder.SetRenderAttachment(mainRtHandle, 0, AccessFlags.Write);
                builder.SetRenderAttachmentDepth(scaledDepthRtHandle, AccessFlags.Write);

                // Light Sources Renderer List
                SortingCriteria sortingCriteria = passData.cameraData.defaultOpaqueSortFlags;
                LayerMask lightSourcesLayerMask =
                    m_Settings != null ? m_Settings.lightSourcesLayerMask : (LayerMask)(-1);
                FilteringSettings lightSourcesFiltering = new(
                    RenderQueueRange.all,
                    lightSourcesLayerMask
                );

                DrawingSettings lightSourcesDrawing = RenderingUtils.CreateDrawingSettings(
                    new ShaderTagId("RainRustLightSource"),
                    passData.renderingData,
                    passData.cameraData,
                    passData.lightData,
                    sortingCriteria
                );
                lightSourcesDrawing.SetShaderPassName(1, new ShaderTagId("UniversalForward"));
                lightSourcesDrawing.SetShaderPassName(2, new ShaderTagId("UniversalForwardOnly"));
                lightSourcesDrawing.SetShaderPassName(3, new ShaderTagId("SRPDefaultUnlit"));
                lightSourcesDrawing.SetShaderPassName(4, new ShaderTagId("Lit"));

                passData.lightSourcesRendererList = renderGraph.CreateRendererList(
                    new RendererListParams(
                        passData.renderingData.cullResults,
                        lightSourcesDrawing,
                        lightSourcesFiltering
                    )
                );
                builder.UseRendererList(passData.lightSourcesRendererList);

                builder.SetRenderFunc(
                    static (RainRustDrawObjectsPassData data, RasterGraphContext context) =>
                        context.cmd.DrawRendererList(data.lightSourcesRendererList)
                );
            }

            // 5. Record Receivers Pass
            using (
                var builder = renderGraph.AddRasterRenderPass<RainRustDrawObjectsPassData>(
                    "RainRust Draw Receivers Pass",
                    out var passData
                )
            )
            {
                builder.AllowPassCulling(false);
                InitDrawObjectsPassData(
                    cameraData,
                    renderingData,
                    lightData,
                    resourceData,
                    ref passData
                );
                passData.rrReceiverRt = receiverRtHandle;
                passData.rrMainDepthRt = mainDepthRtHandle;

                builder.SetRenderAttachment(receiverRtHandle, 0, AccessFlags.Write);
                builder.SetRenderAttachmentDepth(mainDepthRtHandle, AccessFlags.Write);

                // Receivers Renderer List
                SortingCriteria sortingCriteria = passData.cameraData.defaultOpaqueSortFlags;
                LayerMask receiversLayerMask =
                    m_Settings != null ? m_Settings.receiversLayerMask : (LayerMask)(-1);
                FilteringSettings receiversFiltering = new(
                    RenderQueueRange.all,
                    receiversLayerMask
                );
                
                // 专门寻找接收者标签
                DrawingSettings receiversDrawing = RenderingUtils.CreateDrawingSettings(
                    new ShaderTagId("RainRustLighting"),
                    passData.renderingData,
                    passData.cameraData,
                    passData.lightData,
                    sortingCriteria
                );

                passData.receiversRendererList = renderGraph.CreateRendererList(
                    new RendererListParams(
                        passData.renderingData.cullResults,
                        receiversDrawing,
                        receiversFiltering
                    )
                );
                builder.UseRendererList(passData.receiversRendererList);

                builder.SetRenderFunc(
                    static (RainRustDrawObjectsPassData data, RasterGraphContext context) =>
                        context.cmd.DrawRendererList(data.receiversRendererList)
                );
            }
        }

        private void InitRendererLists(
            ref RainRustDrawObjectsPassData passData,
            RenderGraph renderGraph
        )
        {
            SortingCriteria sortingCriteria = passData.cameraData.defaultOpaqueSortFlags;

            // Light Sources
            LayerMask lightSourcesLayerMask =
                m_Settings != null ? m_Settings.lightSourcesLayerMask : (LayerMask)(-1);
            FilteringSettings lightSourcesFiltering = new(
                RenderQueueRange.all,
                lightSourcesLayerMask
            );
            DrawingSettings lightSourcesDrawing = RenderingUtils.CreateDrawingSettings(
                new ShaderTagId("RainRustLightSource"),
                passData.renderingData,
                passData.cameraData,
                passData.lightData,
                sortingCriteria
            );
            lightSourcesDrawing.SetShaderPassName(1, new ShaderTagId("UniversalForward"));
            lightSourcesDrawing.SetShaderPassName(2, new ShaderTagId("UniversalForwardOnly"));
            lightSourcesDrawing.SetShaderPassName(3, new ShaderTagId("SRPDefaultUnlit"));

            passData.lightSourcesRendererList = renderGraph.CreateRendererList(
                new RendererListParams(
                    passData.renderingData.cullResults,
                    lightSourcesDrawing,
                    lightSourcesFiltering
                )
            );

            // Receivers
            LayerMask receiversLayerMask =
                m_Settings != null ? m_Settings.receiversLayerMask : (LayerMask)(-1);
            FilteringSettings receiversFiltering = new(RenderQueueRange.all, receiversLayerMask);
            DrawingSettings receiversDrawing = RenderingUtils.CreateDrawingSettings(
                new ShaderTagId("RainRustLighting"),
                passData.renderingData,
                passData.cameraData,
                passData.lightData,
                sortingCriteria
            );

            passData.receiversRendererList = renderGraph.CreateRendererList(
                new RendererListParams(
                    passData.renderingData.cullResults,
                    receiversDrawing,
                    receiversFiltering
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
            out TextureHandle scaledDepthRtHandle,
            out TextureHandle receiverRtHandle,
            out TextureHandle jfaFirstRtHandle,
            out TextureHandle jfaSecondRtHandle,
            out TextureHandle distanceRtHandle,
            out TextureHandle lightingRtHandle
        )
        {
            var stack = VolumeManager.instance.stack.GetComponent<RainRustVolume>();
            float scalar = stack.resolutionScalar.value;

            // 1. Scaled Descriptor (for lighting calculation)
            RenderTextureDescriptor scaledDescriptor = cameraData.cameraTargetDescriptor;
            scaledDescriptor.width = Mathf.Max(1, (int)(scaledDescriptor.width * scalar));
            scaledDescriptor.height = Mathf.Max(1, (int)(scaledDescriptor.height * scalar));
            scaledDescriptor.colorFormat = RenderTextureFormat.ARGB32;
            scaledDescriptor.depthStencilFormat = GraphicsFormat.None;
            scaledDescriptor.msaaSamples = 1;
            scaledDescriptor.useMipMap = false;

            // 2. Full Descriptor (for final composition / receiver rendering)
            RenderTextureDescriptor fullDescriptor = cameraData.cameraTargetDescriptor;
            fullDescriptor.colorFormat = RenderTextureFormat.ARGB32;
            fullDescriptor.depthStencilFormat = GraphicsFormat.None;
            fullDescriptor.msaaSamples = 1;
            fullDescriptor.useMipMap = false;

            // Main RT (Source for lighting) - Scaled
            mainRtHandle = UniversalRenderer.CreateRenderGraphTexture(
                renderGraph,
                scaledDescriptor,
                "RainRust Main Texture",
                true,
                FilterMode.Bilinear,
                TextureWrapMode.Clamp
            );

            // Receiver RT (Final visual output) - Full Resolution
            receiverRtHandle = UniversalRenderer.CreateRenderGraphTexture(
                renderGraph,
                fullDescriptor,
                "RainRust Receiver Texture",
                true,
                FilterMode.Bilinear,
                TextureWrapMode.Clamp
            );

            // Depth for Light Sources - Scaled Resolution
            RenderTextureDescriptor scaledDepthDescriptor = scaledDescriptor;
            scaledDepthDescriptor.colorFormat = RenderTextureFormat.Depth;
            scaledDepthDescriptor.depthStencilFormat = GraphicsFormat.D24_UNorm_S8_UInt;

            scaledDepthRtHandle = UniversalRenderer.CreateRenderGraphTexture(
                renderGraph,
                scaledDepthDescriptor,
                "RainRust Scaled Depth Texture",
                true,
                FilterMode.Point,
                TextureWrapMode.Clamp
            );

            // Depth for Receivers - Full Resolution
            RenderTextureDescriptor fullDepthDescriptor = fullDescriptor;
            fullDepthDescriptor.colorFormat = RenderTextureFormat.Depth;
            fullDepthDescriptor.depthStencilFormat = GraphicsFormat.D24_UNorm_S8_UInt;

            mainDepthRtHandle = UniversalRenderer.CreateRenderGraphTexture(
                renderGraph,
                fullDepthDescriptor,
                "RainRust Main Depth Texture",
                true,
                FilterMode.Point,
                TextureWrapMode.Clamp
            );

            // Lighting calculation textures - Scaled
            scaledDescriptor.colorFormat = RenderTextureFormat.ARGBFloat;
            jfaFirstRtHandle = UniversalRenderer.CreateRenderGraphTexture(
                renderGraph,
                scaledDescriptor,
                "RainRust Jfa Texture 0",
                true,
                FilterMode.Bilinear,
                TextureWrapMode.Clamp
            );
            jfaSecondRtHandle = UniversalRenderer.CreateRenderGraphTexture(
                renderGraph,
                scaledDescriptor,
                "RainRust Jfa Texture 1",
                true,
                FilterMode.Bilinear,
                TextureWrapMode.Clamp
            );
            distanceRtHandle = UniversalRenderer.CreateRenderGraphTexture(
                renderGraph,
                scaledDescriptor,
                "RainRust Distance Texture",
                true,
                FilterMode.Bilinear,
                TextureWrapMode.Clamp
            );
            lightingRtHandle = UniversalRenderer.CreateRenderGraphTexture(
                renderGraph,
                scaledDescriptor,
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

            // Draw the objects in the lists
            cmd.DrawRendererList(data.lightSourcesRendererList);
            cmd.DrawRendererList(data.receiversRendererList);
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
