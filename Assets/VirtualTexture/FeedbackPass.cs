using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace VirtualTexture
{
    public class FeedbackPass : RenderPass
    {
        private readonly VirtualTextureSettings m_Settings;
        private readonly RenderTexture m_ColorTexture = new(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.Linear);
        private readonly RenderTexture m_DepthTexture = new(Screen.width, Screen.height, 24, RenderTextureFormat.Depth,
            RenderTextureReadWrite.Linear);
        private readonly ComputeBuffer m_FeedbackBuffer;
        private readonly int[] m_FeedbackBufferData;

        public FeedbackPass(VirtualTextureSettings settings)
        {
            m_Settings = settings;
            m_FeedbackBuffer = new ComputeBuffer(CalcFeedbackBufferLength(), sizeof(int), ComputeBufferType.Default);
            m_FeedbackBufferData = new int [CalcFeedbackBufferLength()];
        }

        public List<Page> Execute(ScriptableRenderContext context, Camera camera)
        {
            ClearFeedbackBuffer();
            
            var cmd = CommandBufferPool.Get();
            context.SetupCameraProperties(camera);
            cmd.SetRenderTarget(m_ColorTexture, m_DepthTexture);
            cmd.ClearRenderTarget(true, true, new Color(0.0f, 0.0f, 0.0f, 0.0f), 1.0f);
            cmd.SetGlobalVector("_VTFeedbackParam", new Vector4(
                m_Settings.pageSize,
                m_Settings.pageResolution,
                m_Settings.maxMipmapLevel,
                m_Settings.minMipmapLevel
            ));
            // cmd.SetGlobalBuffer("_FeedbackBuffer", m_FeedbackBuffer);
            Graphics.SetRandomWriteTarget(1, m_FeedbackBuffer);
            context.ExecuteCommandBuffer(cmd);
            RenderObjects(context, camera, "VirtualTextureFeedback");
            context.Submit();
            CommandBufferPool.Release(cmd);
            
            return ReadBack();
            // ReadBackFromColorTexture(); // too slow
        }
        
        public void Test(ScriptableRenderContext context, Camera camera)
        {
            var cmd = CommandBufferPool.Get();
            cmd.Blit(m_ColorTexture, camera.targetTexture);
            context.ExecuteCommandBuffer(cmd);
            context.Submit();
            CommandBufferPool.Release(cmd);
        }

        private List<Page> ReadBack()
        {
            m_FeedbackBuffer.GetData(m_FeedbackBufferData);
            var count = 0;
            var pages = new List<Page>();
            for (var mip = m_Settings.minMipmapLevel; mip <= m_Settings.maxMipmapLevel; ++mip)
            {
                for (var row = 0; row < m_Settings.pageSize; ++row)
                {
                    for (var col = 0; col < m_Settings.pageSize; ++col)
                    {
                        var idx = col + row * m_Settings.pageSize + mip * m_Settings.pageSize * m_Settings.pageSize;
                        if (m_FeedbackBufferData[idx] != 1) continue;
                        pages.Add(new Page(mip, row, col));
                        count++;
                    }
                }
            }
            // Debug.Log(count + ": " + pages);
            return pages;
        }
        
        // private void ReadBackFromColorTexture()
        // {
        //     var width = m_ColorTexture.width;
        //     var height = m_ColorTexture.height;
        //     var t2d = new Texture2D(width, height, TextureFormat.ARGB32, false);
        //     var oldRT = RenderTexture.active;
        //     RenderTexture.active = m_ColorTexture;
        //     t2d.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
        //     t2d.Apply();
        //     RenderTexture.active = oldRT;
        //     var pixels = t2d.GetPixels(0, 0, width, height);
        //     // var bytes = t2d.EncodeToPNG();
        //     // File.WriteAllBytes("Assets\\test.png", bytes);
        //     var pages = new HashSet<Tuple<int, int, int>>();
        //     foreach (var pixel in pixels)
        //     {
        //         if (pixel.a < 0.5f) continue;
        //         pages.Add(new Tuple<int, int, int>(
        //             (int)(pixel.r * m_Settings.pageSize),
        //             (int)(pixel.g * m_Settings.pageSize),
        //             (int)(pixel.b * m_Settings.maxMipmapLevel)
        //         ));
        //     }
        //     // Debug.Log(pages.Count);
        //     var ss = pages.Aggregate($"{pages.Count}: ", (current, page) => current + $"({page.Item1}-{page.Item2}-{page.Item3}) ");
        //     Debug.Log(ss);
        // }
     
        private int CalcFeedbackBufferLength()
        {
            return m_Settings.pageSize * m_Settings.pageSize *
                   (m_Settings.maxMipmapLevel - m_Settings.minMipmapLevel + 1);
        }

        private void ClearFeedbackBuffer()
        {
            for (var i = 0; i < m_FeedbackBufferData.Length; i++)
            {
                m_FeedbackBufferData[i] = 0;
            }
            m_FeedbackBuffer.SetData(m_FeedbackBufferData);
        }
    }
}