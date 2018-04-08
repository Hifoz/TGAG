using UnityEngine;
/// <summary>
/// Lowlands w/ larger lakes and dense forest
/// </summary>
class LowlandForestBiome : BiomeBase {
    public LowlandForestBiome() :
        base(
            name:"lowlands",
            //General
            minGroundHeight:1,
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
            corruptionRate: 0.5f,
            corruptionFrequency: 0.025f,
            //Foliage
            maxTreesPerChunk: 14,
            treeLineLength: 2.0f,
            treeVoxelSize: 1.0f,
            treeThickness: 0.5f,
            treeLeafThickness: 3f,
            grammarRecursionDepth: 4
        ) { }
}
