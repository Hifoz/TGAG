using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Class controlling main menu UI
/// </summary>
public class MainMenuUI : MonoBehaviour {

    public GameObject MainMenu;
    public GameObject OptionsMenu;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    /// <summary>
    /// Launches the game.
    /// </summary>
    public void LaunchGame() {
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

    /// <summary>
    /// Enables the options menu.
    /// </summary>
    public void EnterOptions() {
        MainMenu.SetActive(false);
        OptionsMenu.SetActive(true);
    }

    /// <summary>
    /// Enables the main menu.
    /// </summary>
    public void EnterMainMenu() {
        MainMenu.SetActive(true);
        OptionsMenu.SetActive(false);
    }
}
