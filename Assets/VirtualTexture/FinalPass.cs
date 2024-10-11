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
        
        public override void Execute(ScriptableRenderContext context, Camera camera)
        {
            var cmd = CommandBufferPool.Get();
            context.SetupCameraProperties(camera);
            cmd.ClearRenderTarget(true, true, new Color(0.0f, 0.0f, 0.0f, 0.0f), 1.0f);
            RenderObjects(context, camera, "SRPDefaultUnlit");
            context.DrawSkybox(camera);
            context.Submit();
            CommandBufferPool.Release(cmd);
        }
    }
}