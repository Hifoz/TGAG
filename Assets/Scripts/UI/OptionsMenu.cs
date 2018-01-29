using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Class for OptionsMenu UI
/// </summary>
public class OptionsMenu : MonoBehaviour {

    public InputField MaxChunkLaunchesPerUpdate;
    public Slider WorldGenThreads;

    // Use this for initialization
    void Awake () {
        Settings.load();
        MaxChunkLaunchesPerUpdate.text = Settings.MaxChunkLaunchesPerUpdate.ToString();        
        WorldGenThreads.maxValue = Environment.ProcessorCount;
        WorldGenThreads.value = Settings.WorldGenThreads;
        WorldGenThreads.minValue = 1;
    }	

    /// <summary>
    /// Function called when MaxChunkLaunchesPerUpdate field is used.
    /// Keeps value above 0.
    /// </summary>
    public void OptionMaxChunkLaunchesPerUpdate() {
        int value = int.Parse(MaxChunkLaunchesPerUpdate.text);
        if (value < 1) {
            value = 1;
            MaxChunkLaunchesPerUpdate.text = value.ToString();
        }
        Settings.MaxChunkLaunchesPerUpdate = value;
        Settings.save();
    }

    /// <summary>
    /// Function called when the WorldGenThreads slider is used.
    /// </summary>
    public void OptionWorldGenThreads() {
        int value = (int)WorldGenThreads.value;
        Settings.WorldGenThreads = value;
        WorldGenThreads.transform.Find("Value").GetComponent<Text>().text = value.ToString();
        Settings.save();
    }
}
