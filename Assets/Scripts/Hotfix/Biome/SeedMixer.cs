using ThirdParty;

/// <summary>
/// Work by LCG Mixer
/// </summary>
public class SeedMixer
{
    private readonly long A = LCG.MMIX.Multiplier;
    private readonly long B = LCG.MMIX.Addend;
    
    
    public SeedMixer()
    {
        
    }
    
    public static long MixSeed(long seed, long salt)
    {
        seed *= seed * LCG.MMIX.Multiplier + LCG.MMIX.Addend;
        seed += salt;
        return seed;
    }
}