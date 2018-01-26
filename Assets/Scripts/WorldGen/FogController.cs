using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class FogController : MonoBehaviour {
    public Material skyboxMaterial;

    public float fogLevel;

    public void Start() {
        SetFog();
    }


    public void Update() {
        if (Input.GetKeyDown(KeyCode.F))
            SetFog(fogLevel);
    }

    public void SetFog(float density = 0.005f) {
        RenderSettings.fog = true;
        RenderSettings.fogDensity = density;
        RenderSettings.fogColor = new Color(0.75f, 0.75f, 0.75f);
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        //GameObject.Find("Main Light").GetComponent<Light>().color = new Color(0.8f, 0.8f, 0.8f);
        RenderSettings.skybox = skyboxMaterial;
    }

}