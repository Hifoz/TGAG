using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockData {
    public enum BlockType {
        AIR, DIRT, STONE, SAND
    }

    public enum ModifierType {
        NONE, GRASS, SNOW
    }

    public BlockType blockType;
    public ModifierType modifier;

    public BlockData(BlockType baseType, ModifierType modifierType = ModifierType.NONE) {
        blockType = baseType;
        modifier = modifierType;
    }
}