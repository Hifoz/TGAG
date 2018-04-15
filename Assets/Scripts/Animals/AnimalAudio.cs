using System;
using System.Collections;
using UnityEngine;


/// <summary>
/// Audio Player for an animal
/// </summary>
[RequireComponent(typeof(AudioSource))]
class AnimalAudio : MonoBehaviour {
    private enum SoundName {
        TALK, WALK_DIRT, WALK_LEAF
    }

    private AnimalState state;

    private AudioClip[] clips;
    private AudioSource source;

    private System.Random rng;

    private static float basePitch = 1;
    private static float pitchRange = 0.4f;
    private static Pair<int, int> speakDelay = new Pair<int, int>(10, 20);

    private void Awake() {
        state = GetComponent<Animal>().getState();
    }


    private void Start() {
        AudioManager.initAnimalAudio(this);
    }


    /// <summary>
    /// Initializes the audio player
    /// </summary>
    /// <param name="seed"></param>
    /// <param name="animalSound"></param>
    public void init(int seed, AudioClip[] animalSounds) {
        source = GetComponent<AudioSource>();
        clips = animalSounds;
        rng = new System.Random(seed);

        StartCoroutine(player());
        StartCoroutine(movementPlayer());
    }

    /// <summary>
    /// Plays animal "talking" sounds
    /// </summary>
    /// <returns></returns>
    private IEnumerator player() {
        while (true) {
            if (GetComponent<Player>() == null) {
                source.pitch = basePitch + pitchRange * ((float)rng.NextDouble() - 0.5f);
                source.PlayOneShot(clips[(int)SoundName.TALK]);
            }

            yield return new WaitForSeconds(clips[(int)SoundName.TALK].length + rng.Next(speakDelay.first, speakDelay.second));
        }
    }

    /// <summary>
    /// Updates the volume of the audio source
    /// </summary>
    /// <param name="newVol">New volume</param>
    public void updateVolume(float newVol) {
        source.volume = newVol;
    }


    /// <summary>
    /// Plays animal movement sounds
    /// </summary>
    /// <returns></returns>
    private IEnumerator movementPlayer() {
        /*
         * TODO:
         * - Make movement sound have higher volume
         * - Add wing flapping if flying animal in air
         * - Add Swimming sounds (maybe have to be different per animal type?)
         * - Add fish flapping on ground sound
         * - Make the pace of the walk differ depending on animal type, and not  just play every 2 seconds
         * 
         */


        while (true){
            source.pitch = 1;
            if (state.grounded) {
                Debug.Log("playing walking sound");
                BlockData.BlockType bt = VoxelPhysics.voxelAtPos(transform.position);
                if (bt == BlockData.BlockType.LEAF || bt == BlockData.BlockType.WOOD)
                    source.PlayOneShot(clips[(int)SoundName.WALK_LEAF]);
                else
                    source.PlayOneShot(clips[(int)SoundName.WALK_DIRT]);
            }
            yield return new WaitForSeconds(2);
        }
    }

}