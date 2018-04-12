using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Class resposible for handling of audio
/// </summary>
class AudioManager : MonoBehaviour{

    public WorldGenManager worldGenManager;
    public Camera playerCamera;
    public int waterSearchRadius; // How many blocks around the player should be searched for water

    public static float masterVolume = 1;
    public static float gameVolume = 1;
    public static float musicVolume = 1;

    private bool isUnderWater;

    // Environment
    private AudioSource oceanSource;
    private AudioSource ambientSource;

    // Music
    private AudioSource musicSource;
    private AudioClip[] musicClips;



    private void Start() {
        worldGenManager = GameObject.Find("WorldGenManager").GetComponent<WorldGenManager>();

        // TODO : Load volume settings from PlayerPrefs

        StartCoroutine(musicPlayer());
        StartCoroutine(environmentPlayer());
    }

    private void Update() {
        
    }

    /// <summary>
    /// Cycles through all the music tracks
    /// </summary>
    private IEnumerator musicPlayer() {
        musicClips = Resources.LoadAll<AudioClip>("Audio/Music/");
        GameObject musicObj = new GameObject() { name = "MusicPlayer" };
        musicSource = musicObj.AddComponent<AudioSource>();
        musicSource.volume = 0;

        // TODO: Make the music clips fade in/out with overlap to make the transition between the tracks smoother

        int clipIndex = UnityEngine.Random.Range(0, musicClips.Length);
        while (true) {
            musicSource.PlayOneShot(musicClips[clipIndex]);
            //Debug.Log("Playing " + _musicClips[clipIndex].name);
            yield return new WaitForSeconds(musicClips[clipIndex].length);
            clipIndex = (clipIndex + 1) % musicClips.Length;
        }
    }


    private IEnumerator environmentPlayer() {
        GameObject oceanPlayer = new GameObject() { name = "oceanSoundPlayer" };
        oceanSource = oceanPlayer.AddComponent<AudioSource>();
        oceanSource.volume = 0;
        oceanSource.loop = true;
        oceanSource.clip = Resources.Load<AudioClip>("Audio/water_lapping_sea_waves_sandy_beach_5_meters_01");
        oceanSource.Play();

        yield return new WaitForSeconds(1);
        while (true) {
            updateWaterVolume();
            yield return null;
        }

    }


    /// <summary>
    /// Uses the world chunk data to find closest water block and base sound level off of that.
    /// </summary>
    private void updateWaterVolume() {
        /*
         TODO 
             check if the camera is IN water first
         */



        Vector3 cameraPos = playerCamera.transform.position;
        Vector2 cameraPosXZ = new Vector2(cameraPos.x, cameraPos.z);
        float closestWaterBlockDist = waterSearchRadius;

        // Find the radius of chunks that need to be checked
        int chunkRadius = 2;// Mathf.CeilToInt(waterSearchRadius / (float)WorldGenConfig.chunkSize) * WorldGenConfig.chunkSize + 1;
        int c = 0;
        int d = 0;
        // Search all inrange chunks for the closest water block
        foreach (ChunkData chunk in worldGenManager.getChunkGrid()) {
            if (chunk == null)
                continue;

            Vector3 inChunkPosition = cameraPos - chunk.pos;
            if (new Vector2(inChunkPosition.x, inChunkPosition.z).magnitude > 50)
                continue;
            d++;
            for (int x = -waterSearchRadius; x < waterSearchRadius; x++) {
                for (int y = -waterSearchRadius; y < waterSearchRadius; y++) {
                    for (int z = -waterSearchRadius; z < waterSearchRadius; z++) {
                        float distFromBlock = Vector3.Distance(new Vector3(x, y, z) + chunk.pos, cameraPos);
                        if (distFromBlock < closestWaterBlockDist && chunk.blockDataMap.mapdata[chunk.blockDataMap.index1D(x, y, z)].blockType == BlockData.BlockType.WATER) {
                            closestWaterBlockDist = distFromBlock;
                        }
                        c++;
                    }
                }
            }
        }
        Debug.Log(c + "  ---  " + d);
        if (closestWaterBlockDist < waterSearchRadius)
            oceanSource.volume = closestWaterBlockDist / closestWaterBlockDist * gameVolume;
        else
            oceanSource.volume = 0;
        oceanSource.pitch = VoxelPhysics.isWater(VoxelPhysics.voxelAtPos(playerCamera.transform.position)) ? 0.2f : 1f;

    }





}
