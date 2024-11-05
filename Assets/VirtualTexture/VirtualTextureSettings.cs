using System;

namespace VirtualTexture
{
    [Serializable]
    public struct VirtualTextureSettings
    {
        public int pageResolution;

        public int pageSize;
        
        public int minMipmapLevel;
        
        public int maxMipmapLevel;

        public string texturesDir;

        public int phyPageRows;
        
        public int phyPageCols;
    }
}