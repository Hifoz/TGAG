using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Class controlling menu UI
/// </summary>
public class MenuUI : MonoBehaviour {
    public static bool isEnabled = true;

    public GameObject mainButtons;

    public GameObject playButtons;
    public GameObject playPanel;

    public GameObject optionsButtons;
    public GameObject optionsPanel;



    // Use this for initialization
    void Start() {
        mainButtons.SetActive(true);
        if (SceneManager.GetActiveScene().name == "main") {
            GetComponent<Canvas>().enabled = false;
            isEnabled = false;
        }
    }

    // Update is called once per frame
    void Update () {
        if(SceneManager.GetActiveScene().name == "main") {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                isEnabled = !isEnabled;
                GetComponent<Canvas>().enabled = isEnabled;
            }
        }
	}

    #region shared
    /// <summary>
    /// When user clicks "options in main menu
    /// </summary>
    public void openSettings() {
        optionsPanel.SetActive(true);
        optionsButtons.SetActive(true);
        mainButtons.SetActive(false);
    }

    /// <summary>
    /// When the player clicks wants to close options sub-menu
    /// </summary>
    public void closeSettings() {
        optionsPanel.SetActive(false);
        optionsButtons.SetActive(false);
        mainButtons.SetActive(true);
    }
    
    /// <summary>
    /// When the player clicks "apply" in options sub-menu
    /// </summary>
    public void closeAndSave() {
        optionsPanel.GetComponent<SettingsUI>().save();
        closeSettings();
    }

    /// <summary>
    /// Quits the game
    /// </summary>
    public void ExitGame() {
        Application.Quit();
    }
    #endregion

    #region main menu only

    /// <summary>
    /// When the user clicks "play" on the main menu
    /// </summary>
    public void onPlay() {
        playPanel.SetActive(true);
        playButtons.SetActive(true);
        mainButtons.SetActive(false);
    }

    /// <summary>
    /// When the player clicks "back" in the play sub-menu
    /// </summary>
    public void onBack() {
        GameObject.Find("seedInputField").GetComponent<InputField>().text = "";
        playPanel.SetActive(false);
        playButtons.SetActive(false);
        mainButtons.SetActive(true);
    }

    /// <summary>
    /// Launches the game.
    /// </summary>
    public void startGame() {
        string seed = GameObject.Find("seedInputField").GetComponent<InputField>().text;
        if(seed.Trim() == "") {
            System.Random rng = new System.Random(System.DateTime.UtcNow.Millisecond); // Use c# epoch time as rng seed
            WorldGenConfig.seed = rng.Next(0, int.MaxValue);
        } else {
            WorldGenConfig.seed = int.Parse(seed);
        }

        SceneManager.LoadScene("main");
    }

    /// <summary>
    /// Launches benchmark scene.
    /// </summary>
    public void LaunchBenchmark() {
        SceneManager.LoadScene("Benchmark");
    }

    /// <summary>
    /// Launches benchmark scene.
    /// </summary>
    public void LaunchRealBenchmark() {
        SceneManager.LoadScene("RealWorldBenchmark");
    }

    #endregion

    #region ingame only

    public void onResume() {
        GetComponent<Canvas>().enabled = false;
    }

    public void returnToMain() {
        SceneManager.LoadScene("MainMenu");
    }


    #endregion

}
