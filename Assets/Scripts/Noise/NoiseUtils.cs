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
        float x = frac(vec.x * 314.17654f);
        float y = frac(vec.y * 4986.654f);
        float z = frac(vec.z * 1998.8454f);
        return (int)( (x + y + z) * 16987.9964f);
    }

    /// <summary>
    /// Gets the fraction of a float
    /// </summary>
    /// <param name="x">float to get fraction from</param>
    /// <returns>fraction of input float</returns>
    private static float frac(float x) {
        return x - Mathf.Floor(x);
    }

}
