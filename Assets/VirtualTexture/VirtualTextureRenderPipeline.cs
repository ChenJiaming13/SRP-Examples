using UnityEngine;
using UnityEngine.Rendering;

namespace VirtualTexture
{
    public class VirtualTextureRenderPipeline : RenderPipeline
    {
        private readonly FeedbackPass m_FeedbackPass;
        private FinalPass m_FinalPass;
        private PageManager m_PageManager;

        public VirtualTextureRenderPipeline(VirtualTextureSettings settings)
        {
            m_FeedbackPass = new FeedbackPass(settings);
            m_FinalPass = new FinalPass(settings);
            m_PageManager = new PageManager(settings);
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
            var pages = m_FeedbackPass.Execute(context, camera);
            m_PageManager.RequestPages(pages);
            m_FeedbackPass.Test(context, camera);
            // m_FinalPass.Execute(context, camera);
        }
    }
}