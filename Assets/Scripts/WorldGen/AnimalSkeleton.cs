using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Enum used to specify a bodypart
/// </summary>
public enum BodyPart {
    ALL = 0,
    HEAD,
    NECK,
    SPINE,
    RIGHT_LEGS, LEFT_LEGS,
    TAIL
}

/// <summary>
/// Enums used to specify a parameter for the skeleton
/// </summary>
public enum BodyParameter {
    HEAD_SIZE = 0,
    NECK_LENGTH,
    SPINE_LENGTH,
    LEG_PAIRS, LEG_JOINTS, LEG_LENGTH, LEG_JOINT_LENGTH,
    TAIL_JOINTS, TAIL_LENGTH, TAIL_JOINT_LENGTH 
}

/// <summary>
/// Class used to represent a Bone with constraints
/// </summary>
public class Bone {
    public Transform bone;
    public Vector3 minAngles = new Vector3(-180, -180, -180);
    public Vector3 maxAngles = new Vector3(180, 180, 180);
    public List<LineSegment> skinningBones;
}

/// <summary>
/// AnimalSkeleton, represents an animal skeleton through animation bones, and LineSegments.
/// </summary>
public class AnimalSkeleton {

    public int index; // Index of animal in ChunkManager
    
    private static ThreadSafeRng rng = new ThreadSafeRng();

    private MixedDictionary<BodyParameter> bodyParametersRange = new MixedDictionary<BodyParameter>(
        new Dictionary<BodyParameter, object>() {
            { BodyParameter.HEAD_SIZE, new Pair<float>(1, 3) },
            { BodyParameter.NECK_LENGTH, new Pair<float>(2, 4) },
            { BodyParameter.SPINE_LENGTH, new Pair<float>(4, 7) },
            { BodyParameter.LEG_PAIRS, new Pair<int>(2, 4) },
            { BodyParameter.LEG_JOINTS, new Pair<int>(2, 3) },
            { BodyParameter.LEG_LENGTH, new Pair<float>(5, 10) },
            //LEG_JOINT_LENGTH is calculated from LEG_JOINTS and LEG_LENGTH
            { BodyParameter.TAIL_JOINTS, new Pair<int>(2, 5) },
            { BodyParameter.TAIL_LENGTH, new Pair<float>(3, 7) }
            //TAIL_JOINT_LENGTH is calculated from TAIL_JOINTS and TAIL_LENGTH
    });

    private Transform rootBone;
    private MixedDictionary<BodyParameter> bodyParameters = new MixedDictionary<BodyParameter>();
    private Dictionary<BodyPart, List<Bone>> skeletonBones = new Dictionary<BodyPart, List<Bone>>();
    private Dictionary<BodyPart, List<LineSegment>> skeletonLines = new Dictionary<BodyPart, List<LineSegment>>();

    private Vector3 lowerBounds;
    private Vector3 upperBounds;
    private MeshData meshData;
    private List<Matrix4x4> bindPoses = new List<Matrix4x4>();
    private List<BoneWeight> weights;

    private const float skeletonThiccness = 0.5f;
    private const float voxelSize = 0.5f;

    //    _____       _     _ _        __  __      _   _               _     
    //   |  __ \     | |   | (_)      |  \/  |    | | | |             | |    
    //   | |__) |   _| |__ | |_  ___  | \  / | ___| |_| |__   ___   __| |___ 
    //   |  ___/ | | | '_ \| | |/ __| | |\/| |/ _ \ __| '_ \ / _ \ / _` / __|
    //   | |   | |_| | |_) | | | (__  | |  | |  __/ |_| | | | (_) | (_| \__ \
    //   |_|    \__,_|_.__/|_|_|\___| |_|  |_|\___|\__|_| |_|\___/ \__,_|___/
    //                                                                       
    //                                                                       

    /// <summary>
    /// Constructor that generates an AnimalSkeleton
    /// </summary>
    /// <param name="root"></param>
    public AnimalSkeleton(Transform root) {
        generateInMainThread(root);
    }

    /// <summary>
    /// Gets the specified bones.
    /// </summary>
    /// <param name="bodyPart">BodyPart bodyPart</param>
    /// <returns>List<Bone> bones</returns>
    public List<Bone> getBones(BodyPart bodyPart) {
        return skeletonBones[bodyPart];
    }

    /// <summary>
    /// Gets the specified lines
    /// </summary>
    /// <param name="bodyPart">Bodypart of lines to get</param>
    /// <returns>List of lines for bodypart</returns>
    public List<LineSegment> getLines(BodyPart bodyPart) {
        return skeletonLines[bodyPart];
    }

    /// <summary>
    /// Returns bodyparameter of type T
    /// </summary>
    /// <typeparam name="T">The type of the item</typeparam>
    /// <param name="parameter">parameter to get</param>
    /// <returns>T item</returns>
    public T getBodyParameter<T>(BodyParameter parameter) {
        return bodyParameters.Get<T>(parameter);
    }

    /// <summary>
    /// Gets the specified leg
    /// </summary>
    /// <param name="leg">bool rightLeg</param>
    /// <param name="number">Number of leg to get</param>
    /// <returns>List<Bone> leg to get</returns>
    public List<Bone> getLeg(bool rightLeg, int number) {
        int legJoints = bodyParameters.Get<int>(BodyParameter.LEG_JOINTS);
        List<Bone> legs = skeletonBones[(rightLeg) ? BodyPart.RIGHT_LEGS : BodyPart.LEFT_LEGS];
        return legs.GetRange(number * (legJoints + 1), legJoints + 1);
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
        foreach (LineSegment bone in skeletonLines[BodyPart.ALL]) {
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

    //    _____      _            _         __  __      _   _               _     
    //   |  __ \    (_)          | |       |  \/  |    | | | |             | |    
    //   | |__) | __ ___   ____ _| |_ ___  | \  / | ___| |_| |__   ___   __| |___ 
    //   |  ___/ '__| \ \ / / _` | __/ _ \ | |\/| |/ _ \ __| '_ \ / _ \ / _` / __|
    //   | |   | |  | |\ V / (_| | ||  __/ | |  | |  __/ |_| | | | (_) | (_| \__ \
    //   |_|   |_|  |_| \_/ \__,_|\__\___| |_|  |_|\___|\__|_| |_|\___/ \__,_|___/
    //                                                                            
    //                                                                            

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
        createColliders();
    }

    /// <summary>
    /// Initializes the dictionaries (skeletonLines and skeletonBones)
    /// </summary>
    private void initDicts() {
        foreach (BodyPart part in Enum.GetValues(typeof(BodyPart))) {
            skeletonLines.Add(part, new List<LineSegment>());
            skeletonBones.Add(part, new List<Bone>());
        }
    }

    //     _____ _        _      _                __  __      _   _               _     
    //    / ____| |      | |    | |              |  \/  |    | | | |             | |    
    //   | (___ | | _____| | ___| |_ ___  _ __   | \  / | ___| |_| |__   ___   __| |___ 
    //    \___ \| |/ / _ \ |/ _ \ __/ _ \| '_ \  | |\/| |/ _ \ __| '_ \ / _ \ / _` / __|
    //    ____) |   <  __/ |  __/ || (_) | | | | | |  | |  __/ |_| | | | (_) | (_| \__ \
    //   |_____/|_|\_\___|_|\___|\__\___/|_| |_| |_|  |_|\___|\__|_| |_|\___/ \__,_|___/
    //                                                                                  
    //                                                                                  

    /// <summary>
    /// Generates the bodyparamaters for the skeleton,
    /// they are stored in the bodyParameters dictionary
    /// </summary>
    public void generateBodyParams() {
        bodyParameters.Clear();
        foreach (KeyValuePair<BodyParameter, object> pair in bodyParametersRange.getDict()) {
            if (pair.Value.GetType().Equals(typeof(Pair<float>))) {
                Pair<float> range = (Pair<float>)pair.Value;
                bodyParameters.Add(pair.Key, rng.randomFloat(range.first, range.second));
            }

            if (pair.Value.GetType().Equals(typeof(Pair<int>))) {
                Pair<int> range = (Pair<int>)pair.Value;
                bodyParameters.Add(pair.Key, rng.randomInt(range.first, range.second));
            }
        }
        bodyParameters.Add(
            BodyParameter.LEG_JOINT_LENGTH,
            bodyParameters.Get<float>(BodyParameter.LEG_LENGTH) / bodyParameters.Get<int>(BodyParameter.LEG_JOINTS)
        );
        bodyParameters.Add(
            BodyParameter.TAIL_JOINT_LENGTH,
            bodyParameters.Get<float>(BodyParameter.TAIL_LENGTH) / bodyParameters.Get<int>(BodyParameter.TAIL_JOINTS)
        );
    }

    /// <summary>
    /// Makes the skeletonLines for the AnimalSkeleton
    /// </summary>
    private void makeSkeletonLines() {
        generateBodyParams();
        //HEAD
        List<LineSegment> head = createHead(bodyParameters.Get<float>(BodyParameter.HEAD_SIZE));
        addSkeletonLines(head, BodyPart.HEAD);
        //NECK
        LineSegment neckLine = new LineSegment(head[head.Count - 1].b, head[head.Count - 1].b + new Vector3(0, -0.5f, 0.5f).normalized * bodyParameters.Get<float>(BodyParameter.NECK_LENGTH));
        addSkeletonLine(neckLine, BodyPart.NECK);
        //SPINE
        LineSegment spineLine = new LineSegment(neckLine.b, neckLine.b + new Vector3(0, 0, 1) * bodyParameters.Get<float>(BodyParameter.SPINE_LENGTH));
        addSkeletonLine(spineLine, BodyPart.SPINE);
        //
        LineSegment tailLine;
        tailLine = new LineSegment(spineLine.b, spineLine.b + new Vector3(0, 0.5f, 0.5f).normalized * bodyParameters.Get<float>(BodyParameter.TAIL_LENGTH));
        addSkeletonLine(tailLine, BodyPart.TAIL);
        //LEGS
        int legPairs = bodyParameters.Get<int>(BodyParameter.LEG_PAIRS);
        float legLength = bodyParameters.Get<float>(BodyParameter.LEG_LENGTH);
        float spineLength = bodyParameters.Get<float>(BodyParameter.SPINE_LENGTH);
        for (int i = 0; i < legPairs; i++) {
            Vector3 offset = new Vector3(0, 0, 1) * spineLength * ((float)i / (float)(legPairs - 1)) + spineLine.a;
            LineSegment right = new LineSegment(new Vector3(0, 0, 0), new Vector3(-0.5f, -0.5f, 0).normalized * legLength) + offset;
            LineSegment left = new LineSegment(new Vector3(0, 0, 0), new Vector3(0.5f, -0.5f, 0).normalized * legLength) + offset;
            addSkeletonLine(right, BodyPart.RIGHT_LEGS);
            addSkeletonLine(left, BodyPart.LEFT_LEGS);
        }        
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
    /// Populates the skeletonBones dicitonary with bones
    /// </summary>
    private void makeAnimBones() {
        //SPINE
        LineSegment spineLine = skeletonLines[BodyPart.SPINE][0];
        Bone spineBone = createAndBindBone(Vector3.Lerp(spineLine.a, spineLine.b, 0.5f), rootBone, spineLine, "Mid Spine", BodyPart.SPINE);
        spineBone.minAngles = new Vector3(-90, -1, -1);
        spineBone.maxAngles = new Vector3(90, 1, 1);
        //NECK
        Bone neckBoneBase = createAndBindBone(skeletonLines[BodyPart.NECK][0].b, spineBone.bone, skeletonLines[BodyPart.NECK][0], "Neck", BodyPart.NECK);
        Bone neckBone = createAndBindBone(skeletonLines[BodyPart.NECK][0].a, neckBoneBase.bone, "Neck", BodyPart.NECK);
        createAndBindBone(skeletonLines[BodyPart.NECK][0].a, neckBone.bone, skeletonLines[BodyPart.HEAD], "Head", BodyPart.HEAD);
        //TAIL
        int tailJointCount = bodyParameters.Get<int>(BodyParameter.TAIL_JOINTS);
        createAndBindBones(skeletonLines[BodyPart.TAIL][0], spineBone.bone, tailJointCount, "Tail", BodyPart.TAIL);
        //LEGS
        int legPairs = bodyParameters.Get<int>(BodyParameter.LEG_PAIRS);
        int legJointCount = bodyParameters.Get<int>(BodyParameter.LEG_JOINTS);
        for (int i = 0; i < legPairs; i++) {
            createAndBindBones(skeletonLines[BodyPart.RIGHT_LEGS][i], spineBone.bone, legJointCount, string.Format("Right Leg {0}", i), BodyPart.RIGHT_LEGS);
            createAndBindBones(skeletonLines[BodyPart.LEFT_LEGS][i], spineBone.bone, legJointCount, string.Format("Left Leg {0}", i), BodyPart.LEFT_LEGS);
        }
    }

    /// <summary>
    /// Creates and binds a multi joint bone. 
    /// </summary>
    /// <param name="line">Line to create multiple bones for</param>
    /// <param name="parent">parent bone</param>
    /// <param name="jointCount">Number of joints in bone</param>
    /// <param name="name">Name of bone</param>
    /// <param name="bodyPart">Bodypart of bone</param>
    private void createAndBindBones(LineSegment line, Transform parent, int jointCount, string name, BodyPart bodyPart) {
        float jointLength = line.length() / jointCount;
        for(int i = 0; i < jointCount; i++) {
            LineSegment skinningBone = new LineSegment(line.a + line.getDir() * jointLength * i, line.a + line.getDir() * jointLength * (i + 1));
            parent = createAndBindBone(skinningBone.a, parent, skinningBone, name, bodyPart).bone;
        }
        //add Foot
        createAndBindBone(line.b, parent, name + " effector", bodyPart);
    }

    /// <summary>
    /// Makes and binds a bone for animation
    /// </summary>
    /// <param name="pos">Vector3 pos</param>
    /// <param name="root">Transform root</param>
    /// <param name="parent">Transform parent</param>
    /// <param name="skinningBone">Bone used for skinning</param>
    /// <param name="name">string name</param>
    /// <param name="bodyPart">BodyPart bodyPart</param>
    /// <returns>Bone bone</returns>
    private Bone createAndBindBone(Vector3 pos, Transform parent, LineSegment skinningBone, string name, BodyPart bodyPart) {
        Bone bone = createAndBindBone(pos, parent, name, bodyPart);
        bone.skinningBones = new List<LineSegment>() { skinningBone };
        return bone;
    }

    /// <summary>
    /// Makes and binds a bone for animation
    /// </summary>
    /// <param name="pos">Vector3 pos</param>
    /// <param name="root">Transform root</param>
    /// <param name="parent">Transform parent</param>
    /// <param name="skinningBones">Bones used for skinning</param>
    /// <param name="name">string name</param>
    /// <param name="bodyPart">BodyPart bodyPart</param>
    /// <returns>Bone bone</returns>
    private Bone createAndBindBone(Vector3 pos, Transform parent, List<LineSegment> skinningBones, string name, BodyPart bodyPart) {
        Bone bone = createAndBindBone(pos, parent, name, bodyPart);        
        bone.skinningBones = skinningBones;
        return bone;
    }

    /// <summary>
    /// Makes and binds a bone for animation
    /// </summary>
    /// <param name="pos">Vector3 pos</param>
    /// <param name="root">Transform root</param>
    /// <param name="parent">Transform parent</param>
    /// <param name="name">string name</param>
    /// <param name="bodyPart">BodyPart bodyPart</param>
    /// <returns>Bone bone</returns>
    private Bone createAndBindBone(Vector3 pos, Transform parent, string name, BodyPart bodyPart) {
        Transform bone = new GameObject(name).transform;
        bone.parent = parent;
        bone.position = rootBone.position + pos;
        bone.localRotation = Quaternion.identity;
        bindPoses.Add(bone.worldToLocalMatrix * rootBone.localToWorldMatrix);

        Bone b = new Bone();
        b.bone = bone;
        skeletonBones[BodyPart.ALL].Add(b);
        skeletonBones[bodyPart].Add(b);
        return b;
    }

    /// <summary>
    /// Adds colliders to the skeleton
    /// </summary>
    private void createColliders() {
        Bone spine = skeletonBones[BodyPart.SPINE][0];
        BoxCollider col = spine.bone.gameObject.AddComponent<BoxCollider>();
        Vector3 size = col.size;
        size.z = bodyParameters.Get<float>(BodyParameter.SPINE_LENGTH) + 0.1f;
        col.size = size;
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

    //    __  __           _       __  __      _   _               _     
    //   |  \/  |         | |     |  \/  |    | | | |             | |    
    //   | \  / | ___  ___| |__   | \  / | ___| |_| |__   ___   __| |___ 
    //   | |\/| |/ _ \/ __| '_ \  | |\/| |/ _ \ __| '_ \ / _ \ / _` / __|
    //   | |  | |  __/\__ \ | | | | |  | |  __/ |_| | | | (_) | (_| \__ \
    //   |_|  |_|\___||___/_| |_| |_|  |_|\___|\__|_| |_|\___/ \__,_|___/
    //                                                                   
    //                                                                   


    /// <summary>
    /// Generates the MeshData for the animal
    /// </summary>
    private void generateVoxelMeshData() {
        upperBounds = new Vector3(-99999, -99999, -99999);
        lowerBounds = new Vector3(99999, 99999, 99999);
        foreach (LineSegment line in skeletonLines[BodyPart.ALL]) {
            updateBounds(line);
        }

        upperBounds += Vector3.one * (skeletonThiccness + 2.5f);
        lowerBounds -= Vector3.one * (skeletonThiccness + 2.5f);
        Vector3 size = upperBounds - lowerBounds;
        size /= voxelSize;

        BlockDataMap pointMap = new BlockDataMap(Mathf.CeilToInt(size.x), Mathf.CeilToInt(size.y), Mathf.CeilToInt(size.z));
        for (int x = 0; x < pointMap.GetLength(0); x++) {
            for (int y = 0; y < pointMap.GetLength(1); y++) {
                for (int z = 0; z < pointMap.GetLength(2); z++) {
                    int i = pointMap.index1D(x, y, z);
                    Vector3 samplePos = new Vector3(x, y, z) * voxelSize + lowerBounds;
                    pointMap.mapdata[i] = new BlockData(calcBlockType(samplePos), BlockData.BlockType.NONE);
                    if (pointMap.mapdata[i].blockType == BlockData.BlockType.DIRT) {
                        pointMap.mapdata[i].modifier = BlockData.BlockType.SNOW;
                    }
                }
            }
        }
        meshData = new MeshData();
        StopWatch sw = new StopWatch();
        //sw.start();
        meshData = MeshDataGenerator.GenerateMeshData(pointMap, voxelSize, -(lowerBounds / voxelSize), MeshDataGenerator.MeshDataType.TERRAIN)[0];
        //sw.done("Animal mesh generator");
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
    /// Calculates the bone weights for a vertex
    /// </summary>
    /// <param name="vert"></param>
    /// <returns></returns>
    private BoneWeight calcVertBoneWeight(Vector3 vert) {
        float bestDist = 99999f;
        int bestIndex = 0;

        List<Bone> bones = skeletonBones[BodyPart.ALL];

        for (int i = 0; i < bones.Count; i++) {
            if (bones[i].skinningBones != null) {
                float dist = bones[i].skinningBones[0].distance(vert);
                for (int j = 1; j < bones[i].skinningBones.Count; j++) {
                    float dist2 = bones[i].skinningBones[j].distance(vert);
                    if (dist2 < dist) {
                        dist = dist2;
                    }
                }

                if (dist < bestDist) {
                    bestIndex = i;
                    bestDist = dist;
                }
            }
        }

        BoneWeight boneWeight = new BoneWeight();
        boneWeight.boneIndex0 = bestIndex;
        boneWeight.weight0 = 1f;
        return boneWeight;
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
}
