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
    /// Modulos a value (Because % operator isn't proper modulo, but remainder, so it doesn't work properly on negative numbers)
    /// </summary>
    /// <param name="v"></param>
    /// <param name="m"></param>
    /// <returns></returns>
    public static int mod(int v, int m) {
        int r = v % m;
        return r < 0 ? r + m : r;
    }
}