using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*
 * 
 * TODO : 
 * - Seed for animal skin must be consistent
 * - Add first animal to collection
 * - Make animal-switching add the new animal to collection
 * - Show number of animals collected (in total, and per type)
 * - Let the player "browse" through collected animals
 * - Let the player filter the what animal types to browse (something simple, either all types or one specific type)
 * - Add entry point to collection from menu and a keyboard-key
 * 
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

    private void Start() {
        // For testing, add a random animal
        addAnimal(new CollectedAnimal {
            animalType = typeof(LandAnimal),
            skeletonSeed = 1337
        });

        displayAnimal(0);
    }




    /// <summary>
    /// Displays an animal on the CollectionItemDisplay
    /// </summary>
    public void displayAnimal(int index) {
        if (collectedAnimals[index].animalType == typeof(LandAnimal)) {
            displayedAnimal = landDisplayAnimal;
        } else if (collectedAnimals[index].animalType == typeof(AirAnimal)) {
            displayedAnimal = airDisplayAnimal;
        } else { // Water animal
            displayedAnimal = waterDisplayAnimal;
        }

        AnimalSkeleton animalSkeleton = AnimalUtils.createAnimalSkeleton(displayedAnimal, displayedAnimal.GetComponent<Animal>().GetType(), collectedAnimals[index].skeletonSeed);
        animalSkeleton.generateInThread();
        displayedAnimal.GetComponent<Animal>().setSkeleton(animalSkeleton);
        displayedAnimal.SetActive(true);
        displayedAnimal.transform.position = displayCamera.transform.position + new Vector3(0, 0, 20);

        StartCoroutine(rotateDisplay());
    }


    public void hideDisplayedAnimal() {
        displayedAnimal.SetActive(false);
    }


    /// <summary>
    /// Rotates the displayed animal
    /// </summary>
    /// <returns></returns>
    private IEnumerator rotateDisplay() {
        Debug.Log("start rotating");
        float rot = 0.4f;
        while (displayedAnimal.activeInHierarchy) {
            displayedAnimal.transform.Rotate(Vector3.up, rot);
            yield return new WaitForEndOfFrame();
        }
        Debug.Log("stop rotating");
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

}
