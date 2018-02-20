using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class containing worldgen settings
/// </summary>
public static class ChunkConfig {
    //General
    public static int seed = 1337;
    public static int chunkSize = 20;
    public static int chunkCount = 40;
    public static int chunkHeight = 100; // Chunk height must not exceed (5376/(chunkSize^2))
    //2D noise settings
    public static float frequency2D = 0.005f;
    public static float noiseExponent2D = 2;
    public static int octaves2D = 2;
    //3D noise settings
    public static float Structure3DRate = 0.75f;
    public static float Unstructure3DRate = 0.85f;
    public static float frequency3D = 0.0075f;
    //Foliage
    public static int maxTreesPerChunk = 1;
    public static float treeLineLength = 2.0f;
    public static float treeVoxelSize = 1.0f;
    public static float treeThickness = 0.5f;
    public static float treeLeafThickness = 3f;
    public static int grammarRecursionDepth = 4;
}
