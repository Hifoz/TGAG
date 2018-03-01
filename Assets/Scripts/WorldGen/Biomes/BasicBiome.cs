using System;
using System.Collections.Generic;
using UnityEngine;

class BasicBiome : Biome {
    public BasicBiome() :
        base(
            //General
            snowHeight: 90,
            //2D noise settings
            frequency2D: 0.001f,
            noiseExponent2D: 3,
            octaves2D: 6,
            //3D noise settings
            Structure3DRate: 0.75f,
            Unstructure3DRate: 0.85f,
            frequency3D: 0.0075f,
            //Foliage
            maxTreesPerChunk: 1,
            treeLineLength: 2.0f,
            treeVoxelSize: 1.0f,
            treeThickness: 0.5f,
            treeLeafThickness: 3f,
            grammarRecursionDepth: 4
        ) {}


}
