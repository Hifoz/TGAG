using System;
using System.Collections;
using UnityEngine;


/// <summary>
/// Audio Player for an animal
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AnimalAudio : MonoBehaviour {
    private enum SoundName {
        TALK, WALK_DIRT, WALK_LEAF, WING_FLAP
    }

    private AnimalState state;
    private Animal animal;

    private AudioClip[] clips;
    private AudioSource source;

    private System.Random rng;

    private static float basePitch = 1;
    private static float pitchRange = 0.4f;
    private static Pair<int, int> speakDelay = new Pair<int, int>(10, 20);


    private void Awake() {
        animal = GetComponent<Animal>();
        state = animal.getState();
        source = GetComponent<AudioSource>();
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
        clips = animalSounds;
        rng = new System.Random(seed);

        StartCoroutine(player());
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



    public void playWalkSound() {
        source.pitch = 1;
        source.PlayOneShot(clips[(int)SoundName.WALK_DIRT]);
    }


    public void playWingSound() {
        source.pitch = 1;
        source.PlayOneShot(clips[(int)SoundName.WING_FLAP]);
    }

    // THIS WILL BE REPLACED: 
    /// <summary>
    /// Plays animal movement sounds
    /// </summary>
    /// <returns></returns>
    private IEnumerator movementPlayer() {
        while (true){
            source.pitch = 1;
            //source.volume = moveVolume;
            // Play sound
            if (state.grounded) {
                BlockData.BlockType bt = VoxelPhysics.voxelAtPos(transform.position);
                if (bt == BlockData.BlockType.LEAF || bt == BlockData.BlockType.WOOD) {
                    source.PlayOneShot(clips[(int)SoundName.WALK_LEAF]);
                } else {
                    source.PlayOneShot(clips[(int)SoundName.WALK_DIRT]);
                }
            } else if(animal.GetType() == typeof(AirAnimal) && !state.inWater) {
                ; // Play wing flapping
            }


            //source.volume = talkVolume;

            yield return new WaitForSeconds(0.5f);// animal.getAnimationSpeed());
        }
    }

}