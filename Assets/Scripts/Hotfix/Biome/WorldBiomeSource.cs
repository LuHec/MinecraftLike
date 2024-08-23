using System.Collections.Generic;

public class WorldBiomeSource : LayerStack<InitBiomeLayer>
{
    public InitBiomeLayer baseLayer;

    public WorldBiomeSource(long worldSeed) : base(worldSeed)
    {
        Build();
    }

    public void Build()
    {
        this.Add(baseLayer = new ContinentLayer(this.GetWorldSeed(), 1L, baseLayer));
        this.Add(baseLayer = new ScaleLayer(this.GetWorldSeed(), 2000L, ScaleLayer.ScaleType.Fuzzy, baseLayer));
        this.Add(baseLayer = new AddLandLayer(this.GetWorldSeed(), 1L, baseLayer));
        this.Add(baseLayer = new ScaleLayer(this.GetWorldSeed(), 2001L, ScaleLayer.ScaleType.Normal, baseLayer));
        this.Add(baseLayer = new AddLandLayer(this.GetWorldSeed(), 3L, baseLayer));
        this.Add(baseLayer = new AddLandLayer(this.GetWorldSeed(), 4L, baseLayer));
        this.Add(baseLayer = new AddLandLayer(this.GetWorldSeed(), 5L, baseLayer));
        this.Add(baseLayer = new RemoveToMuchOceanLayer(this.GetWorldSeed(), 2L, baseLayer));
        this.Add(baseLayer = new ScaleLayer(this.GetWorldSeed(), 2002L, ScaleLayer.ScaleType.Normal, baseLayer));


        // this.Add(baseLayer = new ScaleLayer(this.GetWorldSeed(), 2003L, baseLayer));
    }

    public int GetBiome(int x, int y, int z)
    {
        return baseLayer.Sample(x, y, z);
    }
}