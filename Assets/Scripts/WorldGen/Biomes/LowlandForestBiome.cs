using UnityEngine;
/// <summary>
/// Lowlands w/ larger lakes and dense forest
/// </summary>
class LowlandForestBiome : BiomeBase {
    public LowlandForestBiome() :
        base(
            //General
            minGroundHeight:0,
            maxGroundHeight:90,
            snowHeight: 80,
            //2D noise settings
            frequency2D: 0.0005f,
            noiseExponent2D: 3,
            octaves2D: 6,
            //3D noise settings
            structure3DRate: 0.5f,
            unstructure3DRate: 0.3f,
            frequency3D: 0.0045f,
            //Foliage
            maxTreesPerChunk: 4,
            treeLineLength: 2.0f,
            treeVoxelSize: 1.0f,
            treeThickness: 0.5f,
            treeLeafThickness: 3f,
            grammarRecursionDepth: 4
        ) { }
}
