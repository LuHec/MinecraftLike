/// <summary>
/// 大陆层，海洋与大陆比例为10:1
/// </summary>
public class ContinentLayer : InitBiomeLayer
{
    public ContinentLayer(long worldSeed, long salt, BiomeLayer parent) : base(worldSeed, salt, parent)
    {
    }

    public override int Sample(int x, int y, int z)
    {
        this.SetSeed(x, z);
        // Plane 0, Water 1
        return x == 0 && z == 0 || this.NextInt(10) == 0 ? Biomes.PLAIN.ID : Biomes.OCEAN.ID;
    }
}