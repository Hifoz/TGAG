using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Used to generate textures
/// </summary>
class TerrainTextureGenerator : MonoBehaviour {


    // Temporarily using this to load the old textures until i get some actual generation up and going:
    private void Awake() {
        TextureManager textureManager = GameObject.Find("TerrainTextureManager").GetComponent<TextureManager>();
        textureManager.Clear();
        string sharedPath = "Textures/temp/";
        textureManager.addTexture(createDirtTexture(textureManager.getTextureSize()));
        textureManager.addTexture(createDirtTexture(textureManager.getTextureSize()));
        textureManager.addTexture(createDirtTexture(textureManager.getTextureSize()));
        //textureManager.loadTextureFromFile(sharedPath + "temp_dirt");
        //textureManager.loadTextureFromFile(sharedPath + "temp_dirt");
        //textureManager.loadTextureFromFile(sharedPath + "temp_dirt");
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

    /// <summary>
    /// Used to create a dirt texture
    /// </summary>
    /// <param name="size">Size of the texture (x and y dimensions)</param>
    /// <returns>Pixeldata for the texture</returns>
    private Color[] createDirtTexture(int size, int seed = 42) {
        Color[] pixels = new Color[size*size];

        System.Random rng = new System.Random(seed);

        float valueNoiseFrequency = 0.005f;
        float saturationNoiseFrequency = 0.02f;

        float[] hue = new float[size * size];
        float[] saturation = new float[size * size];
        float[] value = new float[size * size];


        for (int i = 0; i < size * size; i++) {
            int x = i / size;
            int y = i % size;

            hue[i] = 0.06f + (float)rng.NextDouble() / 100f;

            saturation[i] = 1 - (SimplexNoise.Simplex2D(new Vector3(x, y), saturationNoiseFrequency*0.8f) + SimplexNoise.Simplex2D(new Vector3(x, y), saturationNoiseFrequency*0.3f))/3f;

            value[i] = SimplexNoise.Simplex2D(new Vector3(x, y), valueNoiseFrequency) / 4f + 0.4f;



        }

        // Translate HSV to RGB
        for(int i = 0; i < size * size; i++) {
            pixels[i] = Color.HSVToRGB(hue[i], saturation[i], value[i]);
        }

        return pixels;
    }



}
