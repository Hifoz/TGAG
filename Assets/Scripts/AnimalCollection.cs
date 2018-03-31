using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


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
    private List<CollectedAnimal> collectedAnimals = new List<CollectedAnimal>();
    public GameObject waterDisplayAnimal;
    public GameObject airDisplayAnimal;
    public GameObject landDisplayAnimal;
    public Camera displayCamera;

    private GameObject displayedAnimal;


    private void Start() {
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


    private IEnumerator rotateDisplay() {
        Debug.Log("start rotating");
        float rot = 0.4f;
        while (displayedAnimal.activeInHierarchy) {
            displayedAnimal.transform.Rotate(Vector3.up, rot);// (rot += 0.001f)%360);
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
