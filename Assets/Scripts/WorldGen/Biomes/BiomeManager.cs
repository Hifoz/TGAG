using System;
using System.Collections.Generic;
using UnityEngine;


public class BiomeManager {
    private List<Biome> biomes = new List<Biome>();
    private List<Pair<Biome, Vector2Int>> biomePoints = new List<Pair<Biome, Vector2Int>>();

    public static float borderWidth = 10;
    System.Random rng;


    public BiomeManager(int seed = 42) {
        rng = new System.Random(seed);

        biomes.Add(new BasicBiome());
        biomes.Add(new BasicBiome2());

        biomePoints.Add(new Pair<Biome, Vector2Int>(biomes[0], new Vector2Int(0, 0)));
        biomePoints.Add(new Pair<Biome, Vector2Int>(biomes[1], new Vector2Int(100, 0)));
    }

    /// <summary>
    /// Used to generate new Biome Points when necessary
    /// </summary>
    public void updateBiomes() {
        throw new NotImplementedException("BiomeManager.updateBiomes() not yet implemented.");
    }
    


    /// <summary>
    /// Gets the biomes for this position.
    /// </summary>
    /// <param name="pos">position to sample</param>
    /// <returns>Biomes for this position and the distance from the sample pos to the biome point</returns>
    public List<Pair<Biome, float>> getInRangeBiomes(Vector2Int pos) {

        // Find all points in range
        List<Pair<Biome, float>> inRangeBiomes = new List<Pair<Biome, float>>();    // A list of biomes and their distance from the sample position

        float range = closestBiomePointDist(pos) + borderWidth;
        foreach (Pair<Biome, Vector2Int> bp in biomePoints) {
            float dist = Vector2Int.Distance(pos, bp.second);
            if (dist < range) {
                inRangeBiomes.Add(new Pair<Biome, float>(bp.first, dist));
            }
        }

        return inRangeBiomes;
    }

    /// <summary>
    /// Gets the distance from closest biome point
    /// </summary>
    /// <param name="pos">position to test</param>
    /// <returns>distance from closest biome point</returns>
    private float closestBiomePointDist(Vector2Int pos) {
        float best = float.MaxValue;
        foreach(Pair<Biome, Vector2Int> bp in biomePoints) {
            float dist = Vector2Int.Distance(pos, bp.second);
            if(dist < best) {
                best = dist;
            }
        }
        return best;
    }



    /// <summary>
    /// Loads biomes from a folder
    /// </summary>
    /// <param name="folderpath">path to folder containing biome files</param>
    public void loadFromFile(String folderpath) {
        throw new NotImplementedException("BiomeManager.loadFromFile(...) not yet implemented.");
    }
}