using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public enum BlockType {
    AIR = 0, DIRT, GRASS, SNOW, 
}


/// <summary>
/// Generates int[,,] arrays of voxel data for creation of chunk meshes.
/// </summary>
public class ChunkVoxelDataGenerator {

    /// <summary>
    /// Empty constructor
    /// </summary>
    public ChunkVoxelDataGenerator() { }


        /// <summary>
        /// A function that creates voxel data for a chunk using simplex noise.
        /// </summary>
        /// <param name="pos">The position of the chunk in world space</param>
        /// <returns>int[,,] array containing data about the voxels in the chunk</returns>
        public BlockType[,,] getChunkVoxelData(Vector3 pos) {
        BlockType[,,] data = new BlockType[ChunkConfig.chunkSize, ChunkConfig.chunkHeight, ChunkConfig.chunkSize]; 

        for(int x = 0; x < ChunkConfig.chunkSize; x++) {
            for (int y = 0; y < ChunkConfig.chunkHeight; y++) {
                for (int z = 0; z < ChunkConfig.chunkSize; z++) {
                    Vector3 pos2D = new Vector3(x + pos.x, z + pos.z, 0);
                    Vector3 pos3D = new Vector3(x, y, z) + pos;
                    if ((y < calcHeight(pos2D) || calc3DStructure(pos3D)) && calc3DUnstructure(pos3D))
                        data[x, y, z] = BlockType.DIRT;
                    else
                        data[x, y, z] = BlockType.AIR;
                }
            }
        }

        for (int x = 0; x < ChunkConfig.chunkSize; x++) {
            for (int y = 0; y < ChunkConfig.chunkHeight; y++) {
                for (int z = 0; z < ChunkConfig.chunkSize; z++) {
                    if (data[x, y, z] != BlockType.AIR)
                        data[x, y, z] = decideBlockType(data, new Vector3Int(x, y, z));
                }
            }
        }



        return data;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="data">the generated terrain data</param>
    /// <param name="pos">position of block to find type for</param>
    private BlockType decideBlockType(BlockType[,,] data, Vector3Int pos) {
        BlockType blocktype = BlockType.DIRT;

        // Check if air above
        if((pos.y == ChunkConfig.chunkHeight - 1 || data[pos.x, pos.y + 1, pos.z] == BlockType.AIR) && blocktype == BlockType.DIRT) {
            if (pos.y > 40)
                blocktype = BlockType.SNOW;
            else
                blocktype = BlockType.GRASS; 
        }

        return blocktype;
    }

    /// <summary>
    /// Calculates the height of the chunk at the position
    /// </summary>
    /// <param name="pos">position of voxel</param>
    /// <returns>float height</returns>
    private float calcHeight(Vector3 pos) {
        float finalNoise = 0;
        float noiseScaler = 0;
        float octaveStrength = 1;
        for (int octave = 0; octave < ChunkConfig.octaves2D; octave++) {
            Vector3 samplePos = pos + new Vector3(1, 0, 1) * ChunkConfig.seed * octaveStrength;
            float noise = SimplexNoise.Simplex2D(samplePos, ChunkConfig.frequency2D / octaveStrength);
            float noise01 = (noise + 1f) / 2f;
            finalNoise += noise01 * octaveStrength;
            noiseScaler += octaveStrength;
            octaveStrength = octaveStrength / 2;
        }
        finalNoise = finalNoise / noiseScaler;
        finalNoise = Mathf.Pow(finalNoise, ChunkConfig.noiseExponent2D);
        return  finalNoise * ChunkConfig.chunkHeight;
    }

    private bool calc3DStructure(Vector3 pos) {
        float noise = SimplexNoise.Simplex3D(pos + Vector3.one * ChunkConfig.seed, ChunkConfig.frequency3D);
        float noise01 = (noise + 1f) / 2f;
        noise01 = Mathf.Lerp(noise01, 0, pos.y / ChunkConfig.chunkHeight);
        return ChunkConfig.Structure3DRate < noise01;
    }

    private bool calc3DUnstructure(Vector3 pos) {
        float noise = SimplexNoise.Simplex3D(pos - Vector3.one * ChunkConfig.seed, ChunkConfig.frequency3D);
        float noise01 = (noise + 1f) / 2f;
        noise01 = Mathf.Lerp(0, noise01, pos.y / ChunkConfig.chunkHeight);
        return ChunkConfig.Unstructure3DRate > noise01;
    }
}
