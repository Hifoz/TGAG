using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Struct that contains statistics about what the ChunkManager does
/// </summary>
public struct ChunkManagerStats {
    public int generatedChunks;
    public int generatedAnimals;
    public int cancelledChunks;
}

/// <summary>
/// This class is responsible for handling the chunks that makes up the world.
/// It creates and places chunks into the world, keeping the player at the center of the world.
/// </summary>
public class ChunkManager : MonoBehaviour {

    public ChunkManagerStats stats = new ChunkManagerStats();
    public Transform player;
    public TextureManager textureManager;
    public GameObject chunkPrefab;
    public GameObject treePrefab;
    public GameObject landAnimalPrefab;
    public GameObject airAnimalPrefab;
    private Vector3 offset;

    private List<ChunkData> activeChunks = new List<ChunkData>();
    private ChunkData[,] chunkGrid;
    private GameObjectPool chunkPool;
    private GameObjectPool treePool;

    private ChunkVoxelDataThread[] CVDT;
    private BlockingList<Order> orders;
    private LockingQueue<Result> results; //When CVDT makes a mesh for a chunk the result is put in this queue for this thread to consume.
    private HashSet<Vector3> pendingChunks = new HashSet<Vector3>(); //Chunks that are currently worked on my CVDT

    private GameObject[] animals = new GameObject[20]; //Might want to pool animals in the future too, but for now they're just at a fixed size of 20
    private HashSet<int> orderedAnimals = new HashSet<int>();

    /// <summary>
    /// Generate an initial set of chunks in the world
    /// </summary>
    void Start () {
        textureManager = GameObject.Find("TextureManager").GetComponent<TextureManager>();
        Reset();
        //StartCoroutine(debugRoutine());
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
    /// Prints debug info
    /// </summary>
    /// <returns></returns>
    IEnumerator debugRoutine() {
        while (true) {
            yield return new WaitForSeconds(0.5f);
            Debug.Log("====================================================================");
            Debug.Log("Ordered chunks: " + pendingChunks.Count + " | Inactive Chunks: " + chunkPool.inactiveStack.Count);
            Debug.Log("Inactive trees: " + treePool.inactiveStack.Count);
            Debug.Log("Ordered animals: " + orderedAnimals.Count);
        }
    }

    /// <summary>
    /// Resets the chunkManager, clearing all data and initializing
    /// </summary>
    /// <param name="threadCount">Threadcount to use after reset</param>
    public void Reset(int threadCount = 0) {
        Settings.load();
        clear();

        offset = new Vector3(-ChunkConfig.chunkCount / 2f * ChunkConfig.chunkSize, 0, -ChunkConfig.chunkCount / 2f * ChunkConfig.chunkSize);
        chunkGrid = new ChunkData[ChunkConfig.chunkCount, ChunkConfig.chunkCount];

        if (threadCount == 0) {
            threadCount = Settings.WorldGenThreads;
        }
        orders = new BlockingList<Order>();
        results = new LockingQueue<Result>(); //When CVDT makes a mesh for a chunk the result is put in this queue for this thread to consume.
        CVDT = new ChunkVoxelDataThread[threadCount];
        for (int i = 0; i < threadCount; i++) {
            CVDT[i] = new ChunkVoxelDataThread(orders, results, i);
        }

        chunkPool = new GameObjectPool(chunkPrefab, transform, "chunk", false);
        treePool = new GameObjectPool(treePrefab, transform, "tree", false);

        if (landAnimalPrefab != null) {
            for (int i = 0; i < animals.Length; i++) {
                animals[i] = Instantiate((Random.Range(0, 2) == 0) ? landAnimalPrefab : airAnimalPrefab);
                animals[i].transform.position = new Vector3(9999, 9999, 9999);
            }
        }

        GameObject playerObj = player.gameObject;
        if (player.tag == "Player") { //To account for dummy players
            Camera.main.GetComponent<CameraController>().cameraHeight = 7.5f;
            AnimalSkeleton playerSkeleton = new LandAnimalSkeleton(playerObj.transform);
            playerSkeleton.generateInThread();
            playerObj.GetComponent<LandAnimalPlayer>().setSkeleton(playerSkeleton);
            playerObj.GetComponent<Player>().initPlayer(animals);
        }
    }

    /// <summary>
    /// Clears and resets the ChunkManager, used when changing WorldGen settings at runtime.
    /// </summary>
    public void clear() {
        stopThreads();
        orderedAnimals.Clear();
        pendingChunks.Clear();

        while (activeChunks.Count > 0) {
            Destroy(activeChunks[0].terrainChunk[0].transform.parent.gameObject);

            foreach(var chunk in activeChunks[0].terrainChunk) {
                Destroy(chunk);
            }

            foreach (var chunk in activeChunks[0].waterChunk) {
                Destroy(chunk);
            }

            foreach (var tree in activeChunks[0].trees) {
                Destroy(tree);
            }

            activeChunks.RemoveAt(0);
        }

        if (chunkPool != null) {
            chunkPool.destroyAllGameObjects();
        }
        if (treePool != null) {
            treePool.destroyAllGameObjects();
        }

        foreach (var animal in animals) {
            if (animal != null) {
                Destroy(animal);
            }
        }
    }

    /// <summary>
    /// Handles spawning of animals.
    /// </summary>
    private void handleAnimals() {
        if (landAnimalPrefab) {
            for (int i = 0; i < animals.Length; i++) {
                GameObject animal = animals[i];
                if (animal.activeSelf && isAnimalTooFarAway(animal.transform.position)) {
                    Vector3 spawnPos = calculateValidSpawnPosition();
                    if (spawnPos != Vector3.down) {
                        animal.transform.position = spawnPos;
                        AnimalSkeleton animalSkeleton = AnimalUtils.createAnimalSkeleton(animal, animal.GetComponent<Animal>().GetType());
                        animalSkeleton.index = i;
                        orders.Add(new Order(animal.transform.position, animalSkeleton, Task.ANIMAL));
                        orderedAnimals.Add(i);
                        animal.SetActive(false);
                    }
                }
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
            Vector3Int chunkPos = wolrd2ChunkPos(activeChunks[i].pos);
            if (checkBounds(chunkPos.x, chunkPos.z)) {
                chunkGrid[chunkPos.x, chunkPos.z] = activeChunks[i];
            } else {
                GameObject chunk = activeChunks[i].chunkParent;
                for (int j = 0; j < activeChunks[i].terrainChunk.Count; j++) {
                    activeChunks[i].terrainChunk[j].transform.parent = this.transform;
                    chunkPool.returnObject(activeChunks[i].terrainChunk[j]);
                }
                for (int j = 0; j < activeChunks[i].waterChunk.Count; j++) {
                    activeChunks[i].waterChunk[j].transform.parent = this.transform;
                    chunkPool.returnObject(activeChunks[i].waterChunk[j]);
                }

                Destroy(chunk);

                foreach(var tree in activeChunks[i].trees) {
                    treePool.returnObject(tree);
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
                Vector3 chunkPos = chunkPos2world(new Vector3(x, 0, z));
                if (chunkGrid[x, z] == null && !pendingChunks.Contains(chunkPos)) {
                    orders.Add(new Order(chunkPos, Task.CHUNK));
                    pendingChunks.Add(chunkPos);
                }
            }
        }
    }

    /// <summary>
    /// Consumes results from Worker threads.
    /// </summary>
    private void consumeThreadResults() {
        int consumed = 0;
        while(results.getCount() > 0 && consumed < Settings.MaxChunkLaunchesPerUpdate) {
            Result result = results.Dequeue();
            switch (result.task) {
                case Task.CHUNK:
                    launchOrderedChunk(result.chunkVoxelData);
                    stats.generatedChunks++;
                    break;
                case Task.ANIMAL:
                    applyOrderedAnimal(result.animalSkeleton);
                    stats.generatedAnimals++;
                    break;
                case Task.CANCEL:
                    pendingChunks.Remove(result.chunkVoxelData.chunkPos);
                    stats.cancelledChunks++;
                    break;
            }
            consumed++;
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
        chunk.transform.parent = this.transform;
        cd.chunkParent = chunk;

        for (int i = 0; i < chunkMeshData.meshData.Length; i++) {
            GameObject subChunk = chunkPool.getObject();
            subChunk.layer = 8;
            subChunk.transform.parent = chunk.transform;
            subChunk.transform.position = chunkMeshData.chunkPos;
            MeshDataGenerator.applyMeshData(subChunk.GetComponent<MeshFilter>(), chunkMeshData.meshData[i]);
            subChunk.GetComponent<MeshCollider>().sharedMesh = subChunk.GetComponent<MeshFilter>().mesh;
            subChunk.GetComponent<MeshCollider>().isTrigger = false;
            subChunk.GetComponent<MeshCollider>().convex = false;
            subChunk.name = "terrainSubChunk";
            subChunk.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_TexArr", textureManager.getTextureArray());
            subChunk.GetComponent<MeshRenderer>().material.renderQueue = subChunk.GetComponent<MeshRenderer>().material.shader.renderQueue - 1;
            cd.terrainChunk.Add(subChunk);
        }

        for (int i = 0; i < chunkMeshData.waterMeshData.Length; i++) {
            GameObject waterChunk = chunkPool.getObject();
            waterChunk.layer = 0;
            waterChunk.transform.parent = chunk.transform;
            waterChunk.transform.position = chunkMeshData.chunkPos;
            MeshDataGenerator.applyMeshData(waterChunk.GetComponent<MeshFilter>(), chunkMeshData.waterMeshData[i]);
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
            GameObject tree = treePool.getObject();
            tree.transform.position = chunkMeshData.treePositions[i];
            MeshDataGenerator.applyMeshData(tree.GetComponent<MeshFilter>(), chunkMeshData.trees[i]);
            MeshDataGenerator.applyMeshData(tree.GetComponent<MeshCollider>(), chunkMeshData.treeTrunks[i]);
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
        spawnAnimal(animal, animalSkeleton);
        orderedAnimals.Remove(animalSkeleton.index);
    }

    /// <summary>
    /// Answers the question in the function name,
    /// despawn animals that are too far away
    /// </summary>
    /// <param name="pos">pos to check</param>
    /// <returns>bool true false</returns>
    private bool isAnimalTooFarAway(Vector3 pos) {
        float maxDistance = ChunkConfig.chunkCount * ChunkConfig.chunkSize / 2;
        float lower = -maxDistance + LandAnimalNPC.roamDistance;
        float upper = -lower;
        return Vector3.Distance(pos, player.position) > maxDistance;
    }

    /// <summary>
    /// Spawn animal
    /// </summary>
    /// <param name="animal">Animal to spawn</param>
    /// <returns></returns>
    private void spawnAnimal(GameObject animal, AnimalSkeleton skeleton) {
        if (isAnimalTooFarAway(animal.transform.position)) {
            animal.transform.position = calculateValidSpawnPosition();
        }
        animal.GetComponent<Animal>().Spawn(animal.transform.position);
        animal.SetActive(true);
        animal.GetComponent<Animal>().setSkeleton(skeleton);
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
    /// Calculates the cunkPos (Index in chunkgrid) from world pos
    /// </summary>
    /// <param name="worldPos">Worldpos to convert</param>
    /// <returns>chunkpos</returns>
    private Vector3Int wolrd2ChunkPos(Vector3 worldPos) {
        Vector3 chunkPos = (worldPos - offset - getPlayerPos()) / ChunkConfig.chunkSize;
        return new Vector3Int((int)chunkPos.x, (int)chunkPos.y, (int)chunkPos.z);
    } 

    /// <summary>
    /// Calculates worldpos from chunkpos (ChunkGrid index)
    /// </summary>
    /// <param name="chunkPos">Chunkpos to convert</param>
    /// <returns>Wolrd pos</returns>
    private Vector3 chunkPos2world(Vector3 chunkPos) {
        Vector3 world = chunkPos * ChunkConfig.chunkSize + offset + getPlayerPos();
        return world;
    }

    /// <summary>
    /// Calculates a valid spawn position for animals
    /// </summary>
    /// <returns>Vector3 pos, Vector3.down for when no pos can be calculated</returns>
    private Vector3 calculateValidSpawnPosition() {
        if (activeChunks.Count < 1) {
            return Vector3.down;
        }

        Vector3 pos = activeChunks[Random.Range(0, activeChunks.Count)].pos + player.position;
        pos += new Vector3(
            Random.Range(-ChunkConfig.chunkSize / 2, ChunkConfig.chunkSize / 2),
            ChunkConfig.chunkHeight + 10,
            Random.Range(-ChunkConfig.chunkSize / 2, ChunkConfig.chunkSize / 2)
        );
        return pos;
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
