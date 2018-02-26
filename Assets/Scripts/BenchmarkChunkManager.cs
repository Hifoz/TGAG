using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

public class BenchmarkChunkManager : MonoBehaviour {

    Stopwatch stopwatch = new Stopwatch();

    public GameObject chunkPrefab;
    public TextureManager textureManager;
    public GameObject treePrefab;
    public GameObject animalPrefab;
    private Vector3 offset;
    private List<ChunkData> activeChunks = new List<ChunkData>();

    private ChunkVoxelDataThread[] CVDT;
    private BlockingList<Order> orders = new BlockingList<Order>(); //When this thread puts a position in this list, the thread generates a mesh for that position.
    private LockingQueue<Result> results = new LockingQueue<Result>(); //When CVDT makes a mesh for a chunk the result is put in this queue for this thread to consume.
    private HashSet<Vector3> pendingChunks = new HashSet<Vector3>(); //Chunks that are currently worked on my CVDT

    private GameObject[] animals = new GameObject[20];
    private HashSet<int> orderedAnimals = new HashSet<int>();

    bool terrainFlag = true;
    bool animalsFlag = true;

    private bool inProgress = false;
    private string path = "";
    private int currentThreads = 0;

    /// <summary>
    /// Generate an initial set of chunks in the world
    /// </summary>
    void Start() {
        Settings.load();
        ChunkConfig.chunkCount = 20;
        offset = new Vector3(-ChunkConfig.chunkCount / 2f * ChunkConfig.chunkSize, 0, -ChunkConfig.chunkCount / 2f * ChunkConfig.chunkSize);
    }

    public bool InProgress { get { return inProgress; } }
    public string Path { get { return path; } }
    public int CurrentThreads { get { return currentThreads; } }

    public float getTime() {
        return (float)stopwatch.Elapsed.TotalSeconds;
    }

    public float getProgress() {
        int generatedThings = 0;
        int totalThings = 0;

        if (terrainFlag) {
            generatedThings += activeChunks.Count;
            totalThings += ChunkConfig.chunkCount * ChunkConfig.chunkCount;
        }

        if (animalsFlag) {
            generatedThings += (animals.Length - orderedAnimals.Count);
            totalThings += animals.Length;
        }

        if (totalThings == 0) {
            return 1f;
        }
        return generatedThings / (float)totalThings;
    }

    /// <summary>
    /// Call this corutine to start a benchmark run
    /// </summary>
    /// <param name="startThreads">Thread count to start from</param>
    /// <param name="endThreads">Thread count to end on</param>
    /// <param name="step">Step count to increment threads by</param>
    /// <param name="terrain">Set this to true to generate terrain</param>
    /// <param name="animals">Set this to true to generate animals</param>
    public bool Benchmark(int startThreads, int endThreads, int step, bool terrain, bool animals) {
        if (!inProgress) {
            StartCoroutine(BenchmarkWorldGen(startThreads, endThreads, step, terrain, animals));
            return true;
        } else {
            return false;
        }
    }

    /// <summary>
    /// Call this function to start a benchmark run,
    /// if a benchmark is already in progress nothing happends.
    /// </summary>
    /// <param name="startThreads">Thread count to start from</param>
    /// <param name="endThreads">Thread count to end on</param>
    /// <param name="step">Step count to increment threads by</param>
    /// <param name="terrain">Set this to true to generate terrain</param>
    /// <param name="animals">Set this to true to generate animals</param>
    /// <returns>Success flag</returns>
    IEnumerator BenchmarkWorldGen(int startThreads, int endThreads, int step, bool terrain, bool animals) {
        QualitySettings.vSyncCount = 0;
        terrainFlag = terrain;
        animalsFlag = animals;
        inProgress = true;
        yield return 0; //THis random yield i put in for debugging suddenly fixed all bugs.

        path = string.Format("C:/temp/TGAG_MultiThreading_Benchmark_{0}.txt", DateTime.Now.Ticks);
        Directory.CreateDirectory("C:/temp");
        StreamWriter file = File.CreateText(path);

        file.WriteLine(string.Format("Testing from {0} to {1} threads with a step of {2}. ({3}):", startThreads, endThreads, step, DateTime.Now.ToString()));
        file.WriteLine(string.Format("Terrain: {0}", (terrain) ? "Enabled" : "Disabled"));
        file.WriteLine(string.Format("Animals: {0}", (animals) ? "Enabled" : "Disabled"));

        for (int run = startThreads; run <= endThreads; run += step) {
            UnityEngine.Debug.Log(String.Format("Testing with {0} thread(s)!", run));
            clear();
            currentThreads = run;
            CVDT = new ChunkVoxelDataThread[run];
            for (int i = 0; i < run; i++) {
                CVDT[i] = new ChunkVoxelDataThread(orders, results, i);
            }
            stopwatch.Start();

            if (terrain) {
                orderNewChunks();
            }
            if (animals) {
                orderAnimals();
            }

            int frameCount = 0;

            while (!(benchmakrRunFinished())) {
                //foreach (var thread in CVDT) {
                //    UnityEngine.Debug.Log(thread.getThreadState().ToString());
                //}
                frameCount++;
                consumeThreadResults();
                yield return 0;
            }
            stopThreads();
            double time = stopwatch.Elapsed.TotalSeconds;

            stopwatch.Stop();
            stopwatch.Reset();
            string result = String.Format("Time: {0} Seconds | Average fps: {1} | Threads: {2}", time.ToString("N2"), (frameCount / time).ToString("N2"), run);
            UnityEngine.Debug.Log(result);
            file.WriteLine(String.Format(result));
        }
        file.Close();
        UnityEngine.Debug.Log("DONE TESTING!");
        inProgress = false;
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
            Destroy(activeChunks[0].terrainChunk[0].transform.parent.gameObject);
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

    private bool benchmakrRunFinished() {
        bool a = (activeChunks.Count == ChunkConfig.chunkCount * ChunkConfig.chunkCount) || !terrainFlag;
        bool b = orderedAnimals.Count == 0 || !animalsFlag;
        return a && b;
    }

    /// <summary>
    /// Handles spawning of animals.
    /// </summary>
    private void orderAnimals() {
        float maxDistance = ChunkConfig.chunkCount * ChunkConfig.chunkSize / 2;
        float lower = -maxDistance + LandAnimalNPC.roamDistance;
        float upper = -lower;
        for (int i = 0; i < animals.Length; i++) {
            animals[i] = Instantiate(animalPrefab);
            float x = UnityEngine.Random.Range(lower, upper);
            float z = UnityEngine.Random.Range(lower, upper);
            float y = ChunkConfig.chunkHeight + 10;
            animals[i].transform.position = new Vector3(x, y, z);
            animals[i].GetComponent<LandAnimalNPC>().enabled = false;

            AnimalSkeleton animalSkeleton = new AnimalSkeleton(animals[i].transform);
            animalSkeleton.index = i;
            orders.Add(new Order(animals[i].transform.position, animalSkeleton, Task.ANIMAL));
            orderedAnimals.Add(i);
        }
    }

    /// <summary>
    /// Orders needed chunks from the ChunkVoxelDataThreads.
    /// </summary>
    private void orderNewChunks() {
        for (int x = 0; x < ChunkConfig.chunkCount; x++) {
            for (int z = 0; z < ChunkConfig.chunkCount; z++) {
                Vector3 chunkPos = new Vector3(x, 0, z) * ChunkConfig.chunkSize + offset;
                orders.Add(new Order(chunkPos, Task.CHUNK));
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


        GameObject chunk = new GameObject();
        chunk.name = "chunk";
        for (int i = 0; i < chunkMeshData.meshData.Length; i++) {
            GameObject subChunk = createChunk();
            subChunk.transform.parent = chunk.transform;
            subChunk.transform.position = chunkMeshData.chunkPos;
            subChunk.GetComponent<MeshFilter>().mesh = MeshDataGenerator.applyMeshData(chunkMeshData.meshData[i]);
            subChunk.GetComponent<MeshCollider>().sharedMesh = subChunk.GetComponent<MeshFilter>().mesh;
            subChunk.GetComponent<MeshCollider>().isTrigger = false;
            subChunk.GetComponent<MeshCollider>().convex = false;
            subChunk.name = "subchunk";
            subChunk.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_TexArr", textureManager.getTextureArray());
            subChunk.GetComponent<MeshRenderer>().material.renderQueue = subChunk.GetComponent<MeshRenderer>().material.shader.renderQueue - 1;
            cd.terrainChunk.Add(subChunk);
        }

        for (int i = 0; i < chunkMeshData.waterMeshData.Length; i++) {
            GameObject waterChunk = createChunk();
            waterChunk.transform.parent = chunk.transform;
            waterChunk.transform.position = chunkMeshData.chunkPos;
            waterChunk.GetComponent<MeshFilter>().mesh = MeshDataGenerator.applyMeshData(chunkMeshData.waterMeshData[i]);
            waterChunk.GetComponent<MeshCollider>().sharedMesh = waterChunk.GetComponent<MeshFilter>().mesh;
            waterChunk.GetComponent<MeshCollider>().convex = true;
            waterChunk.GetComponent<MeshCollider>().isTrigger = true;
            waterChunk.name = "waterSubChunk";
            waterChunk.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_TexArr", textureManager.getTextureArray());
            waterChunk.GetComponent<MeshRenderer>().material.renderQueue = waterChunk.GetComponent<MeshRenderer>().material.shader.renderQueue;
            cd.waterChunk.Add(waterChunk);
        }
        GameObject[] trees = new GameObject[chunkMeshData.trees.Length];
        for (int i = 0; i < trees.Length; i++) {
            GameObject tree = createTree();
            tree.transform.position = chunkMeshData.treePositions[i];
            tree.GetComponent<MeshFilter>().mesh = MeshDataGenerator.applyMeshData(chunkMeshData.trees[i]);
            tree.GetComponent<MeshCollider>().sharedMesh = MeshDataGenerator.applyMeshData(chunkMeshData.treeTrunks[i]);
            tree.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_TexArr", textureManager.getTextureArray());

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
        GameObject animal = animals[animalSkeleton.index];
        LandAnimalNPC LandAnimalNPC = animal.GetComponent<LandAnimalNPC>();
        LandAnimalNPC.enabled = true;
        animal.GetComponent<LandAnimalNPC>().setSkeleton(animalSkeleton);
        LandAnimalNPC.enabled = false;
        orderedAnimals.Remove(animalSkeleton.index);
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
        if (CVDT != null) {
            foreach (var thread in CVDT) {
                orders.Add(new Order(Vector3.down, Task.CHUNK));
                thread.stop();
            }
        }
    }

    private void OnDestroy() {
        stopThreads();
    }

    private void OnApplicationQuit() {
        stopThreads();
    }
}
