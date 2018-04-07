using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


class WindController : MonoBehaviour {
    public GameObject windZonePrefab;
//    public List<Pair<GameObject, Vector2Int>> windAreas;
    public Dictionary<Vector2Int, GameObject> windAreas = new Dictionary<Vector2Int, GameObject>();
    public float globalWindHeight;
    public float globalWindSpeed;
    public float updateRate = 5;

    private BiomeManager biomeManager = null;
    

    private void Start() {
        biomeManager = GameObject.Find("WorldGenManager").GetComponent<WorldGenManager>().getBiomeManager();
        StartCoroutine(checkForNewWindAreas());
    }


    /// <summary>
    /// Looks for new wind areas to create at a fixed interval.
    /// </summary>
    private IEnumerator checkForNewWindAreas() {

        yield return new WaitForSeconds(4); // Wait a few seconds so the biome manager has time to initialize

        while (true) {
            Vector2Int pos = new Vector2Int((int)Player.playerPos.get().x, (int)Player.playerPos.get().z);
            List<Pair<BiomeBase, Vector2Int>> biomes = biomeManager.getInRangeBiomes(pos, (int)biomeManager.getRadius() * 3);
            foreach(Pair<BiomeBase, Vector2Int> biome in biomes) {
                if (!windAreas.ContainsKey(biome.second)) {
                    if(biome.first.biomeName == "ocean") {
                        createWindArea(biome.second);
                    }
                }
            }
            yield return new WaitForSeconds(updateRate);
        }

    }



    /// <summary>
    /// Creates a new wind area
    /// </summary>
    /// <param name="pos"></param>
    private void createWindArea(Vector2Int pos) {
        GameObject wa = Instantiate(windZonePrefab);
        windAreas.Add(pos, wa);
        wa.transform.localScale = new Vector3(biomeManager.getRadius() * 1.25f, WorldGenConfig.chunkHeight, biomeManager.getRadius() * 1.25f);
        wa.transform.position = new Vector3(pos.x, WorldGenConfig.chunkHeight * 0.5f, pos.y);
        wa.transform.SetParent(transform);
    }

}