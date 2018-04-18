using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Contains the data needed to restore an animal for showing in the collection display
/// </summary>
public class CollectedAnimal {
    public Type animalType;
    public int skeletonSeed;

    public bool equals(CollectedAnimal other) {
        return this.animalType == other.animalType && this.skeletonSeed == other.skeletonSeed;
    }
}

/// <summary>
/// Responsible for handling animal collection
/// </summary>
public class AnimalCollection : MonoBehaviour {

    public Camera displayCamera;
    public float rotationSpeed = 0.4f;
    public Text noAnimalText;

    public GameObject waterDisplayAnimal;
    public GameObject airDisplayAnimal;
    public GameObject landDisplayAnimal;

    private List<CollectedAnimal> collectedAnimals = new List<CollectedAnimal>();
    private List<CollectedAnimal> displayCollection = new List<CollectedAnimal>();
    private GameObject displayAnimal;
    private Type displayType = typeof(Animal);
    private int displayIndex = 0;

    private void Start() {
        transform.localScale = new Vector3(0, 0, 0);
        foreach(GameObject a in new GameObject[] {waterDisplayAnimal, airDisplayAnimal, landDisplayAnimal }) {
            a.GetComponent<MeshFilter>().mesh = new Mesh();
        }
    }

    private void Update() {
        float rot = 0.3f;
        if (displayAnimal != null && displayAnimal.activeInHierarchy) {
            displayAnimal.transform.Rotate(Vector3.up, rot);
        }

        if (Input.GetKeyDown(KeyCode.Escape)) {
            closeDisplay();
        }
    }

    #region collection ui

    /// <summary>
    /// Opens the animal collection display
    /// </summary>
    public void openDisplay() {
        showAnimal(0);
        transform.localScale = new Vector3(1, 1, 1);
    }

    /// <summary>
    /// Closes the animal collection display
    /// </summary>
    public void closeDisplay() {
        hideAnimal();
        transform.localScale = new Vector3(0, 0, 0);

    }

    /// <summary>
    /// Displays an animal on the CollectionItemDisplay
    /// </summary>
    /// <param name="index">index of the animal to display</param>
    public void showAnimal(int index) {
        if (displayCollection.Count == 0) {
            noAnimalText.gameObject.SetActive(true);
            return;
        }
        noAnimalText.gameObject.SetActive(false);

        displayIndex = Utils.mod(index, displayCollection.Count);

        if (displayCollection[displayIndex].animalType == typeof(LandAnimal)) {
            displayAnimal = landDisplayAnimal;
        } else if (displayCollection[displayIndex].animalType == typeof(AirAnimal)) {
            displayAnimal = airDisplayAnimal;
        } else if (displayCollection[displayIndex].animalType == typeof(WaterAnimal)) {
            displayAnimal = waterDisplayAnimal;
        } else {
            throw new Exception("AnimalCollection.collectedOfDisplayType[" + 
                                displayIndex + "] has an illegal type.");
        }

        AnimalSkeleton animalSkeleton = AnimalUtils.createAnimalSkeleton( 
            displayAnimal, 
            displayAnimal.GetComponent<Animal>().GetType(), 
            displayCollection[displayIndex].skeletonSeed
        );
        animalSkeleton.generateInThread();
        displayAnimal.GetComponent<Animal>().enableDisplayMode();
        displayAnimal.GetComponent<Animal>().setSkeleton(animalSkeleton);
        AnimalUtils.addAnimalBrainNPC(displayAnimal.GetComponent<Animal>());
        displayAnimal.SetActive(true);
        displayAnimal.transform.localPosition =  new Vector3(0, 0, 20);
        displayAnimal.transform.Rotate(new Vector3(0, 180, 0));
    }

    /// <summary>
    /// Displays the next animal
    /// </summary>
    public void displayNext() {
        hideAnimal();
        showAnimal(displayIndex + 1);
    }

    /// <summary>
    /// Displays the previous animal
    /// </summary>
    public void displayPrevious() {
        hideAnimal();
        showAnimal(displayIndex - 1);
    }


    /// <summary>
    /// Used to hide a displayed animal from the CollectionItemDisplay
    /// </summary>
    public void hideAnimal() {
        if(displayAnimal != null)
            displayAnimal.SetActive(false);
    }


    /// <summary>
    /// Sets what type of animal to show in the display
    /// </summary>
    /// <param name="type"></param>
    public void setAnimalDisplayType(int type) {
        Type oldType = displayType;
        switch (type) {
            case 0:
                displayType = typeof(Animal);
                break;
            case 1:
                displayType = typeof(LandAnimal);
                break;
            case 2:
                displayType = typeof(WaterAnimal);
                break;
            case 3:
                displayType = typeof(AirAnimal);
                break;
            default:
                throw new ArgumentOutOfRangeException("type must be in the range of 0-3");
        }

        if (oldType == displayType) // No need to refresh the display if the type didn't change
            return;

        // Refresh the display and start at 0 with new animal type
        if (displayType == typeof(Animal))
            displayCollection = collectedAnimals;
        else
            displayCollection = collectedAnimals.Where((CollectedAnimal ca) => (ca.animalType == displayType)).ToList();
        hideAnimal();
        showAnimal(0);


    }


    #endregion

    #region collection interfacing

    /// <summary>
    /// Add a new animal to the collection
    /// </summary>
    /// <param name="animal">The animal to add</param>
    public void addAnimal(CollectedAnimal animal) {
        if (animal.animalType.BaseType != typeof(Animal))
            throw new Exception("Trying to add an animal of a non-animal type");

        foreach(CollectedAnimal ca in collectedAnimals) {
            if (ca.equals(animal)) return;
        }
        collectedAnimals.Add(animal);

        if(animal.animalType == displayType || displayType == typeof(Animal)) {
            displayCollection.Add(animal);
        }

        // Update the animal counts
        GameObject.Find("totalAnimalsBtn").GetComponentInChildren<Text>().text = "Total Animals: " + collectedAnimals.Count();
        if (animal.animalType == typeof(LandAnimal)) {
            GameObject.Find("landAnimalsBtn").GetComponentInChildren<Text>().text = "Land Animals: " + collectedAnimals.Count((CollectedAnimal ca) => (ca.animalType == typeof(LandAnimal)));
        } else if (animal.animalType == typeof(AirAnimal)) {
            GameObject.Find("airAnimalsBtn").GetComponentInChildren<Text>().text = "Air Animals: " + collectedAnimals.Count((CollectedAnimal ca) => (ca.animalType == typeof(AirAnimal)));
        } else if (animal.animalType == typeof(WaterAnimal)) {
            GameObject.Find("waterAnimalsBtn").GetComponentInChildren<Text>().text = "Water Animals: " + collectedAnimals.Count((CollectedAnimal ca) => (ca.animalType == typeof(WaterAnimal)));
        }

    }

    /// <summary>
    /// Get total animal count
    /// </summary>
    public int getAnimalCount() {
        return collectedAnimals.Count;
    }

    /// <summary>
    /// Get count of a specific animal type
    /// </summary>
    /// <param name="type">Type of animal to get the count of</param>
    public int getAnimalCount(Type animalType) {
        return collectedAnimals.Count(delegate(CollectedAnimal ca) { return ca.animalType == animalType; });
    }


    /// <summary>Returns a copy of the list of collected animals</summary>
    public List<CollectedAnimal> getAnimalList() {
        return collectedAnimals.ToList();
    }

    #endregion
}
