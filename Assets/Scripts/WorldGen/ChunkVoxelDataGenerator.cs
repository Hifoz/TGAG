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
    /// A function that creates voxel data for a chunk using simplex noise.
    /// </summary>
    /// <param name="pos">The position of the chunk in world space</param>
    /// <returns>int[,,] array containing data about the voxels in the chunk</returns>
    public int[,,] getChunkVoxelData(Vector3 pos) {
        int[,,] data = new int[ChunkConfig.chunkSize, ChunkConfig.chunkHeight, ChunkConfig.chunkSize]; 

        for(int x = 0; x < ChunkConfig.chunkSize; x++) {
            for (int y = 0; y < ChunkConfig.chunkHeight; y++) {
                for (int z = 0; z < ChunkConfig.chunkSize; z++) {
                    Vector3 samplePos = new Vector3(x + pos.x, z + pos.z, 0);
                    if (y < calcHeight(samplePos))
                        data[x, y, z] = 1;
                    else
                        data[x, y, z] = 0;
                }
            }
        }
        return data;
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
        for (int octave = 0; octave < ChunkConfig.octaves; octave++) {
            Vector3 samplePos = pos + new Vector3(1, 0, 1) * ChunkConfig.seed * octaveStrength;
            float noise = SimplexNoise.Simplex2D(samplePos, ChunkConfig.frequency / octaveStrength);
            float noise01 = (noise + 1f) / 2f;
            finalNoise += noise01 * octaveStrength;
            noiseScaler += octaveStrength;
            octaveStrength = octaveStrength / 2;
        }
        finalNoise = finalNoise / noiseScaler;
        finalNoise = Mathf.Pow(finalNoise, ChunkConfig.noiseExponent);
        return  finalNoise * ChunkConfig.chunkHeight;
    }
}
