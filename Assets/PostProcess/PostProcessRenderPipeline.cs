using UnityEngine;
using UnityEngine.Rendering;

namespace PostProcess
{
    public class PostProcessRenderPipeline : RenderPipeline
    {
        private RenderTexture m_ColorBuffer;
        private RenderTexture m_DepthBuffer;
        private int m_ScreenWidth;
        private int m_ScreenHeight;

        private void AllocRenderTextureIfNecessary()
        {
            if (m_ScreenWidth == Screen.width && m_ScreenHeight == Screen.height) return;
            m_ScreenWidth = Screen.width;
            m_ScreenHeight = Screen.height;
            m_DepthBuffer = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.Depth,
                RenderTextureReadWrite.Linear);
            m_ColorBuffer = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.Linear);
        }
        
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            AllocRenderTextureIfNecessary();
            foreach (var camera in cameras)
            {
                RenderPerCamera(context, camera);
            }
        }

        private void RenderPerCamera(ScriptableRenderContext context, Camera camera)
        {
            var cmd = CommandBufferPool.Get();
            
            // Forward Lit Pass
            context.SetupCameraProperties(camera);
            cmd.SetRenderTarget(m_ColorBuffer, m_DepthBuffer);
            cmd.ClearRenderTarget(true, true, Color.clear, 1.0f);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            if (!camera.TryGetCullingParameters(out var cullingParameters)) return;
            cullingParameters.shadowDistance = camera.farClipPlane;
            var cullingResults = context.Cull(ref cullingParameters);
            var sortingSettings = new SortingSettings(camera);
            var drawingSettings = new DrawingSettings(new ShaderTagId("SRPDefaultUnlit"), sortingSettings);
            var filteringSettings = new FilteringSettings(RenderQueueRange.all);
            context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
            context.DrawSkybox(camera);

            // Post Process Pass
            cmd.SetGlobalTexture("_MainTex", m_ColorBuffer);
            cmd.Blit(m_ColorBuffer, camera.targetTexture, new Material(Shader.Find("Custom/InverseColor")));
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            context.Submit();
            CommandBufferPool.Release(cmd);
        }
    }
}