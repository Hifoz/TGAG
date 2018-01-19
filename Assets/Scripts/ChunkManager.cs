using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// First draft of ChunkManager, the class responsible for handling the chunks in the game world.
/// In the future it will depend on a multithreaded system for procedurally generating voxel meshes for the chunks.
/// </summary>
public class ChunkManager : MonoBehaviour {

    public Transform player;
    public GameObject chunkPrefab;
    Vector3 offset = new Vector3(-ChunkConfig.chunkSize / 2f * ChunkConfig.chunkSize, 0, -ChunkConfig.chunkSize / 2f * ChunkConfig.chunkSize);
    List<GameObject> activeChunks = new List<GameObject>();
    List<GameObject> inactiveChunks = new List<GameObject>();
    GameObject[,] chunkGrid;

    Dictionary<Vector3, ChunkData> chunkStorage = new Dictionary<Vector3, ChunkData>();



    /// <summary>
    /// Generate an initial set of chunks in the world
    /// </summary>
    void Start () {
        chunkGrid = new GameObject[ChunkConfig.chunkCount, ChunkConfig.chunkCount];
        for (int x = 0; x < ChunkConfig.chunkCount; x++) {
            for (int z = 0; z < ChunkConfig.chunkCount; z++) {
                Vector3 chunkPos = new Vector3(x, 0, z) * ChunkConfig.chunkSize + offset + getPlayerPos();
                activeChunks.Add(createChunk(ChunkConfig.chunkSize, chunkPos));
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
        clearChunkGrid();
        updateChunkGrid();
        deployInactiveChunks();
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
            Vector3 chunkPos = (activeChunks[i].transform.position - offset - getPlayerPos()) / ChunkConfig.chunkSize;
            int ix = Mathf.FloorToInt(chunkPos.x);
            int iz = Mathf.FloorToInt(chunkPos.z);
            if (checkBounds(ix, iz)) {
                chunkGrid[ix, iz] = activeChunks[i];
            } else {
                inactiveChunks.Add(activeChunks[i]);
                activeChunks.RemoveAt(i);
                i--;
            }
        }
    }

    /// <summary>
    /// Deploys inactive chunks into empty cells of the chunkgrid.
    /// </summary>
    private void deployInactiveChunks() {
        for (int x = 0; x < ChunkConfig.chunkCount; x++) {
            for (int z = 0; z < ChunkConfig.chunkCount; z++) {
                if (inactiveChunks.Count == 0) return;
                if (chunkGrid[x, z] == null) {
                    var chunk = inactiveChunks[0];
                    inactiveChunks.RemoveAt(0);
                    chunkGrid[x, z] = chunk;
                    Vector3 chunkPos = new Vector3(x, 0, z) * ChunkConfig.chunkSize + offset + getPlayerPos();
                    chunk.transform.position = chunkPos;
                    chunk.GetComponent<MeshFilter>().mesh = getChunkData(chunkPos).getMesh();
                    activeChunks.Add(chunk);
                }
            }
        }
    }

    /// <summary>
    /// Gets the "chunk normalized" player position.
    /// </summary>
    /// <returns>Player position</returns>
    private Vector3 getPlayerPos() {
        float x = player.position.x;
        float z = player.position.z;
        x = Mathf.Floor(x / 10) * 10;
        z = Mathf.Floor(z / 10) * 10;
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
    /// A temporary function for creating a cube chunk.
    /// </summary>
    /// <param name="size">The size of the chunk</param>
    /// <param name="pos">The position of the chunk</param>
    /// <returns>GameObject Chunk</returns>
    private GameObject createChunk(float size, Vector3 pos) {

        GameObject chunk = Instantiate(chunkPrefab);
        chunk.transform.parent = transform;
        chunk.name = "chunk";
        ChunkData chunkData = new ChunkData(pos);
        chunk.GetComponent<MeshFilter>().mesh = chunkData.getMesh();
        chunk.transform.position = pos;
        return chunk;
    }


    /// <summary>
    /// Gets chunkdata for a chunk.
    /// Tries to get an existing, but will create a new ChunkData object if there is none for the given chunk
    /// </summary>
    /// <param name="pos">position of chunk</param>
    /// <returns>a chunk</returns>
    private ChunkData getChunkData(Vector3 pos) {
        if (chunkStorage.ContainsKey(pos)) {
            return chunkStorage[pos];
        }
        else {
            ChunkData newChunk = new ChunkData(pos);
            chunkStorage.Add(pos, newChunk);
            return newChunk;
        }
    }
}
