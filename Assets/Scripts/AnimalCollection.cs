using System;
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

    /// <summary>
    /// Displays an animal on the CollectionItemDisplay
    /// </summary>
    public void displayAnimal(int index) {
        GameObject animalDisplay;
        if (collectedAnimals[index].animalType == typeof(LandAnimal)) {
            animalDisplay = landDisplayAnimal;
        } else if (collectedAnimals[index].animalType == typeof(AirAnimal)) {
            animalDisplay = airDisplayAnimal;
        } else { // Water animal
            animalDisplay = waterDisplayAnimal;
        }

        AnimalSkeleton animalSkeleton = AnimalUtils.createAnimalSkeleton(animalDisplay, animalDisplay.GetComponent<Animal>().GetType());
        animalSkeleton.generateInThread();
        animalDisplay.GetComponent<Animal>().setSkeleton(animalSkeleton);


        throw new NotImplementedException("AnimalCollection.displayAnimal() has not yet been implemented.");
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
