using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Generates int[,,] arrays of voxel data for creation of chunk meshes.
/// </summary>
public class ChunkVoxelDataGenerator {

    /// <summary>
    /// Empty constructor
    /// </summary>
    public ChunkVoxelDataGenerator() { }

    /// <summary>
    /// Determines if there is a voxel at the given location.
    /// </summary>
    /// <param name="pos">The position to investigate</param>
    /// <returns>bool contains voxel</returns>
    public static bool posContainsVoxel(Vector3 pos) {
        Vector3 pos2D = new Vector3(pos.x, pos.z, 0);
        return (pos.y < calcHeight(pos2D) || calc3DStructure(pos)) && calc3DUnstructure(pos);
    }

    /// <summary>
    /// A function that creates voxel data for a chunk using simplex noise.
    /// </summary>
    /// <param name="pos">The position of the chunk in world space</param>
    /// <returns>int[,,] array containing data about the voxels in the chunk</returns>
    public BlockData[,,] getChunkVoxelData(Vector3 pos) {
        BlockData[,,] data = new BlockData[ChunkConfig.chunkSize, ChunkConfig.chunkHeight, ChunkConfig.chunkSize];

        for (int x = 0; x < ChunkConfig.chunkSize; x++) {
            for (int y = 0; y < ChunkConfig.chunkHeight; y++) {
                for (int z = 0; z < ChunkConfig.chunkSize; z++) {
                    if (posContainsVoxel(new Vector3(x, y, z) + pos))
                        data[x, y, z] = new BlockData(BlockData.BlockType.DIRT);
                    else
                        data[x, y, z] = new BlockData(BlockData.BlockType.AIR);
                }
            }
        }

        for (int x = 0; x < ChunkConfig.chunkSize; x++) {
            for (int y = 0; y < ChunkConfig.chunkHeight; y++) {
                for (int z = 0; z < ChunkConfig.chunkSize; z++) {
                    if (data[x, y, z].blockType != BlockData.BlockType.AIR)
                        decideBlockType(data, new Vector3Int(x, y, z));
                }
            }
        }

        return data;
    }


    /// <summary>
    /// Used to decide what type of block goes on a position
    /// </summary>
    /// <param name="data">the generated terrain data</param>
    /// <param name="pos">position of block to find type for</param>
    private void decideBlockType(BlockData[,,] data, Vector3Int pos) {
        BlockData blockData = data[pos.x, pos.y, pos.z];

        // Add block type here:



        // Add modifier type:
        if ((pos.y == ChunkConfig.chunkHeight - 1 || data[pos.x, pos.y + 1, pos.z].blockType == BlockData.BlockType.AIR) && blockData.blockType != BlockData.BlockType.AIR) {
            if (pos.y > 40) {
                blockData.modifier = BlockData.ModifierType.SNOW;
            } else if (blockData.blockType == BlockData.BlockType.DIRT) {
                blockData.modifier = BlockData.ModifierType.GRASS;
            }
        }

    }

    /// <summary>
    /// Calculates the height of the chunk at the position
    /// </summary>
    /// <param name="pos">position of voxel</param>
    /// <returns>float height</returns>
    private static float calcHeight(Vector3 pos) {
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

    /// <summary>
    /// Used to calculate areas of the world that should have voxels.
    /// Uses 3D simplex noise.
    /// </summary>
    /// <param name="pos">Sample pos</param>
    /// <returns>bool</returns>
    private static bool calc3DStructure(Vector3 pos) {
        float noise = SimplexNoise.Simplex3D(pos + Vector3.one * ChunkConfig.seed, ChunkConfig.frequency3D);
        float noise01 = (noise + 1f) / 2f;
        noise01 = Mathf.Lerp(noise01, 0, pos.y / ChunkConfig.chunkHeight);
        return ChunkConfig.Structure3DRate < noise01;
    }

    /// <summary>
    /// Used to calculate areas of the world that should not have voxels.
    /// Uses 3D simplex noise.
    /// </summary>
    /// <param name="pos">Sample pos</param>
    /// <returns>bool</returns>
    private static bool calc3DUnstructure(Vector3 pos) {
        float noise = SimplexNoise.Simplex3D(pos - Vector3.one * ChunkConfig.seed, ChunkConfig.frequency3D);
        float noise01 = (noise + 1f) / 2f;
        noise01 = Mathf.Lerp(0, noise01, pos.y / ChunkConfig.chunkHeight);
        return ChunkConfig.Unstructure3DRate > noise01;
    }
}
