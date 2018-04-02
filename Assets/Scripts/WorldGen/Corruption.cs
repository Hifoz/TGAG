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
        return Mathf.Clamp01(corruptionFactor);
    }
}
