using UnityEngine;
using UnityEngine.Rendering;

namespace VirtualTexture
{
    [CreateAssetMenu(menuName="Custom/VirtualTexture")]
    public class VirtualTextureRenderPipelineAsset : RenderPipelineAsset
    {
        public VirtualTextureSettings settings;
        protected override RenderPipeline CreatePipeline()
        {
            return new VirtualTextureRenderPipeline(settings);
        }
    }
}
