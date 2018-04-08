using UnityEngine;
using System.Collections;

/// <summary>
/// This class is a container for some utility functions for noise
/// </summary>
public static class NoiseUtils {

    /// <summary>
    /// Turns a vector3 into an int seed.
    /// </summary>
    /// <param name="vec">Vector3 to convert</param>
    /// <returns>int seed</returns>
    public static int Vector2Seed(Vector3 vec) {
        return (int)((hash(vec.x) + hash(vec.y) + hash(vec.z)) * 16987.9964f);
    }

    /// <summary>
    /// Gets the fraction of a float
    /// </summary>
    /// <param name="x">float to get fraction from</param>
    /// <returns>fraction of input float</returns>
    private static float frac(float x) {
        return x - Mathf.Floor(x);
    }

    /// <summary>
    /// Hash function, gotten from a forum post
    /// https://forum.unity.com/threads/perlin-noise-procedural-shader.33725/
    /// </summary>
    /// <param name="n"></param>
    /// <returns></returns>
    public static float hash(float n) {
        return frac(Mathf.Abs(Mathf.Sin(n) * 43758.5453f));
    }
}
