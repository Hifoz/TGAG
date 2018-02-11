using UnityEngine;
using System.Collections;

/// <summary>
/// Class representing a line, this is used to represent trees
/// </summary>
public class LineSegment {
    public LineSegment(Vector3 a, Vector3 b, bool endLine = false) {
        this.a = a;
        this.b = b;
        this.endLine = endLine;
    }

    public Vector3 a;
    public Vector3 b;
    public bool endLine;
    
    /// <summary>
    /// Adds a vector to the line
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns>Resulting lineSegment</returns>
    public static LineSegment operator+ (LineSegment a, Vector3 b) {
        return new LineSegment(a.a + b, a.b + b);
    }

    /// <summary>
    /// Subtracts a vector from the line
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns>Resulting lineSegment</returns>
    public static LineSegment operator- (LineSegment a, Vector3 b) {
        return new LineSegment(a.a - b, a.b - b);
    }

    /// <summary>
    /// Add function for when you don't want to change the reference
    /// </summary>
    /// <param name="point">point to add</param>
    public void add(Vector3 point) {
        a += point;
        b += point;
    }

    /// <summary>
    /// Computes the distance between a point and a line segment.
    /// Based on: http://geomalgorithms.com/a02-_lines.html
    /// </summary>
    /// <param name="P">Point</param>
    /// <param name="S">Line Segment</param>
    /// <returns>float distance</returns>
    public float distance(Vector3 P) {
        Vector3 v = b - a;
        Vector3 w = P - a;

        float c1 = Vector3.Dot(w, v);
        if (c1 <= 0)
            return Vector3.Distance(P, a);

        float c2 = Vector3.Dot(v, v);
        if (c2 <= c1)
            return Vector3.Distance(P, b);

        float e = c1 / c2;
        Vector3 Pb = a + e * v;
        return Vector3.Distance(P, Pb);
    }
}