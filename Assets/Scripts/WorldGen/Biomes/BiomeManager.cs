using System;
using System.Collections.Generic;
using UnityEngine;

class BiomePoint {
    public Biome biome;
    public Vector2Int point;
}


public class BiomeManager {
    private List<Biome> biomes = new List<Biome>();
    private List<BiomePoint> biomePoints = new List<BiomePoint>();

    private float borderWidth = 10;
    private float minPointDistance = 15;



    System.Random rng;

    public BiomeManager(int seed = 42) {
        rng = new System.Random(seed);

        biomes.Add(new BasicBiome());
        biomes.Add(new BasicBiome2());

        biomePoints.Add(new BiomePoint() {
            biome = biomes[0],
            point = new Vector2Int(0, 0)
        });
        biomePoints.Add(new BiomePoint() {
            biome = biomes[0],
            point = new Vector2Int(100, 100)
        });
        biomePoints.Add(new BiomePoint() {
            biome = biomes[1],
            point = new Vector2Int(0, 100)
        });
        biomePoints.Add(new BiomePoint() {
            biome = biomes[1],
            point = new Vector2Int(100, 0)
        });
    }

    /// <summary>
    /// Used to update the biomes
    /// </summary>
    public void updateBiomes() {
        // Should make sure that it is possible to get a biome for any chunk that is loaded

        throw new NotImplementedException();
    }






    /*
     * 
     * Find the closest N biomepoints and find some weighted avg. using the border width
     * so as to make a smooth transition from one biome to another
     * Need to find a way to figure out N though
     * Find the closest point and then all other points which are no further away than the border width? can then use that to do the stufferinos?
     * 
     */

    /// <summary>
    /// Gets the biome for this position. Might be from the closest biome point, or some avg. of multiple biomes
    /// </summary>
    /// <param name="pos">position to sample</param>
    /// <returns>Biome for this position</returns>
    public Biome getBiome(Vector2Int pos) {

        // Find all points in range
        List<Pair<Biome, float>> inRangeBiomes = new List<Pair<Biome, float>>();    // A lsit of biomes and their distance from the sample position

        float range = closestBiomePointDist(pos) + borderWidth;
        foreach (BiomePoint bp in biomePoints) {
            float dist = Vector2Int.Distance(pos, bp.point);
            if (dist < range) {
                inRangeBiomes.Add(new Pair<Biome, float>(bp.biome, dist));
            }
        }

        if (inRangeBiomes.Count == 1) {
            return inRangeBiomes[0].first;
        }
        return new Biome(inRangeBiomes);
    }

    /// <summary>
    /// Gets the distance from closest biome point
    /// </summary>
    /// <param name="pos">position to test</param>
    /// <returns>distance from closest biome point</returns>
    private float closestBiomePointDist(Vector2Int pos) {
        float best = float.MaxValue;
        foreach(BiomePoint bp in biomePoints) {
            float dist = Vector2Int.Distance(pos, bp.point);
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