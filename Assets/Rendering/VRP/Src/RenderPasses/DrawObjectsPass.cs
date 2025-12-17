using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.RenderGraphModule;

namespace VanishRenderPipeline
{
    public partial class VanishRenderGraphRecorder
    {
        internal class DrawObjectsPassData
        {
            internal TextureHandle backbufferHandle;

            // internal TextureHandle depthHandle;
            internal RendererListHandle opaqueRendererListHandle;
            internal RendererListHandle transparentRendererListHandle;
        }

        private void AddDrawObjectsPass(RenderGraph renderGraph, ContextContainer frameData)
        {
            CameraData cameraData = frameData.Get<CameraData>();

            using (
                var builder = renderGraph.AddRasterRenderPass<DrawObjectsPassData>(
                    "Draw Objects Pass",
                    out var passData,
                    sDrawObjectsProfilingSampler
                )
            )
            {
                {
                    RendererListDesc opaqueRendererListDesc = new(
                        sShaderTagId,
                        cameraData.cullingResults,
                        cameraData.camera
                    );
                    opaqueRendererListDesc.sortingCriteria = SortingCriteria.CommonOpaque;
                    opaqueRendererListDesc.renderQueueRange = RenderQueueRange.opaque;

                    passData.opaqueRendererListHandle = renderGraph.CreateRendererList(
                        opaqueRendererListDesc
                    );

                    builder.UseRendererList(passData.opaqueRendererListHandle);
                }

                {
                    //创建半透明对象渲染列表
                    RendererListDesc transparentRendererListDesc = new(
                        sShaderTagId,
                        cameraData.cullingResults,
                        cameraData.camera
                    );
                    transparentRendererListDesc.sortingCriteria = SortingCriteria.CommonTransparent;
                    transparentRendererListDesc.renderQueueRange = RenderQueueRange.transparent;

                    passData.transparentRendererListHandle = renderGraph.CreateRendererList(
                        transparentRendererListDesc
                    );

                    builder.UseRendererList(passData.transparentRendererListHandle);
                }

                {
                    passData.backbufferHandle = renderGraph.ImportBackbuffer(
                        BuiltinRenderTextureType.CurrentActive
                    );
                    builder.SetRenderAttachment(passData.backbufferHandle, 0, AccessFlags.Write);
                }

                // 设置全局状态
                {
                    builder.AllowPassCulling(false);
                }

                {
                    builder.SetRenderFunc(
                        (DrawObjectsPassData passData, RasterGraphContext context) =>
                        {
                            context.cmd.DrawRendererList(passData.opaqueRendererListHandle);
                            context.cmd.DrawRendererList(passData.transparentRendererListHandle);
                        }
                    );
                }
            }
        }

        private static readonly ProfilingSampler sDrawObjectsProfilingSampler = new("Draw Objects");
        private static readonly ShaderTagId sShaderTagId = new("SRPDefaultUnlit");
    }
}
