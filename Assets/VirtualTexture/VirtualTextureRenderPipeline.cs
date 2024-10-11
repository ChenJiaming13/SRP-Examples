using UnityEngine;
using UnityEngine.Rendering;

namespace VirtualTexture
{
    public class VirtualTextureRenderPipeline : RenderPipeline
    {
        private readonly FeedbackPass m_FeedbackPass;
        private FinalPass m_FinalPass;

        public VirtualTextureRenderPipeline(VirtualTextureSettings settings)
        {
            m_FeedbackPass = new FeedbackPass(settings);
            m_FinalPass = new FinalPass(settings);
        }

        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            foreach (var camera in cameras)
            {
                RenderPerCamera(context, camera);
            }
        }

        private void RenderPerCamera(ScriptableRenderContext context, Camera camera)
        {
            m_FeedbackPass.Execute(context, camera);
            m_FeedbackPass.Test(context, camera);
            // m_FinalPass.Execute(context, camera);
        }
    }
}