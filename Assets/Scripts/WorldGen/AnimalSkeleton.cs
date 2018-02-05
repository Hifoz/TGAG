using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This class is currently in a prototype stage.
/// This class is currently in a prototype stage.
/// This class is currently in a prototype stage.
/// </summary>
public class AnimalSkeleton {

    //These various data structures are experimental and subject to change.
    // I am not sure what data we will need to animate and create the animals yet,
    // so this is just my guess.
    List<LineSegment> allBones = new List<LineSegment>();//all lines
    List<LineSegment> head = new List<LineSegment>();//head
    List<LineSegment> neck = new List<LineSegment>();//neck
    List<LineSegment> spine = new List<LineSegment>();//spine
    List<LineSegment> rightLegs = new List<LineSegment>();//right legs
    List<LineSegment> leftLegs = new List<LineSegment>();//left legs
    List<LineSegment> tail = new List<LineSegment>();//tail

    private Transform root;
    public List<Transform> AallBones = new List<Transform>();//all lines
    public List<Transform> Ahead = new List<Transform>();//head
    public List<Transform> Aneck = new List<Transform>();//neck
    public List<Transform> Aspine = new List<Transform>();//spine
    public List<Transform> ArightLegs = new List<Transform>();//right legs
    public List<Transform> AleftLegs = new List<Transform>();//left legs
    public List<Transform> Atail = new List<Transform>();//tail
    public List<Matrix4x4> bindPoses = new List<Matrix4x4>();

    private static ThreadSafeRng rng = new ThreadSafeRng();

    public void generate(Transform root) {
        this.root = root;

        float headSize = rng.randomFloat(1, 3);
        float neckLen = rng.randomFloat(1, 3);
        float spineLen = rng.randomFloat(2, 7);
        int legPairs = 2;// rng.randomInt(2, 6);
        float legLen = rng.randomFloat(2, 6);
        float tailLen = rng.randomFloat(1, 5);

        var headLines = createHead(headSize);
        allBones.AddRange(headLines);
        head.AddRange(headLines);
        LineSegment neckLine = new LineSegment(head[head.Count - 1].b, head[head.Count - 1].b + new Vector3(0, -0.5f, 0.5f).normalized * neckLen);
        allBones.Add(neckLine);
        neck.Add(neckLine);
        LineSegment spineLine = new LineSegment(neckLine.b, neckLine.b + new Vector3(0, 0, 1) * spineLen);
        allBones.Add(spineLine);
        spine.Add(spineLine);
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

        LineSegment tailLine = new LineSegment(spineLine.b, spineLine.b + new Vector3(0, 0.5f, 0.5f).normalized * tailLen);
        tail.Add(tailLine);
        allBones.Add(tailLine);

        makeAnimBones(root);
    }

    public Mesh createMesh() {
        Mesh mesh = new Mesh();

        List<Vector3> verticies = new List<Vector3>();
        List<int> indexes = new List<int>();
        List<BoneWeight> weights = new List<BoneWeight>();
        int i = 0;
        foreach(var bone in allBones) {
            verticies.Add(bone.a);
            indexes.Add(i++);
            weights.Add(calcVertBoneWeight(bone.a));

            verticies.Add(bone.b);
            indexes.Add(i++);
            weights.Add(calcVertBoneWeight(bone.b));
        }

        mesh.SetVertices(verticies);
        mesh.SetIndices(indexes.ToArray(), MeshTopology.Lines, 0);

        
        mesh.boneWeights = weights.ToArray();
        mesh.bindposes = bindPoses.ToArray();
        return mesh;
    }

    private BoneWeight calcVertBoneWeight(Vector3 vert) {
        var bones = AallBones;
        float[] bestDist = new float[2] { 99999, 999999 };
        int[] bestIndex = new int[2] { 0, 0 };

        for (int i = 0; i < bones.Count; i++) {
            float dist = Vector3.Distance(bones[i].position, vert + root.position);
            if (dist < bestDist[0]) {
                if (bones[i].gameObject.name == "neck") Debug.Log("Hey");
                bestIndex[0] = i;
                bestDist[0] = dist;
            } else if (dist < bestDist[1]) {
                bestIndex[1] = i;
                bestDist[1] = dist;
            }
        }

        BoneWeight boneWeight = new BoneWeight();
        boneWeight.boneIndex0 = bestIndex[0];
        boneWeight.weight0 = 1f - bestDist[0] / (bestDist[0] + bestDist[1]);
        boneWeight.boneIndex1 = bestIndex[1];
        boneWeight.weight1 = 1f - bestDist[1] / (bestDist[0] + bestDist[1]);

        return boneWeight;
    }

    private void makeAnimBones(Transform root) {
        createAndBindBone(spine[0].a, root, root, "Upper Spine", Aspine);
        createAndBindBone(spine[0].b, root, root, "Lower Spine", Aspine);
        createAndBindBone(neck[0].a, root, Aspine[0], "Neck", Aneck);
        createAndBindBone(tail[0].b, root, Aspine[1], "Tail", Atail);
        for(int i = 0; i < 2; i++) {
            createAndBindBone(rightLegs[i].b, root, Aspine[i], "Right Leg " + i, ArightLegs);
            createAndBindBone(leftLegs[i].b, root, Aspine[i], "Left Leg " + i, AleftLegs);
        }
    }

    private void createAndBindBone(Vector3 pos, Transform root, Transform parent, string name, List<Transform> section) {
        Transform bone = new GameObject(name).transform;
        bone.parent = parent;
        bone.position = root.position + pos;
        bone.localRotation = Quaternion.identity;
        Matrix4x4 mat = bone.worldToLocalMatrix * root.localToWorldMatrix;

        bindPoses.Add(mat);
        AallBones.Add(bone);
        section.Add(bone);
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
