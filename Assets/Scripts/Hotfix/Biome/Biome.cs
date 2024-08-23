using System;
using System.Collections.Generic;
using UnityEngine;

public class Biome
{
    public Biome(int id, Color color)
    {
        this.id = id;
        this.color = color;
    }

    public int ID => id;
    public Color Color => color;

    private int id;
    private Color color;

    public static bool IsShallowOcean(int id)
    {
        return Biomes.GetBiome(id) == Biomes.OCEAN;
    }

    public static int IsEqualsOrDefault(int a, int b, int fallback)
    {
        return a == b ? a : fallback;
    }

    public static bool ApplyAll(Func<int, bool> func, params int[] param)
    {
        foreach (var i in param)
        {
            if (!func(i)) return false;
        }

        return true;
    }
}

public class Biomes
{
    private static Dictionary<int, Biome> biomeDict = new();

    public static readonly Biome OCEAN = Register(new Biome(0, new Color(0,0,(float)112/255)));
    public static readonly Biome PLAIN = Register(new Biome(1, new Color((float)141/255,(float)179/255,(float)96/255)));
    public static readonly Biome FOREST = Register(new Biome(2, Color.green));

    public static Biome GetBiome(int id) => biomeDict[id];

    private static Biome Register(Biome biome)
    {
        biomeDict.Add(biome.ID, biome);
        return biome;
    }
}