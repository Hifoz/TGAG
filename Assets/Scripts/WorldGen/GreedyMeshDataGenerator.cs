using System;
using System.Collections.Generic;
using UnityEngine;



class VoxelFace {
    public BlockData data;
    public int dir;
    public bool isFlipped;
    public bool nodraw = false;

    public VoxelFace(BlockData data, int dir, bool isFlipped = false) {
        this.data = data;
        this.dir = dir;
        this.isFlipped = isFlipped;
    }



    public enum FaceDirection {
        xp, xm, yp, ym, zp, zm
    }

    public bool equals(VoxelFace other) {
        if (other == null) return false;
        return data.equals(other.data) && isFlipped == other.isFlipped && nodraw == other.nodraw;
    }
}



/// <summary>
/// Implementation of greedy meshing.
/// Based on javascript implementation by Mikola Lysenko
/// https://github.com/mikolalysenko/mikolalysenko.github.com/blob/gh-pages/MinecraftMeshes2/js/greedy_tri.js
/// </summary>
class GreedyMeshDataGenerator {
    BlockDataMap blockData;

    List<Vector3> vertices = new List<Vector3>();
    List<Vector3> normals = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Color> colors = new List<Color>();
    List<Vector2> uvs = new List<Vector2>();

    // Could these three also be moved into the MeshDataExtras? (Or voxelSize and offset at least?)
    MeshDataGenerator.MeshDataType meshDataType;
    float voxelSize; // Is there any good reason for why voxelSize and offset is done on the meshdata 
    Vector3 offset;  //   and not just by moving and scaling the GO the mesh is attached to?

    MeshDataExtras extras;

    /// <summary>
    /// Constructor for the greedy mesh data generator.
    /// </summary>
    /// <param name="blockData">The BlockDataMap containing the pointdata to create the meshdata from</param>
    /// <param name="voxelSize">size of the voxels</param>
    /// <param name="offset"></param>
    /// <param name="meshDataType">The type of the meshdata</param>
    /// <param name="extras">An object containing any extra/optional data that can be used in generation of the mesh</param>
    public GreedyMeshDataGenerator(BlockDataMap blockData, float voxelSize = 1f, Vector3 offset = default(Vector3), 
                                   MeshDataGenerator.MeshDataType meshDataType = MeshDataGenerator.MeshDataType.TERRAIN, 
                                   MeshDataExtras extras = null) {
        this.blockData = blockData;
        this.meshDataType = meshDataType;
        this.voxelSize = voxelSize;

        if(this.meshDataType == MeshDataGenerator.MeshDataType.ANIMAL) {
            if (extras == null) {
                throw new UnassignedReferenceException("You need to include a MeshDataExtras object for \"extras\" " +
                                                       "parameter if you want to make MeshData of type \"ANIMAL\"");
            } else if (extras.animalData.Equals(Vector2.negativeInfinity)) {
                throw new ArgumentException("\"extras.animalData\" can not be Vector2.negativeInfinity if you want" +
                                            " to make MeshData of type \"ANIMAL\". Did you forget to set the value?");
            }
            this.extras = extras;
        }
    }


    /// <summary>
    /// Generates the meshdata
    /// </summary>
    /// <returns></returns>
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
                        if (checkVoxel(a) == checkVoxel(b)){
                            mask[n] = new VoxelFace(BlockData.Empty, d);
                        } else if(checkVoxel(a)) {
                            mask[n] = new VoxelFace(a, d, false);  
                        } else {
                            mask[n] = new VoxelFace(b, d, true);
                        }

                        // Make the outside blocks nodraw:
                        if (x[v] < 1 || x[v] >= blockData.GetLength(v) - 1) mask[n].nodraw = true;
                        if (x[u] < 1 || x[u] >= blockData.GetLength(u) - 1) mask[n].nodraw = true;
                        if (x[d] < 1 || x[d] >= blockData.GetLength(d) - 1) mask[n].nodraw = true;


                    }
                }

                x[d]++;
                n = 0;
                
                // Generating the actual mesh data from the mask:
                for (int j = 0; j < blockData.GetLength(v); j++) {
                    for (int i = 0; i < blockData.GetLength(u);) {
                        VoxelFace c = mask[n];
                        if (c != null && !c.data.equals(BlockData.Empty) && !c.nodraw) {
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
                            Vector3[] verts = new Vector3[]{
                                new Vector3(x[0],                 x[1],                 x[2]),
                                new Vector3(x[0] + du[0],         x[1] + du[1],         x[2] + du[2]),
                                new Vector3(x[0] + du[0] + dv[0], x[1] + du[1] + dv[1], x[2] + du[2] + dv[2]),
                                new Vector3(x[0]         + dv[0], x[1]         + dv[1], x[2]         + dv[2])
                            };

                            addFace(verts, w, h, mask[n]);


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
    /// <param name="v0">vertex 0</param>
    /// <param name="v1">vertex 1</param>
    /// <param name="v2">vertex 2</param>
    /// <param name="v3">vertex 3</param>
    /// <param name="width">width of face</param>
    /// <param name="height">height of face</param>
    /// <param name="voxel">the VoxelFace containing the blockdata and direction</param>
    private void addFace(Vector3[] verts, int width, int height, VoxelFace voxel) {
        int vertIndex = vertices.Count;

        for(int i = 0; i < verts.Length; i++) {
            verts[i] = (verts[i] - offset) * voxelSize;
        }

        if(!voxel.isFlipped)
            vertices.AddRange(new Vector3[] { verts[0], verts[1], verts[2], verts[3] });
        else
            vertices.AddRange(new Vector3[] { verts[2], verts[1], verts[0], verts[3] });


        triangles.AddRange(new int[] { vertIndex, vertIndex + 1, vertIndex + 2 });
        triangles.AddRange(new int[] { vertIndex, vertIndex + 2, vertIndex + 3 });

        Vector3 normalDir = Vector3.zero;
        normalDir[voxel.dir] = voxel.isFlipped ? -1 : 1;
        normals.AddRange(new Vector3[] { normalDir, normalDir, normalDir, normalDir });


        if(meshDataType == MeshDataGenerator.MeshDataType.ANIMAL) {
            addAnimalTextureData(vertices.GetRange(vertIndex, 4));
        } else {
            addTextureCoordinates(width, height, voxel);
            addTextureTypeData(voxel);
        }


    }

    /// <summary>
    /// Encodes colors as unique positions used for noise calculations in animal shader
    /// </summary>
    /// <param name="verts">Verticies to encode into colors</param>
    private void addAnimalTextureData(List<Vector3> verts) {
        Vector3 scalingVector = new Vector3(blockData.GetLength(0), blockData.GetLength(1), blockData.GetLength(2));
        foreach (Vector3 v in verts) {
            colors.Add(new Color(v.x / scalingVector.x,
                                 v.y / scalingVector.y,
                                 v.z / scalingVector.z));
            uvs.Add(extras.animalData);
        }
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
    /// Checks the block type
    /// </summary>
    /// <param name="voxelPos">position of voxel</param>
    /// <returns>
    ///     If meshDataType == TERRAIN: Whether the voxel is opaque.
    ///     If meshDataType == WATER: Whether the voxel is something other than water.
    /// </returns>
    protected bool checkVoxel(BlockData data) {
        switch (meshDataType) {
            case MeshDataGenerator.MeshDataType.WATER:
                return data.blockType == BlockData.BlockType.WATER;
            default: // MeshDataType.TERRAIN
                return !(data.blockType == BlockData.BlockType.NONE || data.blockType == BlockData.BlockType.WATER);
        }
    }
}