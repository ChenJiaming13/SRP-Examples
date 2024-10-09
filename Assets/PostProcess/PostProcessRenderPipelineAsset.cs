using UnityEngine;
using UnityEngine.Rendering;

namespace PostProcess
{
    [CreateAssetMenu(menuName = "Custom/PostProcess")]
    public class PostProcessRenderPipelineAsset : RenderPipelineAsset
    {
        protected override RenderPipeline CreatePipeline()
        {
            return new PostProcessRenderPipeline();
        }
    }
}