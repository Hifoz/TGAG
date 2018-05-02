using UnityEngine;
using System.Collections;

/// <summary>
/// Class dealing with corruption
/// </summary>
public class Corruption : MonoBehaviour {
    public Material materialWater;
    public Transform sun;

    private Vector3 normalSun = new Vector3(50, -30, 0);
    private Vector3 corruptSun = new Vector3(10, -30, 0);
    private Vector3 currentSunTarget = new Vector3(50, -30, 0);
    private Vector3 lastSunTarget = new Vector3(50, -30, 0);
    private float timer = 1;

    private const float maxWorldDistance = 20000f; //Distance to edge of the world
    private const float pristineWorldDistance = 1000f;

    private void Update() {
        float cf = corruptionFactor(Player.playerPos.get());
        RenderSettings.skybox.SetFloat("_CorruptionFactor", cf);
        materialWater.SetFloat("_CorruptionFactor", cf);
        if (cf > 0) {
            timer += Time.deltaTime * cf;
            if (timer >= 1) {
                timer = 0;
                lastSunTarget = currentSunTarget;
                currentSunTarget = Vector3.Lerp(normalSun, corruptSun, Random.Range(0f, 1f));
            }           
            sun.rotation = Quaternion.Euler(Vector3.Lerp(lastSunTarget, currentSunTarget, timer));
        }        
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
        return Mathf.Clamp01(corruptionFactor);
    }

    /// <summary>
    /// Adjusts a water height for corruption
    /// </summary>
    /// <param name="y">height to adjust</param>
    /// <param name="corruptionFactor">corruption factor float</param>
    /// <returns>new height</returns>
    public static int corruptWaterHeight(int y, float corruptionFactor) {
        float maxWaterHeight = WorldGenConfig.chunkHeight * 0.75f - WorldGenConfig.waterEndLevel;
        return (int)(maxWaterHeight * corruptionFactor) + y;
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

    private void OnDestroy() {
        //reset corruption when ending a play session
        RenderSettings.skybox.SetFloat("_CorruptionFactor", 0);
        materialWater.SetFloat("_CorruptionFactor", 0);
    }
}
