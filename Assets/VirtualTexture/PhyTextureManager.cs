using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VirtualTexture
{
    public class PhyTextureManager
    {
        private readonly VirtualTextureSettings m_Settings;
        private readonly Texture2D m_PhyTexturePool;
        private int[,] m_PhyTexturePoolStatus;
        private readonly Dictionary<Page, Texture2D> m_TextureCache = new();

        public PhyTextureManager(VirtualTextureSettings settings)
        {
            m_Settings = settings;
            m_PhyTexturePool = new Texture2D(
                m_Settings.pageResolution * m_Settings.phyPageCols,
                m_Settings.pageResolution * m_Settings.phyPageRows
            );
            m_PhyTexturePoolStatus = new int[m_Settings.phyPageRows, m_Settings.phyPageCols];
        }

        private void AddTexture(ref Page page, int row, int col)
        {
            if (!m_TextureCache.ContainsKey(page))
                m_TextureCache.Add(page, LoadTextureFromDisk(ref page));
            var texture = m_TextureCache[page];
            var x = col * m_Settings.pageResolution;
            var y = row * m_Settings.pageResolution;
            m_PhyTexturePool.SetPixels(x, y, m_Settings.pageResolution, m_Settings.pageResolution, texture.GetPixels());
        }
        
        private Texture2D LoadTextureFromDisk(ref Page page)
        {
            var texturePath = Path.Combine(m_Settings.texturesDir, $"{page.Mip}-{page.Row}-{page.Col}.png");
            if (File.Exists(texturePath))
            {
                var fileData = File.ReadAllBytes(texturePath);
                var texture = new Texture2D(m_Settings.pageResolution, m_Settings.pageResolution);
                texture.LoadImage(fileData);
                return texture;
            }
            Debug.LogError($"Texture not found at path: {texturePath}");
            return null;
        }
    }
}