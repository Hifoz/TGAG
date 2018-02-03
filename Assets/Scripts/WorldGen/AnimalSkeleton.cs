using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This class is currently in a prototype stage.
/// This class is currently in a prototype stage.
/// This class is currently in a prototype stage.
/// </summary>
public class AnimalSkeleton {

    List<LineSegment> allBones = new List<LineSegment>();//all lines
    List<LineSegment> head = new List<LineSegment>();//head
    List<LineSegment> neck = new List<LineSegment>();//neck
    List<LineSegment> spine = new List<LineSegment>();//spine
    List<LineSegment> rightLegs = new List<LineSegment>();//right legs
    List<LineSegment> leftLegs = new List<LineSegment>();//left legs
    List<LineSegment> tail = new List<LineSegment>();//tail

    public AnimalSkeleton generate() {
        float headSize = 2.5f;
        float neckLen = 3f;
        float spineLen = 10f;
        int legPairs = 2;
        float legLen = 5f;
        float tailLen = 5f;

        var headLines = createHead(headSize);
        allBones.AddRange(headLines);
        head.AddRange(headLines);
        LineSegment neckLine = new LineSegment(head[head.Count - 1].b, head[head.Count - 1].b + new Vector3(0, -0.5f, 0.5f).normalized * neckLen);
        allBones.Add(neckLine);
        neck.Add(neckLine);
        LineSegment spineLine = new LineSegment(neckLine.b, neckLine.b + new Vector3(0, 0, 1) * spineLen);
        allBones.Add(spineLine);
        neck.Add(spineLine);
        for (int i = 0; i < legPairs; i++) {
            LineSegment left = new LineSegment(new Vector3(0, 0, 0), new Vector3(0.5f, -0.5f, 0) * legLen);
            LineSegment right = new LineSegment(new Vector3(0, 0, 0), new Vector3(-0.5f, -0.5f, 0) * legLen);
            left += new Vector3(0, 0, 1) * spineLen * ((float)i / (float)(legPairs - 1)) + spineLine.a;
            right += new Vector3(0, 0, 1) * spineLen * ((float)i / (float)(legPairs - 1)) + spineLine.a;
            leftLegs.Add(left);
            rightLegs.Add(right);
        }
        allBones.AddRange(leftLegs);
        allBones.AddRange(rightLegs);

        LineSegment tailLine = new LineSegment(spineLine.b, spineLine.b + new Vector3(0, -0.5f, 0.5f).normalized * tailLen);
        tail.Add(tailLine);
        allBones.Add(tailLine);

        return new AnimalSkeleton();
    }

    public static Mesh createMesh(AnimalSkeleton skeleton) {
        Mesh mesh = new Mesh();

        List<Vector3> verticies = new List<Vector3>();
        List<int> indexes = new List<int>();
        int i = 0;
        foreach(var BoneWeight in skeleton.allBones) {
            verticies.Add(BoneWeight.a);
            indexes.Add(i++);
            verticies.Add(BoneWeight.b);
            indexes.Add(i++);
        }

        mesh.SetVertices(verticies);
        mesh.SetIndices(indexes.ToArray(), MeshTopology.Lines, 0);

        return mesh;
    }

    private List<LineSegment> createHead(float headSize) {
        List<LineSegment> head = new List<LineSegment>();
        for (int i = -1; i <= 1; i += 2) {
            for (int j = -1; j <= 1; j += 2) {
                head.Add(new LineSegment(new Vector3(0, 0, 0), new Vector3(i, j, 1) * headSize/2f));
                head.Add(new LineSegment(new Vector3(i, j, 1) * headSize / 2f, new Vector3(0, 0, headSize)));
            }
        }
        return head;
    }
}
