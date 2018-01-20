using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A structure containing data on a chunk
/// </summary>
public class ChunkData {
    ChunkVoxelMesh CVM = new ChunkVoxelMesh();

    private Mesh mesh; // Might want to use multiple meshes for each chunk?, as each mesh can only be 53 tall with a chunkSize of 10 (current)
    private Vector3 position;

    /// <summary>
    /// Empty constructor
    /// </summary>
    public ChunkData() {

    }

    /// <summary>
    /// Construct a new chunk
    /// </summary>
    /// <param name="pos">position of the chunk</param>
    public ChunkData(Vector3 pos) {
        position = pos;

        mesh = CVM.getVoxelMesh(pos);
    }

    /// <summary>
    /// Construct a new chunk
    /// </summary>
    /// <param name="pos">position of the chunk</param>
    public ChunkData(ChunkVoxelData CVD) {
        position = CVD.chunkPos;

        mesh = CVM.getVoxelMesh(CVD.chunkPos);
    }

    public Vector3 getPos() {
        return position;
    }

    public Mesh getMesh() {
        return mesh;
    }
}