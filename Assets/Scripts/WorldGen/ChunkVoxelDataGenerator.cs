using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Generates voxel data for creation of chunk meshes.
/// </summary>
public static class ChunkVoxelDataGenerator {

    /// <summary>
    /// Determines if there is a voxel at the given location, with a precalculated 2d height.
    /// </summary>
    /// <param name="pos">The position to investigate</param>
    /// <param name="isAlreadyVoxel">If the location currently contains a voxel</param>
    /// <returns>Whether the location contains a voxel</returns>
    public static bool posContainsVoxel(Vector3 pos, int height, Biome biome) {
        return (pos.y < height || biome.Structure3DRate > calc3DStructure(pos, biome, height) ) && biome.Unstructure3DRate < calc3DUnstructure(pos, biome, height);
    }

    /// <summary>
    /// Determines if there is a voxel at a given location, using all biomes covering the sample position.
    /// </summary>
    /// <param name="pos">sample position</param>
    /// <param name="heights">height from all biomes covering the sample position</param>
    /// <param name="biomes">the biomes covering the sample position and the distance from the sample pos and the biome points</param>
    /// <returns></returns>
    public static bool posContainsVoxel(Vector3 pos, int height, List<Pair<Biome, float>> biomes) {
        if (biomes.Count == 1)
            return posContainsVoxel(pos, height, biomes[0].first);

        float structure = 0;
        float unstructure = 0;
        float structureRate = 0;
        float unstructureRate = 0;

        for (int i = 0; i < biomes.Count; i++) {
            structure += calc3DStructure(pos, biomes[i].first, height) * biomes[i].second;
            unstructure += calc3DUnstructure(pos, biomes[i].first, height) * biomes[i].second;
            structureRate += biomes[i].first.Structure3DRate * biomes[i].second;
            unstructureRate += biomes[i].first.Unstructure3DRate * biomes[i].second;
        }

        return (pos.y < height || structureRate > structure) && unstructureRate < unstructure;
    }


    /// <summary>
    /// A function that creates voxel data for a chunk using simplex noise.
    /// </summary>
    /// <param name="pos">The position of the chunk in world space</param>
    /// <returns>Voxel data for the chunk</returns>
    public static BlockDataMap getChunkVoxelData(Vector3 pos, BiomeManager biomeManager) {
        BlockDataMap data = new BlockDataMap(ChunkConfig.chunkSize + 2, ChunkConfig.chunkHeight, ChunkConfig.chunkSize + 2);

        // Pre-calculate 2d heightmap and biomemap:
        List<Pair<Biome, float>>[,] biomemap = new List<Pair<Biome, float>>[ChunkConfig.chunkSize + 2, ChunkConfig.chunkSize + 2]; // Very proud of this beautiful thing /jk
        int[,] heightmap = new int[ChunkConfig.chunkSize + 2, ChunkConfig.chunkSize + 2];

        for (int x = 0; x < ChunkConfig.chunkSize + 2; x++) {
            for (int z = 0; z < ChunkConfig.chunkSize + 2; z++) {
                biomemap[x, z] = biomeManager.getInRangeBiomes(new Vector2Int(x + (int)pos.x, z + (int)pos.z));
                heightmap[x, z] = (int)calcHeight(pos + new Vector3(x, 0, z), biomemap[x, z]);
                //for (int i = 0; i < biomemap[x, z].Count; i++) {
                //    heightmap[x, z] += (int)(calcHeight(pos + new Vector3(x, 0, z), biomemap[x, z][i].first) * biomemap[x, z][i].second);
                //}
            }
        }

        // Calculate 3d noise and apply 2d and 3d noise
        for (int x = 0; x < ChunkConfig.chunkSize + 2; x++) {
            for (int y = 0; y < ChunkConfig.chunkHeight; y++) {
                for (int z = 0; z < ChunkConfig.chunkSize + 2; z++) {
                    int i = data.index1D(x, y, z);
                    if (posContainsVoxel(pos + new Vector3(x, y, z), heightmap[x, z], biomemap[x, z]))
                        data.mapdata[i] = new BlockData(BlockData.BlockType.DIRT);
                    else if (y < ChunkConfig.waterHeight)
                        data.mapdata[i] = new BlockData(BlockData.BlockType.WATER);
                    else
                        data.mapdata[i] = new BlockData(BlockData.BlockType.NONE);
                }
            }
        }

        //for (int x = 1; x < ChunkConfig.chunkSize + 1; x++) {
        //    for (int y = 0; y < ChunkConfig.chunkHeight; y++) {
        //        for (int z = 1; z < ChunkConfig.chunkSize + 1; z++) {
        //            int count = 0;
        //            if (data.mapdata[data.index1D(x + 1, y, z)].blockType == BlockData.BlockType.DIRT) count++;
        //            if (data.mapdata[data.index1D(x - 1, y, z)].blockType == BlockData.BlockType.DIRT) count++;
        //            if (data.mapdata[data.index1D(x + 1, y, z + 1)].blockType == BlockData.BlockType.DIRT) count++;
        //            if (data.mapdata[data.index1D(x,     y, z + 1)].blockType == BlockData.BlockType.DIRT) count++;
        //            if (data.mapdata[data.index1D(x - 1, y, z + 1)].blockType == BlockData.BlockType.DIRT) count++;
        //            if (data.mapdata[data.index1D(x + 1, y, z - 1)].blockType == BlockData.BlockType.DIRT) count++;
        //            if (data.mapdata[data.index1D(x,     y, z - 1)].blockType == BlockData.BlockType.DIRT) count++;
        //            if (data.mapdata[data.index1D(x - 1, y, z - 1)].blockType == BlockData.BlockType.DIRT) count++;

        //            if (data.mapdata[data.index1D(x, y, z)].blockType == BlockData.BlockType.DIRT && count <= 3)
        //                data.mapdata[data.index1D(x, y, z)].blockType = BlockData.BlockType.NONE;
        //            else if (data.mapdata[data.index1D(x, y, z)].blockType == BlockData.BlockType.NONE && count >= 5)
        //                data.mapdata[data.index1D(x, y, z)].blockType = BlockData.BlockType.DIRT;
        //        }
        //    }
        //}


        for (int x = 0; x < ChunkConfig.chunkSize + 2; x++) {
            for (int y = 0; y < ChunkConfig.chunkHeight; y++) {
                for (int z = 0; z < ChunkConfig.chunkSize + 2; z++) {
                    if (data.mapdata[data.index1D(x, y, z)].blockType != BlockData.BlockType.NONE && data.mapdata[data.index1D(x, y, z)].blockType != BlockData.BlockType.WATER)
                        decideBlockType(data, new Vector3Int(x, y, z), biomemap[x, z]); // TODO make this use biomes in some way?
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
    private static void decideBlockType(BlockDataMap data, Vector3Int pos, List<Pair<Biome, float>> biomes) {
        int pos1d = data.index1D(pos.x, pos.y, pos.z);

        // Calculate snow height:
        float snowHeight = 0;
        foreach(Pair<Biome, float> p in biomes) {
            snowHeight += p.first.snowHeight * p.second;
        }


        // Add block type here:
        if (pos.y < ChunkConfig.waterHeight)
            data.mapdata[pos1d].blockType = BlockData.BlockType.SAND;

        // Add modifier type:
        if (pos.y == ChunkConfig.chunkHeight - 1 || data.mapdata[data.index1D(pos.x, pos.y + 1, pos.z)].blockType == BlockData.BlockType.NONE) {
            if (pos.y > snowHeight) {
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
        return finalNoise * ChunkConfig.chunkHeight;
    }


    /// <summary>
    /// Calculates the height of the chunk at the position
    /// </summary>
    /// <param name="pos">position of voxel</param>
    /// <returns>float height</returns>
    public static float calcHeight(Vector3 pos, List<Pair<Biome, float>> biomes) {
        pos = new Vector3(pos.x, pos.z, 0);
        float finalNoise = 0;
        float noiseScaler = 0;
        float octaveStrength = 1;
        for (int octave = 0; octave < 6; octave++) {
            Vector3 samplePos = pos + new Vector3(1, 1, 0) * ChunkConfig.seed * octaveStrength;
            float noise = 0;
            for (int b = 0; b < biomes.Count; b++) {
                noise += SimplexNoise.Simplex2D(samplePos, biomes[b].first.frequency2D / octaveStrength) * biomes[b].second;
            }
            float noise01 = (noise + 1f) / 2f;
            finalNoise += noise01 * octaveStrength;
            noiseScaler += octaveStrength;
            octaveStrength = octaveStrength / 2;
        }
        finalNoise = finalNoise / noiseScaler;
        finalNoise = Mathf.Pow(finalNoise, 3);
        return finalNoise * ChunkConfig.chunkHeight;
    }

    /// <summary>
    /// Used to calculate areas of the world that should have voxels.
    /// Uses 3D simplex noise.
    /// </summary>
    /// <param name="pos">Sample pos</param>
    /// <returns>bool</returns>
    private static float calc3DStructure(Vector3 pos, Biome biome, int height) {
        float noise = SimplexNoise.Simplex3D(pos + Vector3.one * ChunkConfig.seed, biome.frequency3D) +
            SimplexNoise.Simplex3D(pos + new Vector3(0, 500, 0) + Vector3.one * ChunkConfig.seed, biome.frequency3D);
        noise *= 0.5f;
        float noise01 = (noise + 1f) * 0.5f;
        return Mathf.Lerp(noise01, 1, pos.y / ChunkConfig.chunkHeight); //Because you don't want an ugly flat "ceiling" everywhere.
    }

    /// <summary>
    /// Used to calculate areas of the world that should not have voxels.
    /// Uses 3D simplex noise.
    /// </summary>
    /// <param name="pos">Sample pos</param>
    /// <returns>bool</returns>
    private static float calc3DUnstructure(Vector3 pos, Biome biome, int height) {
        float noise = SimplexNoise.Simplex3D(pos - Vector3.one * ChunkConfig.seed, biome.frequency3D) + 
            SimplexNoise.Simplex3D(pos + new Vector3(0, 500, 0) - Vector3.one * ChunkConfig.seed, biome.frequency3D);
        noise *= 0.5f;
        float noise01 = (noise + 1f)  * 0.5f;
        return Mathf.Lerp(1, noise01, pos.y / ChunkConfig.chunkHeight); //Because you don't want the noise to remove the ground creating a void.
    }
}
