﻿using System;
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

        float valueNoiseFrequency = 0.004f;
        float saturationNoiseFrequency = 0.02f;

        const float baseHue = 0.1f;
        const float baseSaturation = 0.6f;
        const float baseValue = 0.9f;


        float[] hue = new float[size * size];
        float[] saturation = new float[size * size];
        float[] value = new float[size * size];


        for (int i = 0; i < size * size; i++) {
            int x = i / size;
            int y = i % size;

            hue[i] = baseHue;

            saturation[i] = baseSaturation;// 1 - (SimplexNoise.Simplex2D(new Vector3(x, y), saturationNoiseFrequency*0.8f) + SimplexNoise.Simplex2D(new Vector3(x, y), saturationNoiseFrequency*0.3f))/3f;


            Vector3 modPos = new Vector3(
                x + SimplexNoise.Simplex2D(new Vector3(x, y), 0.01f) * 50,
                y + SimplexNoise.Simplex2D(new Vector3(x, y), 0.01f) * 50
            );

            Vector3 modPos2 = new Vector3(
                x + SimplexNoise.Simplex2D(new Vector3(28541 + x, y), 0.01f) * 50,
                y + SimplexNoise.Simplex2D(new Vector3(x, y + 79146), 0.01f) * 50
            );

            value[i] = baseValue;
            float mod1 = (SimplexNoise.Simplex2D(modPos, valueNoiseFrequency)) * 0.15f;
            float mod2 = (SimplexNoise.Simplex2D(modPos, 0.01f)) * 0.05f;
            float mod3 = (SimplexNoise.Simplex2D(modPos2, valueNoiseFrequency)) * 0.15f;

            float mod = mod1 * 0.75f + (int)(mod1 * 20) / 20f * 0.25f; // Add mod with effect
            mod += mod3 * 0.75f + (int)(mod3 * 20) / 20f * 0.25f; // Add mod with effect
            mod += mod2;
            mod *= 0.4f;

            value[i] = Mathf.Clamp01(baseValue + mod) * 0.7f;


        }

        // Translate HSV to RGB
        for(int i = 0; i < size * size; i++) {
            pixels[i] = Color.HSVToRGB(hue[i], saturation[i], value[i]);
        }

        return pixels;
    }



}