using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


/**
 * Poisson Disk Sampler
 * 
 * Inspired by https://bl.ocks.org/mbostock/19168c663618b7f07158
 *             http://www.cs.ubc.ca/~rbridson/docs/bridson-siggraph07-poissondisk.pdf
 */
class PoissonDiscSampler {
    private const int maxTestSamples = 30; // Max number of samples to test for any active list items

    private int radius;
    private int width;
    private int height;
    private bool wrap;

    private List<Vector2Int> activeList;
    private bool[,] grid;

    private System.Random rng;

    #region constructors

    /// <summary>
    /// Constructor for a sampler.
    /// </summary>
    /// <param name="radius">
    /// Sampling radius. All points will be minimum 'radius' meters away from eachother, 
    /// and there will be no position further away than 2 * 'radius' from a point.
    /// </param>
    /// <param name="width">Width of sampling area.</param>
    /// <param name="height">Height of sampling area.</param>
    /// <param name="seed">Seed for the generator</param>
    /// <param name="wrap">
    /// Should the points wrap around? 
    /// This is useful if you want to generate a repeatable pattern.
    /// </param>
    public PoissonDiscSampler(int radius, int width, int height, int seed, bool wrap = false) {
        this.radius = radius;
        this.width = width;
        this.height = height;
        this.wrap = wrap;

        rng = new System.Random(seed);

        activeList = new List<Vector2Int>();
        grid = new bool[width, height];
    }

    /// <summary>
    /// Constructor for a sampler with a collection of pre-defined points. 
    /// Good to use if you want to grow an existing sample set.
    /// </summary>
    /// <param name="radius">
    /// Sampling radius. All points will be minimum 'radius' meters away from eachother, 
    /// and there will be no position further away than 2 * 'radius' from a point.
    /// </param>
    /// <param name="width">Width of sampling area.</param>
    /// <param name="height">Height of sampling area.</param>
    /// <param name="preExistingPoints">An array of pre-existing points.</param>
    /// <param name="seed">Seed for the generator</param>
    public PoissonDiscSampler(int radius, int width, int height, Vector2Int[] preExistingPoints, int seed) {
        this.radius = radius;
        this.width = width;
        this.height = height;
        this.wrap = false;

        rng = new System.Random(seed);

        activeList = new List<Vector2Int>(0);
        grid = new bool[width, height];

        foreach (Vector2Int point in preExistingPoints) {
            addSample(point);
        }
    }

    #endregion

    #region public functions

    /// <summary>
    /// Generates samples using Poisson Disk Sampling
    /// </summary>
    /// <returns>Resulting samples</returns>
    public IEnumerable<Vector2Int> sample() {
        if(activeList.Count == 0) {
            yield return addSample(new Vector2Int(rng.Next(1, width), rng.Next(1, height)));
        }

        while(activeList.Count > 0) {
            int activeSampleIndex = rng.Next(0, activeList.Count);
            Vector2Int activeSample = activeList[activeSampleIndex];
            bool addedNew = false;
            for(int i = 0; i < maxTestSamples; i++) {
                float angle = 2 * Mathf.PI * (float)rng.NextDouble();
                float distance = radius + radius * (float)rng.NextDouble();
                Vector2Int newPos = activeSample + new Vector2Int((int)(Mathf.Cos(angle) * distance), 
                                                                  (int)(Mathf.Sin(angle) * distance));

                if (wrap) {
                    newPos = new Vector2Int(Utils.mod(newPos.x, width), Utils.mod(newPos.y, height));
                }

                if ((wrap || inBounds(newPos)) && validateSample(newPos)) {
                    addedNew = true;
                    yield return addSample(newPos);
                    break;
                }
            }
            if (!addedNew) {
                activeList.RemoveAt(activeSampleIndex);
            }
        }
    }

    #endregion

    #region private functions

    /// <summary>
    /// Adds a sample to the active and accepted lists.
    /// </summary>
    /// <param name="position">Position of sample</param>
    /// <returns>The added position</returns>
    private Vector2Int addSample(Vector2Int position) {
        activeList.Add(position);
        grid[position.x, position.y] = true;
        return position;
    }


    /// <summary>
    /// Checks if the sample clears its neighbourhood.
    /// </summary>
    /// <param name="samplePos">Position to validate</param>
    /// <returns>Whether the sample position clears its neighbourhood</returns>
    private bool validateSample(Vector2Int samplePos) {
        int gridX = 0;
        int gridY = 0;
        for(int x = samplePos.x - radius; x <= samplePos.x + radius; x++) {
            for(int y = samplePos.y - radius; y <= samplePos.y + radius; y++) {
                gridX = x;
                gridY = y;

                if (wrap) {
                    gridX = Utils.mod(gridX, width);
                    gridY = Utils.mod(gridY, height);
                } else if(!inBounds(new Vector2Int(x, y))) {
                    continue;
                }

                if (grid[gridX, gridY] && Vector2.Distance(new Vector2Int(x, y), samplePos) < radius)
                    return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Check if a position is within bounds of the grid
    /// </summary>
    /// <param name="pos">Position to check</param>
    /// <returns>If the position is within bounds of the grid</returns>
    private bool inBounds(Vector2Int pos) {
        return (pos.x >= 0 && pos.y >= 0 && pos.x < width && pos.y < height);
    }

    #endregion
}