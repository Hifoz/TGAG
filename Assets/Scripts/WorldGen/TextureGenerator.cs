using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TextureGenerator {
    private Texture2DArray textureArray;
    private int maxTextures;
    private int size;
    private int texturesUsed;
    private string path;


    public TextureGenerator(int size, int maxTextures, string path) {
        this.size = size;
        this.maxTextures = maxTextures;
        this.path = path;
        textureArray = new Texture2DArray(size, size, maxTextures, TextureFormat.RGBAFloat, false);
    }


    public void generateTexture(Color[] pixelData, int index) {
        textureArray.SetPixels(pixelData, texturesUsed);
        texturesUsed++;
    }

    public int getTexturesUsed() {
        return texturesUsed;
    }

    public string buildAsset() {
        textureArray.Apply();
        AssetDatabase.CreateAsset(textureArray, "Assets/Resources/Textures/" + path);
        return path;
    }

    public Texture2DArray getTextureArray() {
        textureArray.Apply();
        return textureArray;
    }




}
