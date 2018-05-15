﻿using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The different kind of stats for the chunk manager
/// </summary>
public enum WorldGenManagerStatsType {
    GENERATED_CHUNKS = 0,
    ORDERED_CHUNKS,
    CANCELLED_CHUNKS,
    DISCARDED_CHUNKS,
    GENERATED_ANIMALS,
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
    private const float worldShiftDistance = 1000f;

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
    private GameObjectPool windPool;

    // Thread communication
    private ChunkVoxelDataThread[] CVDT;
    private BlockingList<Order> orders;
    private LockingQueue<Result> results; //When CVDT makes a mesh for a chunk the result is put in this queue for this thread to consume.
    private HashSet<Vector3> pendingChunks = new HashSet<Vector3>(); //Chunks that are currently worked on my CVDT
    private List<Result> waitingChunks = new List<Result>();
    private const float chunkLaunchDistance = 275f; //Only chunks inside this range get deployed

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
        biomeManager = new BiomeManager();
        Reset();
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
#region public functions

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

    /// <summary>
    /// Gets chunkgrid
    /// </summary>
    /// <returns></returns>
    public ChunkData[,] getChunkGrid() {
        return chunkGrid;
    }

    /// <summary>
    /// Returns the biomemanager
    /// </summary>
    public BiomeManager getBiomeManager() {
        return biomeManager;
    }

    /// <summary>
    /// Calculates the cunkPos (Index in chunkgrid) from world pos
    /// </summary>
    /// <param name="worldPos">Worldpos to convert</param>
    /// <returns>chunkpos</returns>
    public Vector3Int world2ChunkIndex(Vector3 worldPos) {
        Vector3 chunkPos = (worldPos - offset - getPlayerPos()) / WorldGenConfig.chunkSize;
        return new Vector3Int((int)chunkPos.x, (int)chunkPos.y, (int)chunkPos.z);
    }

    /// <summary>
    /// Calculates worldpos from chunkpos (ChunkGrid index)
    /// </summary>
    /// <param name="chunkPos">Chunkpos to convert</param>
    /// <returns>Wolrd pos</returns>
    public Vector3 chunkIndex2world(Vector3 chunkPos) {
        Vector3 world = chunkPos * WorldGenConfig.chunkSize + offset + getPlayerPos();
        return world;
    }

    /// <summary>
    /// Checks if X and Y are in bound for the ChunkGrid array.
    /// </summary>
    /// <param name="x">x index</param>
    /// <param name="y">y index (worldspace z)</param>
    /// <returns>bool in bound</returns>
    public bool checkBounds(int x, int y) {
        return (x >= 0 && x < WorldGenConfig.chunkCount && y >= 0 && y < WorldGenConfig.chunkCount);
    }
#endregion
#region main functions

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
        foreach (GameObjectPool animalPool in animalPools) {
            Stack<GameObject> deSpawn = new Stack<GameObject>();
            foreach (GameObject animalObj in animalPool.activeList) {
                if (!orderedAnimals.Contains(animalObj) && isAnimalTooFarAway(animalObj.transform.position)) {
                    deSpawn.Push(animalObj);
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
            Vector3Int chunkPos = world2ChunkIndex(activeChunks[i].pos);
            if (checkBounds(chunkPos.x, chunkPos.z)) {
                chunkGrid[chunkPos.x, chunkPos.z] = activeChunks[i];
            } else {
                GameObject chunk = activeChunks[i].chunkParent;
                for (int j = 0; j < activeChunks[i].terrainChunk.Count; j++) {
                    activeChunks[i].terrainChunk[j].transform.parent = transform;
                    chunkPool.returnObject(activeChunks[i].terrainChunk[j]);
                }
                for (int j = 0; j < activeChunks[i].waterChunk.Count; j++) {
                    activeChunks[i].waterChunk[j].transform.parent = transform;
                    chunkPool.returnObject(activeChunks[i].waterChunk[j]);
                }
                if (activeChunks[i].windParticleSystem != null) {
                    activeChunks[i].windParticleSystem.transform.parent = transform;
                    windPool.returnObject(activeChunks[i].windParticleSystem);
                }
                Destroy(chunk);

                foreach(var tree in activeChunks[i].trees) {
                    tree.transform.parent = transform;
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
        for (int x = 0; x < WorldGenConfig.chunkCount; x++) {
            for (int z = 0; z < WorldGenConfig.chunkCount; z++) {
                Vector3 chunkPos = chunkIndex2world(new Vector3(x, 0, z)) + worldOffset;
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
                launchOrderedChunk(result);
                return;                
            } else {
                Vector3Int chunkPos = world2ChunkIndex(waitingChunks[i].chunkVoxelData.chunkPos - worldOffset);
                if (!checkBounds(chunkPos.x, chunkPos.z)) {
                    pendingChunks.Remove(waitingChunks[i].chunkVoxelData.chunkPos);
                    waitingChunks.RemoveAt(i);
                    i--;
                    stats.aggregateValues[WorldGenManagerStatsType.DISCARDED_CHUNKS]++;
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
                    launchOrderedChunk(result);
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
    private void launchOrderedChunk(Result result) {
        ChunkData cd = launchOrderedChunk(result.chunkVoxelData);
        StartCoroutine(orderAnimals(cd));
        stats.aggregateValues[WorldGenManagerStatsType.GENERATED_CHUNKS]++;
    }

    /// <summary>
    /// Deploys ordered chunks from the ChunkVoxelDataThreads.
    /// </summary>
    private ChunkData launchOrderedChunk(ChunkVoxelData chunkMeshData) {
        pendingChunks.Remove(chunkMeshData.chunkPos);
        chunkMeshData.chunkPos -= worldOffset;
        ChunkData cd = new ChunkData(chunkMeshData.chunkPos);

        GameObject chunk = new GameObject();
        chunk.name = "chunk";
        chunk.transform.parent = transform;
        cd.chunkParent = chunk;
        cd.blockDataMap = chunkMeshData.blockDataMap;

        // Create terrain subchunks
        for (int i = 0; i < chunkMeshData.meshData.Length; i++) {
            GameObject subChunk = chunkPool.getObject();
            subChunk.layer = 8;
            subChunk.transform.parent = chunk.transform;
            subChunk.transform.position = chunkMeshData.chunkPos;
            subChunk.transform.localScale = Vector3.one;
            MeshDataGenerator.applyMeshData(subChunk.GetComponent<MeshFilter>(), chunkMeshData.meshData[i]);
            subChunk.name = "terrainSubChunk";
            subChunk.tag = "terrainSubChunk";
            subChunk.GetComponent<MeshRenderer>().sharedMaterial = materialTerrain;
            subChunk.GetComponent<MeshRenderer>().material.renderQueue = subChunk.GetComponent<MeshRenderer>().material.shader.renderQueue - 1;
            subChunk.GetComponent<MeshRenderer>().enabled = true;
            cd.terrainChunk.Add(subChunk);
        }

        // Create water subchunks
        if (chunkMeshData.waterMeshData != null) {
            for (int i = 0; i < chunkMeshData.waterMeshData.Length; i++) {
                GameObject waterChunk = chunkPool.getObject();
                waterChunk.layer = 4;
                waterChunk.transform.parent = chunk.transform;
                waterChunk.transform.position = chunkMeshData.chunkPos;
                waterChunk.transform.localScale = Vector3.one;
                MeshDataGenerator.applyMeshData(waterChunk.GetComponent<MeshFilter>(), chunkMeshData.waterMeshData[i]);
                waterChunk.name = "waterSubChunk";
                waterChunk.tag = "waterSubChunk";
                waterChunk.GetComponent<MeshRenderer>().sharedMaterial = materialWater;
                waterChunk.GetComponent<MeshRenderer>().material.renderQueue = waterChunk.GetComponent<MeshRenderer>().material.shader.renderQueue;
                waterChunk.GetComponent<MeshRenderer>().enabled = true;
                cd.waterChunk.Add(waterChunk);
            }
        }

        if (chunkMeshData.chunkPos.magnitude > 100) {
            // Add wind particle system to chunks
            GameObject particleSystem = windPool.getObject();
            particleSystem.transform.SetParent(chunk.transform);
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
        }


        GameObject[] trees = new GameObject[chunkMeshData.trees.Length];
        Mesh[] treeColliders = new Mesh[chunkMeshData.trees.Length];
        for (int i = 0; i < trees.Length; i++) {
            GameObject tree = treePool.getObject();
            tree.transform.position = chunkMeshData.treePositions[i] + chunkMeshData.chunkPos;
            tree.transform.parent = chunk.transform;
            MeshDataGenerator.applyMeshData(tree.GetComponent<MeshFilter>(), chunkMeshData.trees[i]);
            MeshDataGenerator.applyMeshData(tree.GetComponent<MeshCollider>(), chunkMeshData.treeTrunks[i]);
            tree.GetComponent<MeshCollider>().enabled = true;
            trees[i] = tree;
        }
        cd.trees = trees;
        activeChunks.Add(cd);
        return cd;
    }

    /// <summary>
    /// Orders animals for chunk
    /// </summary>
    /// <param name="cd">chunkdata</param>
    private IEnumerator orderAnimals(ChunkData cd) {
        const float animalSpawnChance = 0.08f; //This means that a chunk has a 2% chance to spawn an animal
        if (UnityEngine.Random.Range(0f, 1f) < animalSpawnChance) {
            yield return 0; //Give the chunks spawning this animal time to end up in physics system

            //Calculate spawn position
            Vector3 spawnPos = cd.pos + Vector3.up * (WorldGenConfig.chunkHeight + 10) + new Vector3(WorldGenConfig.chunkSize / 2, 0, WorldGenConfig.chunkSize / 2);
            
            VoxelRayCastHit hitWater = VoxelPhysics.rayCast(new Ray(spawnPos, Vector3.down), 200, VoxelRayCastTarget.WATER);
            bool water = VoxelPhysics.isWater(hitWater.type);
            if (water) {
                spawnPos = hitWater.point;
            } else {
                VoxelRayCastHit hitGround = VoxelPhysics.rayCast(new Ray(spawnPos, Vector3.down), 200, VoxelRayCastTarget.SOLID);
                if (VoxelPhysics.isSolid(hitGround.type)) {
                    spawnPos = hitGround.point + Vector3.up * 10;
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
                playerScript.worldOffset = worldOffset;
            }

            foreach (ChunkData chunk in activeChunks) {
                chunk.chunkParent.transform.position -= offset;
                chunk.pos -= offset;
            }

            foreach (GameObjectPool pool in animalPools) {
                foreach (GameObject animal in pool.activeList) {
                    animal.GetComponent<Animal>().applyOffset(offset);
                }
            }
        }        
    }

    #endregion
#region helper functions
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
    #endregion
#region benchmark functions
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
        windPool = new GameObjectPool(windParticleSystemPrefab, transform, "WindPE", false);

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
            Camera.main.GetComponent<CameraController>().setTarget(player);
        }

        VoxelPhysics.init(this);
    }

    /// <summary>
    /// Clears and resets the WorldGenManager, used when changing WorldGen settings at runtime.
    /// </summary>
    public void clear() {
        VoxelPhysics.clear();
        stopThreads();
        orderedAnimals.Clear();
        pendingChunks.Clear();
        waitingChunks.Clear();

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

            activeChunks.RemoveAt(0);
        }

        if (chunkPool != null) {
            chunkPool.destroyAllGameObjects();
        }
        if (treePool != null) {
            treePool.destroyAllGameObjects();
        }
        if (windPool != null) {
            windPool.destroyAllGameObjects();
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
    #endregion
#region misc functions
    //    __  __ _             __                  _   _                 
    //   |  \/  (_)           / _|                | | (_)                
    //   | \  / |_ ___  ___  | |_ _   _ _ __   ___| |_ _  ___  _ __  ___ 
    //   | |\/| | / __|/ __| |  _| | | | '_ \ / __| __| |/ _ \| '_ \/ __|
    //   | |  | | \__ \ (__  | | | |_| | | | | (__| |_| | (_) | | | \__ \
    //   |_|  |_|_|___/\___| |_|  \__,_|_| |_|\___|\__|_|\___/|_| |_|___/
    //                                                                   
    //        

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
        VoxelPhysics.clear();
    }

    private void OnApplicationQuit() {
        stopThreads();
        VoxelPhysics.clear();
    }
#endregion
}
