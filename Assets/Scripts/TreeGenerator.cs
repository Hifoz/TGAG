using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TreeGenerator {
    private class LineSegment{
        public LineSegment() {
            child = null;
        }

        public LineSegment(Vector3 a, Vector3 b) {
            this.a = a;
            this.b = b;
            child = null;
        }

        public LineSegment(Vector3 a, Vector3 b, LineSegment child) {
            this.a = a;
            this.b = b;
            child = null;
        }

        public Vector3 a;
        public Vector3 b;
        public LineSegment child;
    }

    const int size = 80;
    const int height = 80;

    /// <summary>
    /// Generates the MeshData for a tree
    /// </summary>
    /// <param name="pos">Position of the tree</param>
    /// <returns>Meshdata</returns>
    public static MeshData generateMeshData(Vector3 pos) {

        List<LineSegment> tree = generateTree(pos);

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



    private static List<LineSegment> generateTree(Vector3 pos) {
        List<LineSegment> tree = new List<LineSegment>();
        LineSegment root = new LineSegment(Vector3.zero, new Vector3(0, 5, 0));
        LineSegment current = root;
        tree.Add(current);
        System.Random rng = new System.Random((int)(pos.x + pos.y + pos.z));

        int trunkLines = (int)(rng.NextDouble() * 9);
        Debug.Log(trunkLines);
        for (int i = 0; i < trunkLines; i++) {
            Vector3 point = randomPointOnHalfSphere(rng);
            current.child = new LineSegment();
            current.child.a = current.b;
            current = current.child;
            current.b = point * (float)rng.NextDouble() * 7f;
            current.b = current.b + current.a;
            tree.Add(current);
        }

        return tree;
    }

    private static float positionalNoise(ref Vector3 pos2D) {
        Vector3 sampleIncrement = Vector3.one * 0.93f;
        const float frequency = 2237.17f;
        return SimplexNoise.scale01(SimplexNoise.Simplex2D(pos2D, frequency));
    }

    private static Vector3 randomPointOnHalfSphere(System.Random rng) {
        float angle1 = (float)rng.NextDouble() * Mathf.PI * 2f;
        float angle2 = (float)(rng.NextDouble() - 0.5f) * Mathf.PI;

        Vector3 point = new Vector3();

        point.x = Mathf.Cos(angle1) * Mathf.Sin(angle2);
        point.z = Mathf.Sin(angle1) * Mathf.Sin(angle2);
        point.y = Mathf.Cos(angle2);
        return point;
    }

    /// <summary>
    /// Calculates the blocktype based on position and tree lines.
    /// </summary>
    /// <param name="pos">Position being investigated</param>
    /// <param name="tree">Tree lines</param>
    /// <returns>Blocktype for position</returns>
    private static BlockData.BlockType calcBlockType(Vector3 pos, List<LineSegment> tree) {
        foreach(var line in tree) {
            if (distance(pos, line) < 1.5f) {
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
