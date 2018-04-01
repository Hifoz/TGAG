using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*
 * 
 * TODO : 
 * - Seed for animal skin must be consistent
 *      -- Current seed storing solution doesn't seem to work for body parameters, this must also be fixed.
 * - Show number of animals collected (in total, and per type)
 * - Make animals have some sort of animation when displayed
 *      -- Disable ragdolling?
 * 
 * 
 * Should the player be able to look at all animals ever collected from main menu?
 *  - Or should we just save stats and have a statistics page in main menu with different things?
 *      -- Most animals collected in a playthrough (Also per type?)
 *      -- Least animals collected in a playthrough (Also per type?)
 *      -- etc.
 *      -- Other stats not related to animal collection(eg. shortest time spent to complete a playthrough)
 *
 *
 */




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

public class AnimalCollection : MonoBehaviour {

    public Camera displayCamera;
    public float rotationSpeed = 0.4f;

    public GameObject waterDisplayAnimal;
    public GameObject airDisplayAnimal;
    public GameObject landDisplayAnimal;

    private GameObject displayedAnimal;
    private List<CollectedAnimal> collectedAnimals = new List<CollectedAnimal>();
    private List<CollectedAnimal> collectedOfDisplayType = new List<CollectedAnimal>();
    private Type displayType = typeof(Animal);
    private int displayedAnimalIndex;

    private void Start() {
        transform.localScale = new Vector3(0, 0, 0);
    }

    private void Update() {
        float rot = 0.4f;
        if (displayedAnimal != null && displayedAnimal.activeInHierarchy) {
            displayedAnimal.transform.Rotate(Vector3.up, rot);
        }

        if (Input.GetKeyDown(KeyCode.Escape)) {
            closeDisplay();
        }
    }

    public void openDisplay() {
        displayAnimal(0);
        transform.localScale = new Vector3(1, 1, 1);
    }


    public void closeDisplay() {
        hideDisplayedAnimal();
        transform.localScale = new Vector3(0, 0, 0);

    }

    /// <summary>
    /// Displays an animal on the CollectionItemDisplay
    /// </summary>
    /// <param name="index">index of the animal to display</param>
    public void displayAnimal(int index) {
        if (collectedOfDisplayType.Count == 0)
            return;
        displayedAnimalIndex = Utils.mod(index, collectedOfDisplayType.Count);

        if (collectedOfDisplayType[displayedAnimalIndex].animalType == typeof(LandAnimal)) {
            displayedAnimal = landDisplayAnimal;
        } else if (collectedOfDisplayType[displayedAnimalIndex].animalType == typeof(AirAnimal)) {
            displayedAnimal = airDisplayAnimal;
        } else { // Water animal
            displayedAnimal = waterDisplayAnimal;
        }

        AnimalSkeleton animalSkeleton = AnimalUtils.createAnimalSkeleton(displayedAnimal, displayedAnimal.GetComponent<Animal>().GetType(), collectedAnimals[displayedAnimalIndex].skeletonSeed);
        animalSkeleton.generateInThread();
        displayedAnimal.GetComponent<Animal>().setSkeleton(animalSkeleton);
        displayedAnimal.SetActive(true);
        displayedAnimal.transform.position = displayCamera.transform.position + new Vector3(0, 0, 20);
    }

    public void displayNext() {
        hideDisplayedAnimal();
        displayAnimal(displayedAnimalIndex + 1);
    }

    public void displayPrevious() {
        hideDisplayedAnimal();
        displayAnimal(displayedAnimalIndex - 1);
    }


    /// <summary>
    /// Used to hide a displayed animal from the CollectionItemDisplay
    /// </summary>
    public void hideDisplayedAnimal() {
        if(displayedAnimal != null)
            displayedAnimal.SetActive(false);
    }


    /// <summary>
    /// Add a new animal to the collection
    /// </summary>
    /// <param name="animal">The animal to add</param>
    public void addAnimal(CollectedAnimal animal) {
        foreach(CollectedAnimal ca in collectedAnimals) {
            if (ca.equals(animal)) return;
        }
        collectedAnimals.Add(animal);
        Debug.Log("Animal Added");

        if(animal.animalType == displayType || displayType == typeof(Animal)) {
            collectedOfDisplayType.Add(animal);
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


    /// <summary>
    /// 
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
            collectedOfDisplayType = collectedAnimals;
        else
            collectedOfDisplayType = collectedAnimals.Where((CollectedAnimal ca) => (ca.animalType == displayType)).ToList();
        hideDisplayedAnimal();
        displayAnimal(0);


    }



}
