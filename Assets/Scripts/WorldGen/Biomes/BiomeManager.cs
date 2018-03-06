using System;
using System.Collections.Generic;
using UnityEngine;

public class BiomeManager {
    private List<Biome> biomes = new List<Biome>();
    private List<Pair<Biome, Vector2Int>> biomePoints = new List<Pair<Biome, Vector2Int>>();

    private Pair<Biome, Vector2Int>[,] biomeGrid;
    private int gridScale = 100; // Number of meters per grid-element

    private PoissonDiscSampler poissonSampler;
    private int radius = 5;
    private int gridWidth = 500;
    private int gridHeight = 500;


    public static float borderWidth = 75;

    private System.Random rng;


    public BiomeManager(ChunkManager chunkManager) {
        rng = new System.Random(ChunkConfig.seed);

        biomes.Add(new BasicBiome());
        biomes.Add(new BasicBiome2());
        biomes.Add(new BasicBiome3());

        biomeGrid = new Pair<Biome, Vector2Int>[gridWidth, gridHeight];

        poissonSampler = new PoissonDiscSampler(radius, gridWidth, gridHeight, wrap: true, seed: ChunkConfig.seed);

        Vector2Int offset = new Vector2Int(gridWidth/2, gridHeight/2);
        foreach(Vector2Int sample in poissonSampler.sample()) {
            biomeGrid[sample.x, sample.y] = new Pair<Biome, Vector2Int>(biomes[rng.Next(0, biomes.Count)], (sample - offset) * gridScale);

            //biomePoints.Add(new Pair<Biome, Vector2Int>(biomes[rng.Next(0, biomes.Count)], (sample - offset) * 150));
            if(chunkManager != null)
                chunkManager.generateBiomeBeacon((sample - offset) * gridScale);
        }

    }



    /// <summary>
    /// Gets the biomes for this position.
    /// </summary>
    /// <param name="pos">position to sample</param>
    /// <returns>Biomes for this position and the distance from the sample pos to the biome point</returns>
    public List<Pair<Biome, float>> getInRangeBiomes(Vector2Int pos) {

        // Find all points in range
        List<Pair<Biome, float>> inRangeBiomes = new List<Pair<Biome, float>>();    // Might want to separate the biome from the weight, because we often just use one of them at the time.

        float range = closestBiomePointDist(pos) + borderWidth;
        /*
        foreach (Pair<Biome, Vector2Int> bp in biomePoints) {
            float dist = Vector2Int.Distance(pos, bp.second);
            if (dist < range) {
                inRangeBiomes.Add(new Pair<Biome, float>(bp.first, dist));
            }
        }/*/

        Vector2Int offset = new Vector2Int(gridWidth / 2, gridHeight / 2);
        Vector2Int posInGrid = new Vector2Int(pos.x / gridScale, pos.y / gridScale) + offset;
        int r3 = radius * 3;
        for (int x = posInGrid.x - r3; x < posInGrid.x + r3; x++) {
            for (int y = posInGrid.y - r3; y < posInGrid.y + r3; y++) {
                int gridX = Utils.mod(x, gridWidth);
                int gridY = Utils.mod(y, gridHeight);

                if (biomeGrid[gridX, gridY] != null) {
                    float dist = Vector2Int.Distance(pos, biomeGrid[gridX, gridY].second);
                    if (dist < range) {
                        inRangeBiomes.Add(new Pair<Biome, float>(biomeGrid[gridX, gridY].first, dist));
                    }
                }
            }
        }/**/


        if (inRangeBiomes.Count == 1) {
            inRangeBiomes[0].second = 1;
            return inRangeBiomes;
        }


        // Calculate the weights
        float sD = float.MaxValue;
        float bW = borderWidth;
        foreach (Pair<Biome, float> p in inRangeBiomes) {
            if (p.second < sD)
                sD = p.second;
        }
        float tot = 0;
        foreach (Pair<Biome, float> p in inRangeBiomes) {
            p.second = 1 - (p.second - sD)/bW;
            tot += p.second;
        }
        foreach (Pair<Biome, float> p in inRangeBiomes) {
            p.second = p.second / tot;
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
        /*
        foreach (Pair<Biome, Vector2Int> bp in biomePoints) {
            if(bp != null) {
                float dist = Vector2Int.Distance(pos, bp.second);
                if (dist < best) {
                    best = dist;
                }
            }
        }/*/
        int r3 = radius * 3;
        Vector2Int offset = new Vector2Int(gridWidth / 2, gridHeight / 2);
        Vector2Int posInGrid = new Vector2Int(pos.x / gridScale, pos.y / gridScale) + offset;
        for (int x = posInGrid.x - r3; x < posInGrid.x + r3; x++) {
            for (int y = posInGrid.y - r3; y < posInGrid.y + r3; y++) {
                int gridX = Utils.mod(x, gridWidth);
                int gridY = Utils.mod(y, gridHeight);

                if (biomeGrid[gridX, gridY] != null) {
                    float dist = Vector2Int.Distance(pos, biomeGrid[gridX, gridY].second);
                    if (dist < best) {
                        best = dist;
                    }
                }
            }
        }/**/


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