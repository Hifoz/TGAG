using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Used to generate textures
/// </summary>
class TreeTextureGenerator : MonoBehaviour {

    /// <summary>
    /// Used to generate/load textures for trees
    /// </summary>
    private void Start() {
        TextureManager textureManager = GameObject.Find("TreeTextureManager").GetComponent<TextureManager>();
        textureManager.Clear();
        string sharedPath = "Textures/temp/";
        textureManager.loadTextureFromFile(sharedPath + "temp_dirt", TextureData.TextureType.WOOD);
        textureManager.loadTextureFromFile(sharedPath + "temp_grass_top", TextureData.TextureType.LEAF);
    }

}
