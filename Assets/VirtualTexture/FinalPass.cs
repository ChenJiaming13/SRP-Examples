using UnityEngine;
using UnityEngine.Rendering;

namespace VirtualTexture
{
    public class FinalPass : RenderPass
    {
        private VirtualTextureSettings m_Settings;

        public FinalPass(VirtualTextureSettings settings)
        {
            m_Settings = settings;
        }
        
        public void Execute(ScriptableRenderContext context, Camera camera)
        {
            context.SetupCameraProperties(camera);
            var cmd = CommandBufferPool.Get();
            cmd.ClearRenderTarget(true, true, new Color(0.0f, 0.0f, 0.0f, 0.0f), 1.0f);
            cmd.SetGlobalVector("_PhyTextureParam", new Vector4(
                m_Settings.phyPageRows,
                m_Settings.phyPageCols,
                0.0f,
                0.0f
            ));
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
            cmd.Clear();
            RenderObjects(context, camera, "SRPDefaultUnlit");
            context.DrawSkybox(camera);
            context.DrawGizmos(camera, GizmoSubset.PreImageEffects);
            context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
            context.Submit();
        }
    }
}