using UnityEngine;
using UnityEngine.Rendering;

namespace PostProcess
{
    [CreateAssetMenu(menuName = "Custom/ForwardPbr")]
    public class PostProcessRenderPipelineAsset : RenderPipelineAsset
    {
        protected override RenderPipeline CreatePipeline()
        {
            return new PostProcessRenderPipeline();
        }
    }
}