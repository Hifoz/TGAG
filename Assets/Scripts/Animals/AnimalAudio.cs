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
    private Animal animal;

    private AudioClip[] clips;
    private AudioSource source;

    private System.Random rng;

    private float talkVolume;
    private float moveVolume = 1;


    private static float basePitch = 1;
    private static float pitchRange = 0.4f;
    private static Pair<int, int> speakDelay = new Pair<int, int>(10, 20);


    private void Awake() {
        animal = GetComponent<Animal>();
        state = animal.getState();
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
        source.volume = 1;
        clips = animalSounds;
        rng = new System.Random(seed);

        //StartCoroutine(player());
        if(GetComponent<Player>() != null)
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
        talkVolume = newVol;
        //source.volume = newVol;
    }


    /*
     * Leaving the movement sound un-implemented for the time being. The resaon for this is because of the 
     *  effort that it is going to take just to sync the tracks to the animation with the method I have
     *  currently started on. A better implementation would probably be to somehow incorporate playing of 
     *  the tracks into the animation cycle so that it would be up to the person working on the animations 
     *  to make sure foot-steps and such is played at the right time.
     * 
     */
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
            Debug.Log(animal.getAnimationSpeed() + " -- " + state.speed);

            yield return new WaitForSeconds(0.5f);// animal.getAnimationSpeed());
        }
    }

}