using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TreeGenerator {

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
        return MeshDataGenerator.GenerateMeshData(pointMap, 0.2f, true);
    }

    /// <summary>
    /// Generates a tree built of lines.
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private static List<LineSegment> generateTree(Vector3 pos) {
        //const float maxLineLength = 7f;
        //const float minLineLength = 3f;

        //System.Random rng = new System.Random((int)(pos.x + pos.y + pos.z));
        List<LineSegment> tree = new List<LineSegment>();
        //LineSegment root = nextLineDirection(rng, new LineSegment(Vector3.zero, Vector3.one), Mathf.PI / 8f);
        //root.b *= (float)rng.NextDouble() * (maxLineLength - minLineLength) + minLineLength;
        //LineSegment current = root;
        //tree.Add(current);       

        //int trunkLines = (int)(rng.NextDouble() * 6) + 3;
        //Debug.Log(trunkLines);
        //for (int i = 0; i < trunkLines; i++) {
        //    current.child = nextLineDirection(rng, current, Mathf.PI / 8f);
        //    current.child.b *= (float)rng.NextDouble() * (maxLineLength - minLineLength) + minLineLength;
        //    current.child.a = current.b;
        //    current.child.b = current.b + current.child.b;
        //    current = current.child;
        //    //Debug.Log(current.a + "___" + current.b);
        //    tree.Add(current);
        //}

        return tree;
    }

    /// <summary>
    /// Generates a direction that deviates at most radianCap radiasn from the passed line.
    /// </summary>
    /// <param name="rng"></param>
    /// <param name="line"></param>
    /// <param name="radianCap"></param>
    /// <returns>LineSegment that deviates from input line</returns>
    private static LineSegment nextLineDirection(System.Random rng, LineSegment line, float radianCap) {
        Vector3 parent = line.b - line.a;
        parent.Normalize();

        float angle1 = Mathf.Acos(parent.y);
        float angle2 = Mathf.Acos(parent.x / Mathf.Sin(angle1));
        angle1 += (float)(rng.NextDouble() - 0.5f) * 2f * radianCap;
        angle2 += (float)(rng.NextDouble() - 0.5f) * 2f * radianCap;

        Vector3 point = new Vector3();

        point.x = Mathf.Cos(angle2) * Mathf.Sin(angle1);
        point.z = Mathf.Sin(angle2) * Mathf.Sin(angle1);
        point.y = Mathf.Cos(angle1);
        return new LineSegment(Vector3.zero, point);
    }

    /// <summary>
    /// Calculates the blocktype based on position and tree lines.
    /// </summary>
    /// <param name="pos">Position being investigated</param>
    /// <param name="tree">Tree lines</param>
    /// <returns>Blocktype for position</returns>
    private static BlockData.BlockType calcBlockType(Vector3 pos, List<LineSegment> tree) {
        //foreach(var line in tree) {
        //    float dist = distance(pos, line);
        //    if (dist < 1.5f) {
        //        return BlockData.BlockType.DIRT;
        //    } else if (line.child == null && dist < 10f && leafPos(pos)) {
        //        return BlockData.BlockType.STONE;
        //    }
        //}
        return BlockData.BlockType.AIR;
    }

    /// <summary>
    /// Is this a position for a leaf?
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private static bool leafPos(Vector3 pos) {
        pos += Vector3.one * 1000; // remove offset
        Vector3Int p = new Vector3Int((int)pos.x, (int)pos.y, (int)pos.z);
        bool evenPos = (p.x % 2 == 0 && p.y % 2 == 0 && p.z % 2 == 0);
        bool oddPos = (p.x % 2 == 1 && p.y % 2 == 1 && p.z % 2 == 1);
        return evenPos || oddPos;
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
