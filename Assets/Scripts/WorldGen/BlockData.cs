using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores data about a block
/// </summary>
public struct BlockData { // 8 bytes. 4 bytes per blocktype stored
    public enum BlockType {
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

    public BlockType blockType;
    public BlockType modifier;

    public BlockData(BlockType baseType, BlockType modifier = BlockType.NONE) {
        this.blockType = baseType;
        this.modifier = modifier;
    }
}