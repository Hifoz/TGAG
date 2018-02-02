using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Manages the textures for the terrain
/// </summary>
public class TextureManager : MonoBehaviour {

    /// <summary>
    ///  Texture Type
    /// </summary>
    public enum TextureType {
        // All texture types must correspond to a block type, append "_SIDE", "_TOP", "_BOTTOM" where neccessary (these types must also be added in the check in MeshDataGenerator.addSliceData())
        NONE,
        DIRT,
        STONE,
        SAND,
        GRASS_TOP, GRASS_SIDE,
        SNOW_TOP, SNOW_SIDE,
        WOOD,
        LEAF,

        COUNT
    }

    private List<int>[] sliceTypeList = new List<int>[(int)TextureType.COUNT]; // Constains a list for each textureType containg the slices in the textureArray that contains a texture for it.

    public int textureSize = 512;

    private List<Color[]> textureList = new List<Color[]>();
    private Texture2DArray textureArray;

    // Settings for the textureArray
    public bool mipmap = true;
    public TextureFormat textureFormat = TextureFormat.RGBA32;

    private bool hasChanged = true;


    private void Awake() {
        for (int i = 0; i < sliceTypeList.Length; i++)
            sliceTypeList[i] = new List<int>();

        addEmpty(); // We want the first entry to be clear, so that whenever a block has no modifier, the modifier can pick slice 0
    }

    /// <returns>size required for textures</returns>
    public int getTextureSize() {
        return textureSize;
    }


    /// <summary>
    /// Generates a texture and stores it.
    /// </summary>
    /// <param name="pixelData">Data to create texture from</param>
    /// <returns>Whether the texture was successfully created and stored.</returns>
    public void addTexture(Color[] pixelData, TextureType type) {
        sliceTypeList[(int)type].Add(textureList.Count);
        textureList.Add(pixelData);
        hasChanged = true;
    }

    /// <summary>
    /// Used to generate one fully transparent texture to be used if modifier == BlockType.NONE
    /// </summary>
    public void addEmpty() {
        Color[] e = new Color[textureSize * textureSize];
        for (int i = 0; i < e.Length; i++)
            e[i] = new Color(1, 1, 1, 0);

        addTexture(e, TextureType.NONE);
    }

    /// <summary>
    /// Generates and return a textureArray with the textures added.
    /// </summary>
    /// <returns>Returns a texture array with all generated textures.</returns>
    public Texture2DArray getTextureArray() {
        if (!hasChanged)
            return textureArray;


        hasChanged = false;
        textureArray = new Texture2DArray(textureSize, textureSize, textureList.Count, textureFormat, mipmap);

        for (int i = 0; i < textureList.Count; i++) {
            textureArray.SetPixels(textureList[i], i);
        }
        textureArray.wrapMode = TextureWrapMode.Clamp;
        textureArray.filterMode = FilterMode.Bilinear;
        textureArray.Apply();
        return textureArray;
    }


    public List<int>[] getSliceTypeList() {
        return sliceTypeList;
    }

    /// <summary>
    /// Saves the texture array as an asset.
    /// </summary>
    /// <param name="path">Path for saving asset, relative to "Resources/"</param>
    public void saveArrayToFile(string path) {
        textureArray.Apply();
        AssetDatabase.CreateAsset(textureArray, "Assets/Resources/Textures/" + path);
    }

    /// <summary>
    /// Tries to load a Texture2DArray from file.
    /// </summary>
    /// <param name="path">Path of asset, relative to "Resources/"</param>
    /// <returns>Whether it could successfullly load the asset</returns>
    public bool loadArrayFromFile(string path) {
        Texture2DArray arr = Resources.Load<Texture2DArray>("Textures/" + path);
        if (arr == null)
            return false;

        for(int i = 0; i < arr.depth; i++) {
            textureList.Add(arr.GetPixels(i));
        }
        hasChanged = true;

        return true;
    }


    /// <summary>
    /// Tries to load a single texture from file and add it to the texture array.
    /// </summary>
    /// <param name="path">Path to the file, relative to "Resources/"</param>
    /// <returns>Whether the texture was successfully loaded.</returns>
    public bool loadTextureFromFile(string path, TextureType texType) {
        Texture2D loadedTexture = Resources.Load<Texture2D>(path);
        if (loadedTexture == null) {
            Debug.Log("Could not find file");
            return false;
        }
        addTexture(loadedTexture.GetPixels(), texType);
        return true;
    }



    public void Clear() {
        hasChanged = true;
        textureList = new List<Color[]>();
    }
}
