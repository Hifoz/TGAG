using UnityEngine;
/// <summary>
/// Desert biome
/// </summary>
class DesertBiome : Biome {
    public DesertBiome() :
        base(
            //General
            minGroundHeight:60,
            maxGroundHeight:140,
            snowHeight: 150,
            //2D noise settings
            frequency2D: 0.0005f,
            noiseExponent2D: 3,
            octaves2D: 6,
            //3D noise settings
            structure3DRate: 0.2f,
            unstructure3DRate: 0.3f,
            frequency3D: 0.0045f,
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
