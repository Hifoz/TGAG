using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ChunkVoxelData {
    public MeshData meshData;
    public Vector3 chunkPos;

    public MeshData[] trees;
    public Vector3[] treePositions;
}

/// <summary>
/// A thread that generates chunkdata based on positions.
/// </summary>
public class ChunkVoxelDataThread {

    private Thread thread;
    private BlockingQueue<Vector3> orders; //When the main thread puts a position in this queue, the thread generates a mesh for that position.
    private LockingQueue<ChunkVoxelData> results; //When this thread makes a mesh for a chunk the result is put in this queue for the main thread to consume.
    private bool run;

    private ChunkVoxelDataGenerator CVDG = new ChunkVoxelDataGenerator();
    /// <summary>
    /// Constructor that takes the two needed queues, also starts thread excecution.
    /// </summary>
    /// <param name="orders"></param>
    /// <param name="results"></param>
    public ChunkVoxelDataThread(BlockingQueue<Vector3> orders, LockingQueue<ChunkVoxelData> results) {        
        this.orders = orders;
        this.results = results;
        run = true;
        thread = new Thread(new ThreadStart(threadRunner)); //This starts running the update function
        thread.Start();
    }

    /// <summary>
    /// Returns thread run state.
    /// </summary>
    /// <returns>bool isRunning</returns>
    public bool isRunning() {
        return run;
    }

    /// <summary>
    /// Stops thread excecution.
    /// </summary>
    public void stop() {
        run = false;
    }

    /// <summary>
    /// The function running the thread, processes orders and returns results to main thread.
    /// </summary>
    private void threadRunner() {
        Debug.Log("Thread alive!");
        while (run) {
            try {
                Vector3 order = orders.Dequeue();
                if (order == Vector3.down) {
                    break;
                }
                results.Enqueue(handleOrder(order));
            } catch(Exception e) {
                Debug.LogException(e);
            }
        }
        Debug.Log("Thread stopped!");
    }

    /// <summary>
    /// Takes an order for a chunk, and produces the data needed to create the chunk.
    /// </summary>
    /// <param name="order">Vector3 order, location of the chunk</param>
    /// <returns>ChunkVoxelData result, data needed by main thread to create chunk</returns>
    private ChunkVoxelData handleOrder(Vector3 order) {
        ChunkVoxelData result = new ChunkVoxelData();
        //Generate the chunk terrain
        result.chunkPos = order;
        result.meshData = MeshDataGenerator.GenerateMeshData(CVDG.getChunkVoxelData(order));
        //Generate the trees in the chunk
        System.Random rng = new System.Random(NoiseUtils.Vector2Seed(order));
        int trees = (int)(rng.NextDouble() * ChunkConfig.maxTreesPerChunk);
        result.trees = new MeshData[trees];
        result.treePositions = new Vector3[trees];

        for (int i = 0; i < trees; i++) {
            Vector3 pos = new Vector3((float)rng.NextDouble() * ChunkConfig.chunkSize, 0, (float)rng.NextDouble() * ChunkConfig.chunkSize);
            pos += order;
            pos = WorldUtils.floor(pos);
            pos = findGroundLevel(pos);
            if (pos != Vector3.negativeInfinity) {
                result.trees[i] = LSystemTreeGenerator.generateMeshData(pos);
                result.treePositions[i] = pos;
            } else {
                i--; //Try again
            }
        }

        return result;
    }

    /// <summary>
    /// Finds the groundlevel for the x and z coordinate.
    /// </summary>
    /// <param name="pos">Vector3 position to investigate</param>
    /// <returns>Vector3 ground level position</returns>
    private Vector3 findGroundLevel(Vector3 pos) {
        const int maxIter = 100;
        int iter = 0;

        float height = ChunkVoxelDataGenerator.calcHeight(pos);
        pos.y = height;
        bool lastVoxel = ChunkVoxelDataGenerator.posContainsVoxel(pos);
        bool currentVoxel = lastVoxel;
        int dir = (lastVoxel) ? 1 : -1;
        
        //TODO - Make accurate.
        while (iter < maxIter) {
            pos.y += dir;
            lastVoxel = currentVoxel;
            currentVoxel = ChunkVoxelDataGenerator.posContainsVoxel(pos);
            if (lastVoxel != currentVoxel) {
                return pos;
            }
            iter++;
        }
        return Vector3.negativeInfinity;
    }
}
