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

        class RainRustDrawObjectsPassData
        {
            public UniversalCameraData cameraData;
            public UniversalRenderingData renderingData;
            public UniversalLightData lightData;
            public UniversalResourceData resourceData;
            public TextureHandle rrMainRt;
            public RendererListHandle rendererListHandle;
        }

        class BlitPassData
        {
            public TextureHandle src;
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
                out TextureHandle jfaFirstRtHandle,
                out TextureHandle jfaSecondRtHandle
            );

            // Store the texture handle in the context data so subsequent passes can use it
            RainRustContextData rainRustData = frameData.Create<RainRustContextData>();
            rainRustData.mainRt = mainRtHandle;
            rainRustData.jfaRt = new TextureHandlePingPong(jfaFirstRtHandle, jfaSecondRtHandle);

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

                // 2. Setup render targets
                builder.SetRenderAttachment(mainRtHandle, 0);

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

            // Record the Blit pass to copy mainRt to the first JFA RT
            RecordBlitPass(renderGraph, mainRtHandle, rainRustData.jfaRt.Current());
            rainRustData.jfaRt.Swap();
        }

        private void RecordBlitPass(
            RenderGraph renderGraph,
            TextureHandle source,
            TextureHandle destination
        )
        {
            using (
                var builder = renderGraph.AddRasterRenderPass<BlitPassData>(
                    "RainRust Blit Main to JFA",
                    out var passData
                )
            )
            {
                passData.src = source;
                builder.UseTexture(source, AccessFlags.Read);
                builder.SetRenderAttachment(destination, 0);

                builder.SetRenderFunc(
                    static (BlitPassData data, RasterGraphContext context) =>
                    {
                        Blitter.BlitTexture(
                            context.cmd,
                            data.src,
                            new Vector4(1, 1, 0, 0),
                            0,
                            false
                        );
                    }
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
            FilteringSettings filteringSettings = new(RenderQueueRange.all, ~0);

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
            out TextureHandle jfaFirstRtHandle,
            out TextureHandle jfaSecondRtHandle
        )
        {
            RenderTextureDescriptor textureDescriptor;
            {
                textureDescriptor = cameraData.cameraTargetDescriptor;
                textureDescriptor.colorFormat = RenderTextureFormat.ARGB32;
                textureDescriptor.depthStencilFormat = GraphicsFormat.None;
                textureDescriptor.msaaSamples = 1;
                textureDescriptor.useMipMap = false;
            }

            mainRtHandle = UniversalRenderer.CreateRenderGraphTexture(
                renderGraph,
                textureDescriptor,
                "RainRust Main Texture",
                false,
                FilterMode.Bilinear,
                TextureWrapMode.Clamp
            );

            textureDescriptor.colorFormat = RenderTextureFormat.RG16;
            jfaFirstRtHandle = UniversalRenderer.CreateRenderGraphTexture(
                renderGraph,
                textureDescriptor,
                "RainRust Jfa Texture 0",
                false,
                FilterMode.Bilinear,
                TextureWrapMode.Clamp
            );
            jfaSecondRtHandle = UniversalRenderer.CreateRenderGraphTexture(
                renderGraph,
                textureDescriptor,
                "RainRust Jfa Texture 1",
                false,
                FilterMode.Bilinear,
                TextureWrapMode.Clamp
            );
        }

        // Remove the unused SetupRenderGraph method to avoid confusion
        // This method is not called anywhere and conflicts with the main implementation

        private static void ExecutePass(
            RainRustDrawObjectsPassData data,
            RasterGraphContext context
        )
        {
            var cmd = context.cmd;

            // Clear render target
            cmd.ClearRenderTarget(true, true, Color.white * 0.03f);

            // Draw the objects in the list
            cmd.DrawRendererList(data.rendererListHandle);
        }

        private static readonly ProfilingSampler sDrawObjectsProfilingSampler = new(
            "RainRust Draw Objects Pass"
        );
        private static readonly ShaderTagId sShaderTagId = new("UniversalForward");
        private RainRustRenderSettings m_CurrentRrSettings;
    }

    internal class RainRustRenderSettings { }
}
