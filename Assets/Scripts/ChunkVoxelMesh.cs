using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkVoxelMesh {

    ChunkVoxelData CVD;

    public ChunkVoxelMesh() {
        CVD = new ChunkVoxelData();
    }

    /// <summary>
    /// Gets a new mesh for given position
    /// </summary>
    /// <param name="pos">position of chunk to get mesh for</param>
    /// <returns></returns>
    public Mesh getVoxelMesh(Vector3 pos) {
        int[,,] voxelData = CVD.getChunkVoxelData(pos);

        Mesh mesh = MeshGenerator.GenerateMesh(voxelData);

        return mesh;
    }


    /// <summary>
    /// Just some test data for mesh generator
    /// </summary>
    /// <returns>test data</returns>
    private int[,,] testData() {
        int[,,] points = new int[ChunkConfig.chunkSize, ChunkConfig.chunkHeight, ChunkConfig.chunkSize];

        int aa = 0;
        for (int i = 0; i < ChunkConfig.chunkSize; i++) {
            aa = Mathf.Abs(aa - 1);
            for (int j = 0; j < ChunkConfig.chunkHeight; j++) {
                aa = Mathf.Abs(aa - 1);
                for (int k = 0; k < ChunkConfig.chunkSize; k++) {
                    points[i, j, k] = aa;
                    aa = Mathf.Abs(aa - 1);
                }
            }
        }

        return points;
    }

}
