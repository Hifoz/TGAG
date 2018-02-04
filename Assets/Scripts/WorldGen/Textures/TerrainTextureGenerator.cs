using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Used to generate textures
/// </summary>
class TerrainTextureGenerator : MonoBehaviour {
    TextureManager textureManager;
    System.Random rnd = new System.Random(DateTime.Now.Millisecond);

    /// <summary>
    /// Used to generate/load textures for the terrain.
    /// </summary>
    private void Start() {
        textureManager = GameObject.Find("TerrainTextureManager").GetComponent<TextureManager>();
        textureManager.Clear();
        string sharedPath = "Textures/temp/";
        // Dirt
        textureManager.addTexture(createTexture(textureManager.getTextureSize(), TextureManager.TextureType.DIRT, rnd.Next(9999)), TextureManager.TextureType.DIRT);
        textureManager.addTexture(createTexture(textureManager.getTextureSize(), TextureManager.TextureType.DIRT, rnd.Next(9999)), TextureManager.TextureType.DIRT);
        textureManager.addTexture(createTexture(textureManager.getTextureSize(), TextureManager.TextureType.DIRT, rnd.Next(9999)), TextureManager.TextureType.DIRT);
        // Grass
        textureManager.addTexture(createTexture(textureManager.getTextureSize(), TextureManager.TextureType.GRASS_SIDE, rnd.Next(9999)), TextureManager.TextureType.GRASS_SIDE);
        textureManager.addTexture(createTexture(textureManager.getTextureSize(), TextureManager.TextureType.GRASS_SIDE, rnd.Next(9999)), TextureManager.TextureType.GRASS_SIDE);
        textureManager.addTexture(createTexture(textureManager.getTextureSize(), TextureManager.TextureType.GRASS_SIDE, rnd.Next(9999)), TextureManager.TextureType.GRASS_SIDE);
        textureManager.addTexture(createTexture(textureManager.getTextureSize(), TextureManager.TextureType.GRASS_SIDE, rnd.Next(9999)), TextureManager.TextureType.GRASS_SIDE);
        textureManager.addTexture(createTexture(textureManager.getTextureSize(), TextureManager.TextureType.GRASS_TOP, rnd.Next(9999)), TextureManager.TextureType.GRASS_TOP);
        textureManager.addTexture(createTexture(textureManager.getTextureSize(), TextureManager.TextureType.GRASS_TOP, rnd.Next(9999)), TextureManager.TextureType.GRASS_TOP);
        textureManager.addTexture(createTexture(textureManager.getTextureSize(), TextureManager.TextureType.GRASS_TOP, rnd.Next(9999)), TextureManager.TextureType.GRASS_TOP);


        // Old
        textureManager.loadTextureFromFile(sharedPath + "temp_stone", TextureManager.TextureType.STONE);
        textureManager.loadTextureFromFile(sharedPath + "temp_sand", TextureManager.TextureType.SAND);
        textureManager.loadTextureFromFile(sharedPath + "temp_snow_top", TextureManager.TextureType.SNOW_TOP);
        textureManager.loadTextureFromFile(sharedPath + "temp_snow_side", TextureManager.TextureType.SNOW_SIDE);
    }

    /// <summary>
    /// Create a procedural texture
    /// </summary>
    /// <param name="size">size of texture (x and y dimension)</param>
    /// <param name="texType">What textureType</param>
    /// <param name="seed">seed to use</param>
    /// <returns>A Color[] containing the pixels for a texture</returns>
    public Color[] createTexture(int size, TextureManager.TextureType texType, int seed = 42) {
        Color[] pixels = new Color[size * size];

        for (int i = 0; i < size * size; i++) {
            int x = i % size;
            int y = i / size;

            float[] pixelHSV;

            switch (texType) {
                case TextureManager.TextureType.DIRT:
                    pixelHSV = createDirtPixelHSV(x + seed, y + seed, seed);
                    break;
                case TextureManager.TextureType.GRASS_SIDE:
                    pixelHSV = createGrassPixelHSV(x + seed, y + seed, seed);
                    pixelHSV[3] = createGrassSideBorder(x, y, seed);
                    break;
                case TextureManager.TextureType.GRASS_TOP:
                    pixelHSV = createGrassPixelHSV(x + seed, y + seed, seed);
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
        const float baseValue = 0.8f;

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
    /// Used to create border for grass side.
    /// </summary>
    /// <param name="x">x position of pixel</param>
    /// <param name="y">y position of pixel</param>
    /// <param name="seed">Seed for texture</param>
    /// <returns>alpha of pixel</returns>
    private float createGrassSideBorder(int x, int y, int seed) {
        const float noiseFrequency = 0.04f;

        float baseGrassHeight = 0.85f * textureManager.getTextureSize();
        float baseGrass2Height = 0.85f * textureManager.getTextureSize();


        Vector3 modPos = new Vector3(
            x + SimplexNoise.Simplex2D(new Vector3(x, y), 0.02f),
            y + SimplexNoise.Simplex2D(new Vector3(x, y), 0.02f)
        );

        float grassHeight = baseGrassHeight +
            SimplexNoise.Simplex2D(new Vector3(x + seed, y), noiseFrequency) * 20 +
            SimplexNoise.Simplex2D(new Vector3(x + seed, y), noiseFrequency * 5) * 15;

        float grass2Height = baseGrass2Height +
            SimplexNoise.Simplex2D(new Vector3(x + seed * 2, y), noiseFrequency) * 10 +
            SimplexNoise.Simplex2D(new Vector3(x + seed * 2, y), noiseFrequency * 5) * 5;


        float alpha = 0;
        if (modPos.y > grassHeight)
            alpha = 1;
        else if (modPos.y > grass2Height)
            alpha = 0.3f;



        return alpha;
    }

    /// <summary>
    /// Used to create a pixel for a grass texture
    /// </summary>
    /// <param name="x">x position of pixel</param>
    /// <param name="y">y position of pixel</param>
    /// <param name="seed">Seed for texture</param>
    /// <returns>HSV of a pixel in a grass
    /// texture</returns>
    private float[] createGrassPixelHSV(int x, int y, int seed) {
        const float noiseFrequency = 0.004f;

        const float baseHue = 0.2f;
        const float baseSaturation = 0.85f;
        const float baseValue = 0.6f;
        float baseGrassHeight = 0.75f * textureManager.getTextureSize();

        Vector2 pos = new Vector2(x, y);


        // Calulate Hue:
        float hue = baseHue;

        // Calculate Saturation:
        float saturation = baseSaturation;

        // Calculate Value:

        float modifierValue = SimplexNoise.Simplex2D(pos, noiseFrequency) * 0.10f +
            SimplexNoise.Simplex2D(pos, noiseFrequency * 3) * 0.10f +
            SimplexNoise.Simplex2D(pos, noiseFrequency * 6) * 0.10f +
            SimplexNoise.Simplex2D(pos, noiseFrequency * 10) * 0.10f +
            SimplexNoise.Simplex2D(pos, noiseFrequency * 30) * 0.15f +
            SimplexNoise.Simplex2D(pos, noiseFrequency * 70) * 0.15f +
            SimplexNoise.Simplex2D(pos, noiseFrequency * 100) * 0.15f;


        float value = Mathf.Clamp01(baseValue + modifierValue * 0.5f);



        return new float[] { hue, saturation, value, 1 };
    }


}
