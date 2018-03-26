using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class constains settigns related to threading and chnk launching
/// </summary>
public static class Settings {
    
    /// <summary>
    /// Loads settings from playerprefs.
    /// </summary>
    public static void load() {
        MaxChunkLaunchesPerUpdate = PlayerPrefs.GetInt("MaxChunkLaunchesPerUpdate", MaxChunkLaunchesPerUpdate);
        WorldGenThreads = PlayerPrefs.GetInt("WorldGenThreads", WorldGenThreads);
    }

    /// <summary>
    /// Saves settings to playerprefs.
    /// </summary>
    public static void save() {
        PlayerPrefs.SetInt("MaxChunkLaunchesPerUpdate", MaxChunkLaunchesPerUpdate);
        PlayerPrefs.SetInt("WorldGenThreads", WorldGenThreads);
    }

    public static int WorldGenThreads = 3;
    public static int MaxChunkLaunchesPerUpdate = 4;
}
