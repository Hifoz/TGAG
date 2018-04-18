using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

/// <summary>
/// Class resposible for management of audio
/// </summary>
class AudioManager : MonoBehaviour{

    public WorldGenManager worldGenManager;
    public Camera playerCamera;

    private static float masterVolume = 1;
    private static float gameVolume = 1;
    private static float musicVolume = 1;


    // Environment
    private AudioSource oceanSource;
    public float oceanVolume = 0.7f;
    public int oceanSoundRange = 100;

    private AudioSource waterSource;
    public float waterVolume = 0.1f;
    public int waterSoundRange = 100;

    private AudioSource windSource;
    public float windVolume = 0.3f;
    public int windSoundRange = 50;

    private AudioSource ambienceSource;
    public float ambienceVolume = 0.05f;

    // Music
    private AudioSource musicSource;
    private AudioClip[] musicClips;

    // Animal
    private static AudioClip[] animalSounds;
    public float animalVolume = 1f;

    private GameObject audioPlayersParent;
    private static System.Random rng;

    private void Awake() {
        rng = new System.Random(12345);

        animalSounds = new AudioClip[]{ // Should be aligned with AnimalAudio.SoundName
            Resources.Load<AudioClip>("Audio/fun_monster_stephane_fuf_dufour_sonissGDC2018"),
            Resources.Load<AudioClip>("Audio/dirt"),
            Resources.Load<AudioClip>("Audio/wing_flap"),
            null, // TODO add swimming sound
            Resources.Load<AudioClip>("Audio/watersplash")
        };
    }


    private void Start() {
        audioPlayersParent = new GameObject() { name = "EnvironmentAudioPlayers" };
        updateVolume();

        StartCoroutine(musicPlayer());

        // No gameplay sounds in main menu
        if(SceneManager.GetActiveScene().name == "main") {
            StartCoroutine(gameplayPlayer());
        }
    }

    /// <summary>
    /// Used for initializing an AnimalAudio component
    /// </summary>
    /// <param name="aa"></param>
    public static void initAnimalAudio(AnimalAudio aa) {
        aa.init(rng.Next(), animalSounds);
    }

    #region audio player coroutines

    /// <summary>
    /// Starts music player and keeps cycling through them
    /// </summary>
    private IEnumerator musicPlayer() {
        musicClips = Resources.LoadAll<AudioClip>("Audio/Music/");
        GameObject musicPlayer = new GameObject() { name = "MusicPlayer" };
        musicPlayer.transform.parent = audioPlayersParent.transform;
        musicSource = musicPlayer.AddComponent<AudioSource>();
        musicSource.volume = MusicVolume * MasterVolume;

        int clipIndex = UnityEngine.Random.Range(0, musicClips.Length);
        while (true) {
            musicSource.PlayOneShot(musicClips[clipIndex]);
            Debug.Log("Now Playing: " + musicClips[clipIndex].name);
            yield return new WaitForSeconds(musicClips[clipIndex].length);
            clipIndex = (clipIndex + 1) % musicClips.Length;
        }
    }

    /// <summary>
    /// Starts gameplay audio players and keeps volumes up-to-date
    /// </summary>
    /// <returns></returns>
    private IEnumerator gameplayPlayer() {

        GameObject ambiencePlayer = new GameObject() { name = "AmbienceSoundPlayer" };
        ambiencePlayer.transform.parent = audioPlayersParent.transform;
        ambienceSource = ambiencePlayer.AddComponent<AudioSource>();
        ambienceSource.volume = ambienceVolume * GameVolume * MasterVolume;
        ambienceSource.loop = true;
        ambienceSource.clip = Resources.Load<AudioClip>("Audio/ambient_ivo_vivic_sonissGDC2018");
        ambienceSource.Play();

        GameObject oceanPlayer = new GameObject() { name = "OceanSoundPlayer" };
        oceanPlayer.transform.parent = audioPlayersParent.transform;
        oceanSource = oceanPlayer.AddComponent<AudioSource>();
        oceanSource.volume = 0;
        oceanSource.loop = true;
        oceanSource.clip = Resources.Load<AudioClip>("Audio/seaWaves_RedSonic_sonissGDC2018");
        oceanSource.Play();

        GameObject waterPlayer = new GameObject() { name = "WaterSoundPlayer" };
        waterPlayer.transform.parent = audioPlayersParent.transform;
        waterSource = waterPlayer.AddComponent<AudioSource>();
        waterSource.volume = 0;
        waterSource.loop = true;
        waterSource.clip = Resources.Load<AudioClip>("Audio/Lake3_Kevin_Durr_sonissGDC2017");
        waterSource.Play();

        GameObject windPlayer = new GameObject() { name = "WindSoundPlayer" };
        windPlayer.transform.parent = audioPlayersParent.transform;
        windSource = windPlayer.AddComponent<AudioSource>();
        windSource.volume = 0;
        windSource.loop = true;
        windSource.clip = Resources.Load<AudioClip>("Audio/wind_Hzandbits_sonissGDC2018"); // TODO : Look for a better track, this one is kinda meh..
        windSource.Play();

        yield return new WaitForSeconds(1);
        while (true) {
            //Instead of finding them from scratch mutliple times
            List<GameObject> windAreas = new List<GameObject>();
            List<GameObject> waterAreas = new List<GameObject>();
            ChunkData[,] chunkgrid = worldGenManager.getChunkGrid();
            Vector3Int cameraIndex = worldGenManager.world2ChunkIndex(playerCamera.transform.position);
            for (int x = cameraIndex.x - 2; x <= cameraIndex.x + 2; x++) {
                for (int z = cameraIndex.z - 2; z <= cameraIndex.z + 2; z++) {
                    if (worldGenManager.checkBounds(x, z) && chunkgrid[x, z] != null) {
                        if (chunkgrid[x, z].hasWind) {
                            windAreas.Add(chunkgrid[x, z].chunkParent);
                        }
                        if (chunkgrid[x, z].waterChunk.Count > 0) {
                            windAreas.AddRange(chunkgrid[x, z].waterChunk);
                        }

                    }
                }
            }


            updateWaterVolume(waterAreas, updateOceanVolume(windAreas));
            updateWindVolume(windAreas);
            yield return null;
        }
    }
    #endregion

    #region volume updators

    /// <summary>
    /// Updates the main audio volumes from PlayerPrefs
    /// </summary>
    public void updateVolume() {
        MasterVolume = PlayerPrefs.GetFloat("Master Volume", 100) / 100f ;
        GameVolume = PlayerPrefs.GetFloat("Gameplay Volume", 100) / 100f;
        MusicVolume = PlayerPrefs.GetFloat("Music Volume", 100) / 100f * 0.25f;
        if (musicSource != null)
            musicSource.volume = MusicVolume * MasterVolume;
        if (ambienceSource != null)
            ambienceSource.volume = ambienceVolume * GameVolume * MasterVolume;

        updateAnimalVolume();
    }


    /// <summary>
    /// Update volume of ocean water
    /// </summary>
    private float updateOceanVolume(List<GameObject> windAreas) {
        bool inWater = VoxelPhysics.isWater(VoxelPhysics.voxelAtPos(playerCamera.transform.position));
        waterSource.pitch = inWater ? 0.2f : 1f;

        if (inWater) {
            waterSource.volume = waterVolume * GameVolume * MasterVolume;
            return 0;
        }       

        float closestRange = oceanSoundRange;

        Vector3 camPosX0Z = playerCamera.transform.position;
        camPosX0Z.y = 0;

        foreach (GameObject windArea in windAreas) {
            Vector3 windAreaCenter = windArea.transform.position + new Vector3(WorldGenConfig.chunkSize * 0.5f, 0, WorldGenConfig.chunkSize * 0.5f);
            windAreaCenter.y = 0;
            float dist = Vector3.Distance(windAreaCenter, camPosX0Z);

            if (dist < closestRange) {
                closestRange = dist;
            }
        }


        float corruptionYOffset = Corruption.corruptWaterHeight(0, Corruption.corruptionFactor(playerCamera.transform.position + worldGenManager.getWorldOffset()));

        // Take the Y distance between the camera and water into account:
        Vector2 diff = Vector2.zero;
        if (playerCamera.transform.position.y > corruptionYOffset + WorldGenConfig.waterEndLevel) { // camera above water 
            diff = new Vector2(playerCamera.transform.position.y - (corruptionYOffset + WorldGenConfig.waterEndLevel), closestRange);
        } else if (playerCamera.transform.position.y < corruptionYOffset) { // player underneath water (not submerged in water, but actually under it)
            diff = new Vector2(corruptionYOffset - playerCamera.transform.position.y, closestRange);
        }
        // Calculate new closestRange with the y distance in account
        if (diff != Vector2.zero) {
            diff.x *= 0.5f;
            closestRange = diff.magnitude;
        }

        // Update the volume
        if (closestRange <= oceanSoundRange) {
            oceanSource.volume = (1 - closestRange / oceanSoundRange) * oceanVolume * gameVolume * masterVolume; // TODO this should not be a linear dropoff
        } else {
            oceanSource.volume = 0;
        }

        return closestRange;
    }


    /// <summary>
    /// Updates the volume of (non-ocean) water
    /// </summary>
    private void updateWaterVolume(List<GameObject> waterChunks, float oceanDist) {
        bool inWater = VoxelPhysics.isWater(VoxelPhysics.voxelAtPos(playerCamera.transform.position));
        waterSource.pitch = inWater ? 0.2f : 1f;

        if (inWater) {
            waterSource.volume =  waterVolume * GameVolume * MasterVolume;
            return;
        }

        float closestVertDist = Mathf.Min(waterSoundRange, oceanDist);
        float closestChunkDist = waterSoundRange + WorldGenConfig.chunkSize;

        // Sort chunks and find closest that contains vertices
        waterChunks = waterChunks.OrderBy(
            delegate (GameObject go) {
                Vector3 chunkPos = go.transform.position;
                Vector3 camPos = playerCamera.transform.position;
                chunkPos.y = 0;
                camPos.y = 0;
                float dist = Vector3.Distance(chunkPos, camPos);
                if (dist < closestChunkDist && go.GetComponent<MeshFilter>().mesh.vertices.Length > 0) {
                    closestChunkDist = dist;
                }
                return dist;
            }
        ).ToList();
        
        if(closestChunkDist < waterSoundRange + WorldGenConfig.chunkSize) {
            Vector3 chunkOffset = new Vector3(1, 0, 1) * WorldGenConfig.chunkSize * 0.5f;
            float chunkDiag = Mathf.Sqrt(Mathf.Pow(WorldGenConfig.chunkSize, 2) * 2);

            // Remove all chunks that are to far away to be in contention for closest vertex
            waterChunks.RemoveAll(
                (go) => {
                    Vector3 chunkPos = go.transform.position;
                    Vector3 camPos = playerCamera.transform.position;
                    chunkPos.y = 0;
                    camPos.y = 0;
                    return Vector3.Distance(chunkPos, camPos) > chunkDiag * 1.5f + closestVertDist;
                }
            );

            // Find closest vertex
            foreach (GameObject waterChunk in waterChunks) {
                float distFromChunkCenter = Vector3.Distance(playerCamera.transform.position, waterChunk.transform.position + chunkOffset);
                if (distFromChunkCenter > chunkDiag * 1.5f + closestVertDist)
                    continue;

                Vector3 chunkPos = waterChunk.transform.position;

                Vector3[] verts = waterChunk.GetComponent<MeshFilter>().mesh.vertices;
                for (int i = 0; i < verts.Length; i+= 16) {
                    Vector3 vertWorldPos = chunkPos + verts[i];
                    float dist = Vector3.Distance(vertWorldPos, playerCamera.transform.position);
                    if (dist < closestVertDist) {
                        closestVertDist = dist;
                    }
                }
            }
        }

        // Update the volume
        if (closestVertDist < waterSoundRange) {
            waterSource.volume = (1 - closestVertDist / waterSoundRange) * waterVolume * GameVolume * MasterVolume;
        } else {
            waterSource.volume = 0;
        }
    }

    /// <summary>
    /// Update volume of wind
    /// </summary>
    private void updateWindVolume(List<GameObject> windAreas) {
        // If we're in water: no wind sound
        if (VoxelPhysics.isWater(VoxelPhysics.voxelAtPos(playerCamera.transform.position))) {
            windSource.volume = 0;
            return;
        }
        float closestRange = WindController.globalWindHeight - playerCamera.transform.position.y;
        // If we're above global wind height: max wind volume
        if (closestRange <= 0) {
            windSource.volume = windVolume * gameVolume * masterVolume;
            return;
        }

        Vector3 camPosX0Z = playerCamera.transform.position;
        camPosX0Z.y = 0;

        foreach (GameObject windArea in windAreas) {
            Vector3 windAreaCenter = windArea.transform.position + new Vector3(WorldGenConfig.chunkSize * 0.5f, 0, WorldGenConfig.chunkSize * 0.5f);
            windAreaCenter.y = 0;
            float dist = Vector3.Distance(windAreaCenter, camPosX0Z);

            if (dist < closestRange) {
                closestRange = dist;
            }
        }

        windSource.volume = (1 - closestRange / windSoundRange) * windVolume * gameVolume * masterVolume;


    }


    /// <summary>
    /// Update the volume of all animals
    /// </summary>
    private void updateAnimalVolume() {
        List<GameObject> animals = GameObject.FindGameObjectsWithTag("Animal").ToList();
        animals.Add(GameObject.FindGameObjectWithTag("Player"));
        foreach(GameObject animal in animals) {
            animal.GetComponent<AnimalAudio>().updateVolume(animalVolume * gameVolume * masterVolume);
        }
    }

    #endregion

    #region accessors

    public static float MasterVolume {
        get {
            return masterVolume;
        }

        private set {
            masterVolume = value;
        }
    }

    public static float GameVolume {
        get {
            return gameVolume;
        }

        private set {
            gameVolume = value;
        }
    }

    public static float MusicVolume {
        get {
            return musicVolume;
        }

        private set {
            musicVolume = value;
        }
    }

    #endregion
}
