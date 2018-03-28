using System;
using System.Threading;
using UnityEngine;
using System.Collections.Generic;

public class ChunkVoxelData {
    public MeshData[] meshData;
    public MeshData[] waterMeshData;
    public Vector3 chunkPos;

    public MeshData[] trees;
    public MeshData[] treeTrunks;
    public Vector3[] treePositions;

    public ChunkVoxelData(Vector3 position) {
        this.chunkPos = position;
    }
}

public enum Task {
    CHUNK = 0,
    ANIMAL,
    CANCEL
}

/// <summary>
/// A class representing an order to be done by the thread
/// </summary>
public class Order {
    public Order(Vector3 position, Task task) {
        this.position = position;
        this.task = task;
    }

    public Order(Vector3 position, AnimalSkeleton animalSkeleton, Task task) {
        this.animalSkeleton = animalSkeleton;
        this.position = position;
        this.task = task;
    }

    public Vector3 position; // Used for chunks
    public AnimalSkeleton animalSkeleton; // Used for animals
    public Task task;
}

/// <summary>
/// A class representing the result of a job done by the thread
/// </summary>
public class Result {
    public Task task;
    public ChunkVoxelData chunkVoxelData; //May not be set
    public AnimalSkeleton animalSkeleton; //May not be set
}

/// <summary>
/// A thread that generates chunkdata based on positions.
/// </summary>
public class ChunkVoxelDataThread {

    private Thread thread;
    private BlockingList<Order> orders;   //When the main thread puts a position in this queue, the thread generates a mesh for that position.
    private LockingQueue<Result> results; //When this thread makes a mesh for a chunk the result is put in this queue for the main thread to consume.
    private bool run;

    BiomeManager biomeManager;


    /// <summary>
    /// Constructor that takes the two needed queues, also starts thread excecution.
    /// </summary>
    /// <param name="orders"></param>
    /// <param name="results"></param>
    public ChunkVoxelDataThread(BlockingList<Order> orders, LockingQueue<Result> results, int index, BiomeManager biomeManager) {
        this.orders = orders;
        this.results = results;
        this.biomeManager = biomeManager;
        run = true;
        thread = new Thread(new ThreadStart(threadRunner)); //This starts running the update function
        thread.Priority = System.Threading.ThreadPriority.Highest;
        thread.Name = "WorldGen Thread " + index.ToString();
        thread.Start();
    }

    public ThreadState getThreadState() {
        return thread.ThreadState;
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
                Order order = orders.Take(getPreferredOrder);
                if(order == null) {
                    Debug.Log("Order is null");
                    continue;
                }
                if (order.position == Vector3.down) {
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
    private Result handleOrder(Order order) {
        Result result = new Result();
        result.task = order.task;

        switch (order.task) {
            case Task.CHUNK:
                result.chunkVoxelData = handleChunkOrder(order);
                break;
            case Task.ANIMAL:
                order.animalSkeleton.generateInThread();
                result.animalSkeleton = order.animalSkeleton;
                break;
            case Task.CANCEL:
                result.task = Task.CANCEL;
                result.chunkVoxelData = new ChunkVoxelData(order.position);
                break;
        }

        return result;
    }

    /// <summary>
    /// Shoudl chunk at position be canceled?
    /// </summary>
    /// <param name="pos">Position of chunk</param>
    /// <returns>bool should cancel</returns>
    private bool cancelChunk(Vector3 pos) {
        Vector3 distFromPlayer = pos - Player.playerPos.get();
        if (Mathf.Abs(distFromPlayer.x) > WorldGenConfig.chunkSize * (WorldGenConfig.chunkCount + 5) * 0.5f || Mathf.Abs(distFromPlayer.z) > WorldGenConfig.chunkSize * (WorldGenConfig.chunkCount + 5) * 0.5f) {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Generates a chunk with trees
    /// </summary>
    /// <param name="order">Order order</param>
    /// <returns>ChunkVoxelData</returns>
    private ChunkVoxelData handleChunkOrder(Order order) {
        ChunkVoxelData result = new ChunkVoxelData(order.position);
        //Generate the chunk terrain:
        BlockDataMap chunkBlockData = ChunkVoxelDataGenerator.getChunkVoxelData(order.position, biomeManager);
        result.meshData = MeshDataGenerator.GenerateMeshData(chunkBlockData);
        result.waterMeshData = WaterMeshDataGenerator.GenerateWaterMeshData(chunkBlockData);


        //Generate the trees in the chunk:
        int maxTrees = biomeManager.getClosestBiome(new Vector2Int((int)order.position.x, (int)order.position.z)).maxTreesPerChunk;

        System.Random rng = new System.Random(NoiseUtils.Vector2Seed(order.position));
        int treeCount = Mathf.CeilToInt(((float)rng.NextDouble() * maxTrees) - 0.5f);

        List<MeshData> trees = new List<MeshData>();
        List<MeshData> treeTrunks = new List<MeshData>();
        List<Vector3> treePositions = new List<Vector3>();

        for (int i = 0; i < treeCount; i++) {
            Vector3 localPos = new Vector3((float)rng.NextDouble() * WorldGenConfig.chunkSize, 0, (float)rng.NextDouble() * WorldGenConfig.chunkSize);
            if (Corruption.corruptionFactor(localPos + order.position) >= 1f) {
                continue;
            }
            localPos = findGroundLevel(Utils.floorVectorToInt(localPos), chunkBlockData);
            if (localPos.y > WorldGenConfig.waterHeight + 2) {
                if(localPos != Vector3.down) {
                    MeshData[] tree = LSystemTreeGenerator.generateMeshData(localPos, order.position, chunkBlockData, biomeManager);
                    trees.Add(tree[0]);
                    treeTrunks.Add(tree[1]);
                    treePositions.Add(localPos);
                } else {
                    i--; //Try again
                }
            }
        }
        result.trees = trees.ToArray();
        result.treeTrunks = treeTrunks.ToArray();
        result.treePositions = treePositions.ToArray();
        return result;
    }

    /// <summary>
    /// Finds the groundlevel for the x and z coordinate.
    /// </summary>
    /// <param name="pos">Vector3 position to investigate</param>
    /// <returns>Vector3 ground level position</returns>
    private Vector3 findGroundLevel(Vector3Int localPos, BlockDataMap data) {
        int halfLength = data.GetLength(1) / 2;
        localPos.y = halfLength;
        bool lastVoxel = data.mapdata[data.index1D(localPos)].blockType != BlockData.BlockType.NONE;
        bool currentVoxel = lastVoxel;
        int dir = (lastVoxel) ? 1 : -1;
        
        for (int i = 0; i < halfLength; i++) { 
            localPos.y += dir;
            lastVoxel = currentVoxel;
            currentVoxel = data.mapdata[data.index1D(localPos)].blockType != BlockData.BlockType.NONE;
            if (lastVoxel != currentVoxel) {
                if (lastVoxel) { //Put the tree in an empty voxel
                    localPos.y -= dir;
                }
                return localPos;
            }
        }
        return Vector3.down;
    }

    /// <summary>
    /// Used to find the index of the order that is most preferrable to handle next.
    /// </summary>
    /// <param name="list">list of orders</param>
    /// <returns>index of preferred order</returns>
    private int getPreferredOrder(List<Order> list) {
        int resultIndex = -1;
        float preferredValue = Int32.MaxValue;
        for(int i = 0; i < list.Count; i++) {
            if (list[i].task == Task.CHUNK) { //Prioritize canceling chunks the most
                if (cancelChunk(list[i].position)) {
                    list[i].task = Task.CANCEL;
                    return i;
                }
            }

            Vector3 chunkPos = list[i].position;
            Vector3 playerPos = Player.playerPos.get();
            Vector3 playerMoveDir = Player.playerSpeed.get();
            Vector3 cameraViewDir = CameraController.cameraDir.get();

            Vector3 preferredDir = playerMoveDir * 2 + cameraViewDir;
            Vector3 chunkDir = (chunkPos - playerPos);
            chunkDir.y = 0;

            float angleFromPreferredDir = Vector3.Angle(preferredDir, chunkDir);
            float distFromPlayer = Vector3.Distance(playerPos, chunkPos);
            float value = angleFromPreferredDir + distFromPlayer;
            if (list[i].animalSkeleton != null) { //Animals are quick to generate.
                value /= 4;
            }

            if (value < preferredValue) {
                resultIndex = i;
                preferredValue = value;
            }
        }
        return resultIndex;
    }
}
