using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

/// <summary>
/// Class resposible for handling of audio
/// </summary>
class AudioManager : MonoBehaviour{

    public WorldGenManager worldGenManager;
    public Camera playerCamera;
    public int waterSoundRange;
    public int oceanSoundRange;

    private static float masterVolume = 1;
    private static float gameVolume = 1;
    private static float musicVolume = 1;


    // Environment
    private AudioSource oceanSource;
    private AudioSource waterSource;
    private AudioSource ambientSource;

    // Music
    private AudioSource musicSource;
    private AudioClip[] musicClips;

    // Animal
    private static AudioClip animalSound;
    private static float animalVolume;

    private static System.Random rng;

    private void Awake() {
        rng = new System.Random(12345);
        updateVolume();

        animalSound = Resources.Load<AudioClip>("Audio/fun_monster_stephane_fuf_dufour_sonissGDC2018");
        animalVolume = 1f;
    }



    private void Start() {
        StartCoroutine(musicPlayer());

        // No gameplay sounds in main menu
        if(SceneManager.GetActiveScene().name == "main") {
            StartCoroutine(gameplayPlayer());
        }
    }


    public static void initAnimalAudio(AnimalAudio aa) {
        aa.init(rng.Next(), animalSound);
    }

    #region audio player coroutines

    /// <summary>
    /// Starts music player and keeps cycling through them
    /// </summary>
    private IEnumerator musicPlayer() {
        musicClips = Resources.LoadAll<AudioClip>("Audio/Music/");
        GameObject musicObj = new GameObject() { name = "MusicPlayer" };
        musicSource = musicObj.AddComponent<AudioSource>();
        musicSource.volume = MusicVolume * MasterVolume;

        // TODO: Make the music clips fade in/out with overlap to make the transition between the tracks smoother

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
        GameObject oceanPlayer = new GameObject() { name = "oceanSoundPlayer" };
        oceanSource = oceanPlayer.AddComponent<AudioSource>();
        oceanSource.volume = 0;
        oceanSource.loop = true;
        oceanSource.clip = Resources.Load<AudioClip>("Audio/water_lapping_sea_waves_sandy_beach_5_meters_01");
        oceanSource.Play();

        GameObject waterPlayer = new GameObject() { name = "waterSoundPlayer" };
        waterSource = waterPlayer.AddComponent<AudioSource>();
        waterSource.volume = 0;
        waterSource.loop = true;
        waterSource.clip = Resources.Load<AudioClip>("Audio/Lake3_Kevin_Durr_sonissGDC2017");
        waterSource.Play();

        yield return new WaitForSeconds(1);
        while (true) {
            updateOceanVolume();
            updateWaterVolume();
            yield return null;
        }
    }

    #endregion

    #region volume updators

    /// <summary>
    /// Updates the main audio volumes from PlayerPrefs
    /// </summary>
    public void updateVolume() {
        MasterVolume = PlayerPrefs.GetFloat("Master Volume", 100) / 100f;
        GameVolume = PlayerPrefs.GetFloat("Gameplay Volume", 100) / 100f;
        MusicVolume = PlayerPrefs.GetFloat("Music Volume", 100) / 100f;
        if (musicSource != null)
            musicSource.volume = MusicVolume * MasterVolume;
    }


    /// <summary>
    /// Update volume of ocean water
    /// </summary>
    private void updateOceanVolume() {
        GameObject[] windAreas = GameObject.FindGameObjectsWithTag("windSubChunk"); // Just checking for wind chunks, because at this point in time windchunks are exclusivly for (all) ocean biome chunks
        float closestRange = oceanSoundRange;

        Vector3 camPosX0Z = playerCamera.transform.position;
        camPosX0Z.y = 0;

        foreach(GameObject windArea in windAreas) {
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
        if(diff != Vector2.zero) {
            diff.x *= 0.5f;
            closestRange = diff.magnitude;
        }

        // Update the volume
        if (closestRange <= oceanSoundRange)
            oceanSource.volume = (1 - closestRange / oceanSoundRange) * GameVolume; // TODO this should not be a linear dropoff
        else
            oceanSource.volume = 0;

        oceanSource.pitch = VoxelPhysics.isWater(VoxelPhysics.voxelAtPos(playerCamera.transform.position)) ? 0.2f : 1f;
    }


    /// <summary>
    /// Updates the volume of (non-ocean) water
    /// </summary>
    private void updateWaterVolume() {
        const float waterVolume = 0.2f;
        bool inWater = VoxelPhysics.isWater(VoxelPhysics.voxelAtPos(playerCamera.transform.position));

        waterSource.pitch = inWater ? 0.2f : 1f;

        if (VoxelPhysics.isWater(VoxelPhysics.voxelAtPos(playerCamera.transform.position))) {
            waterSource.volume =  waterVolume * GameVolume * MasterVolume;
            return;
        }


        List<GameObject> waterChunks = GameObject.FindGameObjectsWithTag("waterSubChunk").ToList();
        float closestRange = waterSoundRange;


        float closestChunkDist = float.MaxValue;
        // Sort chunks and find closest
        waterChunks = waterChunks.OrderBy(delegate (GameObject go) {
            Vector3 chunkPos = go.transform.position;
            chunkPos.y = 0;
            Vector3 camPos = playerCamera.transform.position;
            camPos.y = 0;
            float dist = Vector3.Distance(chunkPos, camPos);
            if (dist < closestChunkDist)
                closestChunkDist = dist;

            return dist;
        }).ToList();

        if(closestChunkDist < waterSoundRange) {
            // Remove all chunks that are to far away to be in contention for closest vertex
            waterChunks = waterChunks.Where(
                delegate (GameObject go) {
                    Vector3 chunkPos = go.transform.position;
                    Vector3 camPos = playerCamera.transform.position;
                    chunkPos.y = 0;
                    camPos.y = 0;
                    return Vector3.Distance(chunkPos, camPos) < closestChunkDist + WorldGenConfig.chunkSize * 2;
                }).ToList();

            // Find closest vertex
            foreach(GameObject waterChunk in waterChunks) {
                Vector3 chunkPos = waterChunk.transform.position;
                foreach (Vector3 vert in waterChunk.GetComponent<MeshFilter>().mesh.vertices) {
                    Vector3 vertWorldPos = chunkPos + vert;
                    float dist = Vector3.Distance(vertWorldPos, playerCamera.transform.position);
                    if (dist < closestRange) {
                        closestRange = dist;
                    }
                }
            }
        }


        if (closestRange < waterSoundRange)
            waterSource.volume = (1 - closestRange / waterSoundRange) * waterVolume * GameVolume * MasterVolume;
        else
            waterSource.volume = 0;
        waterSource.pitch = inWater ? 0.2f : 1f;
    }

    /// <summary>
    /// Update the volume of all animals
    /// </summary>
    private void updateAnimalVolume() {
        List<GameObject> animals = GameObject.FindGameObjectsWithTag("Animal").ToList();
        animals.Add(GameObject.FindGameObjectWithTag("Player"));
        foreach(GameObject animal in animals) {
            if(animal.GetComponent<Player>() == null) {
                animal.GetComponent<AnimalAudio>().updateVolume(AnimalVolume);
            }
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

    public static float AnimalVolume {
        get {
            return animalVolume * gameVolume * masterVolume;
        }

        private set {
            animalVolume = value;
        }
    }

    public static AudioClip AnimalSound {
        get {
            return animalSound;
        }

        private set {
            animalSound = value;
        }
    }

    #endregion
}
