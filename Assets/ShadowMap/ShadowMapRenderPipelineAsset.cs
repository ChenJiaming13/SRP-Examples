using UnityEngine;
using UnityEngine.Rendering;

namespace ShadowMap
{
    [CreateAssetMenu(menuName = "Custom/ShadowMap")]
    public class ShadowMapRenderPipelineAsset : RenderPipelineAsset
    {
        protected override RenderPipeline CreatePipeline()
        {
            return new ShadowMapRenderPipeline();
        }
    }
}