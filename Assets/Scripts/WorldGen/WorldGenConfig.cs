using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class containing world generation settings
/// </summary>
public static class WorldGenConfig {
    //General
    public static int seed = 1337; //161 for watery start
    public static int chunkSize = 20;
    public static int chunkCount = 40;
    public static int chunkHeight = 200;
    private const int waterEndLevel = 18; 
    //2D noise settings
    public static float noiseExponent2D = 3;
    public static int octaves2D = 6;
    //Foliage
    public static float treeLineLength = 2.0f; // Leaving most of the foliage settings here for now, as they are not used on a per-biome basis currently.
    public static float treeVoxelSize = 1.0f;
    public static float treeThickness = 0.5f;
    public static float treeLeafThickness = 3f;
    public static int grammarRecursionDepth = 4;

    /// <summary>
    /// Calculates if a position is in water
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public static bool positionInWater(Vector3 pos) {
        return heightInWater((int)pos.y);
    }

    /// <summary>
    /// Calculates if a position is in water
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public static bool heightInWater(int y) {
        return y < waterEndLevel;
    }

    /// <summary>
    /// Calculates if a position is in water
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public static bool heightInWater(float y) {
        return heightInWater((int)y);
    }

    public static float getWaterEnd(float corruptionFactor) {
        return Mathf.Lerp(waterEndLevel, chunkHeight, corruptionFactor);
    }

    public static float getWaterStart(float corruptionFactor) {
        return Mathf.Lerp(0, chunkHeight - waterEndLevel, corruptionFactor);
    }

    public static int corruptWaterHeight(int y, float corruptionFactor) {
        int relativeWaterPos = waterEndLevel - y;
        return (int)getWaterEnd(corruptionFactor) - relativeWaterPos;
    }
}
