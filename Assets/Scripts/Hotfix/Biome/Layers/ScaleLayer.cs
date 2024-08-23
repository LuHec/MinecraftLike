public class ScaleLayer : InitBiomeLayer
{
    private ScaleType mScaleType;

    public ScaleLayer(long worldSeed, long salt, ScaleType scaleType, BiomeLayer parent) : base(worldSeed, salt, parent)
    {
        this.mScaleType = scaleType;
    }

    public override int Sample(int x, int y, int z)
    {
        // map比例放大一倍，采样比例缩小一倍
        // 在放大的时候不仅仅用原版Noise放大，还加入一些dither
        InitBiomeLayer parentLayer = GetParentLayer<InitBiomeLayer>();

        // 将比例放大一倍后的采样中心
        int center = parentLayer.Get(x >> 1, y, z >> 1);

        // hash坐标seed，取-2是想取二进制头尾作为混合，不取最后一位是因为下面判断坐标用了最后一位，要减少关联性
        this.SetSeed(x & -2, z & -2);

        // 下面这段没有什么数学原理，只是为了让scale增加一些dither
        int xb = x & 1, zb = z & 1;
        if (xb == 0 && zb == 0) return center;

        // 新分辨率下的采样点：(0,0)(0,1)(1,0)(1,1)
        // 由于(0,1)和(1,1)用parentLayer.Get(x >> 1, y, (z + 1) >> 1)结果是一样的，所以通过xb来判断属于哪个
        // | N  y
        // |
        // |_______ E  x
        int north = parentLayer.Get(x >> 1, y, (z + 1) >> 1);
        if (xb == 0)
        {
            // 本层的缩放非uniform scale，而是会略微改变。
            // 非center(即原先尺寸下的采样点)的坐标，都有二分之一的机会变成center值，这样能扩散center
            return this.Choose(north, center);
        }

        int east = parentLayer.Get((x + 1) >> 1, y, z);

        if (zb == 0)
        {
            return this.Choose(east, center);
        }

        int ne = parentLayer.Get((x + 1) >> 1, y, (z + 1) >> 1);
        return this.Sample(ne, north, east, center);
    }

    // TODO:Uniform scale
    public int Sample(int n, int e, int ne, int center)
    {
        // n             ne
        // |-------------
        // |
        // |_____________
        // center        e

        int ret = this.Choose(n, e, ne, center);
        if (mScaleType == ScaleType.Fuzzy)
        {
            return ret;
        }

        // 通过地形数据均匀缩放, 哪个地形类型相同数量多就选择哪个作为采样结果，如果四个一样就随机选择一个
        if (n == e && e == ne) return e;
        if (center == e && n != ne) return center;
        if (center == n && e != ne) return center;
        if (center == ne && n != e) return center;
        if (e == n && center != ne) return e;
        if (e == ne && center != n) return e;
        if (n == ne && center != e) return n;

        return ret;
    }

    public enum ScaleType
    {
        Fuzzy,
        Normal
    }
}