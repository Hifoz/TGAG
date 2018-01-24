using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour {

    public GameObject MainMenu;
    public GameObject OptionsMenu;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void LaunchGame() {
        SceneManager.LoadScene("main");
    }

    public void EnterOptions() {
        MainMenu.SetActive(false);
        OptionsMenu.SetActive(true);
    }

    public void EnterMainMenu() {
        MainMenu.SetActive(true);
        OptionsMenu.SetActive(false);
    }
}
