using UnityEngine;
using System.Collections;

/// <summary>
/// Utils class for world gen related functions
/// </summary>
public static class WorldUtils {

    //Floors the vector
    public static Vector3 floor(Vector3 vec) {
        vec.x = Mathf.Floor(vec.x);
        vec.y = Mathf.Floor(vec.y);
        vec.z = Mathf.Floor(vec.z);
        return vec;
    }
}
