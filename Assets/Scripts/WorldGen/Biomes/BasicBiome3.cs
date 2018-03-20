/// <summary>
/// Mountainous biome
/// </summary>
class BasicBiome3 : Biome {
    public BasicBiome3() :
        base(
            //General
            minGroundHeight: 40,
            maxGroundHeight: 200,
            snowHeight: 10,
            //2D noise settings
            frequency2D: 0.001f,
            noiseExponent2D: 3,
            octaves2D: 6,
            //3D noise settings
            structure3DRate: 0.8f,
            unstructure3DRate: 0.7f,
            frequency3D: 0.009f,
            //Foliage
            maxTreesPerChunk: 2,
            treeLineLength: 2.0f,
            treeVoxelSize: 1.0f,
            treeThickness: 0.5f,
            treeLeafThickness: 3f,
            grammarRecursionDepth: 4
        ) { }


}
