using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Script for the esc menu (ingame).
/// </summary>
public class EscMenu : MonoBehaviour {
    public ChunkManager chunkManager;

    public GameObject menu;
    public GameObject chunkManagerDebug;
    public Text chunkManagerDebugText;

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            menu.SetActive(!menu.activeSelf);
        }

        if (chunkManagerDebug.activeSelf) {
            chunkManagerDebugText.text = chunkManager.getDebugString();
        }
    }

    /// <summary>
    /// Called when the debug toggle is clicked
    /// </summary>
    /// <param name="toggle"></param>
    public void OnDebugToggle(Toggle toggle) {
        chunkManagerDebug.SetActive(toggle.isOn);
    }

    /// <summary>
    /// Send the player back to the main menu.
    /// </summary>
    public void BackToMainMenu() {
        SceneManager.LoadScene("MainMenu");
    }
}
