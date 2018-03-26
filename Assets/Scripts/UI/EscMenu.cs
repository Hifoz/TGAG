using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Script for the esc menu (ingame).
/// </summary>
public class EscMenu : MonoBehaviour {
    public WorldGenManager chunkManager;

    public GameObject menu;
    public GameObject chunkManagerDebug;
    public Text chunkManagerDebugText;

    public GameObject animalDebugger;
    private HashSet<GameObject> debuggedAnimals;
    private List<GameObject> animalDebuggers;
    private GameObjectPool[] animalPools;

    void Start() {
        animalPools = chunkManager.getAnimals();
        debuggedAnimals = new HashSet<GameObject>();
        animalDebuggers = new List<GameObject>();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            menu.SetActive(!menu.activeSelf);
        }

        if (chunkManagerDebug.activeSelf) {
            chunkManagerDebugText.text = chunkManager.getDebugString();

            foreach (GameObjectPool pool in animalPools) {
                foreach (GameObject animal in pool.activeList) {
                    if (!debuggedAnimals.Contains(animal)) {
                        debugAnimal(animal);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Called when the debug toggle is clicked
    /// </summary>
    /// <param name="toggle"></param>
    public void OnDebugToggle(Toggle toggle) {
        chunkManagerDebug.SetActive(toggle.isOn);

        if (!toggle.isOn) { //Cleanup animal debugging       
            foreach (GameObject debugger in animalDebuggers) {
                Destroy(debugger);
            }
            animalDebuggers.Clear();
            debuggedAnimals.Clear();
        } else {
            debugAnimal(chunkManager.player.gameObject);            
        }        
    }

    /// <summary>
    /// Debugs an animal
    /// </summary>
    /// <param name="animal"></param>
    private void debugAnimal(GameObject animal) {
        GameObject debugger = Instantiate(animalDebugger);
        debugger.GetComponent<AnimalDebug>().setAnimal(animal.GetComponent<Animal>());
        animalDebuggers.Add(debugger);
        debuggedAnimals.Add(animal);
    }

    /// <summary>
    /// Send the player back to the main menu.
    /// </summary>
    public void BackToMainMenu() {
        SceneManager.LoadScene("MainMenu");
    }
}
