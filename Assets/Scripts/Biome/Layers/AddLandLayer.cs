public class AddLandLayer : CrossLayer
{
    public AddLandLayer(long worldSeed, long salt, BiomeLayer parent) : base(worldSeed, salt, parent)
    {
    }

    protected override int Sample(int nw, int ne, int sw, int se, int center)
    {
        // 这一层的目标是让海岸线有分形状态，即海水可以侵蚀海岸，海岸也能扩散
        // 在浅水区附近时，陆地有几率变为海水，海水也有几率变成陆地。

        // 处理中心不是浅水，或四周都是浅水
        if (!Biome.IsShallowOcean(center) || Biome.ApplyAll(Biome.IsShallowOcean, nw, ne, sw, se))
        {
            if (Biome.IsShallowOcean(center) || !Biome.ApplyAll(Biome.IsShallowOcean, nw, ne, sw, se) ||
                this.NextInt(5) != 0)
            {
                return center;
            }

            if (Biome.IsShallowOcean(nw))
            {
                return Biome.IsEqualsOrDefault(Biomes.FOREST.ID, center, nw);
            }

            if (Biome.IsShallowOcean(ne))
            {
                return Biome.IsEqualsOrDefault(Biomes.FOREST.ID, center, ne);
            }

            if (Biome.IsShallowOcean(sw))
            {
                return Biome.IsEqualsOrDefault(Biomes.FOREST.ID, center, sw);
            }

            if (Biome.IsShallowOcean(se))
            {
                return Biome.IsEqualsOrDefault(Biomes.FOREST.ID, center, se);
            }

            return center;
        }
        
        // 处理中心是浅水且四周不全是浅水的情况，有机会变成海岸
        int prb = 1;
        int res = 1;
        
        if (!Biome.IsShallowOcean(nw) && this.NextInt(prb++) == 0)
        {
            res = nw;
        }
        
        if (!Biome.IsShallowOcean(ne) && this.NextInt(prb++) == 0)
        {
            res = ne;
        }
        
        if (!Biome.IsShallowOcean(sw) && this.NextInt(prb++) == 0)
        {
            res = sw;
        }
        
        if (!Biome.IsShallowOcean(se) && this.NextInt(prb) == 0)
        {
            res = se;
        }
        
        if (this.NextInt(3) == 0) return res;
        return Biome.IsEqualsOrDefault(res, Biomes.FOREST.ID, center);
    }
}