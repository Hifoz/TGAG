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
    public static bool posContainsVoxel(Vector3 pos, int height, BiomeBase biome, float corruptionFactor) {
        if (corruptionFactor >= 1f) {
            return false;
        }
        if (pos.y <= 0) {
            return true;
        }

        if (pos.y < biome.minGroundHeight)
            return true;
        else if (pos.y > biome.maxGroundHeight)
            return false;
        else {
            float corruption = Corruption.corruptionNoise(pos, biome);
            return (pos.y < height ||
                    biome.structure3DRate > calc3DStructure(pos, biome, height) ||
                    biome.corruptionRate > corruption) &&
                    (biome.unstructure3DRate < calc3DUnstructure(pos, biome, height) ||
                     biome.corruptionRate < corruption);
        }
    }

    /// <summary>
    /// Determines if there is a voxel at a given location, using all biomes covering the sample position.
    /// </summary>
    /// <param name="pos">sample position</param>
    /// <param name="heights">height from all biomes covering the sample position</param>
    /// <param name="biomes">the biomes covering the sample position and the distance from the sample pos and the biome points</param>
    /// <returns></returns>
    public static bool posContainsVoxel(Vector3 pos, int height, List<Pair<BiomeBase, float>> biomes, float corruptionFactor) {
        if (corruptionFactor >= 1f) {
            return false;
        }
        if (pos.y <= 0) {
            return true;
        }

        if (biomes.Count == 1)
            return posContainsVoxel(pos, height, biomes[0].first, corruptionFactor);

        float structure = 0;
        float unstructure = 0;
        float corruption = 0;
        float structureRate = 0;
        float unstructureRate = 0;
        float corruptionRate = 0;
        float minH = 0;
        float maxH = 0;

        for (int i = 0; i < biomes.Count; i++) {
            minH += biomes[i].first.minGroundHeight * biomes[i].second;
            maxH += biomes[i].first.maxGroundHeight * biomes[i].second;
        }

        if (pos.y < minH)
            return true;
        else if (pos.y > maxH)
            return false;

        for (int i = 0; i < biomes.Count; i++) {
            structure += calc3DStructure(pos, biomes[i].first, height) * biomes[i].second;
            unstructure += calc3DUnstructure(pos, biomes[i].first, height) * biomes[i].second;
            corruption += Corruption.corruptionNoise(pos, biomes[i].first) * biomes[i].second;
            structureRate += biomes[i].first.structure3DRate * biomes[i].second;
            unstructureRate += biomes[i].first.unstructure3DRate * biomes[i].second;   
            corruptionRate += biomes[i].first.corruptionRate * biomes[i].second;
        }

        return (pos.y < height ||
                structureRate > structure ||
                corruptionRate > corruption) &&
                (unstructureRate < unstructure ||
                corruptionRate < corruption);
    }


    /// <summary>
    /// A function that creates voxel data for a chunk using simplex noise.
    /// </summary>
    /// <param name="pos">The position of the chunk in world space</param>
    /// <returns>Voxel data for the chunk</returns>
    public static BlockDataMap getChunkVoxelData(Vector3 pos, BiomeManager biomeManager) {
        BlockDataMap data = new BlockDataMap(WorldGenConfig.chunkSize + 2, WorldGenConfig.chunkHeight, WorldGenConfig.chunkSize + 2);
        /*
         * Pre-calculate 2d heightmap and biomemap:
         */

        List<Pair<BiomeBase, float>>[,] biomemap = new List<Pair<BiomeBase, float>>[WorldGenConfig.chunkSize + 2, WorldGenConfig.chunkSize + 2]; // Very proud of this beautiful thing /jk
        int[,] heightmap = new int[WorldGenConfig.chunkSize + 2, WorldGenConfig.chunkSize + 2];
        float[,] corruptionMap = new float[WorldGenConfig.chunkSize + 2, WorldGenConfig.chunkSize + 2];

        for (int x = 0; x < WorldGenConfig.chunkSize + 2; x++) {
            for (int z = 0; z < WorldGenConfig.chunkSize + 2; z++) {
                Vector3 xzPos = new Vector3(x, 0, z);
                biomemap[x, z] = biomeManager.getInRangeBiomes(new Vector2Int(x + (int)pos.x, z + (int)pos.z));
                corruptionMap[x, z] = Corruption.corruptionFactor(pos + xzPos);
                heightmap[x, z] = (int)calcHeight(pos + xzPos,  biomemap[x, z], corruptionMap[x, z]);

                // Initialize the blockdata map with heightmap data
                for (int y = 0; y < heightmap[x, z]; y++) {
                    data.mapdata[data.index1D(x, y, z)].blockType = BlockData.BlockType.DIRT;
                }
            }
        }


        /*
         * Calculate the 3d voxel map:
         */

        ArrayQueue<Vector3Int> active = new ArrayQueue<Vector3Int>(data.mapdata.Length);
        bool[,,] done = new bool[WorldGenConfig.chunkSize + 2, WorldGenConfig.chunkHeight, WorldGenConfig.chunkSize + 2];

        // Add all voxels at heightmap positions (Except the side ones, as they are added in next loop)
        for (int x = 1; x < WorldGenConfig.chunkSize + 1; x++) {
            for (int z = 1; z < WorldGenConfig.chunkSize + 1; z++) {
                active.Enqueue(new Vector3Int(x, heightmap[x, z], z));
                done[x, heightmap[x, z], z] = true;
            }
        }

        // Add voxels on the z-sides
        for (int w = 0; w < WorldGenConfig.chunkSize + 2; w++) {
            for (int y = 0; y < WorldGenConfig.chunkHeight; y++) {
                active.Enqueue(new Vector3Int(w, y, 0));
                done[w, y, 0] = true;
                active.Enqueue(new Vector3Int(w, y, WorldGenConfig.chunkSize + 1));
                done[w, y, WorldGenConfig.chunkSize + 1] = true;
            }
        }
        // Add voxels on the x-sides
        for (int w = 1; w < WorldGenConfig.chunkSize + 1; w++) {
            for (int y = 0; y < WorldGenConfig.chunkHeight; y++) {
                active.Enqueue(new Vector3Int(0, y, w));
                done[0, y, w] = true;
                active.Enqueue(new Vector3Int(WorldGenConfig.chunkSize + 1, y, w));
                done[WorldGenConfig.chunkSize + 1, y, w] = true;
            }
        }


        Vector3Int voxel;
        Vector3Int[] offsets = new Vector3Int[] {
            new Vector3Int(1, 0, 0), new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0),
            new Vector3Int(0, 0, 1), new Vector3Int(0, 0, -1)
        };

        while (active.Any()) {
            voxel = active.Dequeue();
            bool containsVoxel = posContainsVoxel(pos + voxel, heightmap[voxel.x, voxel.z], biomemap[voxel.x, voxel.z], corruptionMap[voxel.x, voxel.z]);

            data.mapdata[data.index1D(voxel.x, voxel.y, voxel.z)].blockType = 
                containsVoxel ? BlockData.BlockType.DIRT : BlockData.BlockType.NONE;

            // Try and add neightbours if block contains voxel above height
            //   or if block does not contain voxel at or under height
            if ((voxel.y > heightmap[voxel.x, voxel.z]) ^ !containsVoxel) {
                foreach (Vector3Int o in offsets) {
                    if (data.checkBounds(voxel + o) && !done[voxel.x + o.x, voxel.y + o.y, voxel.z + o.z]) {
                        active.Enqueue(voxel + o);
                        done[voxel.x + o.x, voxel.y + o.y, voxel.z + o.z] = true;
                    }
                }
            }
        }

        System.Random rng = new System.Random(WorldGenConfig.seed);
        // Set the final block types:
        for (int x = 0; x < WorldGenConfig.chunkSize + 2; x++) {
            for (int y = 0; y < WorldGenConfig.chunkHeight; y++) {
                for (int z = 0; z < WorldGenConfig.chunkSize + 2; z++) {
                    if (data.mapdata[data.index1D(x, y, z)].blockType != BlockData.BlockType.NONE && data.mapdata[data.index1D(x, y, z)].blockType != BlockData.BlockType.WATER)
                        decideBlockType(data, new Vector3Int(x, y, z), biomemap[x, z], rng); // TODO make this use biomes in some way?
                    else if (corruptionMap[x, z] < 1 && WorldGenConfig.heightInWater(y))
                        data.mapdata[data.index1D(x, Corruption.corruptWaterHeight(y, corruptionMap[x, z]), z)].blockType = BlockData.BlockType.WATER;
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
    private static void decideBlockType(BlockDataMap data, Vector3Int pos, List<Pair<BiomeBase, float>> biomes, System.Random rng) {
        int pos1d = data.index1D(pos.x, pos.y, pos.z);

        // Use the biomes to find the block type:
        if (biomes.Count == 1) {
            biomes[0].first.getBlockType(data, pos);
        } else {
            float increment = 0;
            float randVal = (float)rng.NextDouble();
            float tot = 0;

            // Calculate new weights and new total
            foreach (Pair<BiomeBase, float> p in biomes) {
                p.second = Mathf.Pow(p.second * 0.25f, 2f);
                tot += p.second;
            }
            // Normalize values to new total and find what biome to sample.
            foreach (Pair<BiomeBase, float> p in biomes) {
                p.second /= tot;
                increment += p.second;
                if (increment >= randVal) {
                    p.first.getBlockType(data, pos);
                    break;
                }
            }
        }

        // Calculate snow height.
        float snowHeight = 0;
        foreach (Pair<BiomeBase, float> p in biomes) {
            snowHeight += p.first.snowHeight * p.second;
        }
        // Add snow on top of blocks:
        if (pos.y == WorldGenConfig.chunkHeight - 1 || data.mapdata[data.index1D(pos.x, pos.y + 1, pos.z)].blockType == BlockData.BlockType.NONE) {
            if (pos.y > snowHeight) {
                data.mapdata[pos1d].modifier = BlockData.BlockType.SNOW;
            }
        }
    }


    /// <summary>
    /// Calculates the height of the chunk at the position
    /// </summary>
    /// <param name="pos">position of voxel</param>
    /// <param name="corruptionFactor">float for corruptionFactor</param>
    /// <returns>float height</returns>
    public static float calcHeight(Vector3 pos, List<Pair<BiomeBase, float>> biomes, float corruptionFactor) {
        // TODO: Currently, this locks all biomes to the same octaveCount and noiseExponent2D, it might be nice if this was not the case, so one could have differing octave counts and stuff
        //       Left it like this for now though, as all biomes currently made has the same settings for these 2 variables anyways.
        pos = new Vector3(pos.x, pos.z, 0); 
        float finalNoise = 0;
        float noiseScaler = 0;
        float octaveStrength = 1;
        for (int octave = 0; octave < WorldGenConfig.octaves2D; octave++) {
            Vector3 samplePos = pos + new Vector3(1, 1, 0) * WorldGenConfig.seed * octaveStrength;
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
        finalNoise = Mathf.Pow(finalNoise, WorldGenConfig.noiseExponent2D);


        float minH = biomes[0].first.minGroundHeight;
        float maxH = biomes[0].first.maxGroundHeight;

        if(biomes.Count > 1) {
            minH *= biomes[0].second;
            maxH *= biomes[0].second;
            for(int i = 1; i < biomes.Count; i++) {
                minH += biomes[i].first.minGroundHeight * biomes[i].second;
                maxH += biomes[i].first.maxGroundHeight * biomes[i].second;
            }
        }

        return Corruption.corruptHeight(minH + finalNoise * maxH, corruptionFactor);
    }

    /// <summary>
    /// Used to calculate areas of the world that should have voxels.
    /// Uses 3D simplex noise.
    /// </summary>
    /// <param name="pos">Sample pos</param>
    /// <returns>bool</returns>
    private static float calc3DStructure(Vector3 pos, BiomeBase biome, int height) {
        float noise = SimplexNoise.Simplex3D(pos + Vector3.one * WorldGenConfig.seed, biome.frequency3D);
        float noise01 = (noise + 1f) * 0.5f;
        return Mathf.Lerp(noise01, 1, (pos.y - biome.minGroundHeight) / biome.maxGroundHeight); //Because you don't want an ugly flat "ceiling" everywhere.
    }

    /// <summary>
    /// Used to calculate areas of the world that should not have voxels.
    /// Uses 3D simplex noise.
    /// </summary>
    /// <param name="pos">Sample pos</param>
    /// <returns>bool</returns>
    private static float calc3DUnstructure(Vector3 pos, BiomeBase biome, int height) {
        float noise = SimplexNoise.Simplex3D(pos - Vector3.one * WorldGenConfig.seed, biome.frequency3D);
        float noise01 = (noise + 1f) * 0.5f;
        return Mathf.Lerp(1, noise01, (pos.y - biome.minGroundHeight) / biome.maxGroundHeight); //Because you don't want the noise to remove the ground creating a void.
    }
}
