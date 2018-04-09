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

    private HashSet<Vector3Int> activeMap = new HashSet<Vector3Int>();

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
    }

    private void Update() {
        if (ready) {
            boxForAnimal(worldGenManager.player.gameObject);
            foreach (GameObjectPool pool in animalPools) {
                foreach (GameObject animal in pool.activeList) {
                    boxForAnimal(animal);
                }
            }
        }
    }

    private void boxForAnimal(GameObject animal) {
        Vector3Int chunkIndex = worldGenManager.world2ChunkPos(animal.transform.position);
        if (worldGenManager.checkBounds(chunkIndex.x, chunkIndex.y) && chunkGrid[chunkIndex.x, chunkIndex.z] != null) {
            ChunkData chunk = chunkGrid[chunkIndex.x, chunkIndex.z];
            PremissiveBlockDataMap map = new PremissiveBlockDataMap(chunk.pos, chunk.blockDataMap, biomeManager);
            for (int x = -scanArea / 2; x < scanArea / 2; x++) {
                for (int y = -scanArea / 2; y < scanArea / 2; y++) {
                    for (int z = -scanArea / 2; z < scanArea / 2; z++) {
                        Vector3Int index = Utils.floorVectorToInt(animal.transform.position - chunk.pos + new Vector3(x, y, z));
                        if (!map.indexEmpty(index)) {
                            get(Utils.floorVectorToInt(animal.transform.position + new Vector3(x, y, z)), "test", false);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Gets a box at specific location, or does nothing if a box is already in place
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="name"></param>
    /// <param name="isTrigger"></param>
    /// <returns></returns>
    public void get(Vector3Int pos, string name, bool isTrigger) {
        if (activeMap.Contains(pos))
            return;
        activeMap.Add(pos);
        GameObject box;
        if (inactive.Count > 0) {
            box = inactive.Pop();
            initBox(box, pos, name, isTrigger);
        } else {
            box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            initBox(box, pos, name, isTrigger);
        }
    }

    /// <summary>
    /// Makes the pool collect all unused boxes
    /// </summary>
    public void collect() {

    }

    /// <summary>
    /// Inits the box
    /// </summary>
    /// <param name="box"></param>
    /// <param name="pos"></param>
    /// <param name="name"></param>
    /// <param name="isTrigger"></param>
    private void initBox(GameObject box, Vector3Int pos, string name, bool isTrigger) {
        box.transform.position = pos;
        box.name = name;
        box.GetComponent<BoxCollider>().isTrigger = isTrigger;
        box.layer = 8;
        box.name = "terrainSubChunk";
        active.Add(box);
    }
}
