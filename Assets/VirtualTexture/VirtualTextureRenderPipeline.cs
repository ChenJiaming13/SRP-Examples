using UnityEngine;
using UnityEngine.Rendering;

namespace VirtualTexture
{
    public class VirtualTextureRenderPipeline : RenderPipeline
    {
        private readonly RenderTexture m_FeedbackTexture = new(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.Linear);
        private readonly RenderTexture m_FeedbackDepthTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.Depth,
            RenderTextureReadWrite.Linear);

        private const int PAGE_SIZE = 8; // exp2(m_MaxMipmapLevel)
        private const int PAGE_RESOLUTION = 512;
        private const int MIN_MIPMAP_LEVEL = 0;
        private const int MAX_MIPMAP_LEVEL = 3;

        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            foreach (var camera in cameras)
            {
                RenderPerCamera(context, camera);
            }
        }

        private void RenderPerCamera(ScriptableRenderContext context, Camera camera)
        {
            var cmd = CommandBufferPool.Get();
            FeedbackPass(context, camera, cmd);
            // FinalPass(context, camera, cmd);
            TestPass(context, camera, cmd);
            CommandBufferPool.Release(cmd);
        }

        private void TestPass(ScriptableRenderContext context, Camera camera, CommandBuffer cmd)
        {
            cmd.Blit(m_FeedbackTexture, camera.targetTexture);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            context.Submit();
        }
        
        private void FeedbackPass(ScriptableRenderContext context, Camera camera, CommandBuffer cmd)
        {
            context.SetupCameraProperties(camera);
            cmd.SetRenderTarget(m_FeedbackTexture, m_FeedbackDepthTexture);
            cmd.ClearRenderTarget(true, true, new Color(0.0f, 0.0f, 0.0f, 0.0f), 1.0f);
            // x: Page Size (Level 0)
            // y: Page Resolution
            // z: Max Mipmap Level
            // w: Min Mipmap Level
            cmd.SetGlobalVector("_VTFeedbackParam", new Vector4(PAGE_SIZE, PAGE_RESOLUTION, MAX_MIPMAP_LEVEL, MIN_MIPMAP_LEVEL));
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            RenderObjects(context, camera, "VirtualTextureFeedback");
        }

        private void FinalPass(ScriptableRenderContext context, Camera camera, CommandBuffer cmd)
        {
            context.SetupCameraProperties(camera);
            cmd.ClearRenderTarget(true, true, new Color(0.0f, 0.0f, 0.0f, 0.0f), 1.0f);
            RenderObjects(context, camera, "SRPDefaultUnlit");
            context.DrawSkybox(camera);
            context.Submit();
        }
        
        private void RenderObjects(ScriptableRenderContext context, Camera camera, string passName)
        {
            if (!camera.TryGetCullingParameters(out var cullingParameters)) return;
            var cullingResults = context.Cull(ref cullingParameters);
            var sortingSettings = new SortingSettings(camera);
            var drawingSettings = new DrawingSettings(new ShaderTagId(passName), sortingSettings);
            var filteringSettings = new FilteringSettings(RenderQueueRange.all);
            context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
        }
    }
}