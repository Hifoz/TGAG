using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Enum used to specify a bodypart
/// </summary>
public enum BodyPart { ALL = 0, HEAD, NECK, SPINE, RIGHT_LEGS, LEFT_LEGS, TAIL }

/// <summary>
/// Class used to represent a Bone with constraints
/// </summary>
public class Bone {
    public Transform bone;
    public Vector3 minAngles = new Vector3(-180, -180, -180);
    public Vector3 maxAngles = new Vector3(180, 180, 180);
}

/// <summary>
/// AnimalSkeleton, represents an animal skeleton through animation bones, and LineSegments.
/// </summary>
public class AnimalSkeleton {   
    
    private static ThreadSafeRng rng = new ThreadSafeRng();
    private List<Matrix4x4> bindPoses = new List<Matrix4x4>();   
    private Transform rootBone;
    List<BoneWeight> weights;
    public Dictionary<BodyPart, List<Bone>> skeletonBones = new Dictionary<BodyPart, List<Bone>>();
    public Dictionary<BodyPart, List<LineSegment>> skeletonLines = new Dictionary<BodyPart, List<LineSegment>>();
    private MeshData meshData;

    private List<Vector3> threadSafeBones; // CUZ THREADING LOOL
    private Vector3 threadSafeRoot; // CUZ THREADING LOOL

    private const float skeletonThiccness = 0.25f;
    private const float voxelSize = 0.25f;

    private Vector3 lowerBounds;
    private Vector3 upperBounds;

    private float headsize;
    private float neckLen;
    private float spineLen;
    private int legpairs;
    private float legLen;
    private float tailLen;

    /// <summary>
    /// Constructor that generates an AnimalSkeleton
    /// </summary>
    /// <param name="root"></param>
    public AnimalSkeleton(Transform root) {
        generateInMainThread(root);
    }

    public float headSize { get { return headsize; } }
    public float neckLength { get { return neckLen; } }
    public float spineLength { get { return spineLen; } }
    public int legPairs { get { return legPairs; } }
    public float legLength { get { return legLen; } }
    public float tailLength { get { return tailLen; } }

    /// <summary>
    /// Generates the parts of the animal that should be generated in a thread.
    /// use "generateInMainThread" first in the main thread.
    /// </summary>
    public void generateInThread() {
        generateVoxelMeshData();
        weights = new List<BoneWeight>();
        foreach (Vector3 vert in meshData.vertices) {
            weights.Add(calcVertBoneWeight(vert));
        }
    }

    /// <summary>
    /// Generates the parts of the animal that can't be threaded/is fast.
    /// </summary>
    /// <param name="root"></param>
    private void generateInMainThread(Transform root) {
        foreach (Transform child in root) {
            MonoBehaviour.Destroy(child.gameObject);
        }

        rootBone = root;
        initDicts();
        makeSkeletonLines();
        centerSkeletonLines();
        makeAnimBones();

        threadSafeRoot = root.position;
        List<Bone> bones = skeletonBones[BodyPart.ALL];
        threadSafeBones = new List<Vector3>();
        foreach(Bone bone in bones) {
            threadSafeBones.Add(bone.bone.position);
        }
    }

    /// <summary>
    /// Creates a skinned mesh of the animal skeleton data
    /// Call after "generateInMainThread" and "generateInThread".
    /// </summary>
    /// <returns>Mesh mesh</returns>
    public Mesh getMesh() {
        Mesh mesh = MeshDataGenerator.applyMeshData(meshData);
        mesh.boneWeights = weights.ToArray();
        mesh.bindposes = bindPoses.ToArray();
        return mesh;
    }

    /// <summary>
    /// FOR DEBUGGING: Creates a line mesh for the skeleton
    /// </summary>
    /// <returns>Mesh</returns>
    public Mesh generateLineMesh() {
        Mesh mesh = new Mesh();

        List<Vector3> verticies = new List<Vector3>();
        List<int> indexes = new List<int>();
        List<BoneWeight> weights = new List<BoneWeight>();
        int i = 0;
        foreach(LineSegment bone in skeletonLines[BodyPart.ALL]) {
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

    /// <summary>
    /// Generates the MeshData for the animal
    /// </summary>
    private void generateVoxelMeshData() {
        foreach(LineSegment line in skeletonLines[BodyPart.ALL]) {
            updateBounds(line);
        }

        upperBounds += Vector3.one * (skeletonThiccness + 2.5f);
        lowerBounds -= Vector3.one * (skeletonThiccness + 2.5f);
        Vector3 size = upperBounds - lowerBounds;
        size /= voxelSize;
        

        BlockData[,,] pointMap = new BlockData[Mathf.CeilToInt(size.x), Mathf.CeilToInt(size.y), Mathf.CeilToInt(size.z)];
        //Debug.Log("(" + pointMap.GetLength(0) + "," + pointMap.GetLength(1) + "," + pointMap.GetLength(2) + ")");
        for (int x = 0; x < pointMap.GetLength(0); x++) {
            for (int y = 0; y < pointMap.GetLength(1); y++) {
                for (int z = 0; z < pointMap.GetLength(2); z++) {
                    Vector3 samplePos = new Vector3(x, y, z) * voxelSize + WorldUtils.floor(lowerBounds);
                    //samplePos = WorldUtils.floor(samplePos);
                    pointMap[x, y, z] = new BlockData(calcBlockType(samplePos), BlockData.BlockType.NONE);  
                    if (pointMap[x, y, z].blockType == BlockData.BlockType.DIRT) {
                        pointMap[x, y, z].modifier = BlockData.BlockType.SNOW;
                    }
                }
            }
        }
        meshData = new MeshData();
        meshData = MeshDataGenerator.GenerateMeshData(pointMap, voxelSize, -WorldUtils.floor(lowerBounds / voxelSize), MeshDataGenerator.MeshDataType.TERRAIN);
    }

    /// <summary>
    /// Gets the specified bones.
    /// </summary>
    /// <param name="bodyPart">BodyPart bodyPart</param>
    /// <returns>List<Bone> bones</returns>
    public List<Bone> getBones(BodyPart bodyPart) {
        return skeletonBones[bodyPart];
    }

    private BlockData.BlockType calcBlockType(Vector3 pos) {
        List<LineSegment> skeleton = skeletonLines[BodyPart.ALL];
        foreach (var line in skeleton) {
            float dist = line.distance(pos);
            if (dist < skeletonThiccness) {
                return BlockData.BlockType.DIRT;
            } 
        }
        return BlockData.BlockType.NONE;
    }


    /// <summary>
    /// Initializes the dictionaries (skeletonLines and skeletonBones)
    /// </summary>
    private void initDicts() {
        foreach(BodyPart part in Enum.GetValues(typeof(BodyPart))) {
            skeletonLines.Add(part, new List<LineSegment>());
            skeletonBones.Add(part, new List<Bone>());
        }
    }

    /// <summary>
    /// Makes the skeletonLines for the AnimalSkeleton
    /// </summary>
    private void makeSkeletonLines() {
        upperBounds = new Vector3(-99999, -99999, -99999);
        lowerBounds = new Vector3(99999, 99999, 99999);

        headsize = rng.randomFloat(1, 2);
        neckLen = rng.randomFloat(2, 4);
        spineLen = rng.randomFloat(2, 7);
        legpairs = 2;// will add support fo X legpairs in future
        legLen = rng.randomFloat(2, 6);
        tailLen = rng.randomFloat(1, 5);

        //HEAD
        List<LineSegment> head = createHead(headsize);
        addSkeletonLines(head, BodyPart.HEAD);
        //NECK
        LineSegment neckLine = new LineSegment(head[head.Count - 1].b, head[head.Count - 1].b + new Vector3(0, -0.5f, 0.5f).normalized * neckLen);
        addSkeletonLine(neckLine, BodyPart.NECK);
        //SPINE
        LineSegment spineLine = new LineSegment(neckLine.b, neckLine.b + new Vector3(0, 0, 1) * spineLen);
        addSkeletonLine(spineLine, BodyPart.SPINE);
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
            addSkeletonLine(left, BodyPart.LEFT_LEGS);
            addSkeletonLine(left2, BodyPart.LEFT_LEGS);
            addSkeletonLine(right, BodyPart.RIGHT_LEGS);
            addSkeletonLine(right2, BodyPart.RIGHT_LEGS);
        }
        //TAIL
        LineSegment tailLine = new LineSegment(spineLine.b, spineLine.b + new Vector3(0, 0.5f, 0.5f).normalized * tailLen);
        addSkeletonLine(tailLine, BodyPart.TAIL);
    }

    /// <summary>
    /// Adds a list of lines to the skeleton
    /// </summary>
    /// <param name="lines">List<LineSegment> lines</param>
    /// <param name="bodyPart">BodyPart bodyPart</param>
    private void addSkeletonLines(List<LineSegment> lines, BodyPart bodyPart) {
        skeletonLines[bodyPart].AddRange(lines);
        skeletonLines[BodyPart.ALL].AddRange(lines);
    }

    /// <summary>
    /// Adds a skeleton line to the skeleton
    /// </summary>
    /// <param name="line">LineSegment line</param>
    /// <param name="bodyPart">BodyPart bodyPart</param>
    private void addSkeletonLine(LineSegment line, BodyPart bodyPart) {
        skeletonLines[bodyPart].Add(line);
        skeletonLines[BodyPart.ALL].Add(line);
    }

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
        lowerBounds.y = (lowerBounds.y < point.y) ? lowerBounds.y : point.y;
        lowerBounds.z = (lowerBounds.z < point.z) ? lowerBounds.z : point.z;
        upperBounds.x = (upperBounds.x > point.x) ? upperBounds.x : point.x;
        upperBounds.y = (upperBounds.y > point.y) ? upperBounds.y : point.y;
        upperBounds.z = (upperBounds.z > point.z) ? upperBounds.z : point.z;
    }

    /// <summary>
    /// Centers the skeleton around the spine
    /// </summary>
    private void centerSkeletonLines() {
        LineSegment spineLine = skeletonLines[BodyPart.SPINE][0];
        Vector3 center = Vector3.Lerp(spineLine.a, spineLine.b, 0.5f);
        List<LineSegment> skeleton = skeletonLines[BodyPart.ALL];
        for (int i = 0; i < skeleton.Count; i++) {
            skeleton[i].add(-center);
        }
    }

    /// <summary>
    /// Calculates the bone weights for a vertex
    /// </summary>
    /// <param name="vert"></param>
    /// <returns></returns>
    private BoneWeight calcVertBoneWeight(Vector3 vert) {
        float[] bestDist = new float[2] { 99999, 999999 };
        int[] bestIndex = new int[2] { 0, 0 };

        for (int i = 0; i < threadSafeBones.Count; i++) {
            float dist = Vector3.Distance(threadSafeBones[i], vert + threadSafeRoot);
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

    /// <summary>
    /// Populates the skeletonBones dicitonary with bones
    /// </summary>
    private void makeAnimBones() {
        createAndBindBone(Vector3.Lerp(skeletonLines[BodyPart.SPINE][0].a, skeletonLines[BodyPart.SPINE][0].b, 0.5f), rootBone, "Mid Spine", BodyPart.SPINE);
        createAndBindBone(skeletonLines[BodyPart.NECK][0].b, skeletonBones[BodyPart.SPINE][0].bone, "Neck", BodyPart.NECK);
        createAndBindBone(skeletonLines[BodyPart.NECK][0].a, skeletonBones[BodyPart.NECK][0].bone, "Head", BodyPart.HEAD);
        createAndBindBone(skeletonLines[BodyPart.TAIL][0].a, skeletonBones[BodyPart.SPINE][0].bone, "Tail", BodyPart.TAIL);
        for(int i = 0; i < 2; i++) {
            createAndBindBone(skeletonLines[BodyPart.RIGHT_LEGS][i * 2].a, skeletonBones[BodyPart.SPINE][0].bone, "Right Leg " + i, BodyPart.RIGHT_LEGS);
            createAndBindBone(skeletonLines[BodyPart.RIGHT_LEGS][i * 2].b, skeletonBones[BodyPart.RIGHT_LEGS][i * 3].bone, "Right Leg " + i, BodyPart.RIGHT_LEGS);
            createAndBindBone(skeletonLines[BodyPart.RIGHT_LEGS][i * 2 + 1].b, skeletonBones[BodyPart.RIGHT_LEGS][i * 3 + 1].bone, "Right Leg " + i, BodyPart.RIGHT_LEGS);
            createAndBindBone(skeletonLines[BodyPart.LEFT_LEGS][i * 2].a, skeletonBones[BodyPart.SPINE][0].bone, "left Leg " + i, BodyPart.LEFT_LEGS);
            createAndBindBone(skeletonLines[BodyPart.LEFT_LEGS][i * 2].b, skeletonBones[BodyPart.LEFT_LEGS][i * 3].bone, "left Leg " + i, BodyPart.LEFT_LEGS);
            createAndBindBone(skeletonLines[BodyPart.LEFT_LEGS][i * 2 + 1].b, skeletonBones[BodyPart.LEFT_LEGS][i * 3 + 1].bone, "left Leg " + i, BodyPart.LEFT_LEGS);
        }

        addConstraintsLeg(skeletonBones[BodyPart.RIGHT_LEGS].GetRange(0, 3));
        addConstraintsLeg(skeletonBones[BodyPart.RIGHT_LEGS].GetRange(3, 3));
        addConstraintsLeg(skeletonBones[BodyPart.LEFT_LEGS].GetRange(0, 3));
        addConstraintsLeg(skeletonBones[BodyPart.LEFT_LEGS].GetRange(3, 3));
    }

    /// <summary>
    /// Makes and binds a bone for animation
    /// </summary>
    /// <param name="pos">Vector3 pos</param>
    /// <param name="root">Transform root</param>
    /// <param name="parent">Transform parent</param>
    /// <param name="name">string name</param>
    /// <param name="bodyPart">BodyPart bodyPart</param>
    private void createAndBindBone(Vector3 pos, Transform parent, string name, BodyPart bodyPart) {
        Transform bone = new GameObject(name).transform;
        bone.parent = parent;
        bone.position = rootBone.position + pos;
        bone.localRotation = Quaternion.identity;
        bindPoses.Add(bone.worldToLocalMatrix * rootBone.localToWorldMatrix);

        Bone b = new Bone();
        b.bone = bone;
        skeletonBones[BodyPart.ALL].Add(b);
        skeletonBones[bodyPart].Add(b);
    }

    /// <summary>
    /// Adds joint constraints to a leg
    /// </summary>
    /// <param name="leg">List<Bone> leg</param>
    private void addConstraintsLeg(List<Bone> leg) {
        leg[0].minAngles = new Vector3(-120, -120, -90);
        leg[0].maxAngles = new Vector3(120, 120, 90);
        leg[1].minAngles = new Vector3(-90, -90, -90);
        leg[1].maxAngles = new Vector3(90, 90, 90);
    }

    /// <summary>
    /// Creates the head of the animal
    /// </summary>
    /// <param name="headSize">float headSize</param>
    /// <returns>List<LineSegment> head</returns>
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
