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
    public static Animal addAnimalComponent(GameObject target, Type animalType) {
        if (animalType.Equals(typeof(LandAnimal))) {
            return target.AddComponent<LandAnimal>();
        } else if (animalType.Equals(typeof(AirAnimal))) {
            return target.AddComponent<AirAnimal>();
        } else if (animalType.Equals(typeof(WaterAnimal))) {
            return target.AddComponent<WaterAnimal>();
        } else {
            throw new Exception("AnimalUtils, addAnimalComponent error! The provided type is invalid! you provided: " + animalType.Name);
        }
    }

    /// <summary>
    /// Resolves type and adds appropriate brain component to target
    /// </summary>
    /// <param name="target">Target animal</param>
    public static AnimalBrain addAnimalBrainPlayer(Animal target) {
        Type type = target.GetType();
        AnimalBrain brain = null;
        if (type.Equals(typeof(LandAnimal))) {
            brain = new LandAnimalBrainPlayer();
            ((LandAnimal)target).setAnimalBrain(brain);
        } else if (type.Equals(typeof(AirAnimal))) {
            brain = new AirAnimalBrainPlayer();
            ((AirAnimal)target).setAnimalBrain(brain);
        } else if (type.Equals(typeof(WaterAnimal))) {
            brain = new WaterAnimalBrainPlayer();
            ((WaterAnimal)target).setAnimalBrain(brain);
        } else {
            throw new Exception("AnimalUtils, addAnimalBrainNPC error! The provided type is invalid! you provided: " + type.Name);
        }
        return brain;
    }

    /// <summary>
    /// Resolves type and adds appropriate brain component to target
    /// </summary>
    /// <param name="target">Target animal</param>
    public static AnimalBrain addAnimalBrainNPC(Animal target) {
        Type type = target.GetType();
        AnimalBrain brain = null;
        if (type.Equals(typeof(LandAnimal))) {
            brain = new LandAnimalBrainNPC();
            ((LandAnimal)target).setAnimalBrain(brain);
        } else if (type.Equals(typeof(AirAnimal))) {
            brain = new AirAnimalBrainNPC();
            ((AirAnimal)target).setAnimalBrain(brain);
        } else if (type.Equals(typeof(WaterAnimal))) {
            brain = new WaterAnimalBrainNPC();
            ((WaterAnimal)target).setAnimalBrain(brain);
        } else {
            throw new Exception("AnimalUtils, addAnimalBrainNPC error! The provided type is invalid! you provided: " + type.Name);
        }
        return brain;
    }

    /// <summary>
    /// Resolves type and creates appropriate skeleton
    /// </summary>
    /// <param name="target">Target for skeleton</param>
    /// <param name="animalType">Type fo the target</param>
    /// <returns>AnimalSkeleton</returns>
    public static AnimalSkeleton createAnimalSkeleton(GameObject target, Type animalType, int seed = -1) {
        if (animalType.Equals(typeof(LandAnimal))) {
            return new LandAnimalSkeleton(target.transform, seed);
        } else if (animalType.Equals(typeof(AirAnimal))) {
            return new AirAnimalSkeleton(target.transform, seed);
        } else if (animalType.Equals(typeof(WaterAnimal))) {
            return new WaterAnimalSkeleton(target.transform, seed);
        } else {
            throw new Exception("AnimalUtils, createAnimalSkeleton error! The provided type is invalid! you provided: " + animalType.Name);
        }
    }
}
