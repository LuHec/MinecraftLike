using System;
using System.Linq;

public class IntLayerCache
{
    private readonly long[] keys;
    private readonly int[] values;
    private readonly int mask;

    public IntLayerCache(int capacity)
    {
        if (capacity < 1 || !IsPowerOf2(capacity))
        {
            throw new InvalidOperationException("Capacity must be a power of 2");
        }

        this.keys = Enumerable.Repeat(-1L, capacity).ToArray();
        this.values = new int[capacity];
        this.mask = (int)GetMask(NumberOfTrailingZeros(capacity));
    }

    public int Get(int x, int y, int z, Func<int, int, int, int> sampler)
    {
        long key = this.UniqueHash(x, y, z);
        int id = this.Murmur64(key) & this.mask;

        if (this.keys[id] == key)
        {
            return this.values[id];
        }

        int value = sampler(x, y, z);
        this.keys[id] = key;
        this.values[id] = value;
        return value;
    }

    public int GetWithoutStore(int x, int y, int z, Func<int, int, int, int> sampler)
    {
        long key = this.UniqueHash(x, y, z);
        int id = this.Murmur64(key) & this.mask;

        if (this.keys[id] == key)
        {
            return this.values[id];
        }

        return sampler(x, y, z);
    }

    public int ForceStoreAndGet(int x, int y, int z, Func<int, int, int, int> sampler)
    {
        long key = this.UniqueHash(x, y, z);
        int id = this.Murmur64(key) & this.mask;
        int value = sampler(x, y, z);
        this.keys[id] = key;
        this.values[id] = value;
        return value;
    }

    public long UniqueHash(int x, int y, int z)
    {
        long hash = (long)x & GetMask(26);
        hash |= ((long)z & GetMask(26)) << 26;
        hash |= ((long)y & GetMask(8)) << 52;
        return hash;
    }

    public int Murmur64(long value)
    {
        value ^= RightMoveL(value, 33);
        value *= unchecked((long)0xFF51AFD7ED558CCDL);
        value ^= RightMoveL(value, 33);
        value *= unchecked((long)0xC4CEB9FE1A85EC53L);
        value ^= RightMoveL(value, 33);
        return (int)value;
    }

    private static bool IsPowerOf2(int x)
    {
        return (x & (x - 1)) == 0;
    }

    private static long GetMask(int bits)
    {
        return (1L << bits) - 1;
    }

    private static int NumberOfTrailingZeros(int x)
    {
        if (x == 0) return 32;
        int n = 31;
        int y = x << 16;
        if (y != 0)
        {
            n -= 16;
            x = y;
        }

        y = x << 8;
        if (y != 0)
        {
            n -= 8;
            x = y;
        }

        y = x << 4;
        if (y != 0)
        {
            n -= 4;
            x = y;
        }

        y = x << 2;
        if (y != 0)
        {
            n -= 2;
            x = y;
        }

        return n - RightMoveI((x << 1), 31);
    }

    /// <summary>
    /// 无符号右移, 无论正负高位补0
    /// </summary>
    /// <param name="value"></param>
    /// <param name="pos"></param>
    /// <returns></returns>
    public static long RightMoveL(long value, int pos)
    {
        //移动 0 位时直接返回原值
        if (pos != 0)
        {
            long mask = long.MaxValue;
            //无符号整数最高位不表示正负但操作数还是有符号的，有符号数右移1位，正数时高位补0，负数时高位补1
            value >>= 1;
            //和整数最大值进行逻辑与运算，运算后的结果为忽略表示正负值的最高位
            value &= mask;
            //逻辑运算后的值无符号，对无符号的值直接做右移运算，计算剩下的位
            value >>= pos - 1;
        }

        return value;
    }

    /// <summary>
    /// 无符号右移, 无论正负高位补0
    /// </summary>
    /// <param name="value"></param>
    /// <param name="pos"></param>
    /// <returns></returns>
    public static int RightMoveI(int value, int pos)
    {
        //移动 0 位时直接返回原值
        if (pos != 0)
        {
            int mask = int.MaxValue;
            //无符号整数最高位不表示正负但操作数还是有符号的，有符号数右移1位，正数时高位补0，负数时高位补1
            value >>= 1;
            //和整数最大值进行逻辑与运算，运算后的结果为忽略表示正负值的最高位
            value &= mask;
            //逻辑运算后的值无符号，对无符号的值直接做右移运算，计算剩下的位
            value >>= pos - 1;
        }

        return value;
    }
}