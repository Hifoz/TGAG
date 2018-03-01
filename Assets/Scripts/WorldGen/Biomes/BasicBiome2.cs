using System;
using System.Collections.Generic;
using UnityEngine;

class BasicBiome2 : Biome {
    public BasicBiome2() :
        base(
            //General
            snowHeight: 40,
            //2D noise settings
            frequency2D: 0.0005f,
            noiseExponent2D: 3,
            octaves2D: 6,
            //3D noise settings
            Structure3DRate: 0.4f,
            Unstructure3DRate: 0.4f,
            frequency3D: 0.0045f,
            //Foliage
            maxTreesPerChunk: 1,
            treeLineLength: 2.0f,
            treeVoxelSize: 1.0f,
            treeThickness: 0.5f,
            treeLeafThickness: 3f,
            grammarRecursionDepth: 4
        ) { }


}
