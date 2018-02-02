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
        textureManager.addTexture(createTexture(textureManager.getTextureSize(), TextureManager.TextureType.DIRT), TextureManager.TextureType.DIRT);
        textureManager.addTexture(createTexture(textureManager.getTextureSize(), TextureManager.TextureType.DIRT, 152313), TextureManager.TextureType.DIRT);
        textureManager.addTexture(createTexture(textureManager.getTextureSize(), TextureManager.TextureType.DIRT, 661321), TextureManager.TextureType.DIRT);
        textureManager.loadTextureFromFile(sharedPath + "temp_stone", TextureManager.TextureType.STONE);
        textureManager.loadTextureFromFile(sharedPath + "temp_sand", TextureManager.TextureType.SAND);
        textureManager.loadTextureFromFile(sharedPath + "temp_grass_top", TextureManager.TextureType.GRASS_TOP);
        textureManager.loadTextureFromFile(sharedPath + "temp_grass_side", TextureManager.TextureType.GRASS_SIDE);
        textureManager.loadTextureFromFile(sharedPath + "temp_snow_top", TextureManager.TextureType.SNOW_TOP);
        textureManager.loadTextureFromFile(sharedPath + "temp_snow_side", TextureManager.TextureType.SNOW_SIDE);
    }


    public Color[] createTexture(int size, TextureManager.TextureType texType, int seed = 42) {
        Color[] pixels = new Color[size * size];

        System.Random rng = new System.Random(seed);

        for (int i = 0; i < size * size; i++) {
            int x = i / size + seed;
            int y = i % size + seed;

            float[] pixelHSV;

            switch (texType) {
                case TextureManager.TextureType.DIRT:
                    pixelHSV = createDirtPixelHSV(x, y, seed);
                    break;
                default:
                    pixelHSV = new float[3];
                    break;
            }

            // Translate HSV to RGB
            pixels[i] = Color.HSVToRGB(pixelHSV[0], pixelHSV[1], pixelHSV[2]);
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

        return new float[] {hue, saturation, value};
    }


    /// <summary>
    /// Used to create a dirt texture
    /// </summary>
    /// <param name="size">Size of the texture (x and y dimensions)</param>
    /// <returns>Pixeldata for the texture</returns>
    [Obsolete("Use createTexture() With texTypeparameter set to \"TextureManager.TextureType.DIRT\" instead.")]
    private Color[] createDirtTexture(int size, int seed = 42) {
        Color[] pixels = new Color[size*size];

        System.Random rng = new System.Random(seed);

        float valueNoiseFrequency = 0.004f;
        float saturationNoiseFrequency = 0.02f;

        const float baseHue = 0.083f;
        const float baseSaturation = 0.6f;
        const float baseValue = 0.9f;

        for (int i = 0; i < size * size; i++) {
            int x = i / size + seed;
            int y = i % size + seed;

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

            float mod1 = (SimplexNoise.Simplex2D(modPos, valueNoiseFrequency)) * 0.15f;
            float mod2 = (SimplexNoise.Simplex2D(modPos, 0.01f)) * 0.05f;
            float mod3 = (SimplexNoise.Simplex2D(modPos2, valueNoiseFrequency)) * 0.15f;

            float mod = mod1 * 0.75f + (int)(mod1 * 20) / 20f * 0.25f; // Add mod with effect
            mod += mod3 * 0.75f + (int)(mod3 * 20) / 20f * 0.25f; // Add mod with effect
            mod += mod2;
            mod *= 0.4f;

            float value = Mathf.Clamp01(baseValue + mod) * 0.7f;


        // Translate HSV to RGB
            pixels[i] = Color.HSVToRGB(hue, saturation, value);
        }

        return pixels;
    }


    private Color[] createStoneTexture(int size, int seed = 404) {
        return new Color[size * size];
    }


    private Color[] createSandTexture(int size, int seed = 404) {
        return new Color[size * size];
    }


    private Color[] createGrassTopTexture(int size, int seed = 404) {
        return new Color[size * size];
    }


    private Color[] createGrassSideTexture(int size, int seed = 404) {
        return new Color[size * size];
    }


    private Color[] createSnowTopTexture(int size, int seed = 404) {
        return new Color[size * size];
    }


    private Color[] createSnowSideTexture(int size, int seed = 404) {
        return new Color[size * size];
    }


}
