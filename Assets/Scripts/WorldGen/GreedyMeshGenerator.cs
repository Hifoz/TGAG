using System;
using System.Collections.Generic;
using UnityEngine;


class VoxelFace {
    public BlockData data;
    public int dir;
    public bool isFlipped;

    public VoxelFace(BlockData data, int dir, bool isFlipped = false) {
        this.data = data;
        this.dir = dir;
        this.isFlipped = isFlipped;
    }



    public enum FaceDirection {
        xp, xm, yp, ym, zp, zm
    }

    internal bool equals(VoxelFace other) {
        return data.equals(other.data) && isFlipped == other.isFlipped;
    }
}


/// <summary>
/// Implementation of greedy meshing.
/// Based on javascript implementation by Mikola Lysenko
/// https://github.com/mikolalysenko/mikolalysenko.github.com/blob/gh-pages/MinecraftMeshes2/js/greedy_tri.js
/// </summary>
class GreedyMeshGenerator {
    BlockDataMap blockData;

    List<Vector3> vertices = new List<Vector3>();
    List<Vector3> normals = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Color> colors = new List<Color>();
    List<Vector2> uvs = new List<Vector2>();


    public GreedyMeshGenerator(BlockDataMap blockData, float voxelSize = 1f, Vector3 offset = default(Vector3)) {
        this.blockData = blockData;

    }



    public MeshData[] generateMeshData() {
        // Go over the it in the three dimensions
        for(int d = 0; d < 3; d++) {
            int u = (d+1) % 3; // u and v are the other two dimensions, kinda like the "local" x and y in this dimension d
            int v = (d+2) % 3;
            int[] x = new int[] { 0, 0, 0 }; // Contains the position of the voxel in xyz space
            int[] q = new int[] { 0, 0, 0 }; // Offset for second voxel to check
            int w = 0;
            int h = 0;
            int k = 0;
            int l = 0;

            q[d] = 1;
            VoxelFace[] mask = new VoxelFace[blockData.GetLength(u) * blockData.GetLength(v)];

            // Go through all "slices" in dimension d
            for (x[d] = -1; x[d] < blockData.GetLength(d);) {
                int n = 0;

                // Calculate mask:
                for (x[v] = 0; x[v] < blockData.GetLength(v); x[v]++) {
                    for (x[u] = 0; x[u] < blockData.GetLength(u); x[u]++, n++) {
                        BlockData a = x[d] >= 0 ? blockData.mapdata[blockData.index1D(x[0], x[1], x[2])] : new BlockData(BlockData.BlockType.NONE);
                        BlockData b = x[d] < blockData.GetLength(d) - 1 ? blockData.mapdata[blockData.index1D(x[0] + q[0], x[1] + q[1], x[2] + q[2])] : new BlockData(BlockData.BlockType.NONE);
                        if (checkIfSolidVoxel(a) == checkIfSolidVoxel(b)){ // Empty if both are solid blocks or neither are solid blocks
                            mask[n] = new VoxelFace(BlockData.Empty, d);
                        } else if(!a.equals(BlockData.Empty)) { // If a is the only solid one, go with a
                            mask[n] = new VoxelFace(a, d, false);  
                        } else {                                // Otherwise b is only solid one, so go with b
                            mask[n] = new VoxelFace(b, d, true);
                        }
                    }
                }

                x[d]++;
                n = 0;
                // Generating the actual mesh data from the mask:
                for (int j = 0; j < blockData.GetLength(v); j++) {
                    for (int i = 0; i < blockData.GetLength(u);) {
                        VoxelFace c = mask[n];
                        if (c != null && !c.data.equals(BlockData.Empty)) {
                            for (w = 1; i + w < blockData.GetLength(u) && c.equals(mask[n + w]); w++) { }; // Calculate width of face

                            bool done = false;
                            for(h = 1; j + h < blockData.GetLength(v); h++){ // Calculate height of face
                                for(k = 0; k < w; k++) {
                                    if (!c.equals(mask[n + k + h * blockData.GetLength(u)])) {
                                        done = true;
                                        break;
                                    }
                                }
                                if (done) {
                                    break;
                                }
                            }


                            // Find orientation of triangles
                            x[u] = i;
                            x[v] = j;
                            int[] dv = new int[] { 0, 0, 0 };
                            int[] du = new int[] { 0, 0, 0 };
                            dv[v] = h;
                            du[u] = w;

                            // Add vertex data and other face data
                            addFace(new Vector3(x[0],                 x[1],                 x[2]),
                                    new Vector3(x[0] + du[0],         x[1] + du[1],         x[2] + du[2]),
                                    new Vector3(x[0] + du[0] + dv[0], x[1] + du[1] + dv[1], x[2] + du[2] + dv[2]),
                                    new Vector3(x[0]         + dv[0], x[1]         + dv[1], x[2]         + dv[2]),
                                    w,
                                    h,
                                    mask[n]);


                            // Clear mask:
                            for(l = 0; l < h; l++) {
                                for(k = 0; k < w; k++) {
                                    mask[n + k + l * blockData.GetLength(u)] = new VoxelFace(BlockData.Empty, d);
                                }
                            }
                            i += w;
                            n += w;
                        } else {
                            i++;
                            n++;
                        }
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


    #region face construction

    /// <summary>
    /// Add a new face
    /// </summary>
    /// <param name="v0"></param>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <param name="v3"></param>
    /// <param name="width">width of face</param>
    /// <param name="height">height of face</param>
    /// <param name="voxel">the VoxelFace containing the blockdata and direction</param>
    private void addFace(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, int width, int height, VoxelFace voxel) {
        int vertIndex = vertices.Count;

        if(!voxel.isFlipped)
            vertices.AddRange(new Vector3[] { v0, v1, v2, v3 });
        else
            vertices.AddRange(new Vector3[] { v2, v1, v0, v3 });


        triangles.AddRange(new int[] { vertIndex, vertIndex + 1, vertIndex + 2 });
        triangles.AddRange(new int[] { vertIndex, vertIndex + 2, vertIndex + 3 });

        Vector3 normalDir = Vector3.zero;
        normalDir[voxel.dir] = voxel.isFlipped ? -1 : 1;
        normals.AddRange(new Vector3[] { normalDir, normalDir, normalDir, normalDir });


        addTextureCoordinates(width, height, voxel);
        addTextureTypeData(voxel);

    }

    /// <summary>
    /// Adds texture coordinates for a face
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="isFlipped"></param>
    private void addTextureCoordinates(int width, int height, VoxelFace voxel) {
        Vector2[] coords = new Vector2[] {
            new Vector2(0, 0), new Vector2(0, 1),
            new Vector2(1, 0), new Vector2(1, 1)
        };


        int[,] rotations = new int[,] {
            { 1, 3, 2, 0 }, //xm, ym
            { 0, 1, 3, 2 }, // xp, yp
            { 2, 0, 1, 3 }, // zp
            { 3, 2, 0, 1 }  // zm
        };

        int rotation = 0;
        if (voxel.dir == 2)
            rotation = 3;

        if (!voxel.isFlipped)
            rotation += (voxel.dir == 2 ? -1 : 1);

        for (int i = 0; i < 4; i++) {
            uvs.Add(coords[rotations[rotation%rotations.Length, i]]);
        }

    }


    /// <summary>
    /// Stores the indices of the texture slices to use for a face.
    /// </summary>
    /// <param name="blockData">Data of the block</param>
    /// <param name="faceDir">Direction of the face</param>
    protected void addTextureTypeData(VoxelFace voxel) {
        TextureData.TextureType[] texTypes = new TextureData.TextureType[2];

        // Get texture types for base and modifier
        for (int i = 0; i < 2; i++) {
            BlockData.BlockType blockType = (i == 0 ? voxel.data.blockType : voxel.data.modifier);

            // Convert block type to texture type:
            string typeName = blockType.ToString();
            if (blockType == BlockData.BlockType.GRASS || blockType == BlockData.BlockType.SNOW) {
                if (voxel.dir == 1 && !voxel.isFlipped)
                    typeName += "_TOP";
                else if (voxel.dir == 1)
                    typeName = "NONE";
                else
                    typeName += "_SIDE";
            }
            texTypes[i] = (TextureData.TextureType)Enum.Parse(typeof(TextureData.TextureType), typeName);

        }

        for (int i = 0; i < 4; i++)
            colors.Add(new Color((int)texTypes[0], (int)texTypes[1], 0)); // Using the color to store the texture type of the vertices
    }

    #endregion


    /// <summary>
    /// Checks if a voxels is fully opaque
    /// </summary>
    /// <param name="voxelPos">position of voxel</param>
    /// <returns>Whether the voxel is opaque</returns>
    protected bool checkIfSolidVoxel(BlockData data) {
        return !(data.blockType == BlockData.BlockType.NONE || data.blockType == BlockData.BlockType.WATER);
    }
}