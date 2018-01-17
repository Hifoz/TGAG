using System;
using System.Collections.Generic;
using UnityEngine;


class ChunkData {
    private Mesh mesh;

    private Vector3 position;

    ChunkVoxelMesh CVM = new ChunkVoxelMesh();

    /// <summary>
    /// Construct a new chunk
    /// </summary>
    /// <param name="pos">position of the chunk</param>
    public ChunkData(Vector3 pos) {
        position = pos;

        mesh = CVM.getVoxelMesh(pos); // This part can be threaded out.
    }

    public Mesh getMesh() {
        return mesh;
    }

}