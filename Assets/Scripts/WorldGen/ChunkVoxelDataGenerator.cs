using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Generates voxel data for creation of chunk meshes.
/// </summary>
public static class ChunkVoxelDataGenerator {

    /// <summary>
    /// Determines if there is a voxel at the given location.
    /// </summary>
    /// <param name="pos">The position to investigate</param>
    /// <returns>bool contains voxel</returns>
    [Obsolete("Use posContainsVoxel(Vector3 pos, int height) instead. (You will have to pre-calculate the 2d height for this).")]
    public static bool posContainsVoxel(Vector3 pos) {
        return (pos.y < calcHeight(pos, null) || calc3DStructure(pos, -1, null)) && calc3DUnstructure(pos, -1, null);
    }

    /// <summary>
    /// Determines if there is a voxel at the given location, with a precalculated 2d height.
    /// </summary>
    /// <param name="pos">The position to investigate</param>
    /// <param name="isAlreadyVoxel">If the location currently contains a voxel</param>
    /// <returns>Whether the location contains a voxel</returns>
    public static bool posContainsVoxel(Vector3 pos, int height, Biome biome) {
        return (pos.y < height || calc3DStructure(pos, height, biome)) && calc3DUnstructure(pos, height, biome);
    }

    /// <summary>
    /// A function that creates voxel data for a chunk using simplex noise.
    /// </summary>
    /// <param name="pos">The position of the chunk in world space</param>
    /// <returns>Voxel data for the chunk</returns>
    public static BlockDataMap getChunkVoxelData(Vector3 pos, BiomeManager biomeManager) {
        BlockDataMap data = new BlockDataMap(ChunkConfig.chunkSize + 2, ChunkConfig.chunkHeight, ChunkConfig.chunkSize + 2);

        // Pre-calculate 2d heightmap and biomemap:
        int[,] heightmap = new int[ChunkConfig.chunkSize + 2, ChunkConfig.chunkSize + 2];
        Biome[,] biomemap = new Biome[ChunkConfig.chunkSize + 2, ChunkConfig.chunkSize + 2];
        for (int x = 0; x < ChunkConfig.chunkSize + 2; x++) {
            for (int z = 0; z < ChunkConfig.chunkSize + 2; z++) {
                biomemap[x, z] = biomeManager.getBiome(new Vector2Int(x + (int)pos.x, z + (int)pos.z));
                int height = (int)calcHeight(new Vector3(x, 0, z) + pos, biomemap[x, z]);
                heightmap[x, z] = height;
            }
        }

        for (int x = 0; x < ChunkConfig.chunkSize + 2; x++) {
            for (int y = 0; y < ChunkConfig.chunkHeight; y++) {
                for (int z = 0; z < ChunkConfig.chunkSize + 2; z++) {
                    int i = data.index1D(x, y, z);
                    if(posContainsVoxel(pos + new Vector3(x, y, z), heightmap[x, z], biomemap[x, z]))
                        data.mapdata[i] = new BlockData(BlockData.BlockType.DIRT);
                    else if (y < ChunkConfig.waterHeight)
                        data.mapdata[i] = new BlockData(BlockData.BlockType.WATER);
                    else
                        data.mapdata[i] = new BlockData(BlockData.BlockType.NONE);
                }
            }
        }

        for (int x = 0; x < ChunkConfig.chunkSize + 2; x++) {
            for (int y = 0; y < ChunkConfig.chunkHeight; y++) {
                for (int z = 0; z < ChunkConfig.chunkSize + 2; z++) {
                    if (data.mapdata[data.index1D(x, y, z)].blockType != BlockData.BlockType.NONE && data.mapdata[data.index1D(x, y, z)].blockType != BlockData.BlockType.WATER)
                        decideBlockType(data, new Vector3Int(x, y, z), biomemap[x, z]);
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
    private static void decideBlockType(BlockDataMap data, Vector3Int pos, Biome biome) {
        int pos1d = data.index1D(pos.x, pos.y, pos.z);

        // Add block type here:
        if (pos.y < ChunkConfig.waterHeight)
            data.mapdata[pos1d].blockType = BlockData.BlockType.SAND;

        // Add modifier type:
        if (pos.y == ChunkConfig.chunkHeight - 1 || data.mapdata[data.index1D(pos.x, pos.y + 1, pos.z)].blockType == BlockData.BlockType.NONE) {
            if (pos.y > ChunkConfig.snowHeight - SimplexNoise.Simplex2D(new Vector2(pos.x, pos.z), 0.002f)) {
                data.mapdata[pos1d].modifier = BlockData.BlockType.SNOW;
            } else if (data.mapdata[pos1d].blockType == BlockData.BlockType.DIRT) {
                data.mapdata[pos1d].modifier = BlockData.BlockType.GRASS;

            }
        }

    }

    /// <summary>
    /// Calculates the height of the chunk at the position
    /// </summary>
    /// <param name="pos">position of voxel</param>
    /// <returns>float height</returns>
    public static float calcHeight(Vector3 pos, Biome biome) {
        pos = new Vector3(pos.x, pos.z, 0);
        float finalNoise = 0;
        float noiseScaler = 0;
        float octaveStrength = 1;
        for (int octave = 0; octave < biome.octaves2D; octave++) {
            Vector3 samplePos = pos + new Vector3(1, 1, 0) * ChunkConfig.seed * octaveStrength;
            float noise = SimplexNoise.Simplex2D(samplePos, biome.frequency2D / octaveStrength);
            float noise01 = (noise + 1f) / 2f;
            finalNoise += noise01 * octaveStrength;
            noiseScaler += octaveStrength;
            octaveStrength = octaveStrength / 2;
        }
        finalNoise = finalNoise / noiseScaler;
        finalNoise = Mathf.Pow(finalNoise, biome.noiseExponent2D);
        return  finalNoise * ChunkConfig.chunkHeight;
    }

    /// <summary>
    /// Used to calculate areas of the world that should have voxels.
    /// Uses 3D simplex noise.
    /// </summary>
    /// <param name="pos">Sample pos</param>
    /// <returns>bool</returns>
    private static bool calc3DStructure(Vector3 pos, int height, Biome biome) {
        float noise = SimplexNoise.Simplex3D(pos + Vector3.one * ChunkConfig.seed, biome.frequency3D) +
            SimplexNoise.Simplex3D(pos + new Vector3(0, 500, 0) + Vector3.one * ChunkConfig.seed, biome.frequency3D);
        noise *= 0.5f;
        float noise01 = (noise + 1f) / 2f;
        noise01 = Mathf.Lerp(noise01, 1, pos.y / ChunkConfig.chunkHeight); //Because you don't want an ugly flat "ceiling" everywhere.

        return biome.Structure3DRate * 0.75f > noise01;
    }

    /// <summary>
    /// Used to calculate areas of the world that should not have voxels.
    /// Uses 3D simplex noise.
    /// </summary>
    /// <param name="pos">Sample pos</param>
    /// <returns>bool</returns>
    private static bool calc3DUnstructure(Vector3 pos, int height, Biome biome) {
        float noise = SimplexNoise.Simplex3D(pos - Vector3.one * ChunkConfig.seed, biome.frequency3D) + 
            SimplexNoise.Simplex3D(pos + new Vector3(0, 500, 0) - Vector3.one * ChunkConfig.seed, biome.frequency3D);
        noise *= 0.5f;
        float noise01 = (noise + 1f) / 2f;
        noise01 = Mathf.Lerp(1, noise01, pos.y / ChunkConfig.chunkHeight); //Because you don't want the noise to remove the ground creating a void.
        return biome.Unstructure3DRate < noise01;
    }
}
