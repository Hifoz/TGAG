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
    private Mesh mesh;
    private int[,,] pointmap;

    public enum FaceDirection
    {
        xp, xm, yp, ym, zp, zm
    }


    /// <summary>
    /// Initialiizes the contents of the generator
    /// </summary>
    private void Initialize()
    {
        mesh = new Mesh();
        vertices = new List<Vector3>();
        triangles = new List<int>();
        colors = new List<Color>();
    }

    /// <summary>
    /// Generates a mesh of cubes
    /// </summary>
    /// <param name="pointmap">data used to build cubes</param>
    /// <returns>a mesh made from the input data</returns>
    public static Mesh GenerateMesh(int[,,] pointmap)
    {
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
    private void GenerateCube(Vector3 cubePos, int cubetype)
    {
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
    private void GenerateCubeFace(FaceDirection dir, Vector3 pointPos, int cubetype)
    {
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


        switch (cubetype) {
            case 1:
                colors.AddRange(Enumerable.Repeat(Color.white, 4).ToArray());
                break;
            case 2:
                colors.AddRange(Enumerable.Repeat(Color.gray, 4).ToArray());
                break;
        }
    }




    /// <summary>
    /// Clears the mesh
    /// </summary>
    private void ClearMesh()
    {
        vertices.Clear();
        triangles.Clear();
        colors.Clear();
        mesh.Clear();
    }


    /// <summary>
    /// Recalculates the mesh
    /// </summary>
    private void Recalculate()
    {
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.colors = colors.ToArray();
        mesh.RecalculateNormals();
    }

}