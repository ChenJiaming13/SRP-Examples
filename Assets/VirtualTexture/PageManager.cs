using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VirtualTexture
{
    public class PhyPageCoord
    {
        public readonly int Row;
        
        public readonly int Col;
        
        public PhyPageCoord(int row, int col)
        {
            Row = row;
            Col = col;
        }

        public override string ToString()
        {
            return $"({Row}-{Col})";
        }
    }
    
    public class PageManager
    {
        private readonly VirtualTextureSettings m_Settings;
        private readonly Texture2D m_PageTable;
        private readonly Page[,] m_PhysicalPages;
        private readonly bool[,] m_PhysicalPageMarks;
        private readonly Texture2D m_PhysicalTexture;
        private Dictionary<Page, Texture2D> m_VirtualTextures;
        
        public PageManager(VirtualTextureSettings settings)
        {
            m_Settings = settings;
            m_PageTable = new Texture2D(settings.pageSize, settings.pageSize, TextureFormat.RGBA32, true);
            for (var mip = 0; mip < m_PageTable.mipmapCount; mip++)
            {
                var width = Mathf.Max(1, m_PageTable.width >> mip);
                var height = Mathf.Max(1, m_PageTable.height >> mip);
                var colors = new Color[width * height];
                for (var i = 0; i < colors.Length; i++)
                {
                    colors[i] = new Color(0.0f, 0.0f, 0.0f, 0.0f);
                }
                m_PageTable.SetPixels(colors, mip);
            }
            m_PageTable.Apply(false);
            m_PhysicalPages = new Page[settings.phyPageRows, settings.phyPageCols];
            m_PhysicalPageMarks = new bool[settings.phyPageRows, settings.phyPageCols];
            m_PhysicalTexture = new Texture2D(
                settings.pageResolution * settings.phyPageRows,
                settings.pageResolution * settings.phyPageCols,
                TextureFormat.RGBA32,
                false
            );
            m_VirtualTextures = new Dictionary<Page, Texture2D>();
        }
        
        public void RequestPages(List<Page> pages)
        {
            Debug.Log($"curr frame needed: {pages.Count}");
            // reset mark flags
            for (var i = 0; i < m_PhysicalPageMarks.GetLength(0); i++)
            {
                for (var j = 0; j < m_PhysicalPageMarks.GetLength(1); j++)
                {
                    m_PhysicalPageMarks[i, j] = false;
                }
            }
            // lock cached pages
            var uncachedPages = new List<Page>();
            foreach (var page in pages)
            {
                var phyPageCoord = QueryPhyPageCoord(page);
                if (phyPageCoord != null) m_PhysicalPageMarks[phyPageCoord.Row, phyPageCoord.Col] = true;
                else uncachedPages.Add(page);
            }
            // request physical pages
            if (uncachedPages.Count > 0)
            {
                var ss = uncachedPages.Aggregate("", (current, uncachedPage) => current + (uncachedPage + " "));
                Debug.Log($"num of uncached pages: {uncachedPages.Count} | {ss}");
            }
            foreach (var uncachedPage in uncachedPages)
            {
                var phyPageCoord = AllocatePhysicalPage(out var oldPage);
                if (phyPageCoord == null)
                {
                    // Debug.LogWarning(
                    //     $"virtual page: {uncachedPage} cannot be cached because there is not enough space");
                    continue;
                }
                var pte = new Color(
                    phyPageCoord.Row * 1.0f / m_Settings.phyPageRows,
                    phyPageCoord.Col * 1.0f / m_Settings.phyPageCols,
                    0.0f,
                    1.0f
                );
                // If it is a page that is not needed for the current frame but has been cached to the physical pages
                // it needs to be reset pte by m_PageTable.SetPixel
                if (oldPage != null)
                {
                    m_PageTable.SetPixel(oldPage.Col, oldPage.Row, new Color(0.0f, 0.0f, 0.0f, 0.0f), oldPage.Mip);
                }
                m_PageTable.SetPixel(uncachedPage.Col, uncachedPage.Row, pte, uncachedPage.Mip);
                m_PhysicalPages[phyPageCoord.Row, phyPageCoord.Col] = uncachedPage;
                m_PhysicalPageMarks[phyPageCoord.Row, phyPageCoord.Col] = true;
            }
            m_PageTable.Apply(false);
            // TODO: 加载当前未缓存的Page到Physical Pages
        }

        private PhyPageCoord QueryPhyPageCoord(Page page)
        {
            var pte = m_PageTable.GetPixel(page.Col, page.Row, page.Mip);
            if (pte.a == 0.0f) return null;
            var row = Mathf.RoundToInt(pte.r * m_Settings.phyPageRows);
            var col = Mathf.RoundToInt(pte.g * m_Settings.phyPageCols);
            return new PhyPageCoord(row, col);
        }

        private PhyPageCoord AllocatePhysicalPage(out Page page)
        {
            for (var row = 0; row < m_PhysicalPageMarks.GetLength(0); row++)
            {
                for (var col = 0; col < m_PhysicalPageMarks.GetLength(1); col++)
                {
                    if (m_PhysicalPageMarks[row, col]) continue;
                    page = m_PhysicalPages[row, col];
                    return new PhyPageCoord(row, col);
                }
            }
            page = null;
            return null;
        }
    }
}