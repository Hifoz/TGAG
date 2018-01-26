using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeGenerator {
    struct LineSegment{
        public LineSegment(Vector3 a, Vector3 b) {
            this.a = a;
            this.b = b;
        }

        public Vector3 a;
        public Vector3 b;
    }

    const int size = 80;
    const int height = 80;

    /// <summary>
    /// Generates the MeshData for a tree
    /// </summary>
    /// <param name="pos">Position of the tree</param>
    /// <returns>Meshdata</returns>
    public static MeshData generateMeshData(Vector3 pos) {

        LineSegment[] tree = new LineSegment[] {
            new LineSegment(new Vector3(0, 0, 0), new Vector3(0, 10, 0)),
            new LineSegment(new Vector3(0, 10, 0), new Vector3(20, 20, 0)),
            new LineSegment(new Vector3(0, 10, 0), new Vector3(-20, 20, 0))
        };

        BlockData[,,] pointMap = new BlockData[size,height,size];
        for(int x = 0; x < pointMap.GetLength(0); x++) {
            for (int y = 0; y < pointMap.GetLength(1); y++) {
                for (int z = 0; z < pointMap.GetLength(2); z++) {                   
                    pointMap[x, y, z] = new BlockData(calcBlockType(new Vector3(x - size / 2f, y, z - size / 2f), tree));

                }
            }
        }
        return MeshDataGenerator.GenerateMeshData(pointMap, 0.1f, true);
    }

    /// <summary>
    /// Calculates the blocktype based on position and tree lines.
    /// </summary>
    /// <param name="pos">Position being investigated</param>
    /// <param name="tree">Tree lines</param>
    /// <returns>Blocktype for position</returns>
    private static BlockData.BlockType calcBlockType(Vector3 pos, LineSegment[] tree) {
        foreach(var line in tree) {
            if (distance(pos, line) < 2f) {
                return BlockData.BlockType.DIRT;
            }
        }
        return BlockData.BlockType.AIR;
    }

    /// <summary>
    /// Computes the distance between a point and a line segment.
    /// Based on: http://geomalgorithms.com/a02-_lines.html
    /// </summary>
    /// <param name="P">Point</param>
    /// <param name="S">Line Segment</param>
    /// <returns>float distance</returns>
    private static float distance(Vector3 P, LineSegment S) {
        Vector3 v = S.b - S.a;
        Vector3 w = P - S.a;

        float c1 = Vector3.Dot(w, v);
        if (c1 <= 0)
            return Vector3.Distance(P, S.a);

        float c2 = Vector3.Dot(v, v);
        if (c2 <= c1)
            return Vector3.Distance(P, S.b);

        float b = c1 / c2;
        Vector3 Pb = S.a + b * v;
        return Vector3.Distance(P, Pb);
    }
}
