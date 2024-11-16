using UnityEngine;
using UnityEngine.Rendering;

namespace VirtualTexture
{
    public class VirtualTextureRenderPipeline : RenderPipeline
    {
        private readonly FeedbackPass m_FeedbackPass;
        private readonly FinalPass m_FinalPass;
        private readonly PageManager m_PageManager;

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
            // m_FeedbackPass.Test(context, camera);
            var cmd = CommandBufferPool.Get();
            cmd.SetGlobalTexture("_PageTable", m_PageManager.GetPageTable());
            cmd.SetGlobalTexture("_PhysicalTexture", m_PageManager.GetPhysicalTexture());
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
            cmd.Clear();
            m_FinalPass.Execute(context, camera);
        }
    }
}