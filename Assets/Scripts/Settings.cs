using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is for settings in the options menu.
/// </summary>
public static class Settings {
    
    /// <summary>
    /// Loads settings from playerprefs.
    /// </summary>
    public static void load() {
        tryGetInt("MaxChunkLaunchesPerUpdate", ref MaxChunkLaunchesPerUpdate);
        tryGetInt("WorldGenThreads", ref WorldGenThreads);
    }

    /// <summary>
    /// Saves settings to playerprefs.
    /// </summary>
    public static void save() {
        PlayerPrefs.SetInt("MaxChunkLaunchesPerUpdate", MaxChunkLaunchesPerUpdate);
        PlayerPrefs.SetInt("WorldGenThreads", WorldGenThreads);
    }

    private static void tryGetInt(string name, ref int target) {
        int value = PlayerPrefs.GetInt(name);
        if (value != 0) {
            target = value;
        }
    }

    public static int WorldGenThreads = 3;
    public static int MaxChunkLaunchesPerUpdate = 4;
}
