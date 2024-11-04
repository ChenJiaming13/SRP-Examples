namespace VirtualTexture
{
    public class VirtualTextureSettings
    {
        public int pageResolution { get; set; } = 512;

        public int pageSize { get; set; } = 8;
        
        public int minMipmapLevel { get; set; } = 0;
        
        public int maxMipmapLevel { get; set; } = 3;

        public string texturesDir { get; set; } = "Assets/VirtualTexture/TestTextures";

        public int phyPageRows { get; set; } = 4;
        public int phyPageCols { get; set; } = 3;
    }
}