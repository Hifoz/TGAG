using UnityEngine;

/// <summary>
/// Class dealing with corruption
/// </summary>
public class Corruption : MonoBehaviour{
    private const float maxWorldDistance = 5000f; //Distance to edge of the world
    private const float pristineWorldDistance = 1000f;

    private void Update() {
        RenderSettings.skybox.SetFloat("_CorruptionFactor", corruptionFactor(Player.playerPos.get()));
    }

    /// <summary>
    /// Calculates corruption factor of pos.
    /// !Make sure you provide true world pos (position that accounts for world shift)
    /// </summary>
    /// <param name="pos">position to calculate corruption for</param>
    /// <returns></returns>
    public static float corruptionFactor(Vector3 pos) {
        pos.y = 0;
        float corruptionFactor = (pos.magnitude - pristineWorldDistance) / (maxWorldDistance - pristineWorldDistance);
        corruptionFactor = Mathf.Clamp01(corruptionFactor);
        return Mathf.Clamp01(corruptionFactor);
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

    /// <summary>
    /// Calculates corruption noise at position
    /// </summary>
    /// <param name="pos">position to use</param>
    /// <param name="biome">biome to do corruption for</param>
    /// <returns>corruption noise at position</returns>
    public static float corruptionNoise(Vector3 pos, BiomeBase biome) {
        float noise = SimplexNoise.Simplex3D(pos + Vector3.left * WorldGenConfig.seed, biome.corruptionFrequency);
        float noise01 = (noise + 1f) * 0.5f;
        float t = (pos.y - biome.minGroundHeight) / biome.maxGroundHeight;
        return Mathf.Lerp(noise01, 1, t * t); //Because you don't want an ugly flat "ceiling" everywhere.
    }
}
