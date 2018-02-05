using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Used to generate textures
/// </summary>
class TreeTextureGenerator : MonoBehaviour {
    TextureManager textureManager;
    private System.Random rnd = new System.Random(DateTime.Now.Millisecond);


    /// <summary>
    /// Used to generate/load textures for trees
    /// </summary>
    private void Start() {
        textureManager = GameObject.Find("TreeTextureManager").GetComponent<TextureManager>();
        textureManager.Clear();

        // Generate 3 variations for each texture type
        for (int i = 0; i < 4; i++) {
            textureManager.addTexture(createTexture(TextureData.TextureType.WOOD, rnd.Next(9999)));
            textureManager.addTexture(createTexture(TextureData.TextureType.LEAF, rnd.Next(9999)));
        }

        // Old
        string sharedPath = "Textures/temp/";
        //textureManager.loadTextureFromFile(sharedPath + "temp_dirt", TextureData.TextureType.WOOD);
        textureManager.loadTextureFromFile(sharedPath + "temp_grass_top", TextureData.TextureType.LEAF);
    }

    /// <summary>
    /// Create a procedural texture
    /// </summary>
    /// <param name="size">size of texture (x and y dimension)</param>
    /// <param name="texType">What textureType</param>
    /// <param name="seed">seed to use</param>
    /// <returns>A Color[] containing the pixels for a texture</returns>
    public TextureData createTexture(TextureData.TextureType texType, int seed = 42) {
        int size = textureManager.getTextureSize();
        Color[] pixels = new Color[size * size];

        for (int i = 0; i < size * size; i++) {
            int x = i % size;
            int y = i / size;

            float[] pixelHSV;

            switch (texType) {
                case TextureData.TextureType.WOOD:
                    pixelHSV = createWoodPixelHSV(x + seed, y + seed, seed);
                    break;
                case TextureData.TextureType.LEAF:
                    pixelHSV = createLeafPixelHSV(x + seed, y + seed, seed);
                    break;
                default:
                    pixelHSV = new float[4];
                    break;
            }

            // Translate HSV to RGB
            pixels[i] = Color.HSVToRGB(pixelHSV[0], pixelHSV[1], pixelHSV[2]);
            pixels[i].a = pixelHSV[3];
        }

        return new TextureData(pixels, texType);
    }

    /// <summary>
    /// Used to create a pixel for a wood texture
    /// </summary>
    /// <param name="x">x position of pixel</param>
    /// <param name="y">y position of pixel</param>
    /// <param name="seed">Seed for texture</param>
    /// <returns>HSV of a pixel in a wood texture texture</returns>
    private float[] createWoodPixelHSV(int x, int y, int seed) {
        const float valueNoiseFrequency = 0.0004f;

        const float baseHue = 0.083f;
        const float baseSaturation = 0.7f;
        const float baseValue = 0.3f;

        Vector2 pos = new Vector2(x * 5, y);

        Vector3 modPos = new Vector3(pos.x + SimplexNoise.Simplex2D(new Vector2(x, y), 0.005f) * 17, 0);


        // Calulate Hue:
        float hue = baseHue;

        // Calculate Saturation:
        float saturation = baseSaturation;

        // Calculate Value:
        float v1 = SimplexNoise.Simplex2D(pos, valueNoiseFrequency) * SimplexNoise.Simplex2D(pos - new Vector2(seed, seed), valueNoiseFrequency * 2f) * 0.3f +
                   SimplexNoise.Simplex2D(pos, valueNoiseFrequency * 5) * 0.2f +
                   SimplexNoise.Simplex2D(pos, valueNoiseFrequency * 10) * 0.2f;

        float value = Mathf.Clamp01(baseValue + (0.8f + v1) % 0.2f);


        return new float[] { hue, saturation, value, 1 };

        /*
        float mV = SimplexNoise.Simplex2D(pos, valueNoiseFrequency) *
            SimplexNoise.Simplex2D(pos + new Vector2(seed, seed), valueNoiseFrequency) *
            SimplexNoise.Simplex2D(pos - new Vector2(seed, seed), valueNoiseFrequency) *
            SimplexNoise.Simplex2D(pos - new Vector2(seed, seed), valueNoiseFrequency * 0.4f);

        if (mV < 0.05f)
            mV = 1;
        else
            mV = 0;
        */

    }

    /// <summary>
    /// Used to create a pixel for a leaf texture
    /// </summary>
    /// <param name="x">x position of pixel</param>
    /// <param name="y">y position of pixel</param>
    /// <param name="seed">Seed for texture</param>
    /// <returns>HSV of a pixel in a leaf texture texture</returns>
    private float[] createLeafPixelHSV(int x, int y, int seed) {
        const float valueNoiseFrequency = 0.004f;

        const float baseHue = 0.2f;
        const float baseSaturation = 0.85f;
        const float baseValue = 0.6f;

        // Calulate Hue:
        float hue = baseHue;

        // Calculate Saturation:
        float saturation = baseSaturation;

        // Calculate Value:
        float value = baseValue;

        return new float[] { hue, saturation, value, 1 };
    }

}
