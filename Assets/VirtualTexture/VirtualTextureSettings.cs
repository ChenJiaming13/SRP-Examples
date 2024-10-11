namespace VirtualTexture
{
    public class VirtualTextureSettings
    {
        public int pageResolution { get; set; } = 512;

        public int pageSize { get; set; } = 8;
        
        public int minMipmapLevel { get; set; } = 0;
        
        public int maxMipmapLevel { get; set; } = 3;
    }
}