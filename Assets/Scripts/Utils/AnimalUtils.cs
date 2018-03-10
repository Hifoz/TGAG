using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// A collection of utility functions for dealing with animals
/// </summary>
public static class AnimalUtils {

    /// <summary>
    /// Resolves type and adds appropriate AnimalNPC component to target
    /// </summary>
    /// <param name="target">Target animal</param>
    /// <param name="animalType">Type of target animal</param>
    /// <returns>The added component</returns>
    public static Animal addAnimalComponentNPC(GameObject target, Type animalType) {
        if (animalType.BaseType.Equals(typeof(LandAnimal))) {
            return target.AddComponent<LandAnimalNPC>();
        } else if (animalType.BaseType.Equals(typeof(AirAnimal))) {
            return target.AddComponent<AirAnimalNPC>();
        } else if (animalType.BaseType.Equals(typeof(WaterAnimal))) {
            return target.AddComponent<WaterAnimalNPC>();
        } else {
            throw new Exception("AnimalUtils, addAnimalComponentNPC error! The provided type is invalid! you provided: " + animalType.Name);
        }
    }

    /// <summary>
    /// Resolves type and adds appropriate AnimalPlayer component to target
    /// </summary>
    /// <param name="target">Target animal</param>
    /// <param name="animalType">Type of target animal</param>
    /// <returns>The added component</returns>
    public static Animal addAnimalComponentPlayer(GameObject target, Type animalType) {
        if (animalType.BaseType.Equals(typeof(LandAnimal))) {
            return target.AddComponent<LandAnimalPlayer>();
        } else if (animalType.BaseType.Equals(typeof(AirAnimal))) {
            return target.AddComponent<AirAnimalPlayer>();
        } else if (animalType.BaseType.Equals(typeof(WaterAnimal))) {
            return target.AddComponent<WaterAnimalPlayer>();
        } else {
            throw new Exception("AnimalUtils, addAnimalComponentPlayer error! The provided type is invalid! you provided: " + animalType.Name);
        }       
    }

    /// <summary>
    /// Resolves type and creates appropriate skeleton
    /// </summary>
    /// <param name="target">Target for skeleton</param>
    /// <param name="animalType">Type fo the target</param>
    /// <returns>AnimalSkeleton</returns>
    public static AnimalSkeleton createAnimalSkeleton(GameObject target, Type animalType) {
        if (animalType.BaseType.Equals(typeof(LandAnimal))) {
            return new LandAnimalSkeleton(target.transform);
        } else if (animalType.BaseType.Equals(typeof(AirAnimal))) {
            return new AirAnimalSkeleton(target.transform);
        } else if (animalType.BaseType.Equals(typeof(WaterAnimal))) {
            return new WaterAnimalSkeleton(target.transform);
        } else {
            throw new Exception("AnimalUtils, createAnimalSkeleton error! The provided type is invalid! you provided: " + animalType.Name);
        }
    }
}
