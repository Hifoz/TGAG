using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ChunkConfig {
    //public static float voxelSize = 1;
    public static int seed = 1337;
    public static int chunkSize = 10;
    public static int chunkCount = 11;
    public static int chunkHeight = 50; // Chunk height must not exceed (5376/(chunkSize^2))
    public static float frequency = 0.02f;
    public static float noiseExponent = 1;
    public static int octaves = 1;
}
