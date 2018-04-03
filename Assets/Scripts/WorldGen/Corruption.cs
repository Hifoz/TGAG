using UnityEngine;

/// <summary>
/// Class dealing with corruption
/// </summary>
public static class Corruption {
    private const float maxWorldDistance = 5000f; //Distance to edge of the world

    /// <summary>
    /// Calculates corruption factor of pos.
    /// !Make sure you provide true world pos (position that accounts for world shift)
    /// </summary>
    /// <param name="pos">position to calculate corruption for</param>
    /// <returns></returns>
    public static float corruptionFactor(Vector3 pos) {
        return 0.99f;
        pos.y = 0;
        float corruptionFactor = pos.magnitude / maxWorldDistance;
        corruptionFactor = Mathf.Clamp01(corruptionFactor);
        return Mathf.Clamp01(corruptionFactor * corruptionFactor);
    }

    /// <summary>
    /// corrupts the 2D height map data
    /// </summary>
    /// <param name="height">height to corrupt</param>
    /// <param name="cf">corruption factor</param>
    /// <returns>corrupted height</returns>
    public static float corruptHeight(float height, float cf) {
        return height * (1 - cf);
    }

    /// <summary>
    /// Adjusts a water height for corruption
    /// </summary>
    /// <param name="y">height to adjust</param>
    /// <param name="corruptionFactor">corruption factor float</param>
    /// <returns>new height</returns>
    public static int corruptWaterHeight(int y, float corruptionFactor) {
        int relativeWaterPos = WorldGenConfig.waterEndLevel - y;
        return (int)Mathf.Lerp(WorldGenConfig.waterEndLevel, WorldGenConfig.chunkHeight * 0.75f, corruptionFactor) - relativeWaterPos;
    }

    public static float corruptionNoise(Vector3 pos, BiomeBase biome) {
        float noise = SimplexNoise.Simplex3D(pos + Vector3.left * WorldGenConfig.seed, biome.corruptionFrequency);
        float noise01 = (noise + 1f) * 0.5f;
        return noise01;
    }
}
