using UnityEngine;
using UnityEngine.Rendering;

namespace Common
{
    [ExecuteAlways]
    public class SwitchRenderPipeline : MonoBehaviour
    {
        public RenderPipelineAsset asset;
        private void OnEnable()
        {
            GraphicsSettings.renderPipelineAsset = asset;
        }
    }
}
