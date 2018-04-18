using UnityEngine;

/// <summary>
/// Class representing a line
/// Mainly used for trees and animals
/// </summary>
public class LineSegment {
    public Vector3 a; //Start
    public Vector3 b; //End
    public bool endLine; //Last line in chain?
    public float radius; //Radius of line

    /// <summary>
    /// Constructor for line
    /// </summary>
    /// <param name="a">point a</param>
    /// <param name="b">point b</param>
    /// <param name="radius">radius of line</param>
    /// <param name="endLine">last line?</param>
    public LineSegment(Vector3 a, Vector3 b, float radius = 0, bool endLine = false) {
        this.a = a;
        this.b = b;
        this.radius = radius;
        this.endLine = endLine;
    }

    public Vector3 direction { get { return (b - a).normalized; } }
    public float length { get { return (b - a).magnitude; } }

    /// <summary>
    /// Adds a vector to the line
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns>Resulting lineSegment</returns>
    public static LineSegment operator+ (LineSegment a, Vector3 b) {
        return new LineSegment(a.a + b, a.b + b, a.radius, a.endLine);
    }

    /// <summary>
    /// Subtracts a vector from the line
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns>Resulting lineSegment</returns>
    public static LineSegment operator- (LineSegment a, Vector3 b) {
        return new LineSegment(a.a - b, a.b - b, a.radius, a.endLine);
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
