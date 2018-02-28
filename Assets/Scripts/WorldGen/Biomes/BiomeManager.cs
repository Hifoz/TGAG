using System;
using System.Collections.Generic;
using UnityEngine;


class BiomeManager {
    private List<Biome> biomes;
    private List<Vector2> biomePoints;



    BiomeManager() {

    }

    /// <summary>
    /// Used to update the biomes
    /// </summary>
    public void updateBiomes() {
        throw new NotImplementedException();
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
    [Obsolete("Only to be used until loadFromFile(..) is implemented")]
    public void loadBasicType() {
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
    }
}