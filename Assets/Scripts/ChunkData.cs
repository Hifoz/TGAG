using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A structure containing data on a chunk
/// </summary>
class ChunkData {
    ChunkVoxelMesh CVM = new ChunkVoxelMesh();

    private Mesh mesh; // Might want to use multiple meshes for each chunk?, as each mesh can only be 53 tall with a chunkSize of 10 (current)
    private Vector3 position;


    /// <summary>
    /// Construct a new chunk
    /// </summary>
    /// <param name="pos">position of the chunk</param>
    public ChunkData(Vector3 pos) {
        position = pos;

        mesh = CVM.getVoxelMesh(pos); // This part seems like an easy place to start threading out the resource heavy parts of chunk generation :P
    }

    public Mesh getMesh() {
        return mesh;
    }

}