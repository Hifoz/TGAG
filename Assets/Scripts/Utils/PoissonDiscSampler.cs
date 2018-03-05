using System;
using System.Collections.Generic;
using UnityEngine;


class PoissonDiscSampler {
    private const int k = 30; // Max number of samples to test for any active list items

    private int width;
    private int height;

    private float radius;

    private Vector2Int[,] grid;
    private List<Vector2Int> activeList;

    private System.Random rng = new System.Random(ChunkConfig.seed);


    public PoissonDiscSampler(float radius, int width, int height) {
        this.radius = radius;
        this.width = width;
        this.height = height;

        grid = new Vector2Int[width, height];
        activeList = new List<Vector2Int>();
    }

    public void addSample(Vector2Int position) {
        grid[position.x, position.y] = position;
        activeList.Add(position);
    }

    public Vector2[] sample() {
        List<Vector2> acceptedSamples = new List<Vector2>();

        if(activeList.Count == 0) {
            addSample(new Vector2Int(rng.Next(0, width), rng.Next(0, height)));
        }

        while(activeList.Count > 0) {
            int activeSampleIndex = rng.Next(0, activeList.Count);
            Vector2 activeSample = activeList[activeSampleIndex];
            bool addedNew = false;
            for(int i = 0; i < k; i++) {
                float angle = 2 * Mathf.PI * (float)rng.NextDouble();
                float distance = radius + radius * (float)rng.NextDouble();
                Vector2Int samplePos = new Vector2Int((int)(Mathf.Cos(angle) * distance), (int)(Mathf.Sin(angle) * distance));
                if (samplePos.x >= 0 && samplePos.y >= 0 && samplePos.x < width && samplePos.y < height && sampleClearsNeighbourhood(samplePos)) {
                    addedNew = true;
                    grid[samplePos.x, samplePos.y] = samplePos;
                    acceptedSamples.Add(samplePos);
                    activeList.Add(samplePos);
                }
            }
            if (!addedNew) {
                activeList.RemoveAt(activeSampleIndex);
            }
        }



        /*
         * while more active:
         *      get random active A
         *      generate k random samples around A between r and r*2 distance away
         *      for all samples S:
         *          check that it clears the neighbourhood
         *              if it clears, add it to the grid and acceptedsamples
         *          if one clears, add it to active and stop checking
         *      if no samples clears, remove A from active list
         *      
         * 
         * 
         * 
         * 
         */


        return acceptedSamples.ToArray();
    }

    /// <summary>
    /// Checks if the sample clears its neighbourhood
    /// </summary>
    /// <param name="samplePos"></param>
    /// <returns></returns>
    private bool sampleClearsNeighbourhood(Vector2 samplePos) {
        foreach(Vector2Int pos in grid) { // TODO: this doesnt need to check the entire grid, only those grid placements which could block the clearing
            if (Vector2.Distance(pos, samplePos) < radius)
                return false;
        }
        return true;
    }


}