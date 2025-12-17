using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace VanishRenderPipeline
{
    public class VanishRenderPipeline : RenderPipeline
    {
        public VanishRenderPipeline()
        {
            InitializeRenderGraph();
        }

        protected override void Dispose(bool disposing)
        {
            CleanupRenderGraph();
            base.Dispose(disposing);
        }

        private void InitializeRenderGraph()
        {
            mRenderGraph = new RenderGraph("LiteRPRenderGraph");
            mVanishRenderGraphRecorder = new VanishRenderGraphRecorder();
            mContextContainer = new ContextContainer();
        }

        private void CleanupRenderGraph()
        {
            mContextContainer?.Dispose();
            mContextContainer = null;
            mVanishRenderGraphRecorder = null;
            mRenderGraph?.Cleanup();
            mRenderGraph = null;
        }

        protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
        {
            BeginContextRendering(context, cameras);

            for (int i = 0; i < cameras.Count; i++)
            {
                Camera camera = cameras[i];
                RenderCamera(context, camera);
            }

            mRenderGraph.EndFrame();
            EndContextRendering(context, cameras);
        }

        private void RenderCamera(ScriptableRenderContext context, Camera camera)
        {
            BeginCameraRendering(context, camera);

            if (!PrepareFrameData(context, camera))
                return;

            CommandBuffer cmd = CommandBufferPool.Get(camera.name);
            {
                context.SetupCameraProperties(camera);
            }
            {
                RecordAndExecuteRenderGraph(context, cmd);
                context.ExecuteCommandBuffer(cmd);
            }
            {
                cmd.Clear();
                CommandBufferPool.Release(cmd);
                context.Submit();
            }

            EndCameraRendering(context, camera);
        }

        private bool PrepareFrameData(ScriptableRenderContext context, Camera camera)
        {
            ScriptableCullingParameters cullingParameters;
            if (!camera.TryGetCullingParameters(out cullingParameters))
                return false;

            CullingResults cullingResults = context.Cull(ref cullingParameters);

            CameraData cameraData = mContextContainer.GetOrCreate<CameraData>();
            cameraData.camera = camera;
            cameraData.cullingResults = cullingResults;
            return true;
        }

        private void RecordAndExecuteRenderGraph(ScriptableRenderContext context, CommandBuffer cmd)
        {
            RenderGraphParameters renderGraphParameters = new()
            {
                commandBuffer = cmd,
                scriptableRenderContext = context,
                currentFrameIndex = Time.frameCount,
            };

            mRenderGraph.BeginRecording(renderGraphParameters);
            mVanishRenderGraphRecorder.RecordRenderGraph(mRenderGraph, mContextContainer);
            mRenderGraph.EndRecordingAndExecute();
        }

        private static readonly ShaderTagId sShaderTagId = new("SRPDefaultUnlit");
        private RenderGraph mRenderGraph = null;
        private VanishRenderGraphRecorder mVanishRenderGraphRecorder = null;
        private ContextContainer mContextContainer = null;
    }

    public partial class VanishRenderGraphRecorder : IRenderGraphRecorder
    {
        public void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            AddDrawObjectsPass(renderGraph, frameData);
        }
    }
}
