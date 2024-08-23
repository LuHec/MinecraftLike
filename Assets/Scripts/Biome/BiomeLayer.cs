using ThirdParty;
using UnityEngine;

public abstract class BiomeLayer
{
    protected long layerSeed;

    protected long localSeed;

    // A little pram ues to generate different seeds 
    protected long salt;
    private readonly BiomeLayer parentLayer;

    public BiomeLayer(long worldSeed, long salt, BiomeLayer parent)
    {
        this.salt = salt;
        this.layerSeed = BiomeLayer.GetLayerSeed(worldSeed, this.salt);
        this.parentLayer = parent;
    }

    public int NextInt(int bound)
    {
        int res = Mathf.FloorToInt((localSeed >> 24) % bound);
        // LCG Mixer,这里混合的意义是让同一层同一个坐标采样时(local seed set by xyz)，多个nextInt运算有不同的种子
        localSeed = SeedMixer.MixSeed(layerSeed, localSeed);
        return res;
    }

    public int Choose(int a, int b)
    {
        return this.NextInt(2) == 0 ? a : b;
    }

    public int Choose(int a, int b, int c, int d)
    {
        int i = this.NextInt(4);
        return i switch
        {
            0 => a,
            1 => b,
            2 => c,
            _ => d
        };
    }

    public BiomeLayer GetParentLayer() => parentLayer;

    public T GetParentLayer<T>() where T : BiomeLayer => (T)this.GetParentLayer();

    /// <summary>
    /// Hash for seed and coordinate
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    public void SetSeed(int x, int z)
    {
        this.localSeed = BiomeLayer.GetLocalSeed(this.layerSeed, x, z);
    }

    public static long GetLayerSeed(long worldSeed, long salt)
    {
        worldSeed = SeedMixer.MixSeed(worldSeed, salt);
        worldSeed = SeedMixer.MixSeed(worldSeed, salt);
        worldSeed = SeedMixer.MixSeed(worldSeed, salt);
        worldSeed = SeedMixer.MixSeed(worldSeed, salt);
        return worldSeed;
    }

    public static long GetLocalSeed(long layerSeed, long salt)
    {
        layerSeed = SeedMixer.MixSeed(layerSeed, salt);
        layerSeed = SeedMixer.MixSeed(layerSeed, salt);
        layerSeed = SeedMixer.MixSeed(layerSeed, salt);
        layerSeed = SeedMixer.MixSeed(layerSeed, salt);
        return layerSeed;
    }

    /// <summary>
    /// Generate different seeds with coordinate
    /// </summary>
    /// <param name="layerSeed"></param>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public static long GetLocalSeed(long layerSeed, int x, int z)
    {
        layerSeed = SeedMixer.MixSeed(layerSeed, x);
        layerSeed = SeedMixer.MixSeed(layerSeed, z);
        layerSeed = SeedMixer.MixSeed(layerSeed, x);
        layerSeed = SeedMixer.MixSeed(layerSeed, z);
        return layerSeed;
    }
}