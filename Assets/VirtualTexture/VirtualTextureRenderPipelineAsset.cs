using UnityEngine;
using UnityEngine.Rendering;

namespace VirtualTexture
{
    [CreateAssetMenu(menuName="Custom/VirtualTexture")]
    public class VirtualTextureRenderPipelineAsset : RenderPipelineAsset
    {
        private readonly VirtualTextureSettings m_Settings = new();
        protected override RenderPipeline CreatePipeline()
        {
            return new VirtualTextureRenderPipeline(m_Settings);
        }
    }
}
