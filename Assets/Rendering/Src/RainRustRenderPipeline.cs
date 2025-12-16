using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace RainRustRenderPipeline
{
    public class RainRustRenderPipeline : RenderPipeline
    {
        protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
        {
            // Begin rendering context
            BeginContextRendering(context, cameras);

            // Render each camera
            for (int i = 0; i < cameras.Count; i++)
            {
                Camera camera = cameras[i];
                RenderCamera(context, camera);
            }

            // End rendering context
            EndContextRendering(context, cameras);
        }

        private void RenderCamera(ScriptableRenderContext context, Camera camera)
        {
            // Begin rendering the camera
            BeginCameraRendering(context, camera);

            {
                // Camera culling
                ScriptableCullingParameters cullingParameters;
                if (!camera.TryGetCullingParameters(out cullingParameters))
                    return;
                CullingResults cullingResults = context.Cull(ref cullingParameters);

                // Create commandBuffer
                CommandBuffer cmd = CommandBufferPool.Get(camera.name);

                // Set up camera properties
                context.SetupCameraProperties(camera);

                // Clear render target
                {
                    var clearFlags = camera.clearFlags;
                    bool clearSkybox = clearFlags == CameraClearFlags.Skybox;
                    bool clearDepth = clearFlags != CameraClearFlags.Nothing;
                    bool clearColor = clearFlags == CameraClearFlags.Color;
                    cmd.ClearRenderTarget(
                        clearDepth,
                        clearColor,
                        CoreUtils.ConvertSRGBToActiveColorSpace(camera.backgroundColor)
                    );

                    if (clearSkybox)
                    {
                        var skyboxRendererList = context.CreateSkyboxRendererList(camera);
                        cmd.DrawRendererList(skyboxRendererList);
                    }
                }

                // Render opaque objects
                {
                    var sortSettings = new SortingSettings(camera);
                    var drawSettings = new DrawingSettings(s_ShaderTagId, sortSettings);
                    var filterSettings = new FilteringSettings(RenderQueueRange.opaque);

                    var rendererListParams = new RendererListParams(
                        cullingResults,
                        drawSettings,
                        filterSettings
                    );
                    var renderList = context.CreateRendererList(ref rendererListParams);
                    cmd.DrawRendererList(renderList);
                }

                // Render Transparent objects
                {
                    var sortSettings = new SortingSettings(camera);
                    sortSettings.criteria = SortingCriteria.CommonTransparent;
                    var drawSettings = new DrawingSettings(s_ShaderTagId, sortSettings);
                    var filterSettings = new FilteringSettings(RenderQueueRange.transparent);

                    var rendererListParams = new RendererListParams(
                        cullingResults,
                        drawSettings,
                        filterSettings
                    );
                    var renderList = context.CreateRendererList(ref rendererListParams);
                    cmd.DrawRendererList(renderList);
                }

                // Submit command buffer
                context.ExecuteCommandBuffer(cmd);

                // Release command buffer
                cmd.Clear();
                CommandBufferPool.Release(cmd);

                // Submit render context
                context.Submit();
            }

            // End rendering the camera
            EndCameraRendering(context, camera);
        }

        private static readonly ShaderTagId s_ShaderTagId = new("SRPDefaultUnlit");
    }
}
