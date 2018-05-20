using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


class NaiveMeshDataGenerator {
    BlockDataMap blockDataMap;

    List<Vector3> vertices = new List<Vector3>();
    List<Vector3> normals = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Color> colors = new List<Color>();
    List<Vector2> uvs = new List<Vector2>();

    private ThreadSafeRng rng;
    private int seed;


    MeshDataGenerator.MeshDataType meshDataType;
    float voxelSize; // Is there any good reason for why voxelSize and offset is done on the meshdata 
    Vector3 offset;  //   and not just by moving and scaling the GO the mesh is attached to?

    protected Vector2 animalData; //This vector will populate the animal UV, contains data used for noise seed and animal skin type.

    public enum FaceDirection {
        xp, yp, zp, xm, ym, zm
    }


    public NaiveMeshDataGenerator(BlockDataMap blockDataMap, float voxelSize = 1f, Vector3 offset = default(Vector3),
                                  MeshDataGenerator.MeshDataType meshDataType = MeshDataGenerator.MeshDataType.TERRAIN,
                                  int seed = -1) {
        this.blockDataMap = blockDataMap;
        this.meshDataType = meshDataType;
        this.voxelSize = voxelSize;
        this.offset = offset;

        this.seed = seed;
        this.rng = new ThreadSafeRng(seed);

        if (meshDataType == MeshDataGenerator.MeshDataType.ANIMAL) {
            //X = frequency, Y = seed
            animalData = new Vector2(rng.randomFloat(0.2f, 0.8f), rng.randomFloat(0.0f, 1.0f));
        }
    }

    public MeshData[] generateMeshData() {
        for (int x = 1; x < blockDataMap.GetLength(0) - 1; x++) {
            for (int y = 0; y < blockDataMap.GetLength(1); y++) {
                for (int z = 1; z < blockDataMap.GetLength(2) - 1; z++) {
                    BlockData block = blockDataMap.mapdata[blockDataMap.index1D(x, y, z)];
                    if ((meshDataType != MeshDataGenerator.MeshDataType.WATER && block.blockType != BlockData.BlockType.NONE && block.blockType != BlockData.BlockType.WATER && block.blockType != BlockData.BlockType.WIND) ||
                        (meshDataType == MeshDataGenerator.MeshDataType.WATER && block.blockType == BlockData.BlockType.WATER)) {
                        GenerateCube(new Vector3Int(x, y, z), block, voxelSize);
                    }
                }
            }
        }


        MeshData meshData = new MeshData();
        meshData.vertices = vertices.ToArray();
        meshData.normals = normals.ToArray();
        meshData.triangles = triangles.ToArray();
        meshData.colors = colors.ToArray();
        meshData.uvs = uvs.ToArray();
        return meshData.split();
    }

    /// <summary>
    /// Generates the mesh data for a cube
    /// </summary>
    /// <param name="cubePos">point position of the cube</param>
    /// <param name="blockData">data on the block</param>
    private void GenerateCube(Vector3Int cubePos, BlockData blockData, float voxelSize) {
        if (cubePos.x != blockDataMap.GetLength(0) - 1 && checkVoxel(cubePos + new Vector3Int(1, 0, 0)) == false) GenerateCubeFace(FaceDirection.xp, cubePos, blockData, voxelSize);
        if (cubePos.y == blockDataMap.GetLength(1) - 1 || checkVoxel(cubePos + new Vector3Int(0, 1, 0)) == false) GenerateCubeFace(FaceDirection.yp, cubePos, blockData, voxelSize); // Obs. On Y up we also want a face even if it is the outermost layer
        if (cubePos.z != blockDataMap.GetLength(2) - 1 && checkVoxel(cubePos + new Vector3Int(0, 0, 1)) == false) GenerateCubeFace(FaceDirection.zp, cubePos, blockData, voxelSize);
        if (cubePos.x != 0 && checkVoxel(cubePos + new Vector3Int(-1, 0, 0)) == false) GenerateCubeFace(FaceDirection.xm, cubePos, blockData, voxelSize);
        if (cubePos.y != 0 && checkVoxel(cubePos + new Vector3Int(0, -1, 0)) == false) GenerateCubeFace(FaceDirection.ym, cubePos, blockData, voxelSize);
        if (cubePos.z != 0 && checkVoxel(cubePos + new Vector3Int(0, 0, -1)) == false) GenerateCubeFace(FaceDirection.zm, cubePos, blockData, voxelSize);
    }


    /// <summary>
    /// Checks the block type
    /// </summary>
    /// <param name="voxelPos">position of voxel</param>
    /// <returns>
    ///     If meshDataType == TERRAIN or ANIMAL: Whether the voxel is opaque.
    ///     If meshDataType == WATER: Whether the voxel is something other than water.
    /// </returns>
    protected bool checkVoxel(Vector3Int voxelPos) {
        BlockData.BlockType type = blockDataMap.mapdata[blockDataMap.index1D(voxelPos.x, voxelPos.y, voxelPos.z)].blockType;
        switch (meshDataType) {
            case MeshDataGenerator.MeshDataType.WATER:
            case MeshDataGenerator.MeshDataType.BASIC:
                return type == BlockData.BlockType.WATER;
            default:
                return !(type == BlockData.BlockType.NONE  ||
                         type == BlockData.BlockType.WATER ||
                         type == BlockData.BlockType.WIND);
        }
    }


    /// <summary>
    /// Generates the mesh data for a face of a cube
    /// </summary>
    /// <param name="faceDir">direction of face</param>
    /// <param name="pointPos">point position of the cube</param>
    /// <param name="cubetype">what type of cube it is, used to color the cube</param>
    protected void GenerateCubeFace(FaceDirection faceDir, Vector3 pointPos, BlockData blockData, float voxelSize) {
        int vertIndex = vertices.Count;

        pointPos = (pointPos - offset + (Vector3.one * 0.5f)) * voxelSize;

        int dir = (int)faceDir % 3;

        Vector3 du = Vector3.zero;
        Vector3 dv = Vector3.zero;

        du[(dir + 1) % 3] = -voxelSize; // Offset in "relative x direction" \ Relative as if you were facing down a
        dv[(dir + 2) % 3] = -voxelSize; // Offset in "relative y direction" / 2d perspective of the face direction

        Vector3[] verts = new Vector3[] {
            pointPos,
            pointPos + du,
            pointPos + du + dv,
            pointPos      + dv
        };

        if ((int)faceDir < 3) {
            vertices.AddRange(new Vector3[] { verts[0], verts[1], verts[2], verts[3] });
        } else {
            for (int i = 0; i < 4; i++) {
                verts[i][dir] -= voxelSize;
            }
            vertices.AddRange(new Vector3[] { verts[2], verts[1], verts[0], verts[3] });
        }

        Vector3 normalDir = Vector3.zero;
        normalDir[dir] = (int)faceDir < 3 ? 1 : -1;
        normals.AddRange(new Vector3[] { normalDir, normalDir, normalDir, normalDir });

        triangles.AddRange(new int[] { vertIndex, vertIndex + 1, vertIndex + 2 });
        triangles.AddRange(new int[] { vertIndex, vertIndex + 2, vertIndex + 3 });


        switch (meshDataType) {
            case MeshDataGenerator.MeshDataType.ANIMAL:
                encodePositionalData(vertices.GetRange(vertices.Count - 4, 4));
                break;
            case MeshDataGenerator.MeshDataType.TERRAIN:
                addColorDataTerrain(blockData, faceDir);
                addTextureCoordinates(blockData, faceDir);
                break;
            case MeshDataGenerator.MeshDataType.TREE:
                addColorDataTree(blockData, faceDir);
                addTextureCoordinates(blockData, faceDir);
                break;
            case MeshDataGenerator.MeshDataType.BASIC:
            case MeshDataGenerator.MeshDataType.WATER:
                addTextureCoordinates(blockData, faceDir);
                break;
        }
    }

    /// <summary>
    /// Stores color indexes in the color array.
    /// Theres an array of colors in the shader, that are indexed by the numbers in mesh.color.
    /// </summary>
    /// <param name="blockData">Data of the block</param>
    /// <param name="faceDir">Direction of the face</param>
    protected void addColorDataTerrain(BlockData blockData, FaceDirection faceDir) {
        float colorIndex1 = BlockData.blockTypeToColorIndex(blockData.blockType); //5 because COLOR_COUNT in shader is 5
        float colorIndex2 = (blockData.modifier == BlockData.BlockType.NONE) ? colorIndex1 : BlockData.blockTypeToColorIndex(blockData.modifier);

        switch (faceDir) {
            case FaceDirection.yp:
                colors.AddRange(new Color[4] {
                    new Color(colorIndex2, colorIndex2, 0, 0), new Color(colorIndex2, colorIndex2, 0, 0),
                    new Color(colorIndex2, colorIndex2, 0, 0), new Color(colorIndex2, colorIndex2, 0, 0)
                });
                break;
            case FaceDirection.ym:
                colors.AddRange(new Color[4] {
                    new Color(colorIndex1, colorIndex1, 0, 0), new Color(colorIndex1, colorIndex1, 0, 0),
                    new Color(colorIndex1, colorIndex1, 0, 0), new Color(colorIndex1, colorIndex1, 0, 0)
                });
                break;
            default:
                colors.AddRange(new Color[4] {
                    new Color(colorIndex1, colorIndex2, 0, 0), new Color(colorIndex1, colorIndex2, 0, 0),
                    new Color(colorIndex1, colorIndex2, 0, 0), new Color(colorIndex1, colorIndex2, 0, 0)
                });
                break;
        }
    }

    /// <summary>
    /// Stores color indexes in the color array.
    /// Theres an array of colors in the shader, that are indexed by the numbers in mesh.color.
    /// </summary>
    /// <param name="blockData">Data of the block</param>
    /// <param name="faceDir">Direction of the face</param>
    protected void addColorDataTree(BlockData blockData, FaceDirection faceDir) {
        const float COLOR_COUNT = 2; //Size of colors array in shader
        const float smallDelta = 0.01f; //To make the int index = colorIndex * COLOR_COUNT conversion stable
        float colorIndex = (blockData.blockType == BlockData.BlockType.WOOD) ? 0 : 1 / COLOR_COUNT + smallDelta; //5 because COLOR_COUNT in shader is 5

        colors.AddRange(new Color[4] {
            new Color(colorIndex, colorIndex, 0, 0), new Color(colorIndex, colorIndex, 0, 0),
            new Color(colorIndex, colorIndex, 0, 0), new Color(colorIndex, colorIndex, 0, 0)
        });
    }

    /// <summary>
    /// Encodes colors as unique positions used for noise calculations in animal shader
    /// </summary>
    /// <param name="verticies">Verticies to encode into colors</param>
    protected void encodePositionalData(List<Vector3> verts) {
        foreach (Vector3 vert in verts) {
            colors.Add(new Color(vert.x, vert.y, vert.z));
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

        // Texture coordinates in default order.
        Vector2[] coords = new Vector2[]{
            new Vector2(0, 1), new Vector2(1, 0),
            new Vector2(0, 0), new Vector2(1, 1)
        };

        // The offsets for the texture coords for each direction. 
        // Y directions are set to 10 because the rotation doesn't matter in our context, and we don't use the texcoords in y-direction anyways.
        int[] offsets = new int[] {
            0, 10, 3, 1, 10, 2
        };

        for (int i = 0; i < 4; i++) {
            uvs.Add(coords[(i + offsets[(int)faceDir]) % 4]);
        }

    }
}
