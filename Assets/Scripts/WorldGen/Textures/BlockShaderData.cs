using UnityEngine;
using System.Collections;

public static class BlockShaderData {
    public const float maxFrequency = 1500;
    public const float halfFreq = 12;
    public const float noneFreq = 1;
    public const float dirtFreq = 9;
    public const float stoneFreq = 9;
    public const float sandFreq = 9;
    public const float grassTopFreq = 9;
    public const float grassSideFreq = 9;
    public const float snowTopFreq = 9;
    public const float snowSideFreq = 9;
    public const float allWhiteFreq = 9;

    public static readonly Color[] colorData = new Color[] {
        new Color(1, 0, 0, halfFreq / maxFrequency),
        new Color(1, 0, 0, noneFreq / maxFrequency),
        new Color(1, 0, 0, dirtFreq / maxFrequency),
        new Color(1, 0, 0, stoneFreq / maxFrequency),
        new Color(1, 0, 0, sandFreq / maxFrequency),
        new Color(0, 1, 0, grassTopFreq / maxFrequency),
        new Color(0, 1, 0, grassSideFreq / maxFrequency),
        new Color(1, 1, 1, snowTopFreq / maxFrequency),
        new Color(1, 1, 1, snowSideFreq / maxFrequency),
        new Color(1, 0, 0, allWhiteFreq / maxFrequency),
    };

    /// <summary>
    ///  Texture Type
    /// </summary>
    public enum VoxelType {
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
        ALLWHITE,

        COUNT
    }
}
