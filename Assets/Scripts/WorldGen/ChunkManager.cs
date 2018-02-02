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
    private Vector3 offset;
    private List<ChunkData> activeChunks = new List<ChunkData>();
    private Stack<GameObject> inactiveChunks = new Stack<GameObject>();
    private Stack<GameObject> inactiveTrees = new Stack<GameObject>();
    private ChunkData[,] chunkGrid;

    private ChunkVoxelDataThread[] CVDT;
    private BlockingQueue<Vector3> orders = new BlockingQueue<Vector3>(); //When this thread puts a position in this queue, the thread generates a mesh for that position.
    private LockingQueue<ChunkVoxelData> results = new LockingQueue<ChunkVoxelData>(); //When CVDT makes a mesh for a chunk the result is put in this queue for this thread to consume.
    private HashSet<Vector3> pendingChunks = new HashSet<Vector3>(); //Chunks that are currently worked on my CVDT

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
        launchOrderedChunks();
    }

    /// <summary>
    /// Clears and resets the ChunkManager, used when changing WorldGen settings at runtime.
    /// </summary>
    public void clear() {        
        while (pendingChunks.Count > 0) {
            while (results.getCount() > 0) {
                var chunk = results.Dequeue();
                pendingChunks.Remove(chunk.chunkPos);
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
                    orders.Enqueue(chunkPos);
                    pendingChunks.Add(chunkPos);
                }
            }
        }
    }

    /// <summary>
    /// Deploys ordered chunks from the ChunkVoxelDataThreads.
    /// </summary>
    private void launchOrderedChunks() {
        int maxLaunchPerUpdate = Settings.MaxChunkLaunchesPerUpdate;
        
        int launchCount = 0;
        while (results.getCount() > 0 && launchCount < maxLaunchPerUpdate) {
            var chunkMeshData = results.Dequeue();
            pendingChunks.Remove(chunkMeshData.chunkPos);
            ChunkData cd = new ChunkData(chunkMeshData.chunkPos);

            GameObject chunk = getChunk();
            chunk.transform.position = chunkMeshData.chunkPos;
            chunk.GetComponent<MeshFilter>().mesh = MeshDataGenerator.applyMeshData(chunkMeshData.meshData);
            chunk.GetComponent<MeshCollider>().sharedMesh = chunk.GetComponent<MeshFilter>().mesh;
            chunk.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_TexArr", terrainTextureManager.getTextureArray());
            cd.chunk = chunk;

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

            launchCount++;
        }
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
        chunk.name = "chunk";
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
            orders.Enqueue(Vector3.down);
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
