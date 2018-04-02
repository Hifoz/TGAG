using UnityEngine;
using System.Collections;

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
        pos.y = 0;
        float corruptionFactor = pos.magnitude / maxWorldDistance;
        corruptionFactor = Mathf.Clamp01(corruptionFactor);
        return Mathf.Clamp01(corruptionFactor * corruptionFactor);
    }

    /// <summary>
    /// Corrupts the frequency used in 3DStructure calculations
    /// </summary>
    /// <param name="frequency">Original frequency</param>
    /// <param name="cf">corruption factor</param>
    /// <returns>corrupted frequency</returns>
    public static float corrupt3DStructureFrequency(float frequency, float cf) {
        const float maxMult = 2.0f;
        return frequency * Mathf.Lerp(1, maxMult, cf);
    }

    /// <summary>
    /// Corrupts the frequency used in 3DUnstructure calculations
    /// </summary>
    /// <param name="frequency">Original frequency</param>
    /// <param name="cf">corruption factor</param>
    /// <returns>corrupted frequency</returns>
    public static float corrupt3DUnstructureFrequency(float frequency, float cf) {
        const float maxMult = 0.5f;
        return frequency * Mathf.Lerp(1, maxMult, cf);
    }

    /// <summary>
    /// Corrupts the frequency used in height calculations
    /// </summary>
    /// <param name="frequency">Original frequency</param>
    /// <param name="cf">corruption factor</param>
    /// <returns>corrupted frequency</returns>
    public static float corruptHeightFrequency(float frequency, float cf) {
        const float maxMult = 8;
        return frequency * Mathf.Lerp(1, maxMult, cf);
    }

    /// <summary>
    /// Corrupts the rate used in 3DStructure calculations
    /// </summary>
    /// <param name="rate">original structure rate</param>
    /// <param name="cf">corruption factor</param>
    /// <returns>corrupted rate</returns>
    public static float corrupt3DStructureRate(float rate, float cf) {
        const float maxMult = 0.9f;
        return rate * Mathf.Lerp(1, maxMult, cf);
    }

    /// <summary>
    /// Corrupts the rate used in 3DUnstructure calculations
    /// </summary>
    /// <param name="rate">original Unstructure rate</param>
    /// <param name="cf">corruption factor</param>
    /// <returns>corrupted rate</returns>
    public static float corrupt3DUnstructureRate(float rate, float cf) {
        const float maxMult = 1.1f;
        return rate * Mathf.Lerp(1, maxMult, cf);
    }

    /// <summary>
    /// Adjusts a water height for corruption
    /// </summary>
    /// <param name="y">height to adjust</param>
    /// <param name="corruptionFactor">corruption factor float</param>
    /// <returns>new height</returns>
    public static int corruptWaterHeight(int y, float corruptionFactor) {
        int relativeWaterPos = WorldGenConfig.waterEndLevel - y;
        return (int)Mathf.Lerp(WorldGenConfig.waterEndLevel, WorldGenConfig.chunkHeight / 2, corruptionFactor) - relativeWaterPos;
    }
}
