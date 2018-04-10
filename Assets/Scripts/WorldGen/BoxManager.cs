using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoxManager : MonoBehaviour {
    public WorldGenManager worldGenManager;

    private ChunkData[,] chunkGrid;
    private GameObjectPool[] animalPools;
    private BiomeManager biomeManager;

    private List<GameObject> active = new List<GameObject>();
    private Stack<GameObject> inactive = new Stack<GameObject>();

    private HashSet<Vector3> oldGets = new HashSet<Vector3>();
    private HashSet<Vector3> newGets = new HashSet<Vector3>();

    private const int scanArea = 10;

    private bool ready = false;

    private void Start() {
        StartCoroutine(init());
    }

    IEnumerator init() {
        while (chunkGrid == null || animalPools == null || biomeManager == null) {
            chunkGrid = worldGenManager.getChunkGrid();
            animalPools = worldGenManager.getAnimals();
            biomeManager = worldGenManager.getBiomeManager();
            yield return 0;
        }
        ready = true;

        while (true) {
            yield return new WaitForSeconds(0);
            Debug.Log("ACTIVE: " + active.Count + " INACTIVE: " + inactive.Count);
        }
    }

    private void Update() {
        if (ready) {
            requestBoxes();
            collect();
        }
    }


    public void requestBoxes() {
        boxForAnimal(worldGenManager.player.gameObject);
        //foreach (GameObjectPool pool in animalPools) {
        //    foreach (GameObject animal in pool.activeList) {
        //        boxForAnimal(animal);
        //    }
        //}
    }

    /// <summary>
    /// Places boxes for animals
    /// </summary>
    /// <param name="animal"></param>
    private void boxForAnimal(GameObject animal) {
        Vector3Int chunkIndex = worldGenManager.world2ChunkPos(animal.transform.position);
        if (worldGenManager.checkBounds(chunkIndex.x, chunkIndex.y) && chunkGrid[chunkIndex.x, chunkIndex.z] != null) {
            ChunkData chunk = chunkGrid[chunkIndex.x, chunkIndex.z];
            PremissiveBlockDataMap map = new PremissiveBlockDataMap(chunk.pos, chunk.blockDataMap, biomeManager);
            for (int x = -scanArea / 2; x < scanArea / 2; x++) {
                for (int y = -scanArea / 2; y < scanArea / 2; y++) {
                    for (int z = -scanArea / 2; z < scanArea / 2; z++) {
                        Vector3Int index = Utils.floorVectorToInt(animal.transform.position - chunk.pos + new Vector3(x, y, z));
                        if (/*!map.indexEmpty(index)*/ surfaceVoxel(index, map)) {
                            get(Utils.floorVector(animal.transform.position + new Vector3(x, y, z)), "test", false);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Calulates if this is a surface voxel
    /// </summary>
    /// <param name="index"></param>
    /// <param name="map"></param>
    /// <returns></returns>
    private bool surfaceVoxel(Vector3Int index, PremissiveBlockDataMap map) {
        const int maxNeighbours = 3 * 2;
        int neighbourCount = 0;

        if (map.indexEmpty(index)) {
            return false;
        }
       
        for (int axes = 0; axes < 3; axes++) {
            for (int i = -1; i <= 1; i += 2) {
                Vector3Int offset = Vector3Int.zero;
                offset[axes] = i;
                if (!map.indexEmpty(index + offset)) {
                    neighbourCount++;
                }
            }
        }

        return neighbourCount != maxNeighbours;
    }

    /// <summary>
    /// Gets a box at specific location, or does nothing if a box is already in place
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="name"></param>
    /// <param name="isTrigger"></param>
    /// <returns></returns>
    private void get(Vector3 pos, string name, bool isTrigger) {
        if (oldGets.Contains(pos)) {
            newGets.Add(pos);
            return;
        }
        newGets.Add(pos);
        GameObject box;
        if (inactive.Count > 0) {
            box = inactive.Pop();
            initBox(box, pos, name, isTrigger);
        } else {
            box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.GetComponent<MeshRenderer>().enabled = false;
            initBox(box, pos, name, isTrigger);
        }
    }

    /// <summary>
    /// Makes the pool collect all unused boxes
    /// </summary>
    public void collect() {
        HashSet<Vector3> temp = oldGets;
        oldGets = newGets;
        newGets = temp;
        newGets.Clear();

        for (int i = 0; i < active.Count; i++) {
            if (!oldGets.Contains(active[i].transform.position)) {
                inactive.Push(active[i]);
                active.RemoveAt(i);
                i--;
            }
        }
    }

    /// <summary>
    /// Inits the box
    /// </summary>
    /// <param name="box"></param>
    /// <param name="pos"></param>
    /// <param name="name"></param>
    /// <param name="isTrigger"></param>
    private void initBox(GameObject box, Vector3 pos, string name, bool isTrigger) {
        box.transform.position = pos;
        box.name = name;
        box.GetComponent<BoxCollider>().isTrigger = isTrigger;
        box.layer = 8;
        box.name = "terrainSubChunk";
        active.Add(box);
    }
}
