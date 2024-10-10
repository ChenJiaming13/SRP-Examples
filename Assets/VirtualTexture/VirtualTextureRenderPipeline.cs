using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace VirtualTexture
{
    public class VirtualTextureRenderPipeline : RenderPipeline
    {
        private const int PAGE_SIZE = 8; // exp2(m_MaxMipmapLevel)
        private const int PAGE_RESOLUTION = 512;
        private const int MIN_MIPMAP_LEVEL = 0;
        private const int MAX_MIPMAP_LEVEL = 3;
        
        private readonly RenderTexture m_FeedbackTexture = new(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.Linear);
        private readonly RenderTexture m_FeedbackDepthTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.Depth,
            RenderTextureReadWrite.Linear);
        
        private ComputeBuffer m_FeedbackBuffer = new(
            PAGE_SIZE * PAGE_SIZE * (MAX_MIPMAP_LEVEL - MIN_MIPMAP_LEVEL + 1),
            sizeof(int),
            ComputeBufferType.Default
        );
        
        private int[] m_FeedbackBufferData = new int [PAGE_SIZE * PAGE_SIZE * (MAX_MIPMAP_LEVEL - MIN_MIPMAP_LEVEL + 1)];

        public VirtualTextureRenderPipeline()
        {
            for (var i = 0; i < m_FeedbackBufferData.Length; i++)
            {
                m_FeedbackBufferData[i] = 0;
            }
            m_FeedbackBuffer.SetData(m_FeedbackBufferData);
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
            var cmd = CommandBufferPool.Get();
            FeedbackPass(context, camera, cmd);
            FetchFeedbackData();
            // FeedbackRead();
            // FinalPass(context, camera, cmd);
            TestPass(context, camera, cmd);
            CommandBufferPool.Release(cmd);
        }

        private void FetchFeedbackData()
        {
            m_FeedbackBuffer.GetData(m_FeedbackBufferData);
            var ss = "";
            var count = 0;
            for (var mip = MIN_MIPMAP_LEVEL; mip <= MAX_MIPMAP_LEVEL; ++mip)
            {
                for (var row = 0; row < PAGE_SIZE; ++row)
                {
                    for (var col = 0; col < PAGE_SIZE; ++col)
                    {
                        var idx = col + row * PAGE_SIZE + mip * PAGE_SIZE * PAGE_SIZE;
                        if (m_FeedbackBufferData[idx] != 1) continue;
                        ss += $"({mip}-{row}-{col})";
                        count++;
                    }
                }
            }
            Debug.Log(count + ": " + ss);
            for (var i = 0; i < m_FeedbackBufferData.Length; i++)
            {
                m_FeedbackBufferData[i] = 0;
            }
            m_FeedbackBuffer.SetData(m_FeedbackBufferData);
        }
        
        private void FeedbackRead()
        {
            var width = m_FeedbackTexture.width;
            var height = m_FeedbackTexture.height;
            var t2d = new Texture2D(width, height, TextureFormat.ARGB32, false);
            var oldRT = RenderTexture.active;
            RenderTexture.active = m_FeedbackTexture;
            t2d.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
            t2d.Apply();
            RenderTexture.active = oldRT;
            var pixels = t2d.GetPixels(0, 0, width, height);
            // var bytes = t2d.EncodeToPNG();
            // File.WriteAllBytes("Assets\\test.png", bytes);
            
            var pages = new HashSet<Tuple<int, int, int>>();
            foreach (var pixel in pixels)
            {
                if (pixel.a < 0.5f) continue;
                pages.Add(new Tuple<int, int, int>(
                    (int)(pixel.r * PAGE_SIZE),
                    (int)(pixel.g * PAGE_SIZE),
                    (int)(pixel.b * MAX_MIPMAP_LEVEL)
                ));
            }

            Debug.Log(pages.Count);
            // var ss = pages.Aggregate($"{pages.Count}: ", (current, page) => current + $"({page.Item1}-{page.Item2}-{page.Item3}) ");
            // Debug.Log(ss);
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
            cmd.SetGlobalVector("_VTFeedbackParam", new Vector4(PAGE_SIZE, PAGE_RESOLUTION, MAX_MIPMAP_LEVEL, MIN_MIPMAP_LEVEL));
            // cmd.SetGlobalBuffer("_FeedbackBuffer", m_FeedbackBuffer);
            Graphics.SetRandomWriteTarget(2, m_FeedbackBuffer);
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