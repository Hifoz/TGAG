using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// had to change this to make it more thread compatible.

public class MeshData {
    public Vector3[] vertices;
    public int[] triangles;
    public Color[] colors;
    public Vector2[] uvs;
    public Vector2[] uvs2;
}

/// <summary>
/// A Voxel Mesh generator 
/// </summary>
public class MeshDataGenerator {
    public static List<int>[] terrainTextureTypes;
    public static List<int>[] treeTextureTypes;

    protected List<Vector3> vertices = new List<Vector3>();
    protected List<int> triangles = new List<int>();
    protected List<Color> colors = new List<Color>();
    protected List<Vector2> uvs = new List<Vector2>();
    protected List<Vector2> uvs2 = new List<Vector2>();
    protected BlockData[,,] pointmap;

    System.Random rnd = new System.Random(System.DateTime.Now.Millisecond);


    public enum FaceDirection {
        xp, xm, yp, ym, zp, zm
    }
    public enum MeshDataType {
        TERRAIN, TREE, WATER
    }
    protected MeshDataType meshDataType;

    /// <summary>
    /// NB! Not thread safe! Do not call from threads other then the main thread.
    /// Generates a mesh from MeshData.
    /// </summary>
    /// <param name="md">MeshData</param>
    /// <returns>Mesh</returns>
    public static Mesh applyMeshData(MeshData md) {
        Mesh mesh = new Mesh();
        mesh.vertices = md.vertices;
        mesh.triangles = md.triangles;
        mesh.colors = md.colors;
        mesh.uv = md.uvs;
        mesh.uv2 = md.uvs2;
        mesh.RecalculateNormals(); //Normals could be provided by MeshData instead, to save mainthread cpu time.
        return mesh;
    }

    /// <summary>
    /// Generates all data needed for a mesh of cubes
    /// </summary>
    /// <param name="pointmap">Point data used to build the mesh.
    /// The outermost layer (in x and z) is used to decide whether to add faces on the cubes on the second outermost layer (in x and z).</param>
    /// <returns>a mesh made from the input data</returns>
    public static MeshData GenerateMeshData(BlockData[,,] pointmap, float voxelSize = 1f, Vector3 offset = default(Vector3), MeshDataType meshDataType = MeshDataType.TERRAIN) {
        MeshDataGenerator MDG = new MeshDataGenerator();
        MDG.meshDataType = meshDataType;

        MDG.pointmap = pointmap;

        for (int x = 1; x < pointmap.GetLength(0) - 1; x++) {
            for (int y = 0; y < pointmap.GetLength(1); y++) {
                for (int z = 1; z < pointmap.GetLength(2) - 1; z++) {
                    if (pointmap[x, y, z].blockType != BlockData.BlockType.NONE && pointmap[x, y, z].blockType != BlockData.BlockType.WATER)
                        MDG.GenerateCube(new Vector3Int(x, y, z), offset, pointmap[x, y, z], voxelSize);
                }
            }
        }

        var meshData = new MeshData();
        meshData.vertices = MDG.vertices.ToArray();
        meshData.triangles = MDG.triangles.ToArray();
        meshData.colors = MDG.colors.ToArray();
        meshData.uvs = MDG.uvs.ToArray();
        meshData.uvs2 = MDG.uvs2.ToArray();

        return meshData;
    }

    /// <summary>
    /// Generates the mesh data for a cube
    /// </summary>
    /// <param name="cubePos">point position of the cube</param>
    /// <param name="blockData">data on the block</param>
    private void GenerateCube(Vector3Int cubePos, Vector3 offset, BlockData blockData, float voxelSize) {
        if (cubePos.x != pointmap.GetLength(0) - 1 && checkIfSolidVoxel(cubePos + new Vector3Int(1, 0, 0)) == false) GenerateCubeFace(FaceDirection.xp, cubePos - offset, blockData, voxelSize);
        if (cubePos.y == pointmap.GetLength(1) - 1 || checkIfSolidVoxel(cubePos + new Vector3Int(0, 1, 0)) == false) GenerateCubeFace(FaceDirection.yp, cubePos - offset, blockData, voxelSize); // Obs. On Y up we also want a face even if it is the outermost layer
        if (cubePos.z != pointmap.GetLength(2) - 1 && checkIfSolidVoxel(cubePos + new Vector3Int(0, 0, 1)) == false) GenerateCubeFace(FaceDirection.zp, cubePos - offset, blockData, voxelSize);
        if (cubePos.x != 0 && checkIfSolidVoxel(cubePos + new Vector3Int(-1, 0, 0)) == false) GenerateCubeFace(FaceDirection.xm, cubePos - offset, blockData, voxelSize);
        if (cubePos.y != 0 && checkIfSolidVoxel(cubePos + new Vector3Int(0, -1, 0)) == false) GenerateCubeFace(FaceDirection.ym, cubePos - offset, blockData, voxelSize);
        if (cubePos.z != 0 && checkIfSolidVoxel(cubePos + new Vector3Int(0, 0, -1)) == false) GenerateCubeFace(FaceDirection.zm, cubePos - offset, blockData, voxelSize);


        //if (cubePos.x != pointmap.GetLength(0) - 1 && pointmap[(int)cubePos.x + 1, (int)cubePos.y, (int)cubePos.z].blockType == 0) GenerateCubeFace(FaceDirection.xp, cubePos - offset, blockData, voxelSize);
        //if (cubePos.y == pointmap.GetLength(1) - 1 || pointmap[(int)cubePos.x, (int)cubePos.y + 1, (int)cubePos.z].blockType == 0) GenerateCubeFace(FaceDirection.yp, cubePos - offset, blockData, voxelSize); // Obs. On Y up we also want a face even if it is the outermost layer
        //if (cubePos.z != pointmap.GetLength(2) - 1 && pointmap[(int)cubePos.x, (int)cubePos.y, (int)cubePos.z + 1].blockType == 0) GenerateCubeFace(FaceDirection.zp, cubePos - offset, blockData, voxelSize);
        //if (cubePos.x != 0 && pointmap[(int)cubePos.x - 1, (int)cubePos.y, (int)cubePos.z].blockType == 0) GenerateCubeFace(FaceDirection.xm, cubePos - offset, blockData, voxelSize);
        //if (cubePos.y != 0 && pointmap[(int)cubePos.x, (int)cubePos.y - 1, (int)cubePos.z].blockType == 0) GenerateCubeFace(FaceDirection.ym, cubePos - offset, blockData, voxelSize);
        //if (cubePos.z != 0 && pointmap[(int)cubePos.x, (int)cubePos.y, (int)cubePos.z - 1].blockType == 0) GenerateCubeFace(FaceDirection.zm, cubePos - offset, blockData, voxelSize);
    }

    protected bool checkIfSolidVoxel(Vector3Int voxelPos) {
        if (pointmap[voxelPos.x, voxelPos.y, voxelPos.z].blockType == BlockData.BlockType.NONE ||
            pointmap[voxelPos.x, voxelPos.y, voxelPos.z].blockType == BlockData.BlockType.WATER)
            return false;
        return true;
    }


    /// <summary>
    /// Generates the mesh data for a face of a cube
    /// </summary>
    /// <param name="dir">direction of face</param>
    /// <param name="pointPos">point position of the cube</param>
    /// <param name="cubetype">what type of cube it is, used to color the cube</param>
    protected void GenerateCubeFace(FaceDirection dir, Vector3 pointPos, BlockData blockData, float voxelSize) {
        int vertIndex = vertices.Count;

        float delta = voxelSize / 2f;
        pointPos = pointPos * voxelSize;


        switch (dir) {
            case FaceDirection.xp:
                vertices.AddRange(new Vector3[]{pointPos + new Vector3(delta, -delta, -delta),
                                                pointPos + new Vector3(delta,  delta, -delta),
                                                pointPos + new Vector3(delta, -delta,  delta),
                                                pointPos + new Vector3(delta,  delta,  delta)});
                break;
            case FaceDirection.xm:
                vertices.AddRange(new Vector3[]{pointPos + new Vector3(-delta, -delta, -delta),
                                                pointPos + new Vector3(-delta, -delta,  delta),
                                                pointPos + new Vector3(-delta,  delta, -delta),
                                                pointPos + new Vector3(-delta,  delta,  delta)});
                break;
            case FaceDirection.yp:
                vertices.AddRange(new Vector3[]{pointPos + new Vector3(-delta, delta, -delta),
                                                pointPos + new Vector3(-delta, delta,  delta),
                                                pointPos + new Vector3(delta,  delta, -delta),
                                                pointPos + new Vector3(delta,  delta,  delta)});
                break;
            case FaceDirection.ym:
                vertices.AddRange(new Vector3[]{pointPos + new Vector3(-delta, -delta, -delta),
                                                pointPos + new Vector3(delta,  -delta, -delta),
                                                pointPos + new Vector3(-delta, -delta,  delta),
                                                pointPos + new Vector3(delta,  -delta,  delta)});
                break;
            case FaceDirection.zp:
                vertices.AddRange(new Vector3[]{pointPos + new Vector3(-delta, -delta, delta),
                                                pointPos + new Vector3(delta,  -delta, delta),
                                                pointPos + new Vector3(-delta,  delta, delta),
                                                pointPos + new Vector3(delta,   delta, delta)});
                break;
            case FaceDirection.zm:
                vertices.AddRange(new Vector3[]{pointPos + new Vector3(-delta, -delta, -delta),
                                                pointPos + new Vector3(-delta,  delta, -delta),
                                                pointPos + new Vector3(delta,  -delta, -delta),
                                                pointPos + new Vector3(delta,   delta, -delta)});
                break;
        }
        triangles.AddRange(new int[] { vertIndex, vertIndex + 1, vertIndex + 2 });
        triangles.AddRange(new int[] { vertIndex + 2, vertIndex + 1, vertIndex + 3 });

        addTextureCoordinates(blockData, dir);
        addSliceData(blockData, dir);

    }

    /// <summary>
    /// Stores the indices of the texture slices to use for a face of a block.
    /// </summary>
    /// <param name="blockData">Data of the block</param>
    /// <param name="faceDir">Direction of the face</param>
    protected void addSliceData(BlockData blockData, FaceDirection faceDir) {
        TextureData.TextureType[] texTypes = new TextureData.TextureType[2];

        // Get texture types for base and modifier
        for (int i = 0; i < 2; i++) {
            BlockData.BlockType blockType = (i == 0 ? blockData.blockType : blockData.modifier);

            // Convert block type to texture type:
            string typeName = blockType.ToString();
            if(blockType == BlockData.BlockType.GRASS || blockType == BlockData.BlockType.SNOW) {
                if (faceDir == FaceDirection.yp)
                    typeName += "_TOP";
                else if (faceDir == FaceDirection.ym)
                    typeName = "NONE";
                else
                    typeName += "_SIDE";
            }
            texTypes[i] = (TextureData.TextureType)Enum.Parse(typeof(TextureData.TextureType), typeName);

        }




        // Get a slice from the textureType list of choice:
        int slice;
        int modSlice;
        //if (meshDataType == MeshDataType.TERRAIN) {
            slice = terrainTextureTypes[(int)texTypes[0]][rnd.Next(0, terrainTextureTypes[(int)texTypes[0]].Count)];
            modSlice = terrainTextureTypes[(int)texTypes[1]][rnd.Next(0, terrainTextureTypes[(int)texTypes[1]].Count)];
        /*} else if (meshDataType == MeshDataType.TREE) {
            slice = treeTextureTypes[(int)texTypes[0]][rnd.Next(0, treeTextureTypes[(int)texTypes[0]].Count)];
            modSlice = treeTextureTypes[(int)texTypes[1]][rnd.Next(0, treeTextureTypes[(int)texTypes[1]].Count)];
        } else {
            Debug.Log("MeshDataGenerator.meshDataType not set.");
            slice = 0;
            modSlice = 0;
        }*/

        for (int i = 0; i < 4; i++)
            colors.Add(new Color(slice, modSlice, 0));                  // Because Unity does not have an official way of sending 
                                                                        //  the slice info to the shader we store it in the colour.
                                                                        //  Basetype in red channel and modifier in green channel.
                                                                        // If we ever need to use the color channels for something
                                                                        //  more important, this could probably be sent via uv2 instead.
    }

    /// <summary>
    /// Adds texture coordinates for a face of a block.
    /// </summary>
    /// <param name="blockData">Data of the block</param>
    /// <param name="faceDir">Direction of the face</param>
    /// <param name="isBaseType">Whether it is the base block type, if false it is a modifier type</param>
    protected void addTextureCoordinates(BlockData blockData, FaceDirection faceDir) {

        Vector2[] coords = new Vector2[]{
            new Vector2(0, 0), new Vector2(0, 1),
            new Vector2(1, 0), new Vector2(1, 1)
        };

        int[,] rotations = new int[,] {
            { 0, 1, 2, 3 },
            { 2, 0, 3, 1 },
            { 3, 2, 1, 0 },
            { 1, 3, 0, 2 }
        };

        // Select a rotation for the texture
        int rotation;

        switch (faceDir) {
            case FaceDirection.xp:
            case FaceDirection.zm:
                rotation = 0;
                break;
            case FaceDirection.xm:
            case FaceDirection.zp:
                rotation = 1;
                break;
            default: // yp & ym
                rotation = 2;
                break;
        }

        for (int i = 0; i < 4; i++) {
            uvs.Add(coords[rotations[rotation, i]]);
        }

    }

}