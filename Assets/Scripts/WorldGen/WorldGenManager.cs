using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The different kind of stats for the chunk manager
/// </summary>
public enum WorldGenManagerStatsType {
    GENERATED_CHUNKS = 0,
    GENERATED_VERTICES_K,
    ORDERED_CHUNKS,
    GENERATED_ANIMALS,
    CANCELLED_CHUNKS,
    ENABLED_COLLIDERS,
}

/// <summary>
/// Struct that contains statistics about what the WorldGenManager does
/// </summary>
public class WorldGenManagerStats {
    public Dictionary<WorldGenManagerStatsType, int> aggregateValues;
    public Dictionary<WorldGenManagerStatsType, int> lastSecondValues;

    /// <summary>
    /// Initializes the dictionaries
    /// </summary>
    public WorldGenManagerStats() {
        aggregateValues = new Dictionary<WorldGenManagerStatsType, int>();
        lastSecondValues = new Dictionary<WorldGenManagerStatsType, int>();
        foreach (WorldGenManagerStatsType type in Enum.GetValues(typeof(WorldGenManagerStatsType))) {
            aggregateValues.Add(type, 0);
            lastSecondValues.Add(type, 0);
        }        
    }

    /// <summary>
    /// Calculates the per second values
    /// </summary>
    /// <returns></returns>
    public IEnumerator calculatePerSecondStats() {
        Dictionary<WorldGenManagerStatsType, int> oldAggregates = new Dictionary<WorldGenManagerStatsType, int>();
        foreach (WorldGenManagerStatsType type in Enum.GetValues(typeof(WorldGenManagerStatsType))) {
            oldAggregates.Add(type, 0);
        }

        while (true) {
            foreach(KeyValuePair<WorldGenManagerStatsType, int> stats in aggregateValues) {
                oldAggregates[stats.Key] = stats.Value;
            }
            yield return new WaitForSecondsRealtime(1);
            foreach (KeyValuePair<WorldGenManagerStatsType, int> stats in aggregateValues) {
                lastSecondValues[stats.Key] = stats.Value - oldAggregates[stats.Key];
            }
        }
    }
}

/// <summary>
/// This class is responsible for handling the chunks that makes up the world.
/// It creates and places chunks into the world, keeping the player at the center of the world.
/// </summary>
public class WorldGenManager : MonoBehaviour {
    private Vector3 worldOffset;
    private const float worldShiftDistance = 10000f;

    public WorldGenManagerStats stats;
    public Transform player;
    public GameObject chunkPrefab;
    public Material materialWater;
    public Material materialTerrain;
    public Material materialWindDebug;
    public GameObject windParticleSystemPrefab;
    public GameObject treePrefab;
    public GameObject landAnimalPrefab;
    public GameObject airAnimalPrefab;
    public GameObject waterAnimalPrefab;
    private Vector3 offset;

    // Chunks
    private List<ChunkData> activeChunks = new List<ChunkData>();
    private ChunkData[,] chunkGrid;
    private GameObjectPool chunkPool;
    private GameObjectPool treePool;
    private List<Result> waitingChunks = new List<Result>();
    private Queue<ChunkVoxelData> chunkLaunchingQueue = new Queue<ChunkVoxelData>();
    private const float chunkLaunchDistance = 275f; //Only chunks inside this range get deployed

    // Thread communication
    private ChunkVoxelDataThread[] CVDT;
    private BlockingList<Order> orders;
    private LockingQueue<Result> results; //When CVDT makes a mesh for a chunk the result is put in this queue for this thread to consume.
    private HashSet<Vector3> pendingChunks = new HashSet<Vector3>(); //Chunks that are currently worked on my CVDT    

    // Animals
    private GameObjectPool[] animalPools = new GameObjectPool[3];
    private const int LAND_ANIMAL_POOL = 0;
    private const int AIR_ANIMAL_POOL = 1;
    private const int WATER_ANIMAL_POOL = 2;
    private HashSet<GameObject> orderedAnimals = new HashSet<GameObject>();    

    // Biomes
    private BiomeManager biomeManager;

    /// <summary>
    /// Create biome manager
    /// </summary>
    void Awake() {
        biomeManager = new BiomeManager();
    }


    /// <summary>
    /// Generate an initial set of chunks in the world
    /// </summary>
    void Start () {
        Debug.Log("THREADS: " + Settings.WorldGenThreads);
        biomeManager = new BiomeManager();
        Reset();
        StartCoroutine(chunkLauncher());
    }

    // Update is called once per frame
    void Update() {
        offsetWorld();
        clearChunkGrid();
        updateChunkGrid();
        orderNewChunks();
        consumeThreadResults();
        handleAnimals();
    }   

    /// <summary>
    /// Gets animals
    /// </summary>
    /// <returns></returns>
    public GameObjectPool[] getAnimals() {
        return animalPools;
    }

    /// <summary>
    /// Gets world offset
    /// </summary>
    /// <returns></returns>
    public Vector3 getWorldOffset() {
        return worldOffset;
    }

    //    __  __       _          __                  _   _                 
    //   |  \/  |     (_)        / _|                | | (_)                
    //   | \  / | __ _ _ _ __   | |_ _   _ _ __   ___| |_ _  ___  _ __  ___ 
    //   | |\/| |/ _` | | '_ \  |  _| | | | '_ \ / __| __| |/ _ \| '_ \/ __|
    //   | |  | | (_| | | | | | | | | |_| | | | | (__| |_| | (_) | | | \__ \
    //   |_|  |_|\__,_|_|_| |_| |_|  \__,_|_| |_|\___|\__|_|\___/|_| |_|___/
    //                                                                      
    //                                                                      

    /// <summary>
    /// Handles animals
    /// </summary>
    private void handleAnimals() {
        enableColliders(player.position);
        foreach (GameObjectPool animalPool in animalPools) {
            Stack<GameObject> deSpawn = new Stack<GameObject>();
            foreach (GameObject animalObj in animalPool.activeList) {
                if (!orderedAnimals.Contains(animalObj) && isAnimalTooFarAway(animalObj.transform.position)) {
                    deSpawn.Push(animalObj);
                } else {
                    if (!orderedAnimals.Contains(animalObj)) {
                    }
                    if (animalObj.activeSelf) {
                        enableColliders(animalObj.transform.position);
                    }
                }
            }
            while (deSpawn.Count > 0) {
                animalPool.returnObject(deSpawn.Pop());
            }
        }        
    }

    /// <summary>
    /// Clears all elements in the chunkGrid
    /// </summary>
    private void clearChunkGrid() {
        for (int x = 0; x < WorldGenConfig.chunkCount; x++) {
            for (int z = 0; z < WorldGenConfig.chunkCount; z++) {
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
            Vector3Int chunkPos = world2ChunkPos(activeChunks[i].pos);
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
                if(chunk != null) {
                    Transform windParticleEffect = chunk.transform.Find("WindPE");
                    if (windParticleEffect != null)
                        Destroy(windParticleEffect.gameObject);
                }

                Destroy(chunk);

                foreach(var tree in activeChunks[i].trees) {
                    tree.transform.parent = transform;
                    treePool.returnObject(tree);                    
                }

                foreach (var treeCollider in activeChunks[i].treeColliders) {
                    if (treeCollider != null) {
                        Destroy(treeCollider);
                    }
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
        for (int x = 0; x < WorldGenConfig.chunkCount; x++) {
            for (int z = 0; z < WorldGenConfig.chunkCount; z++) {
                Vector3 chunkPos = chunkPos2world(new Vector3(x, 0, z)) + worldOffset;
                if (chunkGrid[x, z] == null && !pendingChunks.Contains(chunkPos)) {
                    orders.Add(new Order(chunkPos, Task.CHUNK));
                    pendingChunks.Add(chunkPos);
                    stats.aggregateValues[WorldGenManagerStatsType.ORDERED_CHUNKS]++;
                }
            }
        }
    }

    /// <summary>
    /// Consumes results from Worker threads.
    /// </summary>
    private void consumeThreadResults() {
        Vector3 realPlayerPos = player.position + worldOffset;
        //Consume waiting chunks
        for (int i = 0; i < waitingChunks.Count; i++) {
            float distance = Vector3.Distance(waitingChunks[i].chunkVoxelData.chunkPos, realPlayerPos);
            if (distance <= chunkLaunchDistance) {
                Result result = waitingChunks[i];
                waitingChunks.RemoveAt(i);
                chunkLaunchingQueue.Enqueue(result.chunkVoxelData);
                i--;
            } else {
                Vector3Int chunkPos = world2ChunkPos(waitingChunks[i].chunkVoxelData.chunkPos - worldOffset);
                if (!checkBounds(chunkPos.x, chunkPos.z)) {
                    pendingChunks.Remove(waitingChunks[i].chunkVoxelData.chunkPos);
                    waitingChunks.RemoveAt(i);
                    i--;
                }
            }
        }
        

        //Consume fresh thread results
        if (results.getCount() > 0) {
            Result result = results.Dequeue();
            switch (result.task) {
                case Task.CHUNK:
                    if (Vector3.Distance(result.chunkVoxelData.chunkPos, realPlayerPos) > chunkLaunchDistance) {
                        waitingChunks.Add(result);
                        break;
                    } 
                    chunkLaunchingQueue.Enqueue(result.chunkVoxelData);                    
                    break;
                case Task.ANIMAL:
                    applyOrderedAnimal(result.animalSkeleton);
                    stats.aggregateValues[WorldGenManagerStatsType.GENERATED_ANIMALS]++;
                    break;
                case Task.CANCEL:
                    pendingChunks.Remove(result.chunkVoxelData.chunkPos);
                    stats.aggregateValues[WorldGenManagerStatsType.CANCELLED_CHUNKS]++;
                    break;
            }
        }
    }

    /// <summary>
    /// Deploys ordered chunks from the ChunkVoxelDataThreads.
    /// </summary>
    private IEnumerator chunkLauncher() {
        const double maxTime = 10f; //Max time spent launching chunks per update in milliseconds
        Timer timer = new Timer();

        while (true) {
            while (chunkLaunchingQueue.Count == 0) {
                yield return 0;
                timer.reset();
            }

            ChunkVoxelData chunkMeshData = chunkLaunchingQueue.Dequeue();

            chunkMeshData.chunkPos -= worldOffset;
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
                subChunk.GetComponent<MeshCollider>().isTrigger = false;
                subChunk.GetComponent<MeshCollider>().convex = false;
                subChunk.GetComponent<MeshCollider>().enabled = false;
                subChunk.name = "terrainSubChunk";
                subChunk.GetComponent<MeshRenderer>().sharedMaterial = materialTerrain;
                subChunk.GetComponent<MeshRenderer>().material.renderQueue = subChunk.GetComponent<MeshRenderer>().material.shader.renderQueue - 1;
                cd.terrainChunk.Add(subChunk);

                stats.aggregateValues[WorldGenManagerStatsType.GENERATED_VERTICES_K] += subChunk.GetComponent<MeshFilter>().mesh.vertices.Length / 1000;
                if (timer.get() > maxTime) {
                    yield return 0;
                    timer.reset();
                }
            }
            
            for (int i = 0; i < chunkMeshData.waterMeshData.Length; i++) {
                GameObject waterChunk = chunkPool.getObject();
                waterChunk.layer = 4;
                waterChunk.transform.parent = chunk.transform;
                waterChunk.transform.position = chunkMeshData.chunkPos;
                MeshDataGenerator.applyMeshData(waterChunk.GetComponent<MeshFilter>(), chunkMeshData.waterMeshData[i]);
                waterChunk.GetComponent<MeshCollider>().convex = true;
                waterChunk.GetComponent<MeshCollider>().isTrigger = true;
                waterChunk.GetComponent<MeshCollider>().enabled = false;
                waterChunk.name = "waterSubChunk";
                waterChunk.GetComponent<MeshRenderer>().sharedMaterial = materialWater;
                waterChunk.GetComponent<MeshRenderer>().material.renderQueue = waterChunk.GetComponent<MeshRenderer>().material.shader.renderQueue;
                cd.waterChunk.Add(waterChunk);

                stats.aggregateValues[WorldGenManagerStatsType.GENERATED_VERTICES_K] += waterChunk.GetComponent<MeshFilter>().mesh.vertices.Length / 1000;
                if (timer.get() > maxTime) {
                    yield return 0;
                    timer.reset();
                }
            }

            if (chunkMeshData.chunkPos.magnitude > 100) {
                // Create wind mesh
                GameObject windChunk = chunkPool.getObject();
                windChunk.layer = 10;
                windChunk.transform.parent = chunk.transform;
                windChunk.transform.position = chunkMeshData.chunkPos - new Vector3(0, WorldGenConfig.chunkHeight * 0.5f, 0);
                windChunk.transform.localScale = new Vector3(1, WorldGenConfig.chunkHeight, 1);
                MeshDataGenerator.applyMeshData(windChunk.GetComponent<MeshFilter>(), chunkMeshData.windData);
                windChunk.GetComponent<MeshCollider>().convex = true;
                windChunk.GetComponent<MeshCollider>().isTrigger = true;
                windChunk.GetComponent<MeshCollider>().enabled = false;
                windChunk.name = "windSubChunk";
                windChunk.GetComponent<MeshRenderer>().sharedMaterial = materialWindDebug;
                windChunk.GetComponent<MeshRenderer>().material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                windChunk.GetComponent<MeshRenderer>().enabled = false;
                cd.waterChunk.Add(windChunk);


                // Add wind particle system to chunks
                GameObject particleSystem = Instantiate(windParticleSystemPrefab);
                particleSystem.transform.SetParent(chunk.transform);
                particleSystem.gameObject.name = "WindPE";
                particleSystem.transform.position = chunkMeshData.chunkPos;

                float heightPos = 150;
                if (biomeManager.getClosestBiome(new Vector2Int((int)chunkMeshData.chunkPos.x, (int)chunkMeshData.chunkPos.z)).biomeName == "ocean") {
                    heightPos += WorldGenConfig.waterEndLevel;
                } else {
                    heightPos += WindController.globalWindHeight;
                }
                particleSystem.transform.position += new Vector3(0, heightPos, 0);

                // Set the velocity
                ParticleSystem ps = particleSystem.GetComponent<ParticleSystem>();
                ParticleSystem.VelocityOverLifetimeModule psVOL = ps.velocityOverLifetime;
                Vector2 vel = new Vector2(chunkMeshData.chunkPos.x, chunkMeshData.chunkPos.z).normalized * -WindController.globalWindSpeed;
                psVOL.x = vel.x;
                psVOL.y = -0.15f;
                psVOL.z = vel.y;

                cd.windParticleSystem = particleSystem;

                if (timer.get() > maxTime) {
                    yield return 0;
                    timer.reset();
                }
            }


            GameObject[] trees = new GameObject[chunkMeshData.trees.Length];
            Mesh[] treeColliders = new Mesh[chunkMeshData.trees.Length];
            for (int i = 0; i < trees.Length; i++) {
                GameObject tree = treePool.getObject();
                tree.transform.position = chunkMeshData.treePositions[i] + chunkMeshData.chunkPos;
                tree.transform.parent = chunk.transform;
                MeshDataGenerator.applyMeshData(tree.GetComponent<MeshFilter>(), chunkMeshData.trees[i]);
                treeColliders[i] = MeshDataGenerator.applyMeshData(chunkMeshData.treeTrunks[i]);
                tree.GetComponent<MeshCollider>().enabled = false;
                trees[i] = tree;

                stats.aggregateValues[WorldGenManagerStatsType.GENERATED_VERTICES_K] += tree.GetComponent<MeshFilter>().mesh.vertices.Length / 1000;
                if (timer.get() > maxTime) {
                    yield return 0;
                    timer.reset();
                }
            }
            cd.trees = trees;
            cd.treeColliders = treeColliders;

            pendingChunks.Remove(chunkMeshData.chunkPos);
            activeChunks.Add(cd);

            stats.aggregateValues[WorldGenManagerStatsType.GENERATED_CHUNKS]++;
            StartCoroutine(orderAnimals(cd));

            if (timer.get() > maxTime) {
                yield return 0;
                timer.reset();
            }
        }
    }

    /// <summary>
    /// Orders animals for chunk
    /// </summary>
    /// <param name="cd">chunkdata</param>
    private IEnumerator orderAnimals(ChunkData cd) {
        const float animalSpawnChance = 0.08f; //This means that a chunk has a 2% chance to spawn an animal
        if (UnityEngine.Random.Range(0f, 1f) < animalSpawnChance) {
            cd.tryEnableColliders();
            yield return new WaitForSeconds(1f); //Give the colliders a frame to initialize

            //Calculate spawn position
            Vector3 spawnPos = cd.pos + Vector3.up * (WorldGenConfig.chunkHeight + 10) + new Vector3(WorldGenConfig.chunkSize / 2, 0, WorldGenConfig.chunkSize / 2);
            
            int layerMaskWater = 1 << 4;
            RaycastHit hitWater;
            bool water = Physics.Raycast(new Ray(spawnPos, Vector3.down), out hitWater, WorldGenConfig.chunkHeight * 1.2f, layerMaskWater);
            if (water) {
                spawnPos = hitWater.point;
            } else {
                int layerMaskGround = 1 << 8;
                RaycastHit hitGround;
                if (Physics.Raycast(new Ray(spawnPos, Vector3.down), out hitGround, WorldGenConfig.chunkHeight * 1.2f, layerMaskGround)) {
                    spawnPos = hitGround.point + Vector3.up * 4;
                } else {
                    Debug.Log("INFO: AnimalOrder, failed to find spawn point for animal, will drop from sky");
                }
            }

            GameObject animal;
            if (water) {
                animal = animalPools[WATER_ANIMAL_POOL].getObject();
            } else {
                if (UnityEngine.Random.Range(0, 2) == 0) {
                    animal = animalPools[LAND_ANIMAL_POOL].getObject();
                } else {
                    animal = animalPools[AIR_ANIMAL_POOL].getObject();
                }
            }

            animal.transform.position = spawnPos;
            AnimalSkeleton animalSkeleton = AnimalUtils.createAnimalSkeleton(animal, animal.GetComponent<Animal>().GetType());
            AnimalUtils.addAnimalBrainNPC(animal.GetComponent<Animal>());
            orders.Add(new Order(animal.transform.position + worldOffset, animalSkeleton, Task.ANIMAL));
            orderedAnimals.Add(animal);
            animal.SetActive(false);
        }
    }

    /// <summary>
    /// Applies the animalSkeleton to the animal
    /// </summary>
    /// <param name="animalSkeleton">AnimalSkeleton animalSkeleton</param>
    private void applyOrderedAnimal(AnimalSkeleton animalSkeleton) {
        GameObject animal = animalSkeleton.getOwner();
        spawnAnimal(animal, animalSkeleton);
        orderedAnimals.Remove(animal);
    }

    /// <summary>
    /// Offsets the entire world back to zero, when player distance from center is greater then MaxWorldDist
    /// </summary>
    private void offsetWorld() {
        Vector3 playerxz = Utils.elementWiseMult(player.position, new Vector3(1, 0, 1));
        playerxz = Utils.floorVector(playerxz);
        if (playerxz.magnitude > worldShiftDistance) {
            Vector3 offset = calculateChunkPos(playerxz);
            worldOffset += offset;

            player.position -= offset;
            Player playerScript = player.gameObject.GetComponent<Player>();
            if (playerScript != null) { //Player might be a dummy
                player.gameObject.GetComponent<Animal>();
                playerScript.worldOffset = worldOffset;
            }

            foreach (ChunkData chunk in activeChunks) {
                chunk.chunkParent.transform.position -= offset;
                chunk.pos -= offset;
                chunk.disableColliders();
            }

            foreach (GameObjectPool pool in animalPools) {
                foreach (GameObject animal in pool.activeList) {
                    animal.transform.position -= offset;
                    animal.GetComponent<Animal>();
                }
            }
        }        
    }

    //    _    _      _                    __                  _   _                 
    //   | |  | |    | |                  / _|                | | (_)                
    //   | |__| | ___| |_ __   ___ _ __  | |_ _   _ _ __   ___| |_ _  ___  _ __  ___ 
    //   |  __  |/ _ \ | '_ \ / _ \ '__| |  _| | | | '_ \ / __| __| |/ _ \| '_ \/ __|
    //   | |  | |  __/ | |_) |  __/ |    | | | |_| | | | | (__| |_| | (_) | | | \__ \
    //   |_|  |_|\___|_| .__/ \___|_|    |_|  \__,_|_| |_|\___|\__|_|\___/|_| |_|___/
    //                 | |                                                           
    //                 |_|                                                           

    /// <summary>
    /// Answers the question in the function name,
    /// despawn animals that are too far away
    /// </summary>
    /// <param name="pos">pos to check</param>
    /// <returns>bool true false</returns>
    private bool isAnimalTooFarAway(Vector3 pos) {
        float xDist = Mathf.Abs(player.position.x - pos.x);
        float zDist = Mathf.Abs(player.position.z - pos.z);
        bool outOfXBounds = xDist > WorldGenConfig.chunkCount / 2 * WorldGenConfig.chunkSize;
        bool outOfZBounds = zDist > WorldGenConfig.chunkCount / 2 * WorldGenConfig.chunkSize;
        bool outOfYBounds = pos.y < -10;
        return outOfXBounds || outOfZBounds || outOfYBounds; 
    }

    /// <summary>
    /// Spawn animal
    /// </summary>
    /// <param name="animal">Animal to spawn</param>
    /// <returns></returns>
    private void spawnAnimal(GameObject animal, AnimalSkeleton skeleton) {
        animal.GetComponent<Animal>().Spawn(animal.transform.position);
        animal.SetActive(true);
        animal.GetComponent<Animal>().setSkeleton(skeleton);
    }

    /// <summary>
    /// Enables colliders in the area
    /// </summary>
    /// <param name="worldPos">Position to use</param>
    private void enableColliders(Vector3 worldPos) {
        const float distanceTreshold = 60 * 60;
        Vector3Int index = world2ChunkPos(worldPos);

        for (int x = index.x - 1; x <= index.x + 1; x++) {
            for (int z = index.z - 1; z <= index.z + 1; z++) {       
                
                if (checkBounds(x, z) && chunkGrid[x, z] != null && 
                    chunkGrid[x, z].chunkParent.activeSelf && 
                    Utils.sqrHeightNormalizedDistance(chunkGrid[x, z].pos, worldPos) < distanceTreshold) {

                    if(chunkGrid[x, z].tryEnableColliders()) {
                        stats.aggregateValues[WorldGenManagerStatsType.ENABLED_COLLIDERS]++;
                    }

                }
            }
        }
    }

    /// <summary>
    /// Takes a nomral world pos and turns it into a chunkpos (chunkSize normalized)
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private Vector3 calculateChunkPos(Vector3 pos) {
        float x = pos.x;
        float z = pos.z;
        x = Mathf.Floor(x / WorldGenConfig.chunkSize) * WorldGenConfig.chunkSize;
        z = Mathf.Floor(z / WorldGenConfig.chunkSize) * WorldGenConfig.chunkSize;
        return new Vector3(x, 0, z);
    }

    /// <summary>
    /// Gets the "chunk normalized" player position.
    /// </summary>
    /// <returns>Player position</returns>
    private Vector3 getPlayerPos() {
        return calculateChunkPos(player.position);
    }

    /// <summary>
    /// Calculates the cunkPos (Index in chunkgrid) from world pos
    /// </summary>
    /// <param name="worldPos">Worldpos to convert</param>
    /// <returns>chunkpos</returns>
    private Vector3Int world2ChunkPos(Vector3 worldPos) {
        Vector3 chunkPos = (worldPos - offset - getPlayerPos()) / WorldGenConfig.chunkSize;
        return new Vector3Int((int)chunkPos.x, (int)chunkPos.y, (int)chunkPos.z);
    } 

    /// <summary>
    /// Calculates worldpos from chunkpos (ChunkGrid index)
    /// </summary>
    /// <param name="chunkPos">Chunkpos to convert</param>
    /// <returns>Wolrd pos</returns>
    private Vector3 chunkPos2world(Vector3 chunkPos) {
        Vector3 world = chunkPos * WorldGenConfig.chunkSize + offset + getPlayerPos();
        return world;
    }

    /// <summary>
    /// Checks if X and Y are in bound for the ChunkGrid array.
    /// </summary>
    /// <param name="x">x index</param>
    /// <param name="y">y index (worldspace z)</param>
    /// <returns>bool in bound</returns>
    private bool checkBounds(int x, int y) {
        return (x >= 0 && x < WorldGenConfig.chunkCount && y >= 0 && y < WorldGenConfig.chunkCount);
    }

    //    ____                  _                          _       __                  _   _                 
    //   |  _ \                | |                        | |     / _|                | | (_)                
    //   | |_) | ___ _ __   ___| |__  _ __ ___   __ _ _ __| | __ | |_ _   _ _ __   ___| |_ _  ___  _ __  ___ 
    //   |  _ < / _ \ '_ \ / __| '_ \| '_ ` _ \ / _` | '__| |/ / |  _| | | | '_ \ / __| __| |/ _ \| '_ \/ __|
    //   | |_) |  __/ | | | (__| | | | | | | | | (_| | |  |   <  | | | |_| | | | | (__| |_| | (_) | | | \__ \
    //   |____/ \___|_| |_|\___|_| |_|_| |_| |_|\__,_|_|  |_|\_\ |_|  \__,_|_| |_|\___|\__|_|\___/|_| |_|___/
    //                                                                                                       
    //                                                                                                       

    /// <summary>
    /// Resets the WorldGenManager, clearing all data and initializing
    /// </summary>
    /// <param name="threadCount">Threadcount to use after reset</param>
    public void Reset(int threadCount = 0) {
        Settings.load();
        clear();
        worldOffset = Vector3.zero;

        Player.playerPos = new ThreadSafeVector3();
        Player.playerRot = new ThreadSafeVector3();
        Player.playerSpeed = new ThreadSafeVector3();


        stats = new WorldGenManagerStats();
        StartCoroutine(stats.calculatePerSecondStats());

        offset = new Vector3(-WorldGenConfig.chunkCount / 2f * WorldGenConfig.chunkSize, 0, -WorldGenConfig.chunkCount / 2f * WorldGenConfig.chunkSize);
        chunkGrid = new ChunkData[WorldGenConfig.chunkCount, WorldGenConfig.chunkCount];

        if (threadCount == 0) {
            threadCount = Settings.WorldGenThreads;
        }
        orders = new BlockingList<Order>();
        results = new LockingQueue<Result>(); //When CVDT makes a mesh for a chunk the result is put in this queue for this thread to consume.
        CVDT = new ChunkVoxelDataThread[threadCount];
        for (int i = 0; i < threadCount; i++) {
            CVDT[i] = new ChunkVoxelDataThread(orders, results, i, biomeManager);
        }

        chunkPool = new GameObjectPool(chunkPrefab, transform, "chunk", false);
        treePool = new GameObjectPool(treePrefab, transform, "tree", false);

        if (landAnimalPrefab != null) {
            foreach(GameObjectPool animalPool in animalPools) {
                if (animalPool != null) {
                    animalPool.destroyAllGameObjects();
                }
            }

            animalPools[LAND_ANIMAL_POOL] = new GameObjectPool(landAnimalPrefab, null, "LandAnimal");
            animalPools[AIR_ANIMAL_POOL] = new GameObjectPool(airAnimalPrefab, null, "AirAnimal");
            animalPools[WATER_ANIMAL_POOL] = new GameObjectPool(waterAnimalPrefab, null, "WaterAnimal");
        }

        GameObject playerObj = player.gameObject;
        if (player.tag == "Player") { //To account for dummy players
            Camera.main.GetComponent<CameraController>().cameraHeight = 7.5f;
            Animal playerAnimal = player.gameObject.GetComponent<Animal>();
            AnimalSkeleton skeleton = AnimalUtils.createAnimalSkeleton(player.gameObject, playerAnimal.GetType());
            skeleton.generateInThread();
            playerAnimal.setSkeleton(skeleton);
            AnimalUtils.addAnimalBrainPlayer(playerAnimal);
            playerObj.GetComponent<Player>().initPlayer(animalPools);
        }
    }

    /// <summary>
    /// Clears and resets the WorldGenManager, used when changing WorldGen settings at runtime.
    /// </summary>
    public void clear() {
        stopThreads();
        orderedAnimals.Clear();
        pendingChunks.Clear();
        waitingChunks.Clear();
        chunkLaunchingQueue.Clear();

        while (activeChunks.Count > 0) {
            Destroy(activeChunks[0].terrainChunk[0].transform.parent.gameObject);

            foreach (var chunk in activeChunks[0].terrainChunk) {
                Destroy(chunk);
            }

            foreach (var chunk in activeChunks[0].waterChunk) {
                Destroy(chunk);
            }

            foreach (var tree in activeChunks[0].trees) {
                Destroy(tree);
            }

            foreach (var treeCollider in activeChunks[0].treeColliders) {
                if (treeCollider != null) {
                    Destroy(treeCollider);
                }
            }

            activeChunks.RemoveAt(0);
        }

        if (chunkPool != null) {
            chunkPool.destroyAllGameObjects();
        }
        if (treePool != null) {
            treePool.destroyAllGameObjects();
        }

        foreach (var animalPool in animalPools) {
            if (animalPool != null) {
                animalPool.destroyAllGameObjects();
            }
        }
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

    //    __  __ _             __                  _   _                 
    //   |  \/  (_)           / _|                | | (_)                
    //   | \  / |_ ___  ___  | |_ _   _ _ __   ___| |_ _  ___  _ __  ___ 
    //   | |\/| | / __|/ __| |  _| | | | '_ \ / __| __| |/ _ \| '_ \/ __|
    //   | |  | | \__ \ (__  | | | |_| | | | | (__| |_| | (_) | | | \__ \
    //   |_|  |_|_|___/\___| |_|  \__,_|_| |_|\___|\__|_|\___/|_| |_|___/
    //                                                                   
    //         

    /// <summary>
    /// Returns the biomemanager
    /// </summary>
    public BiomeManager getBiomeManager() {
        return biomeManager;
    }


    /// <summary>
    /// Produces a string that contains debug data
    /// </summary>
    /// <returns></returns>
    public string getDebugString() {
        string s = "";
        foreach(KeyValuePair<WorldGenManagerStatsType, int> stat in stats.aggregateValues) {
            s += string.Format("{0}: {1}\n{0}_LAST_SECOND: {2}\n\n", stat.Key.ToString(), stat.Value, stats.lastSecondValues[stat.Key]);
        }

        s += "CURRENT_WAITING_CHUNKS: " + waitingChunks.Count + "\n";
        s += "CURRENT_CHUNK_ORDERS: " + pendingChunks.Count + "\n";
        s += "CURRENT_ANIMAL_ORDERS: " + orderedAnimals.Count + "\n\n";

        s += "ACTIVE_CHUNKS: " + activeChunks.Count + "\n";
        int activeAnimals = 0;
        foreach (GameObjectPool pool in animalPools) {
            activeAnimals += pool.activeList.Count;
        }
        s += "ACTIVE_ANIMALS: " + activeAnimals + "\n\n";

        s += "WORLD_OFFSET: " + worldOffset + "\n";
        s += "WORLD_OFFSET_INTERVAL: " + worldShiftDistance + "\n";
        s += "PLAYER_DISTANCE: " + Player.playerPos.get().magnitude + "\n";
        s += "CORRUPTION_FACTOR (Player): " + Corruption.corruptionFactor(Player.playerPos.get());
        return s;
    }

    private void OnDestroy() {
        stopThreads();
    }

    private void OnApplicationQuit() {
        stopThreads();
    }
}
