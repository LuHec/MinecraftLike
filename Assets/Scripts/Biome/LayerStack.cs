using System.Collections.Generic;

public class LayerStack<T> : BiomeSource where T : InitBiomeLayer
{
    protected List<T> layerStack = new();

    public LayerStack(long worldSeed) : base(worldSeed)
    {
    }

    public int StkCount => layerStack.Count;

    public T Add(T layer)
    {
        layerStack.Add(layer);
        return layer;
    }

    public T GetLayer(int idx)
    {
        if (idx < 0 || idx >= StkCount) return null;
        else return layerStack[idx];
    }

    public int Sample(int x, int y, int z)
    {
        return 0;
    }
}