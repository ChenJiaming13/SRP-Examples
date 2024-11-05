using UnityEngine;
using UnityEngine.Rendering;

namespace VirtualTexture
{
    public abstract class RenderPass
    {
        protected static void RenderObjects(ScriptableRenderContext context, Camera camera, string passName)
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