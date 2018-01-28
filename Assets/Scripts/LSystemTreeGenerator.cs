using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineSegment {
    public LineSegment(Vector3 a, Vector3 b) {
        this.a = a;
        this.b = b;
    }

    public LineSegment(Vector3 a, Vector3 b, LineSegment child) {
        this.a = a;
        this.b = b;
    }

    public Vector3 a;
    public Vector3 b;
}


public static class LSystemTreeGenerator {

    public static List<LineSegment> GenerateLSystemTree(Vector3 pos) {
        List<LineSegment> tree = new List<LineSegment>();
        tree.Add(new LineSegment(new Vector3(0, 0, 0), new Vector3(0, 10, 5)));
        tree.Add(new LineSegment(new Vector3(0, 10, 5), new Vector3(10, 15, 5)));
        tree.Add(new LineSegment(new Vector3(0, 10, 5), new Vector3(15, 15, 5)));

        return tree;
    }

}
