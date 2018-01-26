using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is for settings in the options menu.
/// </summary>
public static class Settings {

    public static void load() {
        WorldGenThreads = PlayerPrefs.GetInt("MaxChunkLaunchesPerUpdate");
        MaxChunkLaunchesPerUpdate = PlayerPrefs.GetInt("WorldGenThreads");
    }

    public static void save() {
        PlayerPrefs.SetInt("MaxChunkLaunchesPerUpdate", MaxChunkLaunchesPerUpdate);
        PlayerPrefs.SetInt("WorldGenThreads", WorldGenThreads);
    }

    public static int WorldGenThreads = 3;
    public static int MaxChunkLaunchesPerUpdate = 4;
}
