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
    private float minPointDistance = 15; // Always keep minPointDistance larger than the borderWidth



    System.Random rng;

    public BiomeManager(int seed = 42) {
        rng = new System.Random(seed);

        biomes.Add(loadBasicBiome());
        biomes.Add(loadBasicBiome2());

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
     * Find the closest N biomepoints and find some weighted avg. using the border width
     * so as to make a smooth transition from one biome to another
     * Need to find a way to figure out N though
     * Fidn the closest point and then all other points which are no further away than the border width? can then use that to do the stufferinos?
     * 
     */
    public Biome getBiome(Vector2Int pos) {
        BiomePoint best = biomePoints[0];
        float bestDist = Vector2Int.Distance(pos, biomePoints[0].point);
        foreach (BiomePoint b in biomePoints) {
            float dist = Vector2Int.Distance(pos, b.point);
            if (dist < bestDist) {
                best = b;
                bestDist = dist;
            }
        }
        return best.biome;
    }

    /// <summary>
    /// Loads biomes from a folder
    /// </summary>
    /// <param name="folderpath">path to folder containing biome files</param>
    public void loadFromFile(String folderpath) {
        throw new NotImplementedException("BiomeManager.loadFromFile(String folderpath) not yet implemented.");
    }


    /// <summary>
    /// Loads the basic terrain, matching the contents of the ChunkConfig
    /// </summary>
    /// <returns>the basic biome</returns>
    //[Obsolete("Only to be used until loadFromFile(..) is implemented")]
    public Biome loadBasicBiome() {
        Biome basicBiome = new Biome();

        //General
        basicBiome.snowHeight = 90;
        //2D noise settings
        basicBiome.frequency2D = 0.001f;
        basicBiome.noiseExponent2D = 3;
        basicBiome.octaves2D = 6;
        //3D noise settings
        basicBiome.Structure3DRate = 0.75f;
        basicBiome.Unstructure3DRate = 0.85f;
        basicBiome.frequency3D = 0.0075f;
        //Foliage
        basicBiome.maxTreesPerChunk = 1;
        basicBiome.treeLineLength = 2.0f;
        basicBiome.treeVoxelSize = 1.0f;
        basicBiome.treeThickness = 0.5f;
        basicBiome.treeLeafThickness = 3f;
        basicBiome.grammarRecursionDepth = 4;

        return basicBiome;
    }

    /// <summary>
    /// Loads a basic terrain
    /// </summary>
    /// <returns>the basic biome</returns>
    //[Obsolete("Only to be used until loadFromFile(..) is implemented")]
    public Biome loadBasicBiome2() {
        Biome basicBiome = new Biome();

        //General
        basicBiome.snowHeight = 30;
        //2D noise settings
        basicBiome.frequency2D = 0.0005f;
        basicBiome.noiseExponent2D = 5;
        basicBiome.octaves2D = 2;
        //3D noise settings
        basicBiome.Structure3DRate = 0;
        basicBiome.Unstructure3DRate = 0;
        basicBiome.frequency3D = 0;
        //Foliage
        basicBiome.maxTreesPerChunk = 1;
        basicBiome.treeLineLength = 2.0f;
        basicBiome.treeVoxelSize = 1.0f;
        basicBiome.treeThickness = 0.5f;
        basicBiome.treeLeafThickness = 3f;
        basicBiome.grammarRecursionDepth = 4;

        return basicBiome;
    }
}