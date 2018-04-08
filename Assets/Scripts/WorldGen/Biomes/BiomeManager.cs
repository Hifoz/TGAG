using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class BiomeManager {
    private List<BiomeBase> biomes = new List<BiomeBase>();
    private List<Pair<BiomeBase, Vector2Int>> biomePoints = new List<Pair<BiomeBase, Vector2Int>>();

    private Pair<BiomeBase, Vector2Int>[,] biomeGrid;
    private int gridScale = 100; // Number of meters per grid-element

    private PoissonDiscSampler poissonSampler;
    private int radius = 5;
    private int gridWidth = 500;
    private int gridHeight = 500;


    public static float borderWidth = 50;

    private System.Random rng;

    #region public functions

    /// <summary>
    /// Constructor
    /// </summary>
    public BiomeManager() {
        rng = new System.Random(WorldGenConfig.seed);

        // Initialize biomes
        biomes.Add(new BasicBiome());
        biomes.Add(new MountainBiome());
        biomes.Add(new LowlandForestBiome());
        biomes.Add(new DesertBiome());


        // Initialize biome points
        biomeGrid = new Pair<BiomeBase, Vector2Int>[gridWidth, gridHeight];
        poissonSampler = new PoissonDiscSampler(radius, gridWidth, gridHeight, wrap: true, seed: WorldGenConfig.seed);
        Vector2Int offset = new Vector2Int(gridWidth/2, gridHeight/2);
        foreach (Vector2Int sample in poissonSampler.sample()) {
            biomeGrid[sample.x, sample.y] = new Pair<BiomeBase, Vector2Int>(biomes[rng.Next(0, biomes.Count)], (sample - offset) * gridScale);
        }
    }


    /// <summary>
    /// Gets the biomes for this position.
    /// </summary>
    /// <param name="pos">position to sample</param>
    /// <returns>Biomes for this position and the distance from the sample pos to the biome point</returns>
    public List<Pair<BiomeBase, float>> getInRangeBiomes(Vector2Int pos) {

        pos = modifyPosition(pos);


        // Find all points in range
        List<Pair<BiomeBase, float>> inRangeBiomes = new List<Pair<BiomeBase, float>>();    // Might want to separate the biome from the weight, because we often just use one of them at the time.

        float closestPointDistance = closestBiomePoint(pos).second;
        float range = closestPointDistance + borderWidth;

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
                        inRangeBiomes.Add(new Pair<BiomeBase, float>(biomeGrid[gridX, gridY].first, dist));
                    }
                }
            }
        }

        // Calculate the weights:

        if (inRangeBiomes.Count == 1) {
            inRangeBiomes[0].second = 1;
            return inRangeBiomes;
        }


        float tot = 0;
        foreach (Pair<BiomeBase, float> p in inRangeBiomes) {
            p.second = Mathf.Pow(1 - (p.second - closestPointDistance) / borderWidth, 2);
            tot += p.second;
        }
        foreach (Pair<BiomeBase, float> p in inRangeBiomes) {
            p.second = (p.second / tot);
        }

        return inRangeBiomes;
    }
    
    /// <summary>
    /// Gets the biome that is closest to a position
    /// </summary>
    /// <param name="pos">position to check</param>
    /// <returns>closest biome</returns>
    public BiomeBase getClosestBiome(Vector2Int pos) {
        return closestBiomePoint(pos).first;
    }


    #endregion

    #region private functions
    
    /// <summary>
    /// Gets the distance from closest biome point
    /// </summary>
    /// <param name="pos">position to test</param>
    /// <returns>distance from closest biome point</returns>
    private Pair<BiomeBase, float> closestBiomePoint(Vector2Int pos) {
        Pair<BiomeBase, float> bestBiomePoint = new Pair<BiomeBase, float>(null, float.MaxValue);

        int r3 = radius * 3;
        Vector2Int offset = new Vector2Int(gridWidth / 2, gridHeight / 2);
        Vector2Int posInGrid = new Vector2Int(pos.x / gridScale, pos.y / gridScale) + offset;
        for (int x = posInGrid.x - r3; x < posInGrid.x + r3; x++) {
            for (int y = posInGrid.y - r3; y < posInGrid.y + r3; y++) {
                int gridX = Utils.mod(x, gridWidth);
                int gridY = Utils.mod(y, gridHeight);

                if (biomeGrid[gridX, gridY] != null) {
                    float dist = Vector2Int.Distance(pos, biomeGrid[gridX, gridY].second);
                    if (dist < bestBiomePoint.second) {
                        bestBiomePoint.first = biomeGrid[gridX, gridY].first;
                        bestBiomePoint.second = dist;
                    }
                }
            }
        }
        return bestBiomePoint;
    }


    #endregion


    private Vector2Int modifyPosition(Vector2Int vec) {
        vec.x += (int)(SimplexNoise.Simplex1D(new Vector2(vec.x, vec.y), 0.01f) * 15);
        vec.y += (int)(SimplexNoise.Simplex1D(new Vector2(vec.y + 1234, vec.x + 4444), 0.01f) * 15);

        return vec;
    }



    /// <summary>
    /// Loads biomes from a folder
    /// </summary>
    /// <param name="folderpath">path to folder containing biome files</param>
    public void loadFromFile(String folderpath) {
        throw new NotImplementedException("BiomeManager.loadFromFile(...) not yet implemented.");
    }
}