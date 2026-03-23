using Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace RainRust.Rendering
{
    public class RainRustRayTracingPass : ScriptableRenderPass
    {
        public RainRustRayTracingPass()
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
        }

        class RainRustRayTracingPassData
        {
            internal Material material;
            internal TextureHandle mainRtHandle;
            internal TextureHandle distanceRtHandle;
            internal TextureHandle lightingRtHandle;
            internal Texture noiseTextureHandle;
            internal Vector4 aspect;
            internal Vector4 noiseTilingOffset;
            internal int rayCount;
            internal float intensity;
            internal float power;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (m_RayTracingMaterial == null)
            {
                var shader = Shader.Find(k_RayTracingShaderName);
                if (shader == null)
                {
                    Debug.LogError($"Shader not found: {k_RayTracingShaderName}");
                    return;
                }
                m_RayTracingMaterial = CoreUtils.CreateEngineMaterial(shader);
            }

            var rainRustContextData = frameData.Get<RainRustContextData>();

            var desc = renderGraph.GetTextureDesc(rainRustContextData.jfaRt.OddSource());
            int width = desc.width;
            int height = desc.height;

            Vector2 aspect = new(1f, (float)height / width);

            using (
                var builder = renderGraph.AddRasterRenderPass<RainRustRayTracingPassData>(
                    "RainRust Ray Tracing Pass",
                    out var passData
                )
            )
            {
                passData.material = m_RayTracingMaterial;
                passData.mainRtHandle = rainRustContextData.mainRt;
                passData.distanceRtHandle = rainRustContextData.distanceRt;
                passData.lightingRtHandle = rainRustContextData.lightingRt;
                passData.aspect = aspect;
                // passData.noiseTextureHandle = frameData.Get<UniversalResourceData>()

                builder.UseTexture(passData.mainRtHandle);
                builder.UseTexture(passData.distanceRtHandle);

                builder.SetRenderAttachment(passData.lightingRtHandle, 0, AccessFlags.Write);

                builder.SetRenderFunc(
                    static (RainRustRayTracingPassData data, RasterGraphContext context) =>
                    {
                        var cmd = context.cmd;

                        data.material.SetTexture("_ColorTex", data.mainRtHandle);
                        data.material.SetTexture("_DistTex", data.distanceRtHandle);
                        data.material.SetVector("_Aspect", data.aspect);

                        var stack = VolumeManager.instance.stack.GetComponent<RainRustVolume>();
                        data.material.SetFloat("_Samples", stack.lightSamples.value);
                        data.material.SetFloat("_Intensity", stack.lightIntensity.value);
                        data.material.SetFloat("_LightFalloffAlpha", stack.lightFalloffAlpha.value);
                        data.material.SetFloat("_LightFalloffGamma", stack.lightFalloffGamma.value);

                        data.material.SetFloat("_NoiseScale", stack.noiseScale.value);
                        data.material.SetFloat("_NoiseIntensity", stack.noiseIntensity.value);
                        data.material.SetVector("_NoiseVelocity", stack.noiseVelocity.value);
                        data.material.SetInt("_NoiseType", (int)stack.noiseType.value);

                        switch (stack.noiseMode.value)
                        {
                            case RainRustNoiseMode.None:
                                data.material.DisableKeyword("TEXTURE_RANDOM");
                                data.material.DisableKeyword("FRAGMENT_RANDOM");
                                data.material.SetVector("_NoiseTilingOffset", Vector4.zero);
                                break;
                            case RainRustNoiseMode.Texture:
                                if (stack.noiseTexture.value != null)
                                {
                                    data.material.SetTexture("_NoiseTex", stack.noiseTexture.value);
                                    data.material.EnableKeyword("TEXTURE_RANDOM");
                                    data.material.DisableKeyword("FRAGMENT_RANDOM");
                                    data.material.SetVector(
                                        "_NoiseTilingOffset",
                                        stack.noiseTilingOffset.value
                                    );
                                }
                                else
                                {
                                    data.material.DisableKeyword("TEXTURE_RANDOM");
                                    data.material.DisableKeyword("FRAGMENT_RANDOM");
                                    Core.CLogger.LogWarn(
                                        "Noise mode set to Texture but no noise texture assigned.",
                                        LogTag.Rendering
                                    );
                                }
                                break;
                            case RainRustNoiseMode.Shader:
                                data.material.DisableKeyword("TEXTURE_RANDOM");
                                data.material.EnableKeyword("FRAGMENT_RANDOM");
                                data.material.SetVector(
                                    "_NoiseTilingOffset",
                                    stack.noiseTilingOffset.value
                                );
                                break;
                        }

                        switch (stack.alphaMode.value)
                        {
                            case RainRustAlphaMode.OneAlpha:
                                data.material.EnableKeyword("ONE_ALPHA");
                                data.material.DisableKeyword("OBJECTS_MASK_ALPHA");
                                data.material.DisableKeyword("NORMALIZED_ALPHA");
                                break;
                            case RainRustAlphaMode.ObjectsMaskAlpha:
                                data.material.DisableKeyword("ONE_ALPHA");
                                data.material.EnableKeyword("OBJECTS_MASK_ALPHA");
                                data.material.DisableKeyword("NORMALIZED_ALPHA");
                                break;
                            case RainRustAlphaMode.NormalizedAlpha:
                                data.material.DisableKeyword("ONE_ALPHA");
                                data.material.DisableKeyword("OBJECTS_MASK_ALPHA");
                                data.material.EnableKeyword("NORMALIZED_ALPHA");
                                break;
                        }

                        CoreUtils.DrawFullScreen(cmd, data.material);
                    }
                );
            }
        }

        private Material m_RayTracingMaterial;
        private const string k_RayTracingShaderName = "Hidden/RainRust/RayTracing";
    }
}
