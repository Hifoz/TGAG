using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EscMenu : MonoBehaviour {

    public GameObject menu;

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            menu.SetActive(!menu.activeSelf);
        }
    }

    public void BackToMainMenu() {
        SceneManager.LoadScene("MainMenu");
    }
}
