using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// This class is currently in a prototype stage.
/// This class is currently in a prototype stage.
/// This class is currently in a prototype stage.
/// </summary>
public class AnimalSkeleton {
    public enum BodyPart { ALL = 0, HEAD, NECK, SPINE, RIGHT_LEGS, LEFT_LEGS, TAIL}
    
    private static ThreadSafeRng rng = new ThreadSafeRng();
    private List<Matrix4x4> bindPoses = new List<Matrix4x4>();   
    private Transform rootBone;
    private Dictionary<BodyPart, List<Transform>> skeletonBones = new Dictionary<BodyPart, List<Transform>>();
    private Dictionary<BodyPart, List<LineSegment>> skeletonLines = new Dictionary<BodyPart, List<LineSegment>>();

    private float headsize;
    private float neckLen;
    private float spineLen;
    private int legpairs;
    private float legLen;
    private float tailLen;

    public AnimalSkeleton(Transform root) {
        generate(root);
    }

    public float headSize { get { return headsize; } }
    public float neckLength { get { return neckLen; } }
    public float spineLength { get { return spineLen; } }
    public int legPairs { get { return legPairs; } }
    public float legLength { get { return legLen; } }
    public float tailLength { get { return tailLen; } }

    public void generate(Transform root) {
        initDicts();

        rootBone = root;

        headsize = rng.randomFloat(1, 2);
        neckLen = rng.randomFloat(2, 4);
        spineLen = rng.randomFloat(2, 7);
        legpairs = 2;// rng.randomInt(2, 6);
        legLen = rng.randomFloat(2, 6);
        tailLen = rng.randomFloat(1, 5);

        //HEAD
        List<LineSegment> head = createHead(headsize);
        skeletonLines[BodyPart.ALL].AddRange(head);
        skeletonLines[BodyPart.HEAD].AddRange(head);
        //NECK
        LineSegment neckLine = new LineSegment(head[head.Count - 1].b, head[head.Count - 1].b + new Vector3(0, -0.5f, 0.5f).normalized * neckLen);
        skeletonLines[BodyPart.ALL].Add(neckLine);
        skeletonLines[BodyPart.NECK].Add(neckLine);
        //SPINE
        LineSegment spineLine = new LineSegment(neckLine.b, neckLine.b + new Vector3(0, 0, 1) * spineLen);
        skeletonLines[BodyPart.ALL].Add(spineLine);
        skeletonLines[BodyPart.SPINE].Add(spineLine);
        //LEGS (This can prolly be done "tighter"
        for (int i = 0; i < legpairs; i++) {
            LineSegment left = new LineSegment(new Vector3(0, 0, 0), new Vector3(0.5f, -0.5f, 0) * legLen);
            LineSegment left2 = new LineSegment(new Vector3(0.5f, -0.5f, 0) * legLen, new Vector3(0.5f, -1f, 0) * legLen);
            LineSegment right = new LineSegment(new Vector3(0, 0, 0), new Vector3(-0.5f, -0.5f, 0) * legLen);
            LineSegment right2 = new LineSegment(new Vector3(-0.5f, -0.5f, 0) * legLen, new Vector3(-0.5f, -1f, 0) * legLen);
            left += new Vector3(0, 0, 1) * spineLen * ((float)i / (float)(legpairs - 1)) + spineLine.a;
            left2 += new Vector3(0, 0, 1) * spineLen * ((float)i / (float)(legpairs - 1)) + spineLine.a; 
            right += new Vector3(0, 0, 1) * spineLen * ((float)i / (float)(legpairs - 1)) + spineLine.a;
            right2 += new Vector3(0, 0, 1) * spineLen * ((float)i / (float)(legpairs - 1)) + spineLine.a;
            skeletonLines[BodyPart.LEFT_LEGS].Add(left);
            skeletonLines[BodyPart.LEFT_LEGS].Add(left2);
            skeletonLines[BodyPart.RIGHT_LEGS].Add(right);
            skeletonLines[BodyPart.RIGHT_LEGS].Add(right2);
        }
        skeletonLines[BodyPart.ALL].AddRange(skeletonLines[BodyPart.LEFT_LEGS]);
        skeletonLines[BodyPart.ALL].AddRange(skeletonLines[BodyPart.RIGHT_LEGS]);
        //TAIL
        LineSegment tailLine = new LineSegment(spineLine.b, spineLine.b + new Vector3(0, 0.5f, 0.5f).normalized * tailLen);
        skeletonLines[BodyPart.TAIL].Add(tailLine);
        skeletonLines[BodyPart.TAIL].Add(tailLine);

        makeAnimBones(root);
    }

    public Mesh createMesh() {
        Mesh mesh = new Mesh();

        List<Vector3> verticies = new List<Vector3>();
        List<int> indexes = new List<int>();
        List<BoneWeight> weights = new List<BoneWeight>();
        int i = 0;
        foreach(var bone in skeletonLines[BodyPart.ALL]) {
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

    public List<Transform> getBones(BodyPart bodyPart) {
        return skeletonBones[bodyPart];
    }

    private void initDicts() {
        foreach(BodyPart part in Enum.GetValues(typeof(BodyPart))) {
            skeletonLines.Add(part, new List<LineSegment>());
            skeletonBones.Add(part, new List<Transform>());
        }
    }

    private BoneWeight calcVertBoneWeight(Vector3 vert) {
        List<Transform> bones = skeletonBones[BodyPart.ALL];
        float[] bestDist = new float[2] { 99999, 999999 };
        int[] bestIndex = new int[2] { 0, 0 };

        for (int i = 0; i < bones.Count; i++) {
            float dist = Vector3.Distance(bones[i].position, vert + rootBone.position);
            if (dist < bestDist[0]) {
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
        createAndBindBone(Vector3.Lerp(skeletonLines[BodyPart.SPINE][0].a, skeletonLines[BodyPart.SPINE][0].b, 0.5f), root, root, "Mid Spine", BodyPart.SPINE);
        createAndBindBone(skeletonLines[BodyPart.NECK][0].b, root, skeletonBones[BodyPart.SPINE][0], "Neck", BodyPart.NECK);
        createAndBindBone(skeletonLines[BodyPart.NECK][0].a, root, skeletonBones[BodyPart.NECK][0], "Head", BodyPart.HEAD);
        createAndBindBone(skeletonLines[BodyPart.TAIL][0].a, root, skeletonBones[BodyPart.SPINE][0], "Tail", BodyPart.TAIL);
        for(int i = 0; i < 2; i++) {
            createAndBindBone(skeletonLines[BodyPart.RIGHT_LEGS][i * 2].a, root, skeletonBones[BodyPart.SPINE][0], "Right Leg " + i, BodyPart.RIGHT_LEGS);
            createAndBindBone(skeletonLines[BodyPart.RIGHT_LEGS][i * 2].b, root, skeletonBones[BodyPart.RIGHT_LEGS][i * 3], "Right Leg " + i, BodyPart.RIGHT_LEGS);
            createAndBindBone(skeletonLines[BodyPart.RIGHT_LEGS][i * 2 + 1].b, root, skeletonBones[BodyPart.RIGHT_LEGS][i * 3 + 1], "Right Leg " + i, BodyPart.RIGHT_LEGS);
            createAndBindBone(skeletonLines[BodyPart.LEFT_LEGS][i * 2].a, root, skeletonBones[BodyPart.SPINE][0], "left Leg " + i, BodyPart.LEFT_LEGS);
            createAndBindBone(skeletonLines[BodyPart.LEFT_LEGS][i * 2].b, root, skeletonBones[BodyPart.LEFT_LEGS][i * 3], "left Leg " + i, BodyPart.LEFT_LEGS);
            createAndBindBone(skeletonLines[BodyPart.LEFT_LEGS][i * 2 + 1].b, root, skeletonBones[BodyPart.LEFT_LEGS][i * 3 + 1], "left Leg " + i, BodyPart.LEFT_LEGS);
        }
    }

    private void createAndBindBone(Vector3 pos, Transform root, Transform parent, string name, BodyPart bodyPart) {
        Transform bone = new GameObject(name).transform;
        bone.parent = parent;
        bone.position = root.position + pos;
        bone.localRotation = Quaternion.identity;
        Matrix4x4 mat = bone.worldToLocalMatrix * root.localToWorldMatrix;

        bindPoses.Add(mat);
        skeletonBones[BodyPart.ALL].Add(bone);
        skeletonBones[bodyPart].Add(bone);
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
