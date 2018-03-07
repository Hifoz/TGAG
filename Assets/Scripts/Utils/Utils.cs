using UnityEngine;

/// <summary>
/// Utils class for world gen related functions
/// </summary>
public static class Utils {

    //Floors the vector
    public static Vector3 floorVector(Vector3 vec) {
        vec.x = Mathf.Floor(vec.x);
        vec.y = Mathf.Floor(vec.y);
        vec.z = Mathf.Floor(vec.z);
        return vec;
    }

    /// <summary>
    /// Returns the fraction of the float
    /// </summary>
    /// <param name="number">Number to get fraction from</param>
    /// <returns>float fraction</returns>
    public static float frac(float number) {
        return number - Mathf.Floor(number);
    }

    /// <summary>
    /// Multiplies the provided array of Vector3 by number (element wise multiplication)
    /// </summary>
    /// <param name="source">Vector3[]</param>
    /// <param name="number">float</param>
    /// <returns>resulting Vector3[] Array</returns>
    public static Vector3[] multVectorArray(Vector3[] source, float number) {
        Vector3[] result = new Vector3[source.Length];
        for(int i = 0; i < result.Length; i++) {
            result[i] = source[i] * number;
        }
        return result;
    }
}
