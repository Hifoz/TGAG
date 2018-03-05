using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


/**
 * Implementation of Poisson Dsic Sampling
 * Inspired by https://bl.ocks.org/mbostock/19168c663618b7f07158
 *             http://gregschlom.com/devlog/2014/06/29/Poisson-disc-sampling-Unity.html
 *             http://www.cs.ubc.ca/~rbridson/docs/bridson-siggraph07-poissondisk.pdf
 */
class PoissonDiscSampler {
    private const int k = 30; // Max number of samples to test for any active list items

    private int width;
    private int height;

    private float radius;

    private List<Vector2Int> activeList;
    private List<Vector2Int> acceptedList;

    private System.Random rng = new System.Random(ChunkConfig.seed);

    /// <summary>
    /// Constructor for a blank sampler
    /// </summary>
    /// <param name="radius">Sampling radius. All points will be minimum 'radius' meters away from eachother, and there will be no position further away than 2 * 'radius' from a point</param>
    /// <param name="width">width of sampling area</param>
    /// <param name="height">height of sampling area</param>
    public PoissonDiscSampler(float radius, int width, int height) {
        this.radius = radius;
        this.width = width;
        this.height = height;
        
        activeList = new List<Vector2Int>();
        acceptedList = new List<Vector2Int>();
    }

    /// <summary>
    /// Constructor for a sampler with a collection of pre-defined points. Good to use if you want to grow an existing sample set.
    /// </summary>
    /// <param name="radius">Sampling radius. All points will be minimum 'radius' meters away from eachother, and there will be no position further away than 2 * 'radius' from a point</param>
    /// <param name="width">width of sampling area</param>
    /// <param name="height">height of sampling area</param>
    /// <param name="preExistingPoints">An array of pre-existing points</param>
    public PoissonDiscSampler(float radius, int width, int height, Vector2Int[] preExistingPoints) {
        this.radius = radius;
        this.width = width;
        this.height = height;

        activeList = new List<Vector2Int>(preExistingPoints);
        acceptedList = new List<Vector2Int>(preExistingPoints);
    }


    /// <summary>
    /// Adds a sample to the active and accepted lists.
    /// </summary>
    /// <param name="position">position of sample</param>
    /// <returns></returns>
    private Vector2 addSample(Vector2Int position) {
        activeList.Add(position);
        acceptedList.Add(position);
        return position;
    }


    /*
     * Using PoissonDiscSampler.sample():
     * ------------------
     * 
     * foreach(Vector2 sample in sampler.sampler())
     *      // Do stuff with sample here
     * 
     * 
     * ------- or -------
     * using System.Linq; // Needed for ToList() function
     * ...
     * Vector2[] samplesAsList = sampler.sample().ToList();
     * // Do stuff with samplesAsList
     * 
     * ------------------
     */

    /// <summary>
    /// Returns a lazy sequence for use in foreach loops.
    /// </summary>
    /// <returns>Lazy sequence for </returns>
    public IEnumerable<Vector2> sample() {
        if(activeList.Count == 0) {
            yield return addSample(new Vector2Int(rng.Next(1, width), rng.Next(1, height)));
        }

        while(activeList.Count > 0) {
            int activeSampleIndex = rng.Next(0, activeList.Count);
            Vector2Int activeSample = activeList[activeSampleIndex];
            bool addedNew = false;
            for(int i = 0; i < k; i++) {
                float angle = 2 * Mathf.PI * (float)rng.NextDouble();
                float distance = radius + radius * (float)rng.NextDouble();
                Vector2Int samplePos = activeSample + new Vector2Int((int)(Mathf.Cos(angle) * distance), (int)(Mathf.Sin(angle) * distance));
                if (samplePos.x >= 0 && samplePos.y >= 0 && samplePos.x < width && samplePos.y < height && sampleClearsNeighbourhood(samplePos)) {
                    addedNew = true;
                    yield return addSample(samplePos);
                    break;
                }
            }
            if (!addedNew) {
                activeList.RemoveAt(activeSampleIndex);
            }
        }
    }


    /// <summary>
    /// Checks if the sample clears its neighbourhood
    /// </summary>
    /// <param name="samplePos"></param>
    /// <returns></returns>
    private bool sampleClearsNeighbourhood(Vector2 samplePos) {
        foreach(Vector2Int pos in acceptedList) { // TODO: this doesnt need to check the entire grid, only those grid placements which could block the clearing
            if(pos != Vector2.zero && Vector2.Distance(pos, samplePos) < radius)
                return false;
        }
        return true;
    }


}
 
 
 
 
 