using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Menu UI
/// </summary>
public class MenuUI : MonoBehaviour {

    public Text title;

    public GameObject mainButtons;

    public GameObject playButtons;
    public GameObject playPanel;

    public GameObject optionsButtons;
    public GameObject optionsPanel;

    public GameObject collectionButtons;

    public GameObject benchmarkButtons;

    //Variables for tracking player stats
    private Vector3 oldPlayerPos = Vector3.zero;
    private float distanceTraveled = 0;
    private float timeSpent = 0;

    // Use this for initialization
    void Start() {
        mainButtons.SetActive(true);
        if (SceneManager.GetActiveScene().name == "main") {
            GetComponent<Canvas>().enabled = false;
            Time.timeScale = 1;

            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void OnDestroy() {
        Time.timeScale = 1;
    }

    // Update is called once per frame
    void Update () {
        if(SceneManager.GetActiveScene().name == "main") {
            Vector3 playerPos = Player.playerPos.get();
            distanceTraveled += Vector3.Distance(oldPlayerPos, playerPos);
            oldPlayerPos = playerPos;
            timeSpent += Time.deltaTime;

            if (Corruption.corruptionFactor(playerPos) >= 1f) {
                openWinScreen();
            } else if (Input.GetKeyDown(KeyCode.Escape)) {
                if (GetComponent<Canvas>().enabled) {
                    onResume();
                    Cursor.lockState = CursorLockMode.Locked;
                } else {
                    GetComponent<Canvas>().enabled = true;
                    Time.timeScale = 0;
                    Cursor.lockState = CursorLockMode.None;
                }
            }
        } else {
            if (Input.GetKeyDown(KeyCode.F1)) {
                benchmarkButtons.SetActive(!benchmarkButtons.activeInHierarchy);
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
        optionsPanel.transform.GetComponentInParent<SettingsUI>().save();
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
        int maxSeedVal = 100000;
        if(seed.Trim() == "") {
            System.Random rng = new System.Random(System.DateTime.UtcNow.Millisecond); // Use c# epoch time as rng seed
            WorldGenConfig.seed = rng.Next(0, maxSeedVal);
        } else {
            WorldGenConfig.seed = (int)(long.Parse(seed) % maxSeedVal);
        }

        //SceneManager.LoadScene("main");
        SceneManager.LoadSceneAsync("main");
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
        if (collectionButtons != null) {
            closeCollection();
        }
        GetComponent<Canvas>().enabled = false;
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
    }

    /// <summary>
    /// Switches from collection buttons to main button set
    /// </summary>
    public void closeCollection() {
        title.text = "Paused";
        collectionButtons.SetActive(false);
        mainButtons.SetActive(true);
    }

    /// <summary>
    /// Switches to collection buttons
    /// </summary>
    public void openCollection() {
        title.text = "Animal Collection";
        collectionButtons.SetActive(true);
        mainButtons.SetActive(false);
    }

    /// <summary>
    /// Opens win screen
    /// </summary>
    public void openWinScreen() {
        title.text = string.Format(
                    "You won! Distance: {0}, Minutes: {1}",
                    distanceTraveled.ToString("N2"),
                    (timeSpent / 60f).ToString("N2")
                );
        if (!GetComponent<Canvas>().enabled) {
            GetComponent<Canvas>().enabled = true;
            Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.None;
            collectionButtons.SetActive(false);
            mainButtons.SetActive(true);
        }       
    }


    /// <summary>
    /// Used to return from the game to the main menu scene
    /// </summary>
    public void returnToMain() {
        SceneManager.LoadScene("MainMenu");
    }


    #endregion

}
