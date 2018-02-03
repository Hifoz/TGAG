using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Used to generate textures
/// </summary>
class TerrainTextureGenerator : MonoBehaviour {
    TextureManager textureManager;

    // Temporarily using this to load the old textures until i get some actual generation up and going:
    private void Start() {
        textureManager = GameObject.Find("TerrainTextureManager").GetComponent<TextureManager>();
        textureManager.Clear();
        string sharedPath = "Textures/temp/";
        textureManager.addTexture(createTexture(textureManager.getTextureSize(), TextureManager.TextureType.DIRT), TextureManager.TextureType.DIRT);
        textureManager.addTexture(createTexture(textureManager.getTextureSize(), TextureManager.TextureType.DIRT, 152313), TextureManager.TextureType.DIRT);
        textureManager.addTexture(createTexture(textureManager.getTextureSize(), TextureManager.TextureType.DIRT, 661321), TextureManager.TextureType.DIRT);

        textureManager.addTexture(createTexture(textureManager.getTextureSize(), TextureManager.TextureType.GRASS_SIDE), TextureManager.TextureType.GRASS_SIDE);

        textureManager.loadTextureFromFile(sharedPath + "temp_stone", TextureManager.TextureType.STONE);
        textureManager.loadTextureFromFile(sharedPath + "temp_sand", TextureManager.TextureType.SAND);
        textureManager.loadTextureFromFile(sharedPath + "temp_grass_top", TextureManager.TextureType.GRASS_TOP);
        //textureManager.loadTextureFromFile(sharedPath + "temp_grass_side", TextureManager.TextureType.GRASS_SIDE);
        textureManager.loadTextureFromFile(sharedPath + "temp_snow_top", TextureManager.TextureType.SNOW_TOP);
        textureManager.loadTextureFromFile(sharedPath + "temp_snow_side", TextureManager.TextureType.SNOW_SIDE);
    }


    public Color[] createTexture(int size, TextureManager.TextureType texType, int seed = 42) {
        Color[] pixels = new Color[size * size];

        System.Random rng = new System.Random(seed);

        for (int i = 0; i < size * size; i++) {
            int x = i % size;
            int y = i / size;

            float[] pixelHSV;

            switch (texType) {
                case TextureManager.TextureType.DIRT:
                    pixelHSV = createDirtPixelHSV(x, y, seed);
                    break;
                case TextureManager.TextureType.GRASS_SIDE:
                    pixelHSV = createGrassSidePixelHSV(x, y, seed);
                    break;
                default:
                    pixelHSV = new float[3];
                    break;
            }

            // Translate HSV to RGB
            pixels[i] = Color.HSVToRGB(pixelHSV[0], pixelHSV[1], pixelHSV[2]);
            pixels[i].a = pixelHSV[3];
        }

        return pixels;
    }

    /// <summary>
    /// Used to create a pixel for a dirt texture
    /// </summary>
    /// <param name="x">x position of pixel</param>
    /// <param name="y">y position of pixel</param>
    /// <param name="seed">Seed for texture</param>
    /// <returns>HSV of a pixel in a dirt texture</returns>
    private float[] createDirtPixelHSV(int x, int y, int seed) {
        const float valueNoiseFrequency = 0.004f;
        const float saturationNoiseFrequency = 0.02f;

        const float baseHue = 0.083f;
        const float baseSaturation = 0.6f;
        const float baseValue = 0.9f;

        // Calulate Hue:
        float hue = baseHue;

        // Calculate Saturation:
        float saturation = baseSaturation;

        // Calculate Value:
        Vector3 modPos = new Vector3(
            x + SimplexNoise.Simplex2D(new Vector3(x, y), 0.01f) * 50,
            y + SimplexNoise.Simplex2D(new Vector3(x, y), 0.01f) * 50
        );

        Vector3 modPos2 = new Vector3(
            x + SimplexNoise.Simplex2D(new Vector3(28541 + x, y), 0.01f) * 50,
            y + SimplexNoise.Simplex2D(new Vector3(x, y + 79146), 0.01f) * 50
        );
        
        float v1 = (SimplexNoise.Simplex2D(modPos, valueNoiseFrequency)) * 0.15f;
        float v2 = (SimplexNoise.Simplex2D(modPos, 0.01f)) * 0.05f;
        float v3 = (SimplexNoise.Simplex2D(modPos2, valueNoiseFrequency)) * 0.15f;

        float modifierValue = v1 * 0.75f + (int)(v1 * 20) / 20f * 0.25f; // Add mod with effect
        modifierValue += v3 * 0.75f + (int)(v3 * 20) / 20f * 0.25f; // Add mod with effect
        modifierValue += v2;
        modifierValue *= 0.4f;

        float value = Mathf.Clamp01(baseValue + modifierValue) * 0.7f;

        return new float[] {hue, saturation, value, 1};
    }

    /// <summary>
    /// Used to create a pixel for a grass_side texture
    /// </summary>
    /// <param name="x">x position of pixel</param>
    /// <param name="y">y position of pixel</param>
    /// <param name="seed">Seed for texture</param>
    /// <returns>HSV of a pixel in a grass_side texture</returns>
    private float[] createGrassSidePixelHSV(int x, int y, int seed) {
        const float noiseFrequency = 0.004f;

        const float baseHue = 0.16f;
        const float baseSaturation = 1;
        const float baseValue = 0.9f;
        float baseGrassHeight = 0.75f * textureManager.getTextureSize();


        Vector3 modPos = new Vector3(
            x + SimplexNoise.Simplex1D(new Vector3(x + seed, y + seed * 2), 0.08f) * 25,
            y + SimplexNoise.Simplex1D(new Vector3(x + seed * 3, y - seed), 0.05f) * 50
        );

        Vector3 modPos2 = new Vector3(
            x + SimplexNoise.Simplex2D(new Vector3(x + seed, y + seed * 2), 0.02f) * 100,
            y + SimplexNoise.Simplex2D(new Vector3(x + seed * 3, y - seed), 0.02f) * 100
        );

        float grassHeight = baseGrassHeight + SimplexNoise.Simplex1D(new Vector3(x, y), noiseFrequency) * 20;

        float alpha = (modPos.y * 0.75f + modPos2.y * 0.25f) > grassHeight ? 1 : 0;



        return new float[] { baseHue, baseSaturation, baseValue, alpha };
    }

}
