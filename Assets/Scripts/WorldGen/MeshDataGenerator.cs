using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// Contains any extra/optional data that can be used to generate a mesh.
/// </summary>
public class MeshDataExtras {
    public Vector2 animalData = Vector2.negativeInfinity; // Used for populating the uvs if meshDataType == ANIMAL
}


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

    public enum FaceDirection {
        xp, xm, yp, ym, zp, zm
    }
    public enum MeshDataType {
        TERRAIN, WATER, ANIMAL
    }
    public enum GeneratorMode {
        CULL, GREEDY
    };



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

    #region meshdata generation

    /// <summary>
    /// Generates all data needed for a mesh of cubes
    /// </summary>
    /// <param name="pointmap">Point data used to build the mesh.
    /// The outermost layer (in x and z) is used to decide whether to add faces on the cubes on the second outermost layer (in x and z).</param>
    /// <returns>an array of meshdata objects made from input data</returns>
    public static MeshData[] GenerateMeshData(BlockDataMap pointmap, float voxelSize = 1f, Vector3 offset = default(Vector3), 
                                              MeshDataType meshDataType = MeshDataType.TERRAIN) {
        if(mode == GeneratorMode.GREEDY && meshDataType != MeshDataType.ANIMAL) { // Cannot use greedy generator with animals meshes because of movement
            MeshDataExtras extras = new MeshDataExtras {
                animalData = new Vector2(rng.randomFloat(0.2f, 0.8f), rng.randomFloat(0.0f, 1.0f))
            };
            GreedyMeshDataGenerator gmg = new GreedyMeshDataGenerator(pointmap, voxelSize, offset, meshDataType, extras);
            return gmg.generateMeshData();
        }

        // TODO : Move meshdata generation code in this class into a CullingMeshDataGenerator class.

        MeshDataGenerator MDG = new MeshDataGenerator();
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


    /// <summary>
    /// Generates the mesh data for a cube
    /// </summary>
    /// <param name="cubePos">point position of the cube</param>
    /// <param name="blockData">data on the block</param>
    private void GenerateCube(Vector3Int cubePos, BlockData blockData, float voxelSize) {
        if (cubePos.x != pointmap.GetLength(0) - 1 && checkVoxel(cubePos + new Vector3Int(1, 0, 0)) == false) GenerateCubeFace(FaceDirection.xp, cubePos, blockData, voxelSize);
        if (cubePos.y == pointmap.GetLength(1) - 1 || checkVoxel(cubePos + new Vector3Int(0, 1, 0)) == false) GenerateCubeFace(FaceDirection.yp, cubePos, blockData, voxelSize); // Obs. On Y up we also want a face even if it is the outermost layer
        if (cubePos.z != pointmap.GetLength(2) - 1 && checkVoxel(cubePos + new Vector3Int(0, 0, 1)) == false) GenerateCubeFace(FaceDirection.zp, cubePos, blockData, voxelSize);
        if (cubePos.x != 0 && checkVoxel(cubePos + new Vector3Int(-1, 0, 0)) == false) GenerateCubeFace(FaceDirection.xm, cubePos, blockData, voxelSize);
        if (cubePos.y != 0 && checkVoxel(cubePos + new Vector3Int(0, -1, 0)) == false) GenerateCubeFace(FaceDirection.ym, cubePos, blockData, voxelSize);
        if (cubePos.z != 0 && checkVoxel(cubePos + new Vector3Int(0, 0, -1)) == false) GenerateCubeFace(FaceDirection.zm, cubePos, blockData, voxelSize);
    }

    /// <summary>
    /// Checks the block type
    /// </summary>
    /// <param name="voxelPos">position of voxel</param>
    /// <returns>
    ///     If meshDataType == TERRAIN: Whether the voxel is opaque.
    ///     If meshDataType == WATER: Whether the voxel is something other than water.
    /// </returns>
    protected bool checkVoxel(Vector3Int voxelPos) {
        switch(meshDataType) {
            case MeshDataType.WATER:
                return pointmap.mapdata[pointmap.index1D(voxelPos.x, voxelPos.y, voxelPos.z)].blockType == BlockData.BlockType.WATER;
            default: // MeshDataType.TERRAIN
                return !(pointmap.mapdata[pointmap.index1D(voxelPos.x, voxelPos.y, voxelPos.z)].blockType == BlockData.BlockType.NONE || 
                         pointmap.mapdata[pointmap.index1D(voxelPos.x, voxelPos.y, voxelPos.z)].blockType == BlockData.BlockType.WATER);
        }
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
        pointPos = (pointPos - offset) * voxelSize;

        Vector3 normalDir = Vector3.zero;
        switch (dir) {
            case FaceDirection.xp:
                vertices.AddRange(new Vector3[]{pointPos + new Vector3(delta, -delta, -delta),
                                                pointPos + new Vector3(delta,  delta, -delta),
                                                pointPos + new Vector3(delta, -delta,  delta),
                                                pointPos + new Vector3(delta,  delta,  delta)});
                normalDir = new Vector3(1, 0, 0);
                break;
            case FaceDirection.xm:
                vertices.AddRange(new Vector3[]{pointPos + new Vector3(-delta, -delta, -delta),
                                                pointPos + new Vector3(-delta, -delta,  delta),
                                                pointPos + new Vector3(-delta,  delta, -delta),
                                                pointPos + new Vector3(-delta,  delta,  delta)});
                normalDir = new Vector3(-1, 0, 0);
                break;
            case FaceDirection.yp:
                vertices.AddRange(new Vector3[]{pointPos + new Vector3(-delta, delta, -delta),
                                                pointPos + new Vector3(-delta, delta,  delta),
                                                pointPos + new Vector3(delta,  delta, -delta),
                                                pointPos + new Vector3(delta,  delta,  delta)});
                normalDir = new Vector3(0, 1, 0);
                break;
            case FaceDirection.ym:
                vertices.AddRange(new Vector3[]{pointPos + new Vector3(-delta, -delta, -delta),
                                                pointPos + new Vector3(delta,  -delta, -delta),
                                                pointPos + new Vector3(-delta, -delta,  delta),
                                                pointPos + new Vector3(delta,  -delta,  delta)});
                normalDir = new Vector3(0, -1, 0);
                break;
            case FaceDirection.zp:
                vertices.AddRange(new Vector3[]{pointPos + new Vector3(-delta, -delta, delta),
                                                pointPos + new Vector3(delta,  -delta, delta),
                                                pointPos + new Vector3(-delta,  delta, delta),
                                                pointPos + new Vector3(delta,   delta, delta)});
                normalDir = new Vector3(0, 0, 1);
                break;
            case FaceDirection.zm:
                vertices.AddRange(new Vector3[]{pointPos + new Vector3(-delta, -delta, -delta),
                                                pointPos + new Vector3(-delta,  delta, -delta),
                                                pointPos + new Vector3(delta,  -delta, -delta),
                                                pointPos + new Vector3(delta,   delta, -delta)});
                normalDir = new Vector3(0, 0, -1);
                break;
        }
        normals.AddRange(new Vector3[] { normalDir, normalDir, normalDir, normalDir });

        triangles.AddRange(new int[] { vertIndex, vertIndex + 1, vertIndex + 2 });
        triangles.AddRange(new int[] { vertIndex + 2, vertIndex + 1, vertIndex + 3 });

        
        if (meshDataType == MeshDataType.ANIMAL) {
            addSliceData(vertices.GetRange(vertices.Count - 4, 4));
        } else {
            addTextureCoordinates(blockData, dir);
            addSliceData(blockData, dir);
        }

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

        for (int i = 0; i < 4; i++)
            colors.Add(new Color((int)texTypes[0], (int)texTypes[1], 0)); // Using the color to store the texture type of the vertices
    }

    /// <summary>
    /// Encodes colors as unique positions used for noise calculations in animal shader
    /// </summary>
    /// <param name="verticies">Verticies to encode into colors</param>
    protected void addSliceData(List<Vector3> verts) {
        Vector3 scalingVector = new Vector3(pointmap.GetLength(0), pointmap.GetLength(1), pointmap.GetLength(2));
        foreach (Vector3 vert in verts) {
            colors.Add(new Color(
                    vert.x / scalingVector.x,
                    vert.y / scalingVector.y,
                    vert.z / scalingVector.z
                )
            );
            uvs.Add(animalData);
        }
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

    #endregion
}