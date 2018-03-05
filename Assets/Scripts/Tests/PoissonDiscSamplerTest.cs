using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


class PoissonDiscSamplerTest : MonoBehaviour {
    public GameObject testObject;
    PoissonDiscSampler sampler;


    private void Start() {
        sampler = new PoissonDiscSampler(5, 100, 100);
        StartCoroutine(run());
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
        }

        sw.done("w=100, h=100, r=5; result:" + count + " spheres.");
    }

}
