/// <summary>
/// This biome has the same settings we were using pre-biomes
/// </summary>
class BasicBiome : BiomeBase {
    public BasicBiome() :
        base(
            //General
            minGroundHeight: 1,
            maxGroundHeight: 200,
            snowHeight: 90,
            //2D noise settings
            frequency2D: 0.001f,
            noiseExponent2D: 3,
            octaves2D: 6,
            //3D noise settings
            structure3DRate: 0.57f,
            unstructure3DRate: 0.85f,
            frequency3D: 0.0075f,
            corruptionRate: 0.5f,
            corruptionFrequency: 0.02f,
            //Foliage
            maxTreesPerChunk: 2,
            treeLineLength: 2.0f,
            treeVoxelSize: 1.0f,
            treeThickness: 0.5f,
            treeLeafThickness: 3f,
            grammarRecursionDepth: 4
        ) {}


}
