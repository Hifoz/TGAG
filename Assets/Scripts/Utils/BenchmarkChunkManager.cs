using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

public class BenchmarkChunkManager : MonoBehaviour {

    Stopwatch stopwatch;

    public GameObject chunkPrefab;
    public TextureManager terrainTextureManager;
    public TextureManager treeTextureManager;
    public GameObject treePrefab;
    public GameObject animalPrefab;
    private Vector3 offset;
    private List<ChunkData> activeChunks = new List<ChunkData>();

    private ChunkVoxelDataThread[] CVDT;
    private BlockingQueue<Order> orders = new BlockingQueue<Order>(); //When this thread puts a position in this queue, the thread generates a mesh for that position.
    private LockingQueue<Result> results = new LockingQueue<Result>(); //When CVDT makes a mesh for a chunk the result is put in this queue for this thread to consume.
    private HashSet<Vector3> pendingChunks = new HashSet<Vector3>(); //Chunks that are currently worked on my CVDT

    private GameObject[] animals = new GameObject[20];
    private int orderedAnimalIndex = -1;

    /// <summary>
    /// Generate an initial set of chunks in the world
    /// </summary>
    void Start() {
        Settings.load();
        init();
        StartCoroutine(BenchmarkWorldGen(8, 8));
    }

    // Update is called once per frame
    IEnumerator BenchmarkWorldGen(int startThreads, int endThreads) {
        yield return 0; //THis random yield i put in for debugging suddenly fixed all bugs.
        string path = string.Format("C:/temp/TGAG_MultiThreading_Benchmark_{0}.txt", DateTime.Now.Ticks);
        Directory.CreateDirectory("C:/temp");
        StreamWriter file = File.CreateText(path);

        file.WriteLine(string.Format("Testing from {0} to {1} threads ({2}):", startThreads, endThreads, DateTime.Now.ToString()));

        for (int run = startThreads; run <= endThreads; run++) {
            UnityEngine.Debug.Log(String.Format("Testing with {0} threads!", run));
            clear();
            CVDT = new ChunkVoxelDataThread[run];
            for (int i = 0; i < run; i++) {
                CVDT[i] = new ChunkVoxelDataThread(orders, results);
            }
            stopwatch = new Stopwatch();
            stopwatch.Start();

            orderNewChunks();

            while (!(benchmakrRunFinished())) {
                handleAnimals();
                consumeThreadResults();
                yield return 0;
            }
            stopThreads();
            double time = stopwatch.Elapsed.TotalSeconds;

            stopwatch.Stop();
            UnityEngine.Debug.Log(String.Format("Took {0} seconds with {1} threads!", time, run));
            file.WriteLine(String.Format("Time: {0} Seconds | Threads: {1}", time, run));
        }
        file.Close();
        UnityEngine.Debug.Log("DONE TESTING!");
    }

    /// <summary>
    /// Clears and resets the ChunkManager, used when changing WorldGen settings at runtime.
    /// </summary>
    public void clear() {
        while (pendingChunks.Count > 0) {
            while (results.getCount() > 0) {
                Result result = results.Dequeue();
                if (result.task == Task.CHUNK) {
                    pendingChunks.Remove(result.chunkVoxelData.chunkPos);
                }
            }
        }
        while (activeChunks.Count > 0) {
            Destroy(activeChunks[0].chunk);
            foreach (var tree in activeChunks[0].trees) {
                Destroy(tree);
            }
            activeChunks.RemoveAt(0);
        }
        for (int i = 0; i < animals.Length; i++) {
            if (animals[i] != null) {
                Destroy(animals[i]);
            }
        }
    }

    /// <summary>
    /// Initializes the ChunkManager
    /// </summary>
    public void init() {
        offset = new Vector3(-ChunkConfig.chunkCount / 2f * ChunkConfig.chunkSize, 0, -ChunkConfig.chunkCount / 2f * ChunkConfig.chunkSize);

        MeshDataGenerator.terrainTextureTypes = terrainTextureManager.getSliceTypeList();
        MeshDataGenerator.treeTextureTypes = treeTextureManager.getSliceTypeList();
    }

    private bool benchmakrRunFinished() {
        return (activeChunks.Count == ChunkConfig.chunkCount * ChunkConfig.chunkCount) && (orderedAnimalIndex + 1) == animals.Length;
    }

    /// <summary>
    /// Handles spawning of animals.
    /// </summary>
    private void handleAnimals() {
        if (animalPrefab) {
            float maxDistance = ChunkConfig.chunkCount * ChunkConfig.chunkSize / 2;
            float lower = -maxDistance + LandAnimal.roamDistance;
            float upper = -lower;
            for (int i = 0; i < animals.Length; i++) {
                GameObject animal = animals[i];
                if (animal == null) {
                    if (orderedAnimalIndex == -1) {
                        animals[i] = Instantiate(animalPrefab);
                        AnimalSkeleton animalSkeleton = new AnimalSkeleton(animals[i].transform);
                        orders.Enqueue(new Order(animalSkeleton, Task.ANIMAL));
                        orderedAnimalIndex = i;
                    }
                } else if (animal.activeSelf && Vector3.Distance(animal.transform.position, Vector3.zero) > maxDistance) {
                    LandAnimal landAnimal = animal.GetComponent<LandAnimal>();
                    float x = UnityEngine.Random.Range(lower, upper);
                    float z = UnityEngine.Random.Range(lower, upper);
                    float y = ChunkConfig.chunkHeight + 10;
                    landAnimal.Spawn(new Vector3(x, y, z));
                }
            }
            if (orderedAnimalIndex != -1) {
                animals[orderedAnimalIndex].SetActive(false);
            }
        }
    }

    /// <summary>
    /// Orders needed chunks from the ChunkVoxelDataThreads.
    /// </summary>
    private void orderNewChunks() {
        for (int x = 0; x < ChunkConfig.chunkCount; x++) {
            for (int z = 0; z < ChunkConfig.chunkCount; z++) {
                Vector3 chunkPos = new Vector3(x, 0, z) * ChunkConfig.chunkSize + offset;
                orders.Enqueue(new Order(chunkPos, Task.CHUNK));
                pendingChunks.Add(chunkPos);                
            }
        }
    }

    /// <summary>
    /// Consumes results from Worker threads.
    /// </summary>
    private void consumeThreadResults() {
        while (results.getCount() > 0) {
            Result result = results.Dequeue();
            switch (result.task) {
                case Task.CHUNK:
                    launchOrderedChunk(result.chunkVoxelData);
                    break;
                case Task.ANIMAL:
                    applyOrderedAnimal(result.animalSkeleton);
                    break;
            }
        }
    }

    /// <summary>
    /// Deploys ordered chunks from the ChunkVoxelDataThreads.
    /// </summary>
    private void launchOrderedChunk(ChunkVoxelData chunkMeshData) {
        pendingChunks.Remove(chunkMeshData.chunkPos);
        ChunkData cd = new ChunkData(chunkMeshData.chunkPos);

        GameObject chunk = createChunk();
        chunk.transform.position = chunkMeshData.chunkPos;
        chunk.GetComponent<MeshFilter>().mesh = MeshDataGenerator.applyMeshData(chunkMeshData.meshData);
        chunk.GetComponent<MeshCollider>().sharedMesh = chunk.GetComponent<MeshFilter>().mesh;
        chunk.GetComponent<MeshCollider>().isTrigger = false;
        chunk.GetComponent<MeshCollider>().convex = false;
        chunk.name = "chunk";
        chunk.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_TexArr", terrainTextureManager.getTextureArray());
        chunk.GetComponent<MeshRenderer>().material.renderQueue = chunk.GetComponent<MeshRenderer>().material.shader.renderQueue - 1;
        cd.chunk = chunk;

        GameObject waterChunk = createChunk();
        waterChunk.transform.position = chunkMeshData.chunkPos;
        waterChunk.GetComponent<MeshFilter>().mesh = MeshDataGenerator.applyMeshData(chunkMeshData.waterMeshData);
        waterChunk.GetComponent<MeshCollider>().sharedMesh = waterChunk.GetComponent<MeshFilter>().mesh;
        waterChunk.GetComponent<MeshCollider>().convex = true;
        waterChunk.GetComponent<MeshCollider>().isTrigger = true;
        waterChunk.name = "waterChunk";
        waterChunk.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_TexArr", terrainTextureManager.getTextureArray());
        waterChunk.GetComponent<MeshRenderer>().material.renderQueue = chunk.GetComponent<MeshRenderer>().material.shader.renderQueue;
        cd.waterChunk = waterChunk;

        GameObject[] trees = new GameObject[chunkMeshData.trees.Length];
        for (int i = 0; i < trees.Length; i++) {
            GameObject tree = createTree();
            tree.transform.position = chunkMeshData.treePositions[i];
            tree.GetComponent<MeshFilter>().mesh = MeshDataGenerator.applyMeshData(chunkMeshData.trees[i]);
            tree.GetComponent<MeshCollider>().sharedMesh = MeshDataGenerator.applyMeshData(chunkMeshData.treeTrunks[i]);
            tree.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_TexArr", treeTextureManager.getTextureArray());

            trees[i] = tree;
        }
        cd.trees = trees;

        activeChunks.Add(cd);
    }

    /// <summary>
    /// Applies the animalSkeleton to the animal
    /// </summary>
    /// <param name="animalSkeleton">AnimalSkeleton animalSkeleton</param>
    private void applyOrderedAnimal(AnimalSkeleton animalSkeleton) {
        GameObject animal = animals[orderedAnimalIndex];
        animal.SetActive(true);
        animal.GetComponent<LandAnimal>().setSkeleton(animalSkeleton);
        orderedAnimalIndex = -1;
    }

    /// <summary>
    /// Checks if X and Y are in bound for the ChunkGrid array.
    /// </summary>
    /// <param name="x">x index</param>
    /// <param name="y">y index (worldspace z)</param>
    /// <returns>bool in bound</returns>
    private bool checkBounds(int x, int y) {
        return (x >= 0 && x < ChunkConfig.chunkCount && y >= 0 && y < ChunkConfig.chunkCount);
    }

    /// <summary>
    /// Creates a chunk GameObject
    /// </summary>
    /// <returns>GameObject Chunk</returns>
    private GameObject createChunk() {
        GameObject chunk = Instantiate(chunkPrefab);
        chunk.transform.parent = transform;
        return chunk;
    }


    /// <summary>
    /// Creates a tree GameObject
    /// </summary>
    /// <returns>GameObject tree</returns>
    private GameObject createTree() {
        GameObject tree = Instantiate(treePrefab);
        tree.transform.parent = transform;
        tree.name = "tree";
        return tree;
    }

    /// <summary>
    /// Stops all of the ChunkVoxelDataThreads.
    /// </summary>
    private void stopThreads() {
        foreach (var thread in CVDT) {
            orders.Enqueue(new Order(Vector3.down, Task.CHUNK));
            thread.stop();
        }
    }

    private void OnDestroy() {
        stopThreads();
    }

    private void OnApplicationQuit() {
        stopThreads();
    }
}
