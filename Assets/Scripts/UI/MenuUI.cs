using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Menu UI
/// </summary>
public class MenuUI : MonoBehaviour {

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
            Time.timeScale = 1;
        }
    }

    private void OnDestroy() {
        Time.timeScale = 1;
    }

    // Update is called once per frame
    void Update () {
        if(SceneManager.GetActiveScene().name == "main") {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                if (GetComponent<Canvas>().enabled) {
                    onResume();
                } else {
                    GetComponent<Canvas>().enabled = true;
                    Time.timeScale = 0;
                }
            }
        }
	}

    #region shared functionality
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

    /// <summary>
    /// Used to leave the menu and resume playing
    /// </summary>
    public void onResume() {
        closeSettings();
        GetComponent<Canvas>().enabled = false;
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
    }

    /// <summary>
    /// Used to return from the game to the main menu scene
    /// </summary>
    public void returnToMain() {
        SceneManager.LoadScene("MainMenu");
    }


    #endregion

}
