using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Experimental class
/// </summary>
public class GrassGenerator : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GetComponent<MeshRenderer>().material.SetTexture("_MainTex", generateGrass());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private Texture2D generateGrass() {
        const int size = 100;
        Texture2D grass = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        int i = 0;
        for (int y = 0; y < size; y++) {
            for (int x = 0; x < size; x++) {            
                float noise = SimplexNoise.Simplex2D(new Vector3(x, y), 0.1f);
                float noise01 = (1f - (float)y / (float)size);// (noise + 1f) * 2f;
                pixels[i++] = new Color(noise01, noise01, noise01);
            }
        }
        grass.SetPixels(pixels);
        grass.Apply();
        return grass;
    }
}
