using Core;
using UnityEngine;
using UnityEngine.Rendering;

public class PipelineSetter : MonoBehaviour
{
    public RenderPipelineAsset mUrpRenderPipelineAsset;
    public RenderPipelineAsset mCustomRenderPipelineAsset;

    void OnEnable()
    {
        GraphicsSettings.defaultRenderPipeline = mCustomRenderPipelineAsset;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        GraphicsSettings.defaultRenderPipeline = mCustomRenderPipelineAsset;
    }
#endif
}
