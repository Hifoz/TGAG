using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores data about a block
/// </summary>
public struct BlockData { // 2 bytes. 1 byte per blocktype stored
    public enum BlockType : byte {
        NONE,

        DIRT,
        STONE,
        SAND,
        WOOD,
        LEAF,
        WATER,

        ANIMAL,

        GRASS,
        SNOW,

        ALLWHITE,
        COUNT
    }

    public static BlockData Empty = new BlockData(BlockType.NONE);


    public BlockType blockType;
    public BlockType modifier;

    public BlockData(BlockType baseType, BlockType modifier = BlockType.NONE) {
        this.blockType = baseType;
        this.modifier = modifier;
    }

    //Made this because reordering the enums made the world look really weird
    // Not gonna comb through the code to find out why, this is easier.
    /// <summary>
    /// Maps a blocktype to a color index (index used in shader)
    /// </summary>
    /// <param name="type">blocktype to convert</param>
    /// <returns>index</returns>
    public static int blockTypeToColorIndex(BlockType type) {
        switch (type) {
            case BlockType.DIRT:
                return 0;
            case BlockType.STONE:
                return 1;
            case BlockType.SAND:
                return 2;
            case BlockType.GRASS:
                return 3;
            case BlockType.SNOW:
                return 4;
            default:
                return -1;
        }
    }
}