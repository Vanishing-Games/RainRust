using Core;
using UnityEngine;
using UnityEngine.Rendering;

public class PipelineSetter : MonoBehaviour
{
    public RenderPipelineAsset mUrpRenderPipelineAsset;
    public RenderPipelineAsset mCustomRenderPipelineAsset;
    public bool UseCustomPipeline = true;

    void OnEnable()
    {
        GraphicsSettings.defaultRenderPipeline = UseCustomPipeline
            ? mCustomRenderPipelineAsset
            : mUrpRenderPipelineAsset;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        GraphicsSettings.defaultRenderPipeline = UseCustomPipeline
            ? mCustomRenderPipelineAsset
            : mUrpRenderPipelineAsset;
    }
#endif
}
