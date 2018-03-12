using System;
using System.Collections.Generic;
using UnityEngine;


class VoxelFace {
    public BlockData data;

    public bool isFlipped;

    public VoxelFace(BlockData data, bool isFlipped = false) {
        this.data = data;
        this.isFlipped = isFlipped;
    }



    public enum FaceDirection {
        xp, xm, yp, ym, zp, zm
    }
}


/// <summary>
/// Implementation of greedy meshing.
/// Based on javascript implementation by Mikola Lysenko
/// https://github.com/mikolalysenko/mikolalysenko.github.com/blob/gh-pages/MinecraftMeshes2/js/greedy_tri.js
/// </summary>
class GreedyMeshGenerator {
    BlockDataMap blockData;


    public GreedyMeshGenerator(BlockDataMap blockData, float voxelSize = 1f, Vector3 offset = default(Vector3)) {
        this.blockData = blockData;

    }



    public MeshData[] generateMeshData() {
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Color> colors = new List<Color>();
        List<Vector2> uvs = new List<Vector2>();


        // Go over the it in the three dimensions
        for(int d = 0; d < 3; d++) {
            int u = (d+1) % 3; // u and v are the other two dimensions, kinda like the "local" x and y in this dimension d
            int v = (d+2) % 3;
            int[] x = new int[] { 0, 0, 0 };
            int[] q = new int[] { 0, 0, 0 };
            int w = 0;
            int h = 0;
            int k = 0;
            int l = 0;

            q[d] = 1;
            VoxelFace[] mask = new VoxelFace[blockData.GetLength(u) * blockData.GetLength(v)];

            for (x[d] = -1; x[d] < blockData.GetLength(d);) {
                int n = 0;

                // Calculate mask:
                for (x[v] = 0; x[v] < blockData.GetLength(v); x[v]++) {
                    for (x[u] = 0; x[u] < blockData.GetLength(u); x[u]++, n++) {
                        BlockData a = x[d] >= 0 ? blockData.mapdata[blockData.index1D(x[0], x[1], x[2])] : new BlockData(BlockData.BlockType.NONE);
                        BlockData b = x[d] < blockData.GetLength(d) - 1 ? blockData.mapdata[blockData.index1D(x[0] + q[0], x[1] + q[1], x[2] + q[2])] : new BlockData(BlockData.BlockType.NONE);
                        if (checkIfSolidVoxel(a) == checkIfSolidVoxel(b)){ // Empty if both are solid blocks or neither are solid blocks
                            mask[n] = new VoxelFace(BlockData.Empty);
                        } else if(!a.equals(BlockData.Empty)) { // If a is the only solid one, go with a
                            mask[n] = new VoxelFace(a, false);
                        } else {                                // Otherwise b is only solid one, so go with b
                            mask[n] = new VoxelFace(b, true);
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
                            Debug.Log(n);
                            for (w = 1; c.Equals(mask[n + w]) && i + w < blockData.GetLength(u); w++) { }; // Compute width

                            bool done = false;
                            for(h = 1; j + h < blockData.GetLength(v); h++){
                                for(k = 0; k < w; k++) {
                                    if (!c.Equals(mask[n + k + h * blockData.GetLength(u)])) {
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
                            if(!c.isFlipped) {
                                dv[v] = h;
                                du[u] = w;
                            } else {
                                dv[u] = h;
                                du[v] = w;
                            }

                            // Add mesh data
                            int vertIndex = vertices.Count;

                            vertices.Add(new Vector3(x[0],                 x[1],                 x[2]                ));
                            vertices.Add(new Vector3(x[0] + du[0],         x[1] + du[0],         x[2] + du[0]        ));
                            vertices.Add(new Vector3(x[0] + du[0] + dv[0], x[1] + du[0] + dv[0], x[2] + du[0] + dv[0]));
                            vertices.Add(new Vector3(x[0]         + dv[0], x[1]         + dv[0], x[2]         + dv[0]));

                            triangles.AddRange(new int[] { vertIndex, vertIndex + 1, vertIndex + 2 });
                            triangles.AddRange(new int[] { vertIndex + 2, vertIndex + 1, vertIndex + 3 });

                            Vector3 normalDir = Vector3.zero;
                            normalDir[d] = c.isFlipped ? -1 : 1;
                            normals.AddRange(new Vector3[] { normalDir, normalDir, normalDir, normalDir });

                            // Clear mask:
                            for(l = 0; l < h; l++) {
                                for(k = 0; k < w; k++) {
                                    mask[n + k + l * blockData.GetLength(u)] = new VoxelFace(BlockData.Empty);
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




    /// <summary>
    /// Checks if a voxels is fully opaque
    /// </summary>
    /// <param name="voxelPos">position of voxel</param>
    /// <returns>Whether the voxel is opaque</returns>
    protected bool checkIfSolidVoxel(BlockData data) {
        return !(data.blockType == BlockData.BlockType.NONE || data.blockType == BlockData.BlockType.WATER);
    }
}