using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores data used to create a mesh
/// </summary>
public class MeshData {
    public Vector3[] vertices;
    public Vector3[] normals;
    public int[] triangles;
    public Color[] colors;
    public Vector2[] uvs;

    /// <summary>
    /// Splits up the MeshData into more manageable chunks
    /// </summary>
    /// <param name="maxVertices">max number of vertices per MeshData object</param>
    /// <returns>A list of MeshData objects</returns>
    public MeshData[] split(int maxVertices = 60000) {
        if (vertices.Length <= maxVertices)
            return new MeshData[]{ this };

        List<MeshData> splitData = new List<MeshData>();
        int numMeshes = Mathf.CeilToInt(vertices.Length / (float)maxVertices);

        int maxVertsPerMesh = Mathf.CeilToInt(vertices.Length / numMeshes / 4.0f) * 4;
        int trianglesPerMesh = maxVertsPerMesh * 3 / 2;

        for(int m = 0; m < numMeshes; m++) {
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Color> colors = new List<Color>();
            List<Vector2> uvs = new List<Vector2>();

            for (int v = 0; v < maxVertsPerMesh; v++) {
                if (m * maxVertsPerMesh + v >= this.vertices.Length)
                    break;
                vertices.Add(this.vertices[m * maxVertsPerMesh + v]);
                normals.Add(this.normals[m * maxVertsPerMesh + v]);
                colors.Add(this.colors[m * maxVertsPerMesh + v]);
                uvs.Add(this.uvs[m * maxVertsPerMesh + v]);
            }
            for (int t = 0; t < trianglesPerMesh; t++) {
                if (m * trianglesPerMesh + t >= this.triangles.Length)
                    break;
                triangles.Add(this.triangles[m * trianglesPerMesh + t] - m * maxVertsPerMesh);
            }

            splitData.Add(new MeshData {
                vertices = vertices.ToArray(),
                normals = normals.ToArray(),
                triangles = triangles.ToArray(),
                colors = colors.ToArray(),
                uvs = uvs.ToArray()
            });

        }
        return splitData.ToArray();
    }
}
