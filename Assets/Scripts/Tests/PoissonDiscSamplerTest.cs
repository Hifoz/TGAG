using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


class PoissonDiscSamplerTest : MonoBehaviour {
#pragma warning disable 649
    public GameObject testObject;
#pragma warning restore 649


    PoissonDiscSampler sampler;


    private void Start() {
        sampler = new PoissonDiscSampler(5, 100, 100, 1337, true);
        StartCoroutine(run());
        //randomComparion();
    }

    public IEnumerator run() {
        yield return new WaitForSeconds(1);
        int count = 0;
        StopWatch sw = new StopWatch();
        sw.start();

        foreach (Vector2 sample in sampler.sample()) {
            GameObject sampleObject = Instantiate(testObject);
            sampleObject.transform.position = new Vector3(sample.x, 0, sample.y);
            count++;
            //yield return new WaitForSeconds(0.001f);
        }

        sw.done("w=100, h=100, r=5; result:" + count + " spheres.");
    }

    public void randomComparion() {
        System.Random rng = new System.Random();
        for(int i = 0; i < 273; i++) { // 273 because that was the number of spheres generated during the test for pds
            GameObject ob = Instantiate(testObject);
            ob.transform.position = new Vector3(rng.Next(0, 100), 0, rng.Next(0, 100));
        }
    }

}
