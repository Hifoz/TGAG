using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is responsible for handling the chunks that makes up the world.
/// It creates and places chunks into the world, keeping the player at the center of the world.
/// </summary>
public class ChunkManager : MonoBehaviour {

    public Transform player;
    public GameObject chunkPrefab;
    public TextureManager terrainTextureManager;
    public TextureManager treeTextureManager;
    public GameObject treePrefab;
    public GameObject animalPrefab;
    private Vector3 offset;
    private List<ChunkData> activeChunks = new List<ChunkData>();
    private Stack<GameObject> inactiveChunks = new Stack<GameObject>();
    private Stack<GameObject> inactiveTrees = new Stack<GameObject>();
    private ChunkData[,] chunkGrid;

    private ChunkVoxelDataThread[] CVDT;
    private BlockingQueue<Order> orders = new BlockingQueue<Order>(); //When this thread puts a position in this queue, the thread generates a mesh for that position.
    private LockingQueue<Result> results = new LockingQueue<Result>(); //When CVDT makes a mesh for a chunk the result is put in this queue for this thread to consume.
    private HashSet<Vector3> pendingChunks = new HashSet<Vector3>(); //Chunks that are currently worked on my CVDT

    private GameObject[] animals = new GameObject[20];
    private int orderedAnimalIndex = -1;

    /// <summary>
    /// Generate an initial set of chunks in the world
    /// </summary>
    void Start () {
        Settings.load();
        CVDT = new ChunkVoxelDataThread[Settings.WorldGenThreads];
        for (int i = 0; i < Settings.WorldGenThreads; i++) {
            CVDT[i] = new ChunkVoxelDataThread(orders, results);
        }
        init();
    }
	
	// Update is called once per frame
	void Update () {
        clearChunkGrid();
        updateChunkGrid();
        orderNewChunks();
        consumeThreadResults();
        handleAnimals();
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
        while (inactiveChunks.Count > 0) {
            Destroy(inactiveChunks.Pop());
        }
    }

    /// <summary>
    /// Initializes the ChunkManager
    /// </summary>
    public void init() {
        offset = new Vector3(-ChunkConfig.chunkCount / 2f * ChunkConfig.chunkSize, 0, -ChunkConfig.chunkCount / 2f * ChunkConfig.chunkSize);
        chunkGrid = new ChunkData[ChunkConfig.chunkCount, ChunkConfig.chunkCount];

        MeshDataGenerator.terrainTextureTypes = terrainTextureManager.getSliceTypeList();
        MeshDataGenerator.treeTextureTypes = treeTextureManager.getSliceTypeList();
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
                } else if (Vector3.Distance(animal.transform.position, player.position) > maxDistance) {
                    LandAnimal landAnimal = animal.GetComponent<LandAnimal>();
                    float x = UnityEngine.Random.Range(lower, upper);
                    float z = UnityEngine.Random.Range(lower, upper);
                    float y = ChunkConfig.chunkHeight + 10;
                    landAnimal.Spawn(player.position + new Vector3(x, y, z));
                    if (orderedAnimalIndex == -1 && UnityEngine.Random.Range(0f, 1f) < 0.1f) { // 20% chance of regenerating animal on respawn
                        AnimalSkeleton animalSkeleton = new AnimalSkeleton(animal.transform);
                        orders.Enqueue(new Order(animalSkeleton, Task.ANIMAL));
                        orderedAnimalIndex = i;
                    }
                }
            }
            if (orderedAnimalIndex != -1) {
                animals[orderedAnimalIndex].SetActive(false);
            }
        }
    }

    /// <summary>
    /// Clears all elements in the chunkGrid
    /// </summary>
    private void clearChunkGrid() {
        for (int x = 0; x < ChunkConfig.chunkCount; x++) {
            for (int z = 0; z < ChunkConfig.chunkCount; z++) {
                chunkGrid[x, z] = null;
            }
        }
    }

    /// <summary>
    /// Updates the chunk grid, assigning chunks to cells, 
    ///  and moving chunks that fall outside the grid into the inactive list.
    /// </summary>
    private void updateChunkGrid() {
        for (int i = 0; i < activeChunks.Count; i++) {
            Vector3 chunkPos = (activeChunks[i].chunk.transform.position - offset - getPlayerPos()) / ChunkConfig.chunkSize;
            int ix = Mathf.FloorToInt(chunkPos.x);
            int iz = Mathf.FloorToInt(chunkPos.z);
            if (checkBounds(ix, iz)) {
                chunkGrid[ix, iz] = activeChunks[i];
            } else {
                inactiveChunks.Push(activeChunks[i].chunk);
                inactiveChunks.Push(activeChunks[i].waterChunk);
                inactiveChunks.Peek().SetActive(false);

                foreach(var tree in activeChunks[i].trees) {
                    inactiveTrees.Push(tree);
                    tree.SetActive(false); 
                }

                activeChunks.RemoveAt(i);
                i--;
            }
        }
    }

    /// <summary>
    /// Orders needed chunks from the ChunkVoxelDataThreads.
    /// </summary>
    private void orderNewChunks() {
        for (int x = 0; x < ChunkConfig.chunkCount; x++) {
            for (int z = 0; z < ChunkConfig.chunkCount; z++) {
                Vector3 chunkPos = new Vector3(x, 0, z) * ChunkConfig.chunkSize + offset + getPlayerPos();
                if (chunkGrid[x, z] == null && !pendingChunks.Contains(chunkPos)) {
                    orders.Enqueue(new Order(chunkPos, Task.CHUNK));
                    pendingChunks.Add(chunkPos);
                }
            }
        }
    }

    /// <summary>
    /// Consumes results from Worker threads.
    /// </summary>
    private void consumeThreadResults() {
        while(results.getCount() > 0) {
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

        GameObject chunk = getChunk();
        chunk.transform.position = chunkMeshData.chunkPos;
        chunk.GetComponent<MeshFilter>().mesh = MeshDataGenerator.applyMeshData(chunkMeshData.meshData);
        chunk.GetComponent<MeshCollider>().sharedMesh = chunk.GetComponent<MeshFilter>().mesh;
        chunk.GetComponent<MeshCollider>().isTrigger = false;
        chunk.GetComponent<MeshCollider>().convex = false;
        chunk.name = "chunk";
        chunk.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_TexArr", terrainTextureManager.getTextureArray());
        chunk.GetComponent<MeshRenderer>().material.renderQueue = chunk.GetComponent<MeshRenderer>().material.shader.renderQueue - 1;
        cd.chunk = chunk;

        GameObject waterChunk = getChunk();
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
            GameObject tree = getTree();
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
    /// Gets the "chunk normalized" player position.
    /// </summary>
    /// <returns>Player position</returns>
    private Vector3 getPlayerPos() {
        float x = player.position.x;
        float z = player.position.z;
        x = Mathf.Floor(x / ChunkConfig.chunkSize) * ChunkConfig.chunkSize;
        z = Mathf.Floor(z / ChunkConfig.chunkSize) * ChunkConfig.chunkSize;
        return new Vector3(x, 0, z);
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
    /// Gets an inactive chunk, or creates a new chunk.
    /// </summary>
    /// <returns>An instance of a chunk gameobject</returns>
    private GameObject getChunk() {
        if (inactiveChunks.Count > 0) {
            var chunk = inactiveChunks.Pop();
            chunk.SetActive(true);
            return chunk;
        } else {
            return createChunk();
        }
    }

    /// <summary>
    /// Gets an inactive tree, or creates a new tree.
    /// </summary>
    /// <returns>An instance of a tree gameobject</returns>
    private GameObject getTree() {
        if (inactiveTrees.Count > 0) {
            var tree = inactiveTrees.Pop();
            tree.SetActive(true);
            return tree;
        } else {
            return createTree();
        }
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
