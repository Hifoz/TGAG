using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Used to generate textures
/// </summary>
class TerrainTextureGenerator : MonoBehaviour {

    // Temporarily using this to load the old textures until i get some actual generation up and going:
    private void Start() {
        TextureManager textureManager = GameObject.Find("TerrainTextureManager").GetComponent<TextureManager>();
        textureManager.Clear();
        string sharedPath = "Textures/temp/";
        textureManager.loadTextureFromFile(sharedPath + "temp_dirt");
        textureManager.loadTextureFromFile(sharedPath + "temp_dirt");
        textureManager.loadTextureFromFile(sharedPath + "temp_dirt");
        textureManager.loadTextureFromFile(sharedPath + "temp_stone");
        textureManager.loadTextureFromFile(sharedPath + "temp_stone");
        textureManager.loadTextureFromFile(sharedPath + "temp_stone");
        textureManager.loadTextureFromFile(sharedPath + "temp_sand");
        textureManager.loadTextureFromFile(sharedPath + "temp_sand");
        textureManager.loadTextureFromFile(sharedPath + "temp_sand");
        textureManager.loadTextureFromFile(sharedPath + "temp_grass_top");
        textureManager.loadTextureFromFile(sharedPath + "temp_grass_side");
        textureManager.addEmpty();
        textureManager.loadTextureFromFile(sharedPath + "temp_snow_top");
        textureManager.loadTextureFromFile(sharedPath + "temp_snow_side");
        textureManager.addEmpty();
    }

}
