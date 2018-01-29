using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockData {
    public enum BlockType {
        NONE,
        // Base types:
        DIRT,
       // STONE,
       // SAND,
        
        // Modifiers:
        GRASS,
        SNOW
    }

    public BlockType blockType;
    public BlockType modifier;

    public BlockData(BlockType baseType, BlockType modifier = BlockType.NONE) {
        this.blockType = baseType;
        this.modifier = modifier;
    }
}