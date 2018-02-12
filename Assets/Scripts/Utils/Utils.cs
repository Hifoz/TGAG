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
}
