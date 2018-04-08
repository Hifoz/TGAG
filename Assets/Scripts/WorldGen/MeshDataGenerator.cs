using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// A Voxel Mesh generator 
/// </summary>
public class MeshDataGenerator {
    private static ThreadSafeRng seedGen = new ThreadSafeRng(); //Point of having it static is so that different threads produce different results.

    public enum MeshDataType {
        TERRAIN, // Used for terrain
        WATER,   // Used for water
        ANIMAL,  // Used for animals
        BASIC    // Used for using unity default shader
    }
    public enum GeneratorMode {
        NAIVE, GREEDY
    };

    /// <summary>
    /// Generates all data needed for a mesh of cubes
    /// </summary>
    /// <param name="pointmap">Point data used to build the mesh.
    /// The outermost layer (in x and z) is used to decide whether to add faces on the cubes on the second outermost layer (in x and z).</param>
    /// <returns>an array of meshdata objects made from input data</returns>
    public static MeshData[] GenerateMeshData(BlockDataMap pointmap, float voxelSize = 1f, Vector3 offset = default(Vector3),
                                              MeshDataType meshDataType = MeshDataType.TERRAIN, int seed = -1, GeneratorMode genMode = GeneratorMode.NAIVE) {
        if (seed == -1)
            seed = seedGen.randomInt();

        /* GREEDY DISABLED DUE TO GLITTER ARTIFACTING
        if (mode == GeneratorMode.GREEDY && meshDataType != MeshDataType.ANIMAL) { // Cannot use greedy generator with animals meshes because of movement
            GreedyMeshDataGenerator gmg = new GreedyMeshDataGenerator(pointmap, voxelSize, offset, meshDataType);
            return gmg.generateMeshData();
        }
        */

        NaiveMeshDataGenerator nmg = new NaiveMeshDataGenerator(pointmap, voxelSize, offset, meshDataType, seed);
        return nmg.generateMeshData();
    }


    /// <summary>
    /// NB! Not thread safe! Do not call from threads other then the main thread.
    /// Generates a mesh from MeshData.
    /// </summary>
    /// <param name="md">MeshData</param>
    public static void applyMeshData(MeshFilter meshFilter, MeshData md) {
        Mesh mesh = meshFilter.mesh;
        if (mesh == null) {
            mesh = new Mesh();
        } else {
            mesh.Clear();
        }
        mesh.vertices = md.vertices;
        mesh.normals = md.normals;
        mesh.triangles = md.triangles;
        mesh.colors = md.colors;
        mesh.uv = md.uvs;
        meshFilter.mesh = mesh;
    }

    /// <summary>
    /// NB! Not thread safe! Do not call from threads other then the main thread.
    /// Generates a mesh from MeshData.
    /// </summary>
    /// <param name="md">MeshData</param>
    public static void applyMeshData(MeshCollider meshCollider, MeshData md) {
        Mesh mesh = meshCollider.sharedMesh;
        if (mesh == null) {
            mesh = new Mesh();
        } else {
            mesh.Clear();
        }
        mesh.vertices = md.vertices;
        mesh.normals = md.normals;
        mesh.triangles = md.triangles;
        mesh.colors = md.colors;
        mesh.uv = md.uvs;
        meshCollider.sharedMesh = mesh;
    }

    /// <summary>
    /// NB! Not thread safe! Do not call from threads other then the main thread.
    /// Generates a mesh from MeshData.
    /// This overload can cause memory leaks, you need to delete the mesh when you are done with it (or reuse)
    /// </summary>
    /// <param name="md">MeshData</param>
    /// <returns>Mesh</returns>
    public static Mesh applyMeshData(MeshData md) {
        Mesh mesh = new Mesh();
        mesh.vertices = md.vertices;
        mesh.normals = md.normals;
        mesh.triangles = md.triangles;
        mesh.colors = md.colors;
        mesh.uv = md.uvs;
        return mesh;
    }
}