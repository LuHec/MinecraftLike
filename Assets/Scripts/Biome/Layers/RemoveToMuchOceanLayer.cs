public class RemoveToMuchOceanLayer : CrossLayer
{
    public RemoveToMuchOceanLayer(long worldSeed, long salt, BiomeLayer parent) : base(worldSeed, salt, parent)
    {
    }

    protected override int Sample(int nw, int ne, int sw, int se, int center)
    {
        return Biome.ApplyAll(Biome.IsShallowOcean, center, nw, ne, sw, se) && this.NextInt(2) == 0
            ? Biomes.PLAIN.ID
            : center;
    }
}