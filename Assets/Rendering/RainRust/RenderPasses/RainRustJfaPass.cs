using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace RainRust.Rendering
{
    public class RainRustJfaPass : ScriptableRenderPass
    {
        public RainRustJfaPass()
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
        }

        class RainRustJfaPassData
        {
            public RendererListHandle rendererListHandle;
        }
 
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            // Get the shared data from the previous pass
            var rainRustContextData = frameData.Get<RainRustContextData>();

            // 获取纹理的分辨率
            var jfaRtHandle = rainRustContextData.jfaRt.Current();
            Int32 width = jfaRtHandle.GetTextureDesc().width;
            Int32 height = jfaRtHandle.GetTextureDesc().height;

            var n = Math.Max(width, height);
            var iterations = (Int32)Math.Ceiling(Mathf.Log(n, 2));

            for(int i = 0; i < iterations; i++)
            {
                using (
                    var builder = renderGraph.AddRasterRenderPass<RainRustJfaPassData>(
                        "RainRust JFA Pass",
                        out var passData
                    )
                )
                {
                    builder.AllowPassCulling(false);

                    builder.SetRenderAttachment(
                        rainRustContextData.jfaRt.Current(),
                        0,
                        AccessFlags.Write
                    );

                    InitRendererLists(ref passData, renderGraph);
                    builder.UseRendererList(passData.rendererListHandle);

                    builder.SetRenderFunc(
                        static (RainRustJfaPassData data, RasterGraphContext context) =>
                            ExecutePass(data, context)
                    );
                }

                rainRustContextData.jfaRt.Swap();
            }

            // Swap to make sure the next pass uses the latest result
            rainRustContextData.jfaRt.Swap();
        }



		private void InitRendererLists(ref RainRustJfaPassData passData, RenderGraph renderGraph)
        {
            throw new NotImplementedException();
        }

		private static void ExecutePass(RainRustJfaPassData data, RasterGraphContext context)
        {
            // Clear the render target to black
            context.cmd.ClearRenderTarget(true, true, Color.red);

            // Draw the objects in the list
            context.cmd.DrawRendererList(data.rendererListHandle);
        }

        private static readonly ProfilingSampler sDrawObjectsProfilingSampler = new(
            "RainRust Draw Objects Pass"
        );
        private static readonly ShaderTagId sShaderTagId = new("UniversalForward");
    }
}
