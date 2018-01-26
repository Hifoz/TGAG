using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour {

    public InputField MaxChunkLaunchesPerUpdate;
    public Slider WorldGenThreads;

    // Use this for initialization
    void Start () {
        MaxChunkLaunchesPerUpdate.text = PlayerPrefs.GetInt("MaxChunkLaunchesPerUpdate").ToString();
        WorldGenThreads.maxValue = Environment.ProcessorCount;
        WorldGenThreads.value = PlayerPrefs.GetInt("WorldGenThreads");
    }	

    public void OptionMaxChunkLaunchesPerUpdate() {
        int value = int.Parse(MaxChunkLaunchesPerUpdate.text);
        Settings.MaxChunkLaunchesPerUpdate = value;
        Settings.save();
    }

    public void OptionWorldGenThreads() {
        int value = (int)WorldGenThreads.value;
        Settings.WorldGenThreads = value;
        WorldGenThreads.transform.Find("Value").GetComponent<Text>().text = value.ToString();
        Settings.save();
    }
}
