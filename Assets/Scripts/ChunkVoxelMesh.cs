using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkVoxelMesh {

    ChunkVoxelData CVD;

    public ChunkVoxelMesh() {
        CVD = new ChunkVoxelData();
    }

    public Mesh getVoxelMesh(Vector3 pos) {
        int[,,] voxelData = CVD.getChunkVoxelData(pos);

        return new Mesh();
    }
}
