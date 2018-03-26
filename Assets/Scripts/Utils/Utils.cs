using UnityEngine;

/// <summary>
/// Utils class for world gen related functions
/// </summary>
public static class Utils {

    /// <summary>
    /// Floors the vector
    /// </summary>
    /// <param name="vec">vector to floor</param>
    /// <returns>Floored vector</returns>
    public static Vector3 floorVector(Vector3 vec) {
        vec.x = Mathf.Floor(vec.x);
        vec.y = Mathf.Floor(vec.y);
        vec.z = Mathf.Floor(vec.z);
        return vec;
    }

    /// <summary>
    /// Floors the vector
    /// </summary>
    /// <param name="vec">vector to floor</param>
    /// <returns>Floored vector</returns>
    public static Vector3Int floorVectorToInt(Vector3 vec) {
        return new Vector3Int(Mathf.FloorToInt(vec.x), Mathf.FloorToInt(vec.y), Mathf.FloorToInt(vec.z));
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

    /// <summary>
    /// Shifts the array by a factor of "shift" (provided int)
    /// </summary>
    /// <typeparam name="T">Type of array</typeparam>
    /// <param name="array">Array to shift</param>
    /// <param name="shift">How many indexes to shift by</param>
    /// <returns>Resulting array</returns>
    public static T[] shiftArray<T>(T[] array, int shift) {
        T[] result = new T[array.Length];
        for(int i = 0; i < result.Length; i++) {
            result[i] = array[mod(i + shift, array.Length)];
        }
        return result;
    }

    /// <summary>
    /// Source: https://stackoverflow.com/questions/1082917/mod-of-negative-number-is-melting-my-brain
    /// A mathematically correct modulo operation, that does not return negatives.
    /// </summary>
    /// <param name="x">number to modulate</param>
    /// <param name="m">modulus number</param>
    /// <returns>mod(x, m)</returns>
    public static int mod(int x, int m) {
        return (x % m + m) % m;
    }

    /// <summary>
    /// Does element wise multiplication of two vectos
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns>result</returns>
    public static Vector3 elementWiseMult(Vector3 a, Vector3 b) {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }
}
