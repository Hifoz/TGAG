using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Manages the textures for the terrain
/// </summary>
public class TextureManager {
    private Texture2DArray textureArray;
    private int maxTextures;
    private int size;
    private int texturesUsed;



    /// <summary>
    /// 
    /// </summary>
    /// <param name="size">size of the textures in x and y dimensions</param>
    /// <param name="maxTextures">Max number of textures that can be added</param>
    /// <param name="textureFormat">Format of the texture</param>
    /// <param name="mipmap">Should mipmaps be created?</param>
    public TextureManager(int size, int maxTextures, TextureFormat textureFormat = TextureFormat.RGBAFloat, bool mipmap = false) {
        this.size = size;
        this.maxTextures = maxTextures + 1;
        textureArray = new Texture2DArray(size, size, this.maxTextures, textureFormat, mipmap);

        addEmpty();
    }


    /// <summary>
    /// Generates a texture and stores it.
    /// </summary>
    /// <param name="pixelData">Data to create texture from</param>
    /// <returns>Whether the texture was successfully created and stored.</returns>
    public bool addTexture(Color[] pixelData) {
        if (texturesUsed == maxTextures || pixelData.Length != size*size)
            return false;

        textureArray.SetPixels(pixelData, texturesUsed);
        texturesUsed++;

        return true;
    }

    /// <summary>
    /// Used to generate one fully transparent texture to be used if modifier == BlockType.NONE
    /// </summary>
    private void addEmpty() {
        Color[] e = new Color[size * size];
        for (int i = 0; i < e.Length; i++)
            e[i] = new Color(0, 0, 0, 0);

        addTexture(new Color[size * size]);
    }

    public void skipIndex() {
        texturesUsed++;
    }


    /// <returns>How many textures have been created.</returns>
    public int getTexturesUsed() {
        return texturesUsed;
    }

    /// <summary>
    /// Saves the texture array as an asset.
    /// </summary>
    /// <returns>Path to the asset, relative to "Resources/Textures/"</returns>
    public string buildAsset(string path) {
        textureArray.Apply();
        AssetDatabase.CreateAsset(textureArray, "Assets/Resources/Textures/" + path);
        return path;
    }


    /// <returns>Returns a texture array with all generated textures.</returns>
    public Texture2DArray getTextureArray() {
        textureArray.Apply();
        return textureArray;
    }

    /// <summary>
    /// Tries to load a texture from file and add it to the texture array.
    /// </summary>
    /// <param name="path">path to the file, relative to "Resources/"</param>
    /// <returns>Whether the texture was successfully loaded and stored.</returns>
    public bool loadTextureFromFile(string path) {
        Texture2D loadedTexture = Resources.Load<Texture2D>(path);
        if (loadedTexture == null) {
            Debug.Log("Could not find file");
            return false;
        }

        return addTexture(loadedTexture.GetPixels());
    }

}
