using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ChunkConfig {
    //public static float voxelSize = 1;
    public static int seed = 1337;
    public static int chunkSize = 20;
    public static int chunkCount = 20;
    public static int chunkHeight = 40; // Chunk height must not exceed (5376/(chunkSize^2))
    public static float frequency = 0.01f;
    public static float noiseExponent = 2;
    public static int octaves = 2;
}
