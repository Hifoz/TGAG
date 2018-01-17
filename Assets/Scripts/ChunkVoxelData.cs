using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkVoxelData {

    public ChunkVoxelData() {

    }

    public int[,,] getChunkVoxelData(Vector3 pos) {
        return new int[ChunkConfig.chunkSize, ChunkConfig.chunkHeight, ChunkConfig.chunkSize];
    }
}
