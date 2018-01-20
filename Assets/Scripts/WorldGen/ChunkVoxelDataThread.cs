﻿using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ChunkVoxelData {
    public MeshData meshData;
    public Vector3 chunkPos;
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
    void threadRunner() {
        Debug.Log("Thread alive!");
        while (run) {
            var order = orders.Dequeue();
            var result = new ChunkVoxelData();
            if (result.chunkPos == Vector3.down) {
                break;
            }
            result.chunkPos = order;
            result.meshData = MeshDataGenerator.GenerateMeshData(CVDG.getChunkVoxelData(order));
            results.Enqueue(result);
        }
        Debug.Log("Thread stopped!");
    }
}