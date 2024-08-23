public abstract class CrossLayer : InitBiomeLayer
{
    protected CrossLayer(long worldSeed, long salt, BiomeLayer parent) : base(worldSeed, salt, parent)
    {
    }

    public override int Sample(int x, int y, int z)
    {
        this.SetSeed(x, z);
        
        return Sample(
            this.GetParentLayer<InitBiomeLayer>().Get(x - 1, y, z + 1),
            this.GetParentLayer<InitBiomeLayer>().Get(x + 1, y, z + 1),
            this.GetParentLayer<InitBiomeLayer>().Get(x - 1, y, z - 1),
            this.GetParentLayer<InitBiomeLayer>().Get(x + 1, y, z - 1),
            this.GetParentLayer<InitBiomeLayer>().Get(x, y, z)
        );
    }

    public abstract int Sample(int nw, int ne, int sw, int se, int center);
}