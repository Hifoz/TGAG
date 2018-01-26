using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureGenerator {
    Texture2DArray textures;
    int maxTextures;
    int size;

    public TextureGenerator(int size, int maxTextures) {
        this.size = size;
        this.maxTextures = maxTextures;
        textures = new Texture2DArray(size, size, maxTextures, TextureFormat.RGBAFloat, false);
    }


    public void GenerateTexture(Color[] pixelData, int index) {
        textures.SetPixels(pixelData, 0);
    }


    public Texture2DArray GetTextureArray() {
        return textures;
    }

    public Color[] GetTexture(int index) {
        if (index < maxTextures)
            return textures.GetPixels(index);

        return null;
    }
}
