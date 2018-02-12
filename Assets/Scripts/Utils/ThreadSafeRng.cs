using System;

/// <summary>
/// A threadsafe random number generator;
/// </summary>
public class ThreadSafeRng {

    Random rng = new Random();

    /// <summary>
    /// Generates a number between min and max
    /// </summary>
    /// <param name="min">Lower bound of number</param>
    /// <param name="max">Upper bound of number</param>
    /// <returns>float number</returns>
    public float randomFloat(float min, float max) {
        float number;
        lock (rng) {
            number = (float)rng.NextDouble();
        }

        number *= (max - min);
        number += min;
        return number;
    }

    /// <summary>
    /// Generates a number between min and max
    /// </summary>
    /// <param name="min">Lower bound of number</param>
    /// <param name="max">Upper bound of number</param>
    /// <returns>int number</returns>
    public int randomInt(int min, int max) {
        lock (rng) {
            return rng.Next(min, max);
        }
    }
}
