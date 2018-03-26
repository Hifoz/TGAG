using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// What a list of lines represents
/// </summary>
public enum LineStructureType {
    TREE = 0,
    ANIMAL
}

/// <summary>
/// Class used to represent the bounds of a line structure
/// </summary>
public class LineStructureBounds {
    /// <summary>
    /// Constructor, that also initializes the bounds
    /// </summary>
    /// <param name="lines">The lines to calculate bounds for</param>
    /// <param name="type">The type of structure the lines represents</param>
    /// <param name="boundsModifier">Modifier to use for expanding bounds outside the lines absolute bounds</param>
    public LineStructureBounds(List<LineSegment> lines, LineStructureType type, float boundsModifier, float scale = 1f) {
        this.type = type;
        upperBounds = new Vector3(-99999, -99999, -99999);
        lowerBounds = new Vector3(99999, (type == LineStructureType.TREE) ? 0 : 99999, 99999);         

        foreach (LineSegment line in lines) {
            updateBounds(line);
        }
        upperBounds += Vector3.one * boundsModifier;
        lowerBounds -= ((type == LineStructureType.TREE) ? new Vector3(1, 0, 1) : Vector3.one) * boundsModifier;
        size = upperBounds - lowerBounds;
        size /= scale;

        sizeI = new Vector3Int(Mathf.CeilToInt(size.x), Mathf.CeilToInt(size.y), Mathf.CeilToInt(size.z));
    }

    public Vector3 lowerBounds;
    public Vector3 upperBounds;
    public Vector3 size;
    public Vector3Int sizeI;
    private LineStructureType type;

    /// <summary>
    /// Updates the bounds of the skeleton
    /// </summary>
    /// <param name="line">LineSegment line</param>
    private void updateBounds(LineSegment line) {
        updateBounds(line.a);
        updateBounds(line.b);
    }

    /// <summary>
    /// Updates the bounds of the skeleton
    /// </summary>
    /// <param name="line">Vector3 line</param>
    private void updateBounds(Vector3 point) {
        lowerBounds.x = (lowerBounds.x < point.x) ? lowerBounds.x : point.x;
        if (type != LineStructureType.TREE) {
            lowerBounds.y = (lowerBounds.y < point.y) ? lowerBounds.y : point.y;
        }
        lowerBounds.z = (lowerBounds.z < point.z) ? lowerBounds.z : point.z;
        upperBounds.x = (upperBounds.x > point.x) ? upperBounds.x : point.x;
        upperBounds.y = (upperBounds.y > point.y) ? upperBounds.y : point.y;
        upperBounds.z = (upperBounds.z > point.z) ? upperBounds.z : point.z;
    }
}   

