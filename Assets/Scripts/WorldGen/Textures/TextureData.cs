using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores a texture and its type
/// </summary>
public class TextureData {

    /// <summary>
    ///  Texture Type
    /// </summary>
    public enum TextureType {
        // All texture types must correspond to a block type, append "_SIDE", "_TOP", "_BOTTOM" where neccessary (these types must also be added in the check in MeshDataGenerator.addSliceData())
        // NB! When changing this, make sure the switch in textures.hlsl::getTexelValue() matches
        HALF,
        NONE,
        DIRT,
        STONE,
        SAND,
        GRASS_TOP,
        GRASS_SIDE,
        SNOW_TOP,
        SNOW_SIDE,
        WOOD,
        LEAF,
        WATER,
        ALLWHITE,


        COUNT
    }

    public Color[] pixels;
    public TextureType type;

    public TextureData(Color[] pixels, TextureType type) {
        this.pixels = pixels;
        this.type = type;
    }
}
