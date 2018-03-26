using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Contains logic for displaying debug info in game
/// </summary>
public class DebugUIController : MonoBehaviour {
    public WorldGenManager chunkManager;

    public GameObject chunkManagerDebug;
    public Text chunkManagerDebugText;

    public GameObject animalDebugger;
    private HashSet<GameObject> debuggedAnimals;
    private List<GameObject> animalDebuggers;
    private GameObjectPool[] animalPools;

    private bool debug = true;


    void Start() {
        animalPools = chunkManager.getAnimals();
        debuggedAnimals = new HashSet<GameObject>();
        animalDebuggers = new List<GameObject>();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.F1)) {
            toggleDebugInfo();
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
    /// Toggles the debug info on/off
    /// </summary>
    private void toggleDebugInfo() {
        chunkManagerDebug.SetActive(debug);
        debug = !debug;

        if (debug) { //Cleanup animal debugging       
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

}
