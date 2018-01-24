using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ChunkConfig {
    //public static float voxelSize = 1;
    public static int seed = 1337;
    public static int chunkSize = 20;
    public static int chunkCount = 20;
    public static int chunkHeight = 100; // Chunk height must not exceed (5376/(chunkSize^2))
    public static float frequency2D = 0.01f;
    public static float noiseExponent2D = 2;
    public static int octaves2D = 2;
    public static float Structure3DRate = 0.25f;
    public static float Unstructure3DRate = 0.15f;
    public static float frequency3D = 0.01f;
}
