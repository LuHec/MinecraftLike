public abstract class InitBiomeLayer : BiomeLayer
{
    private IntLayerCache mLayerCache = new(1024);
    
    protected InitBiomeLayer(long worldSeed, long salt, BiomeLayer parent) : base(worldSeed, salt, parent)
    {
    }

    /// <summary>
    /// !!!!!!WARNING!!!!!!-------YOU SHOULD CALL GETBIOME INSTEAD OF SAMPLE-------!!!!!!WARNING!!!!!!
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public abstract int Sample(int x, int y, int z);
    
    public int Get(int x, int y, int z)
    {
        // TODO: 采样缓存器，用xyz构造哈希存储，加速采样。详情参照LayerCache
        return this.mLayerCache.Get(x, y, z, this.Sample);
    }
    
    
}