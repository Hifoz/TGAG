using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// A Voxel Mesh generator 
/// </summary>
public class MeshDataGenerator {
    public static GeneratorMode mode = GeneratorMode.GREEDY;
    protected MeshDataType meshDataType;

    protected List<Vector3> vertices = new List<Vector3>();
    protected List<Vector3> normals = new List<Vector3>();
    protected List<int> triangles = new List<int>();
    protected List<Color> colors = new List<Color>();
    protected List<Vector2> uvs = new List<Vector2>();
    protected Vector2 animalData; //This vector will populate the animal UV, contains data used for noise seed and animal skin type.
    protected BlockDataMap pointmap;

    protected Vector3 offset;

    private static ThreadSafeRng rng = new ThreadSafeRng(); //Point of having it static is so that different threads produce different results.


    public enum MeshDataType {
        TERRAIN, WATER, ANIMAL
    }
    public enum GeneratorMode {
        CULL, GREEDY
    };
    
    #region meshdata generation

    /// <summary>
    /// Generates all data needed for a mesh of cubes
    /// </summary>
    /// <param name="pointmap">Point data used to build the mesh.
    /// The outermost layer (in x and z) is used to decide whether to add faces on the cubes on the second outermost layer (in x and z).</param>
    /// <returns>an array of meshdata objects made from input data</returns>
    public static MeshData[] GenerateMeshData(BlockDataMap pointmap, float voxelSize = 1f, Vector3 offset = default(Vector3), 
                                              MeshDataType meshDataType = MeshDataType.TERRAIN) {

        /*
        if (mode == GeneratorMode.GREEDY && meshDataType != MeshDataType.ANIMAL) { // Cannot use greedy generator with animals meshes because of movement
            GreedyMeshDataGenerator gmg = new GreedyMeshDataGenerator(pointmap, voxelSize, offset, meshDataType);
            return gmg.generateMeshData();
        }
        */


        MeshDataGenerator MDG = new MeshDataGenerator();
        NaiveMeshGenerator nmg = new NaiveMeshGenerator(pointmap, voxelSize, offset, meshDataType);
        MDG.meshDataType = meshDataType;
        MDG.offset = offset;

        MDG.pointmap = pointmap;

        if (meshDataType == MeshDataType.ANIMAL) {
            //X = frequency, Y = seed
            MDG.animalData = new Vector2(rng.randomFloat(0.2f, 0.8f), rng.randomFloat(0.0f, 1.0f));
        }

        for (int x = 1; x < pointmap.GetLength(0) - 1; x++) {
            for (int y = 0; y < pointmap.GetLength(1); y++) {
                for (int z = 1; z < pointmap.GetLength(2) - 1; z++) {
                    if ((meshDataType != MeshDataType.WATER && pointmap.mapdata[pointmap.index1D(x, y, z)].blockType != BlockData.BlockType.NONE && pointmap.mapdata[pointmap.index1D(x, y, z)].blockType != BlockData.BlockType.WATER) ||
                        (meshDataType == MeshDataType.WATER && pointmap.mapdata[pointmap.index1D(x, y, z)].blockType == BlockData.BlockType.WATER)) {
                        MDG.GenerateCube(new Vector3Int(x, y, z), pointmap.mapdata[pointmap.index1D(x, y, z)], voxelSize);
                    }
                }
            }
        }


        MeshData meshData = new MeshData();
        meshData.vertices = MDG.vertices.ToArray();
        meshData.normals = MDG.normals.ToArray();
        meshData.triangles = MDG.triangles.ToArray();
        meshData.colors = MDG.colors.ToArray();
        meshData.uvs = MDG.uvs.ToArray();
        return meshData.split();
    }


    #region mesh building

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

    #endregion
}