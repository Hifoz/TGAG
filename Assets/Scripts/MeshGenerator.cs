using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// A Voxel Mesh generator 
/// </summary>
public class MeshGenerator {
    private List<Vector3> vertices;
    private List<int> triangles;
    private List<Color> colors;
    private List<Vector2> uvs;
    private Mesh mesh;
    private int[,,] pointmap;
    public static Texture textureMap;

    public enum FaceDirection {
        xp, xm, yp, ym, zp, zm
    }


    /// <summary>
    /// Initialiizes the contents of the generator
    /// </summary>
    private void Initialize() {
        mesh = new Mesh();
        vertices = new List<Vector3>();
        triangles = new List<int>();
        colors = new List<Color>();
        uvs = new List<Vector2>();
    }

    /// <summary>
    /// Generates a mesh of cubes
    /// </summary>
    /// <param name="pointmap">data used to build cubes</param>
    /// <returns>a mesh made from the input data</returns>
    public static Mesh GenerateMesh(int[,,] pointmap) {
        MeshGenerator vmg = new MeshGenerator(); // Hack to make interface towards mesh generator static. TODO: Do this in a not so dirty way.
        vmg.Initialize();

        vmg.pointmap = pointmap;
        for (int x = 0; x < pointmap.GetLength(0); x++) {
            for (int y = 0; y < pointmap.GetLength(1); y++) {
                for (int z = 0; z < pointmap.GetLength(2); z++) {
                    if (pointmap[x, y, z] != 0)
                        vmg.GenerateCube(new Vector3(x, y, z), pointmap[x, y, z]);
                }
            }
        }


        vmg.Recalculate();

        return vmg.mesh;
    }



    /// <summary>
    /// Generates the mesh data for a cube
    /// </summary>
    /// <param name="cubePos">point position of the cube</param>
    /// <param name="cubetype">what type of cube it is</param>
    private void GenerateCube(Vector3 cubePos, int cubetype) {
        if (cubePos.x == pointmap.GetLength(0) - 1 || pointmap[(int)cubePos.x + 1, (int)cubePos.y, (int)cubePos.z] == 0) GenerateCubeFace(FaceDirection.xp, cubePos, cubetype);
        if (cubePos.y == pointmap.GetLength(1) - 1 || pointmap[(int)cubePos.x, (int)cubePos.y + 1, (int)cubePos.z] == 0) GenerateCubeFace(FaceDirection.yp, cubePos, cubetype);
        if (cubePos.z == pointmap.GetLength(2) - 1 || pointmap[(int)cubePos.x, (int)cubePos.y, (int)cubePos.z + 1] == 0) GenerateCubeFace(FaceDirection.zp, cubePos, cubetype);
        if (cubePos.x == 0 || pointmap[(int)cubePos.x - 1, (int)cubePos.y, (int)cubePos.z] == 0) GenerateCubeFace(FaceDirection.xm, cubePos, cubetype);
        if (cubePos.y == 0 || pointmap[(int)cubePos.x, (int)cubePos.y - 1, (int)cubePos.z] == 0) GenerateCubeFace(FaceDirection.ym, cubePos, cubetype);
        if (cubePos.z == 0 || pointmap[(int)cubePos.x, (int)cubePos.y, (int)cubePos.z - 1] == 0) GenerateCubeFace(FaceDirection.zm, cubePos, cubetype);
    }


    /// <summary>
    /// Generates the mesh data for a face of a cube
    /// </summary>
    /// <param name="dir">direction of face</param>
    /// <param name="pointPos">point position of the cube</param>
    /// <param name="cubetype">what type of cube it is, used to color the cube</param>
    private void GenerateCubeFace(FaceDirection dir, Vector3 pointPos, int cubetype) {
        int vertIndex = vertices.Count;
        int xOffset = 0;
        int yOffset = 1;

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
                yOffset = 2;
                break;
            case FaceDirection.ym:
                vertices.AddRange(new Vector3[]{pointPos + new Vector3(-0.5f, -0.5f, -0.5f),
                                                pointPos + new Vector3(0.5f,  -0.5f, -0.5f),
                                                pointPos + new Vector3(-0.5f, -0.5f,  0.5f),
                                                pointPos + new Vector3(0.5f,  -0.5f,  0.5f)});
                yOffset = 0;
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

        AddTextureCoordinates(xOffset, yOffset, dir);

    }

    /// <summary>
    /// Adds texture coordinates for a cube face
    /// </summary>
    /// <param name="xOffset">Decided by the type of cube</param>
    /// <param name="yOffset">Decided by the  direction of the face</param>
    private void AddTextureCoordinates(float xOffset, float yOffset, FaceDirection dir) {
        int textureSize = 512;
        int numberOfTextures = Resources.Load<Texture>("Textures/terrainTextures").width / textureSize;
        float padding = 20;


        xOffset %= numberOfTextures;
        xOffset /= numberOfTextures;
        float xOffsetO = xOffset + 1f/numberOfTextures;

        yOffset /= 3;
        float yOffsetO = yOffset + 1f/3;

        int texturemapwidth = numberOfTextures * textureSize;
        float paddingx = padding / texturemapwidth;
        float paddingy = padding / (textureSize * 3);


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
            { 1, 3, 0, 2 }};


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
            default:
                // Select random rotation for top and bottom faces
                rotation = UnityEngine.Random.Range(0, 4);
                break;
        }

        for(int i = 0; i < 4; i++) {
            uvs.Add(coords[rotations[rotation, i]]);
        }

        
    }



    /// <summary>
    /// Clears the mesh
    /// </summary>
    private void ClearMesh() {
        vertices.Clear();
        triangles.Clear();
        colors.Clear();
        uvs.Clear();
        mesh.Clear();
    }


    /// <summary>
    /// Recalculates the mesh
    /// </summary>
    private void Recalculate() {
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.colors = colors.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
    }

}