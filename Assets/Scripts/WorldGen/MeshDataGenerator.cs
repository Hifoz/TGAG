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
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Color> colors = new List<Color>();
    private List<Vector2> uvs = new List<Vector2>();
    private List<Vector2> uvs2 = new List<Vector2>();
    private BlockData[,,] pointmap;


    // private static System.Random rng = new System.Random(); // Used to randomize orientation of textures

    public enum FaceDirection {
        xp, xm, yp, ym, zp, zm
    }


    /// <summary>
    /// Generates all data needed for a mesh of cubes
    /// </summary>
    /// <param name="pointmap">Point data used to build the mesh.
    /// The outermost layer (in x and z) is used to decide whether to add faces on the cubes on the second outermost layer (in x and z).</param>
    /// <returns>a mesh made from the input data</returns>
    public static MeshData GenerateMeshData(BlockData[,,] pointmap) {
        MeshDataGenerator MDG = new MeshDataGenerator();

        MDG.pointmap = pointmap;
        for (int x = 1; x < pointmap.GetLength(0) - 1; x++) {
            for (int y = 0; y < pointmap.GetLength(1); y++) {
                for (int z = 1; z < pointmap.GetLength(2) - 1; z++) {
                    if (pointmap[x, y, z].blockType != 0)
                        MDG.GenerateCube(new Vector3(x, y, z), pointmap[x, y, z]);
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
    private void GenerateCube(Vector3 cubePos, BlockData blockData) {
        if (cubePos.x != pointmap.GetLength(0) - 1 && pointmap[(int)cubePos.x + 1, (int)cubePos.y, (int)cubePos.z].blockType == 0) GenerateCubeFace(FaceDirection.xp, cubePos, blockData);
        if (cubePos.y == pointmap.GetLength(1) - 1 || pointmap[(int)cubePos.x, (int)cubePos.y + 1, (int)cubePos.z].blockType == 0) GenerateCubeFace(FaceDirection.yp, cubePos, blockData); // Obs. On y up we also want a face even if it is the outermost layer
        if (cubePos.z != pointmap.GetLength(2) - 1 && pointmap[(int)cubePos.x, (int)cubePos.y, (int)cubePos.z + 1].blockType == 0) GenerateCubeFace(FaceDirection.zp, cubePos, blockData);
        if (cubePos.x != 0 && pointmap[(int)cubePos.x - 1, (int)cubePos.y, (int)cubePos.z].blockType == 0) GenerateCubeFace(FaceDirection.xm, cubePos, blockData);
        if (cubePos.y != 0 && pointmap[(int)cubePos.x, (int)cubePos.y - 1, (int)cubePos.z].blockType == 0) GenerateCubeFace(FaceDirection.ym, cubePos, blockData);
        if (cubePos.z != 0 && pointmap[(int)cubePos.x, (int)cubePos.y, (int)cubePos.z - 1].blockType == 0) GenerateCubeFace(FaceDirection.zm, cubePos, blockData);
    }


    /// <summary>
    /// Generates the mesh data for a face of a cube
    /// </summary>
    /// <param name="dir">direction of face</param>
    /// <param name="pointPos">point position of the cube</param>
    /// <param name="cubetype">what type of cube it is, used to color the cube</param>
    private void GenerateCubeFace(FaceDirection dir, Vector3 pointPos, BlockData blockData) {
        int vertIndex = vertices.Count;

        switch (dir) {
            case FaceDirection.xp:
                vertices.AddRange(new Vector3[]{pointPos + new Vector3(0.5f, -0.5f, -0.5f),
                                                pointPos + new Vector3(0.5f,  0.5f, -0.5f),
                                                pointPos + new Vector3(0.5f, -0.5f,  0.5f),
                                                pointPos + new Vector3(0.5f,  0.5f,  0.5f)});
                break;
            case FaceDirection.xm:
                vertices.AddRange(new Vector3[]{pointPos + new Vector3(-0.5f, -0.5f, -0.5f),
                                                pointPos + new Vector3(-0.5f, -0.5f,  0.5f),
                                                pointPos + new Vector3(-0.5f,  0.5f, -0.5f),
                                                pointPos + new Vector3(-0.5f,  0.5f,  0.5f)});
                break;
            case FaceDirection.yp:
                vertices.AddRange(new Vector3[]{pointPos + new Vector3(-0.5f, 0.5f, -0.5f),
                                                pointPos + new Vector3(-0.5f, 0.5f,  0.5f),
                                                pointPos + new Vector3(0.5f,  0.5f, -0.5f),
                                                pointPos + new Vector3(0.5f,  0.5f,  0.5f)});
                break;
            case FaceDirection.ym:
                vertices.AddRange(new Vector3[]{pointPos + new Vector3(-0.5f, -0.5f, -0.5f),
                                                pointPos + new Vector3(0.5f,  -0.5f, -0.5f),
                                                pointPos + new Vector3(-0.5f, -0.5f,  0.5f),
                                                pointPos + new Vector3(0.5f,  -0.5f,  0.5f)});
                break;
            case FaceDirection.zp:
                vertices.AddRange(new Vector3[]{pointPos + new Vector3(-0.5f, -0.5f, 0.5f),
                                                pointPos + new Vector3(0.5f,  -0.5f, 0.5f),
                                                pointPos + new Vector3(-0.5f,  0.5f, 0.5f),
                                                pointPos + new Vector3(0.5f,   0.5f, 0.5f)});
                break;
            case FaceDirection.zm:
                vertices.AddRange(new Vector3[]{pointPos + new Vector3(-0.5f, -0.5f, -0.5f),
                                                pointPos + new Vector3(-0.5f,  0.5f, -0.5f),
                                                pointPos + new Vector3(0.5f,  -0.5f, -0.5f),
                                                pointPos + new Vector3(0.5f,   0.5f, -0.5f)});
                break;
        }
        triangles.AddRange(new int[] { vertIndex, vertIndex + 1, vertIndex + 2 });
        triangles.AddRange(new int[] { vertIndex + 2, vertIndex + 1, vertIndex + 3 });





        AddTextureStuff(blockData, dir, true);
    }



    private void AddTextureStuff(BlockData blockData, FaceDirection faceDir, bool isBaseType) {
        float slice = (int)blockData.blockType - 1;// The slice is the index of the texture in the Texture2dArray we want to set the face to

        for(int i = 0; i < 4; i++)
            colors.Add(new Color(slice/255f, 0, 0)); // Because Unity does not have an official way of sending the slice info to the shader, we store it in the colour, in the red channel


        Vector2[] coords = new Vector2[]{
            new Vector2(0, 0),
            new Vector2(0, 1),
            new Vector2(1, 0),
            new Vector2(1, 1)
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

    /// <summary>
    /// Adds texture coordinates for a cube face
    /// </summary>
    /// <param name="xOffset">Decided by the cubetype</param>
    /// <param name="yOffset">Decided by the direction of the face</param>
    /// <param name="dir">Direction of the face</param>
    /// <param name="isBaseType">Whether it is the base block type, if false it is a modifier type</param>
    private void AddTextureCoordinates(float xOffset, float yOffset, FaceDirection dir, bool isBaseType) {


        int blockTextureSize = 512;
        int numberOfTextures = isBaseType ? 3 : 2;

        float padding = 20;
        
        xOffset /= numberOfTextures;
        float xOffsetO = xOffset + 1f/numberOfTextures;

        yOffset /= 3;
        float yOffsetO = yOffset + 1f/3;

        int texturemapwidth = numberOfTextures * blockTextureSize;
        float paddingx = padding / texturemapwidth;
        float paddingy = padding / (blockTextureSize * 3);


        Vector2[] coords = new Vector2[]{
            new Vector2(xOffset  + paddingx, yOffset  + paddingy),
            new Vector2(xOffset  + paddingx, yOffsetO - paddingy),
            new Vector2(xOffsetO - paddingx, yOffset  + paddingy),
            new Vector2(xOffsetO - paddingx, yOffsetO - paddingy)
        };

        int[,] rotations = new int[,] {
            { 0, 1, 2, 3 },
            { 2, 0, 3, 1 },
            { 3, 2, 1, 0 },
            { 1, 3, 0, 2 }
        };


        // Select a rotation for the texture
        int rotation;

            switch (dir) {
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


        if (isBaseType) {
            for(int i = 0; i < 4; i++) {
                uvs.Add(coords[rotations[rotation, i]]);
            }
        }
        else {
            for (int i = 0; i < 4; i++) {
                uvs2.Add(coords[rotations[rotation, i]]);
            }
        }

    }

}