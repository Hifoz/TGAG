using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    /// <summary>
    /// Called when terrain toggle is recieved
    /// </summary>
    /// <param name="toggle">Calling UI element</param>
    public void OnTerrain(Toggle toggle) {
        terrain = toggle.isOn;
    }

    /// <summary>
    /// Called when animals toggle is recieved
    /// </summary>
    /// <param name="toggle">Calling UI element</param>
    public void OnAnimals(Toggle toggle) {
        animals = toggle.isOn;
    }

    /// <summary>
    /// Called when start threads is recieved
    /// </summary>
    /// <param name="inputField">Calling UI element</param>
    public void OnStartThreads(InputField inputField) {
        int value = int.Parse(inputField.text);
        value = (value >= 0) ? value : 0;
        value = (value <= endThreads) ? value : endThreads;
        inputField.text = value.ToString();
        startThreads = value;
    }

    /// <summary>
    /// Called when end threads is recieved
    /// </summary>
    /// <param name="inputField">Calling UI element</param>
    public void OnEndThreads(InputField inputField) {
        int value = int.Parse(inputField.text);
        value = (value >= 0) ? value : 0;
        value = (value >= startThreads) ? value : startThreads;
        inputField.text = value.ToString();
        endThreads = value;
    }

    /// <summary>
    /// Called when step count input is recieved
    /// </summary>
    /// <param name="inputField">Calling UI element</param>
    public void OnStep(InputField inputField) {
        int value = int.Parse(inputField.text);
        value = (value >= 0) ? value : 0;
        inputField.text = value.ToString();
        step = value;
    }

    /// <summary>
    /// Called when start button is clicked
    /// </summary>
    public void OnStart() {
        BenchmarkManager.Benchmark(startThreads, endThreads, step, terrain, animals);
    }

    /// <summary>
    /// Called when back button is clicked
    /// </summary>
    public void OnBack() {
        SceneManager.LoadScene("MainMenu");
    }
}
