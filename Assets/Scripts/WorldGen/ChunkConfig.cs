using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class containing worldgen settings
/// </summary>
public static class ChunkConfig {
    //General
    public static int seed = 1337; //161 for watery start
    public static int chunkSize = 20;
    public static int chunkCount = 40;
    public static int chunkHeight = 200;
    public static int waterHeight = 18;
    //2D noise settings
    public static float noiseExponent2D = 3;
    public static int octaves2D = 6;
    //Foliage
    public static float treeLineLength = 2.0f;
    public static float treeVoxelSize = 1.0f;
    public static float treeThickness = 0.5f;
    public static float treeLeafThickness = 3f;
    public static int grammarRecursionDepth = 4;
}
