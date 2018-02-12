using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Used to generate textures for the terrain
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
        for (int i = 0; i < TextureManager.textureVariety; i++) {
            textureManager.addTexture(createTexture(TextureData.TextureType.WOOD, rnd.Next(9999)));
            textureManager.addTexture(createTexture(TextureData.TextureType.LEAF, rnd.Next(9999)));
        }
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

}
