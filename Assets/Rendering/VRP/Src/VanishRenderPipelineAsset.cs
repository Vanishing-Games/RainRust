using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace VanishRenderPipeline
{
    [CreateAssetMenu(menuName = "Vanish Render Pipeline/Vanish Render Pipeline Asset")]
    public class VanishRenderPipelineAsset : RenderPipelineAsset
    {
        protected override RenderPipeline CreatePipeline()
        {
            return new VanishRenderPipeline();
        }
    }
}
