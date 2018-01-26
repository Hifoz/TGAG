using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Script for the esc menu (ingame).
/// </summary>
public class EscMenu : MonoBehaviour {

    public GameObject menu;

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            menu.SetActive(!menu.activeSelf);
        }
    }

    /// <summary>
    /// Send the player back to the main menu.
    /// </summary>
    public void BackToMainMenu() {
        SceneManager.LoadScene("MainMenu");
    }
}
