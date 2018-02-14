using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BenchmarkUI : MonoBehaviour {

    public BenchmarkChunkManager BenchmarkManager;

    public Text filePathLabel;
    public Text statusLabel;
    public Text timeLabel;
    public RectTransform progressBar;
    public GameObject progressBarObj;

    int startThreads = 3;
    int endThreads = 3;
    int step = 1;
    bool terrain = true;
    bool animals = true;


    private void Update() {
        bool inProgress = BenchmarkManager.InProgress;

        statusLabel.text = (!inProgress) ? "Benchmark not running..." : string.Format("Benchmark running... Current Threads: {0}", BenchmarkManager.CurrentThreads);
        timeLabel.text = string.Format("{0}s", BenchmarkManager.getTime().ToString("N2"));

        if (inProgress) {
            filePathLabel.text = string.Format("Result file path: {0}", BenchmarkManager.Path);
            progressBarObj.SetActive(true);
            progressBar.anchorMax = new Vector2(BenchmarkManager.getProgress(), 1);
        } else {
            filePathLabel.text = "";
            progressBarObj.SetActive(false);
        }
    }

    public void OnTerrain(Toggle toggle) {
        terrain = toggle.isOn;
    }

    public void OnAnimals(Toggle toggle) {
        animals = toggle.isOn;
    }

    public void OnStartThreads(InputField inputField) {
        int value = int.Parse(inputField.text);
        value = (value >= 0) ? value : 0;
        value = (value <= endThreads) ? value : endThreads;
        inputField.text = value.ToString();
        startThreads = value;
    }

    public void OnEndThreads(InputField inputField) {
        int value = int.Parse(inputField.text);
        value = (value >= 0) ? value : 0;
        value = (value >= startThreads) ? value : startThreads;
        inputField.text = value.ToString();
        endThreads = value;
    }

    public void OnStep(InputField inputField) {
        int value = int.Parse(inputField.text);
        value = (value >= 0) ? value : 0;
        inputField.text = value.ToString();
        step = value;
    }

    public void OnStart() {
        BenchmarkManager.Benchmark(startThreads, endThreads, step, terrain, animals);
    }
}
