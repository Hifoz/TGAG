using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

class WaterMeshDataGenerator : MeshDataGenerator {

    #region meshdata generation

    /// <summary>
    /// Generates all data needed for a mesh of cubes
    /// </summary>
    /// <param name="pointmap">Point data used to build the mesh.
    /// The outermost layer (in x and z) is used to decide whether to add faces on the cubes on the second outermost layer (in x and z).</param>
    /// <returns>a mesh made from the input data</returns>
    public static MeshData[] GenerateWaterMeshData(BlockDataMap pointmap, float voxelSize = 1f, Vector3 offset = default(Vector3)) {
        WaterMeshDataGenerator MDG = new WaterMeshDataGenerator();
        MDG.meshDataType = MeshDataType.TERRAIN;

        MDG.pointmap = pointmap;


        for (int i = 0; i < (int)FaceDirection.COUNT; i++)
            MDG.dmd[i] = new MeshDataSection();



        for (int x = 1; x < pointmap.GetLength(0) - 1; x++) {
            for (int y = 0; y < pointmap.GetLength(1); y++) {
                for (int z = 1; z < pointmap.GetLength(2) - 1; z++) {
                    if (MDG.checkIfWaterVoxel(new Vector3Int(x, y, z)))
                        MDG.GenerateWaterCube(new Vector3Int(x, y, z), offset, pointmap.mapdata[pointmap.index1D(x, y, z)], voxelSize);
                }
            }
        }


        foreach (MeshDataSection d in MDG.dmd) {
            MDG.vertices.AddRange(d.vertices);
            MDG.normals.AddRange(d.normals);
            MDG.triangles.AddRange(d.triangles);
            MDG.colors.AddRange(d.colors);
            MDG.uvs.AddRange(d.uvs);
        }



        MeshData meshData = new MeshData();
        meshData.vertices = MDG.vertices.ToArray();
        meshData.normals = MDG.normals.ToArray();
        meshData.triangles = MDG.triangles.ToArray();
        meshData.colors = MDG.colors.ToArray();
        meshData.uvs = MDG.uvs.ToArray();

        return meshData.split();
    }

    private void GenerateWaterCube(Vector3Int cubePos, Vector3 offset, BlockData blockData, float voxelSize) {
        if (cubePos.x != pointmap.GetLength(0) - 1 && checkIfWaterVoxel(cubePos + new Vector3Int(1, 0, 0)) == false) GenerateCubeFace(FaceDirection.xp, cubePos - offset, blockData, voxelSize);
        if (cubePos.y == pointmap.GetLength(1) - 1 || checkIfWaterVoxel(cubePos + new Vector3Int(0, 1, 0)) == false) GenerateCubeFace(FaceDirection.yp, cubePos - offset, blockData, voxelSize); // Obs. On Y up we also want a face even if it is the outermost layer
        if (cubePos.z != pointmap.GetLength(2) - 1 && checkIfWaterVoxel(cubePos + new Vector3Int(0, 0, 1)) == false) GenerateCubeFace(FaceDirection.zp, cubePos - offset, blockData, voxelSize);
        if (cubePos.x != 0 && checkIfWaterVoxel(cubePos + new Vector3Int(-1, 0, 0)) == false) GenerateCubeFace(FaceDirection.xm, cubePos - offset, blockData, voxelSize);
        if (cubePos.y != 0 && checkIfWaterVoxel(cubePos + new Vector3Int(0, -1, 0)) == false) GenerateCubeFace(FaceDirection.ym, cubePos - offset, blockData, voxelSize);
        if (cubePos.z != 0 && checkIfWaterVoxel(cubePos + new Vector3Int(0, 0, -1)) == false) GenerateCubeFace(FaceDirection.zm, cubePos - offset, blockData, voxelSize);
    }

    protected bool checkIfWaterVoxel(Vector3Int voxelPos) {
        return pointmap.mapdata[pointmap.index1D(voxelPos.x, voxelPos.y, voxelPos.z)].blockType == BlockData.BlockType.WATER;

    }

    #endregion
}
