public abstract class BiomeSource
{
    private readonly long worldSeed;

    public BiomeSource(long worldSeed)
    {
        this.worldSeed = worldSeed;
    }

    protected long GetWorldSeed()
    {
        return worldSeed;
    }
}