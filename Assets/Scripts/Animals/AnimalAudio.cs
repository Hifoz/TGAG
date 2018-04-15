using System;
using System.Collections;
using UnityEngine;



[RequireComponent(typeof(AudioSource))]
class AnimalAudio : MonoBehaviour {
    private AudioClip clip;
    private AudioSource source;

    private System.Random rng;

    private static float basePitch = 1;
    private static float pitchRange = 0.4f;
    private static Pair<int, int> speakDelay = new Pair<int, int>(10, 20);

    private void Start() {
        AudioManager.initAnimalAudio(this);
    }

    public void init(int seed, AudioClip animalSound) {
        source = GetComponent<AudioSource>();
        clip = AudioManager.AnimalSound;
        source.clip = clip;
        rng = new System.Random(seed);

        StartCoroutine(player());
    }


    private IEnumerator player() {
        while (true) {
            if (GetComponent<Player>() == null) {
                source.pitch = basePitch + pitchRange * ((float)rng.NextDouble() - 0.5f);
                source.Play();
            }

            yield return new WaitForSeconds(clip.length + new System.Random().Next(speakDelay.first, speakDelay.second));
        }
    }

    public void updateVolume(float newVol) {
        source.volume = newVol;
    }
}