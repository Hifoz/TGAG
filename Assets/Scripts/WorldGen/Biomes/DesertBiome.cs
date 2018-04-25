using UnityEngine;


/// <summary>
/// Desert biome
/// </summary>
class DesertBiome : BiomeBase {
    public DesertBiome() :
        base(
            name: "desert",
            //General
            minGroundHeight:30,
            maxGroundHeight:160,
            snowHeight: 150,
            //2D noise settings
            frequency2D: 0.001f,
            noiseExponent2D: 3,
            octaves2D: 6,
            //3D noise settings
            structure3DRate: 0.2f,
            unstructure3DRate: 0.3f,
            frequency3D: 0.0045f,
            corruptionRate: 0.5f,
            corruptionFrequency: 0.01f,
            //Foliage
            maxTreesPerChunk: 0,
            treeLineLength: 2.0f,
            treeVoxelSize: 1.0f,
            treeThickness: 0.5f,
            treeLeafThickness: 3f,
            grammarRecursionDepth: 4
        ) { }

    /// <summary>
    /// Get blockType 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="pos"></param>
    public override void getBlockType(BlockDataMap data, Vector3Int pos) {
        data.mapdata[data.index1D(pos.x, pos.y, pos.z)].blockType = BlockData.BlockType.SAND;
    }
}
