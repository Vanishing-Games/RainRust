using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace RainRust.Rendering
{
    public class RainRustDistancePass : ScriptableRenderPass
    {
        public RainRustDistancePass()
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
        }

        class PassData
        {
            public RendererListHandle rendererListHandle;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            using var builder = renderGraph.AddRasterRenderPass<PassData>(
                "RainRust Draw Objects Pass",
                out var passData
            );

            SetupRenderGraph(builder, passData, renderGraph, frameData);

            builder.SetRenderFunc(
                static (PassData data, RasterGraphContext context) => ExecutePass(data, context)
            );
        }

        private static void SetupRenderGraph(
            IRasterRenderGraphBuilder builder,
            PassData passData,
            RenderGraph renderGraph,
            ContextContainer frameData
        )
        {
            // Get the data needed to create the list of objects to draw
            UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            UniversalLightData lightData = frameData.Get<UniversalLightData>();
            SortingCriteria sortFlags = cameraData.defaultOpaqueSortFlags;
            RenderQueueRange renderQueueRange = RenderQueueRange.opaque;
            FilteringSettings filterSettings = new(renderQueueRange, ~0);

            // Redraw only objects that have their LightMode tag set to UniversalForward
            ShaderTagId shadersToOverride = new("UniversalForward");

            // Create drawing settings
            DrawingSettings drawSettings = RenderingUtils.CreateDrawingSettings(
                shadersToOverride,
                renderingData,
                cameraData,
                lightData,
                sortFlags
            );

            // Add the override material to the drawing settings
            // drawSettings.overrideMaterial = materialToUse;

            // Create the list of objects to draw
            var rendererListParameters = new RendererListParams(
                renderingData.cullResults,
                drawSettings,
                filterSettings
            );

            // Convert the list to a list handle that the render graph system can use
            passData.rendererListHandle = renderGraph.CreateRendererList(rendererListParameters);

            // Set the render target as the color and depth textures of the active camera texture
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            builder.UseRendererList(passData.rendererListHandle);
            builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
            builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.Write);
        }

        private static void ExecutePass(PassData data, RasterGraphContext context)
        {
            // Clear the render target to black
            context.cmd.ClearRenderTarget(true, true, Color.black);

            // Draw the objects in the list
            context.cmd.DrawRendererList(data.rendererListHandle);
        }

        private static readonly ProfilingSampler sDrawObjectsProfilingSampler = new(
            "RainRust Draw Objects Pass"
        );
        private static readonly ShaderTagId sShaderTagId = new("UniversalForward");
    }
}
