using System;
using System.Collections.Generic;
using UnityEngine;

class PoissonDiscSamplerTest : MonoBehaviour {
    public GameObject testObject;
    PoissonDiscSampler sampler;


    private void Start() {
        sampler = new PoissonDiscSampler(15, 1000, 1000);
        Vector2[] samples = sampler.sample();

        foreach(Vector2 sample in samples) {
            GameObject sampleObject = Instantiate(testObject);
            sampleObject.transform.position = new Vector3(sample.x, 0, sample.y);
        }
    }

}
