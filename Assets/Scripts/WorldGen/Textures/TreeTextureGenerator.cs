using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Used to generate textures
/// </summary>
class TreeTextureGenerator : MonoBehaviour {

    // Temporarily using this to load the old textures until i get some actual generation up and going:
    private void Start() {
        TextureManager textureManager = GameObject.Find("TreeTextureManager").GetComponent<TextureManager>();
        textureManager.Clear();
        string sharedPath = "Textures/temp/";
        textureManager.loadTextureFromFile(sharedPath + "temp_dirt");
        textureManager.loadTextureFromFile(sharedPath + "temp_dirt");
        textureManager.loadTextureFromFile(sharedPath + "temp_dirt");
        textureManager.loadTextureFromFile(sharedPath + "temp_grass_top");
        textureManager.loadTextureFromFile(sharedPath + "temp_grass_top");
        textureManager.loadTextureFromFile(sharedPath + "temp_grass_top");
    }

}
