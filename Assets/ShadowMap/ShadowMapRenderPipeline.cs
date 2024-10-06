using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ShadowMap
{
    public class ShadowMapRenderPipeline : RenderPipeline
    {
        private readonly RenderTexture m_ShadowMapBuffer;
        private readonly Camera m_LightCamera;
        
        public ShadowMapRenderPipeline()
        {
            const int shadowMapResolution = 1024;
            m_ShadowMapBuffer = new RenderTexture(shadowMapResolution, shadowMapResolution, 24, RenderTextureFormat.Depth,
                RenderTextureReadWrite.Linear);
            m_LightCamera = RenderSettings.sun.gameObject.GetComponent<Camera>();
            Debug.Assert(m_LightCamera != null);
            m_LightCamera.targetTexture = m_ShadowMapBuffer;
        }
        
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            Debug.Assert(m_LightCamera != null);
            var cmd = CommandBufferPool.Get();
            cmd.SetGlobalVector("_LightDir", m_LightCamera.transform.forward);
            var lightMatrix = GL.GetGPUProjectionMatrix(m_LightCamera.projectionMatrix, true) *
                              m_LightCamera.worldToCameraMatrix;
            cmd.SetGlobalMatrix("_LightMatrix", lightMatrix);
            cmd.SetGlobalTexture("_ShadowMap", m_ShadowMapBuffer);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            foreach (var camera in cameras)
            {
                if (camera == m_LightCamera) continue;
                RenderPerCamera(context, camera, cmd);
            }
            CommandBufferPool.Release(cmd);
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
        
        private void RenderPerCamera(ScriptableRenderContext context, Camera camera, CommandBuffer cmd)
        {
            // Caster Shadow Pass
            context.SetupCameraProperties(m_LightCamera);
            cmd.ClearRenderTarget(true, true, Color.clear, 1.0f);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            RenderObjects(context, m_LightCamera, "CasterShadow");
            context.Submit();
            
            // Render Objects Pass
            context.SetupCameraProperties(camera);
            cmd.ClearRenderTarget(true, true, Color.clear, 1.0f);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            RenderObjects(context, camera, "SRPDefaultUnlit");
            context.Submit();
            
            // Skybox Pass & Gizmos Pass
            context.DrawSkybox(camera);
            if (Handles.ShouldRenderGizmos())
            {
                context.DrawGizmos(camera, GizmoSubset.PreImageEffects);
                context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
            }
            context.Submit();
        }
    }
}