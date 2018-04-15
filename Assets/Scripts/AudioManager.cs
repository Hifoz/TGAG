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

    public static float masterVolume = 1;
    public static float gameVolume = 1;
    public static float musicVolume = 1;


    // Environment
    private AudioSource oceanSource;
    private AudioSource waterSource;
    private AudioSource ambientSource;

    // Music
    private AudioSource musicSource;
    private AudioClip[] musicClips;



    private void Start() {
        updateVolume();

        StartCoroutine(musicPlayer());

        // No gameplay sounds in main menu
        if(SceneManager.GetActiveScene().name == "main") {
            StartCoroutine(gameplayPlayer());
        }
    }


    /// <summary>
    /// Updates the main audio volumes from PlayerPrefs
    /// </summary>
    public void updateVolume() {
        Debug.Log("updating volume");
        masterVolume = PlayerPrefs.GetFloat("Master Volume", 100) / 100f;
        gameVolume = PlayerPrefs.GetFloat("Gameplay Volume", 100) / 100f;
        musicVolume = PlayerPrefs.GetFloat("Music Volume", 100) / 100f;
        if (musicSource != null)
            musicSource.volume = musicVolume * masterVolume;
    }

    /// <summary>
    /// Cycles through all the music tracks
    /// </summary>
    private IEnumerator musicPlayer() {
        musicClips = Resources.LoadAll<AudioClip>("Audio/Music/");
        GameObject musicObj = new GameObject() { name = "MusicPlayer" };
        musicSource = musicObj.AddComponent<AudioSource>();
        musicSource.volume = musicVolume * masterVolume;

        // TODO: Make the music clips fade in/out with overlap to make the transition between the tracks smoother

        int clipIndex = UnityEngine.Random.Range(0, musicClips.Length);
        while (true) {
            musicSource.PlayOneShot(musicClips[clipIndex]);
            Debug.Log("Now Playing: " + musicClips[clipIndex].name);
            yield return new WaitForSeconds(musicClips[clipIndex].length);
            clipIndex = (clipIndex + 1) % musicClips.Length;
        }
    }


    private IEnumerator gameplayPlayer() {
        GameObject oceanPlayer = new GameObject() { name = "oceanSoundPlayer" };
        oceanSource = oceanPlayer.AddComponent<AudioSource>();
        oceanSource.volume = musicVolume;
        oceanSource.loop = true;
        oceanSource.clip = Resources.Load<AudioClip>("Audio/water_lapping_sea_waves_sandy_beach_5_meters_01");
        oceanSource.Play();

        GameObject waterPlayer = new GameObject() { name = "waterSoundPlayer" };
        waterSource = waterPlayer.AddComponent<AudioSource>();
        waterSource.volume = 0;
        waterSource.loop = true;
        waterSource.clip = Resources.Load<AudioClip>("Audio/Lake3_Kevin_Durr_sonissGDC2017"); // Find new water sound for this
        waterSource.Play();

        yield return new WaitForSeconds(1);
        while (true) {
            updateOceanVolume();
            updateWaterVolume();
            yield return null;
        }

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


        if (playerCamera.transform.position.y > corruptionYOffset + WorldGenConfig.waterEndLevel) {
            Vector2 a = new Vector2(playerCamera.transform.position.y - (corruptionYOffset + WorldGenConfig.waterEndLevel), closestRange);
            a.x *= 0.5f;
            closestRange = a.magnitude;
        } else if (playerCamera.transform.position.y < corruptionYOffset) {
            Vector2 a = new Vector2(corruptionYOffset - playerCamera.transform.position.y, closestRange);
            a.x *= 0.5f;
            closestRange = a.magnitude;
        }

        if (closestRange < oceanSoundRange)
            oceanSource.volume = (1 - closestRange / oceanSoundRange) * gameVolume; // TODO this should not be a linear dropoff
        else
            oceanSource.volume = 0;
        oceanSource.pitch = VoxelPhysics.isWater(VoxelPhysics.voxelAtPos(playerCamera.transform.position)) ? 0.2f : 1f;
    }

    /// <summary>
    /// Uses the world chunk data to find closest water block and base sound level off of that.
    /// This method is fairly expensive, so only use for small ranges
    /// </summary>
    private void updateWaterVolume() {
        List<GameObject> waterChunks = GameObject.FindGameObjectsWithTag("waterSubChunk").ToList(); // Just checking for wind chunks, because at this point in time windchunks are exclusivly for (all) ocean biome chunks
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
            waterSource.volume = (1 - closestRange / waterSoundRange) * gameVolume * masterVolume * 0.2f;
        else
            waterSource.volume = 0;
        waterSource.pitch = VoxelPhysics.isWater(VoxelPhysics.voxelAtPos(playerCamera.transform.position)) ? 0.2f : 1f;
    }



}
