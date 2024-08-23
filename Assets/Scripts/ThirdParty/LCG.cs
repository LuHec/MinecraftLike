using System;

namespace ThirdParty
{
    public class LCG
    {
        public static readonly LCG CC65_M23 = new LCG(65793L, 4282663L, 1L << 23);
        public static readonly LCG VISUAL_BASIC = new LCG(1140671485L, 12820163L, 1L << 24);
        public static readonly LCG RTL_UNIFORM = new LCG(2147483629L, 2147483587L, (1L << 31) - 1);
        public static readonly LCG MINSTD_RAND0_C = new LCG(16807L, 0L, (1L << 31) - 1);
        public static readonly LCG MINSTD_RAND_C = new LCG(48271, 0L, (1L << 31) - 1);
        public static readonly LCG CC65_M31 = new LCG(16843009L, 826366247L, 1L << 23);
        public static readonly LCG RANDU = new LCG(65539L, 0L, 1L << 31);
        public static readonly LCG GLIB_C = new LCG(1103515245L, 12345L, 1L << 31);
        public static readonly LCG BORLAND_C = new LCG(22695477L, 1L, 1L << 32);
        public static readonly LCG PASCAL = new LCG(134775813L, 1L, 1L << 32);
        public static readonly LCG OPEN_VMS = new LCG(69069L, 1L, 1L << 32);
        public static readonly LCG NUMERICAL_RECIPES = new LCG(1664525L, 1013904223L, 1L << 32);
        public static readonly LCG MS_VISUAL_C = new LCG(214013L, 2531011L, 1L << 32);
        public static readonly LCG JAVA = new LCG(25214903917L, 11L, 1L << 48);
        public static readonly LCG JAVA_UNIQUIFIER_OLD = new LCG(181783497276652981L, 0L);
        public static readonly LCG JAVA_UNIQUIFIER_NEW = new LCG(1181783497276652981L, 0L);
        public static readonly LCG MMIX = new LCG(6364136223846793005L, 1442695040888963407L);
        public static readonly LCG NEWLIB_C = new LCG(6364136223846793005L, 1L);
        public static readonly LCG XKCD = new LCG(0L, 4L);

        public readonly long Multiplier;
        public readonly long Addend;
        public readonly long Modulus;

        private readonly bool _isPowerOf2;
        private readonly int _trailingZeros;

        public LCG(long multiplier, long addend) : this(multiplier, addend, 0)
        {
        }

        public LCG(long multiplier, long addend, long modulus)
        {
            Multiplier = multiplier;
            Addend = addend;
            Modulus = modulus;

            _isPowerOf2 = IsPowerOf2(Modulus);
            _trailingZeros = _isPowerOf2 ? NumberOfTrailingZeros(Modulus) : -1;
        }

        public static LCG Combine(params LCG[] lcgs)
        {
            LCG lcg = lcgs[0];

            for (int i = 1; i < lcgs.Length; i++)
            {
                lcg = lcg.Combine(lcgs[i]);
            }

            return lcg;
        }

        public bool IsModPowerOf2()
        {
            return _isPowerOf2;
        }

        public int GetModTrailingZeroes()
        {
            return _trailingZeros;
        }

        public bool IsMultiplicative()
        {
            return Addend == 0;
        }

        public long NextSeed(long seed)
        {
            return Mod(seed * Multiplier + Addend);
        }

        public long Mod(long n)
        {
            if (IsModPowerOf2())
            {
                return n & (Modulus - 1);
            }
            else if (n <= 1L << 32)
            {
                return n % Modulus;
            }

            throw new NotSupportedException();
        }

        public LCG Combine(long steps)
        {
            long multiplier = 1;
            long addend = 0;

            long intermediateMultiplier = Multiplier;
            long intermediateAddend = Addend;

            for (long k = steps; k != 0; k >>= 1)
            {
                if ((k & 1) != 0)
                {
                    multiplier *= intermediateMultiplier;
                    addend = intermediateMultiplier * addend + intermediateAddend;
                }

                intermediateAddend = (intermediateMultiplier + 1) * intermediateAddend;
                intermediateMultiplier *= intermediateMultiplier;
            }

            multiplier = Mod(multiplier);
            addend = Mod(addend);

            return new LCG(multiplier, addend, Modulus);
        }

        public LCG Combine(LCG lcg)
        {
            if (Modulus != lcg.Modulus)
            {
                throw new NotSupportedException();
            }

            return new LCG(Multiplier * lcg.Multiplier, lcg.Multiplier * Addend + lcg.Addend, Modulus);
        }

        public LCG Invert()
        {
            return Combine(-1);
        }

        public long Distance(long seed1, long seed2)
        {
            throw new NotSupportedException("DiscreteLog is not supported by this LCG");
        }

        public override bool Equals(object obj)
        {
            if (obj == this) return true;
            if (!(obj is LCG)) return false;
            LCG lcg = (LCG)obj;
            return Multiplier == lcg.Multiplier && Addend == lcg.Addend && Modulus == lcg.Modulus;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Multiplier, Addend, Modulus);
        }

        public override string ToString()
        {
            return $"LCG{{multiplier={Multiplier}, addend={Addend}, modulo={Modulus}}}";
        }

        public string ToPrettyString()
        {
            return
                $"Multiplier: 0x{Multiplier:X} ({Multiplier}), Addend: 0x{Addend:X} ({Addend}), Modulo: 0x{Modulus:X} ({Modulus})";
        }

        private static bool IsPowerOf2(long x)
        {
            return (x & (x - 1)) == 0;
        }

        private static int NumberOfTrailingZeros(long x)
        {
            if (x == 0) return 64;
            int n = 1;
            if ((x & 0xFFFFFFFF) == 0)
            {
                n += 32;
                x >>= 32;
            }

            if ((x & 0xFFFF) == 0)
            {
                n += 16;
                x >>= 16;
            }

            if ((x & 0xFF) == 0)
            {
                n += 8;
                x >>= 8;
            }

            if ((x & 0xF) == 0)
            {
                n += 4;
                x >>= 4;
            }

            if ((x & 0x3) == 0)
            {
                n += 2;
                x >>= 2;
            }

            return n - (int)(x & 1);
        }
    }
}