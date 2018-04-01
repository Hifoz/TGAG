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
    TAIL,
    RIGHT_WING, LEFT_WING
}

/// <summary>
/// Enums used to specify a parameter for the skeleton
/// </summary>
public enum BodyParameter {
    SCALE = 0,
    HEAD_SIZE, HEAD_RADIUS, //Radius is the thichness of the lines
    NECK_LENGTH, NECK_RADIUS,
    SPINE_LENGTH, SPINE_JOINTS, SPINE_JOINT_LENGTH, SPINE_RADIUS,
    LEG_PAIRS, LEG_JOINTS, LEG_LENGTH, LEG_JOINT_LENGTH, LEG_RADIUS,
    TAIL_JOINTS, TAIL_LENGTH, TAIL_JOINT_LENGTH, TAIL_RADIUS,
    WING_LENGTH, WING_JOINTS, WING_JOINT_LENGTH, WING_RADIUS
}

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
public abstract class AnimalSkeleton {

    /// <summary>
    /// Helper class for performance reasons.
    /// Every vertex needs to be assigned to a bone, 
    ///     and having a list of bones in one place is good for memory performance
    /// </summary>
    protected struct SkinningBone {
        public SkinningBone(int index, LineSegment line) {
            indexOfBone = index;
            boneLine = line;
        }
        public int indexOfBone; //This is a reference to the index of the bone that this skinning bone belongs to
        public LineSegment boneLine; //line of bone to use for skinning
    }
    //Misc members
    protected static ThreadSafeRng seedGen = new ThreadSafeRng();
    protected ThreadSafeRng rng;
    protected int seed;

    //Skeleton related members
    protected MixedDictionary<BodyParameter> bodyParametersRange;
    protected Transform rootBone;
    protected MixedDictionary<BodyParameter> bodyParameters = new MixedDictionary<BodyParameter>();

    protected Dictionary<BodyPart, List<Bone>> skeletonBones = new Dictionary<BodyPart, List<Bone>>();
    protected Dictionary<BodyPart, List<LineSegment>> skeletonLines = new Dictionary<BodyPart, List<LineSegment>>();

    //Mesh related member variables
    private MeshData meshData;
    private List<Matrix4x4> bindPoses = new List<Matrix4x4>();
    private List<BoneWeight> weights;
    private List<SkinningBone> skinningBones = new List<SkinningBone>();
    private const float pointMapBoundsModifier = 1.5f; //Number for expanding the size of pointmap, needed to get the egdes of animals meshified
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
    /// Gets the gameobject that owns this skeleton
    /// </summary>
    /// <returns>owner</returns>
    public GameObject getOwner() {
        return rootBone.gameObject;
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
    /// Creates a skinned mesh of the animal skeleton data
    /// Call after "generateInMainThread" and "generateInThread".
    /// </summary>
    /// <returns>Mesh mesh</returns>
    public void applyMeshData() {
        GameObject animal = rootBone.gameObject;
        SkinnedMeshRenderer renderer = animal.GetComponent<SkinnedMeshRenderer>();

        Mesh mesh = renderer.sharedMesh;
        if (mesh == null) {
            mesh = new Mesh();            
        } else {
            mesh.Clear();
        }

        mesh.vertices = meshData.vertices;
        mesh.normals = meshData.normals;
        mesh.triangles = meshData.triangles;
        mesh.colors = meshData.colors;
        mesh.uv = meshData.uvs;
        mesh.boneWeights = weights.ToArray();
        mesh.bindposes = bindPoses.ToArray();
        renderer.sharedMesh = mesh;
    }    

    /// <summary>
    /// Generates the parts of the animal that should be generated in a thread.
    /// use "generateInMainThread" first in the main thread.
    /// </summary>
    public void generateInThread() {
        generateVoxelMeshData();
        weights = new List<BoneWeight>();
        for (int i = 0; i < meshData.vertices.Length; i++) {
            weights.Add(calcVertBoneWeight(meshData.vertices[i]));
        }
    }

    /// <summary>
    /// Gets the seed used to generate the skeleton
    /// </summary>
    /// <returns></returns>
    public int getSeed() {
        return seed;
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
    protected void generateInMainThread(Transform root) {
        root.rotation = Quaternion.identity;
        foreach (Transform child in root) {
            MonoBehaviour.Destroy(child.gameObject);
        }

        rootBone = root;
        initDicts();
        makeSkeletonLines();
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
    virtual protected void generateBodyParams() {
        bodyParameters.Clear();
        bool scaleReady = false;
        foreach (KeyValuePair<BodyParameter, object> pair in bodyParametersRange.getDict()) {
            float scale = 1;
            if (scaleReady) {
                scale = bodyParameters.Get<float>(BodyParameter.SCALE);
            } 

            if (pair.Value.GetType().Equals(typeof(Range<float>))) {
                Range<float> range = (Range<float>)pair.Value;
                bodyParameters.Add(pair.Key, rng.randomFloat(range.Min, range.Max) * scale);
            }

            if (pair.Value.GetType().Equals(typeof(Range<int>))) {
                Range<int> range = (Range<int>)pair.Value;
                bodyParameters.Add(pair.Key, rng.randomInt(range.Min, range.Max));
            }

            if (!scaleReady) { //Scale is the first enum
                scaleReady = true;
            }
        }
    }

    /// <summary>
    /// Makes the skeletonLines for the AnimalSkeleton
    /// </summary>
    protected abstract void makeSkeletonLines();

    /// <summary>
    /// Adds a list of lines to the skeleton
    /// </summary>
    /// <param name="lines">List<LineSegment> lines</param>
    /// <param name="bodyPart">BodyPart bodyPart</param>
    protected void addSkeletonLines(List<LineSegment> lines, BodyPart bodyPart) {
        skeletonLines[bodyPart].AddRange(lines);
        skeletonLines[BodyPart.ALL].AddRange(lines);
    }

    /// <summary>
    /// Adds a skeleton line to the skeleton
    /// </summary>
    /// <param name="line">LineSegment line</param>
    /// <param name="bodyPart">BodyPart bodyPart</param>
    protected void addSkeletonLine(LineSegment line, BodyPart bodyPart) {
        skeletonLines[bodyPart].Add(line);
        skeletonLines[BodyPart.ALL].Add(line);
    }

    /// <summary>
    /// Populates the skeletonBones dicitonary with bones
    /// </summary>
    protected abstract void makeAnimBones();

    /// <summary>
    /// Creates and binds a multi joint bone. 
    /// </summary>
    /// <param name="line">Line to create multiple bones for</param>
    /// <param name="parent">parent bone</param>
    /// <param name="jointCount">Number of joints in bone</param>
    /// <param name="name">Name of bone</param>
    /// <param name="bodyPart">Bodypart of bone</param>
    /// <returns>List<Bone> bones</returns>
    protected List<Bone> createAndBindBones(LineSegment line, Transform parent, int jointCount, string name, BodyPart bodyPart) {
        List<Bone> bones = new List<Bone>();
        float jointLength = line.length / jointCount;
        for(int i = 0; i < jointCount; i++) {
            LineSegment skinningBone = new LineSegment(line.a + line.direction * jointLength * i, line.a + line.direction * jointLength * (i + 1));
            Bone bone = createAndBindBone(skinningBone.a, parent, skinningBone, name, bodyPart);
            parent = bone.bone;
            bones.Add(bone);
        }
        //add Foot
        createAndBindBone(line.b, parent, name + " effector", bodyPart);
        return bones;
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
    protected Bone createAndBindBone(Vector3 pos, Transform parent, LineSegment skinningBone, string name, BodyPart bodyPart) {
        Bone bone = createAndBindBone(pos, parent, name, bodyPart);
        skinningBones.Add(new SkinningBone(skeletonBones[BodyPart.ALL].Count - 1, skinningBone));
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
    protected Bone createAndBindBone(Vector3 pos, Transform parent, List<LineSegment> skinningBone, string name, BodyPart bodyPart) {
        Bone bone = createAndBindBone(pos, parent, name, bodyPart);
        for (int i = 0; i < skinningBone.Count; i++) {
            skinningBones.Add(new SkinningBone(skeletonBones[BodyPart.ALL].Count - 1, skinningBone[i]));
        }
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
    protected Bone createAndBindBone(Vector3 pos, Transform parent, string name, BodyPart bodyPart) {
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
    virtual protected void createColliders() {
        Bone spine = skeletonBones[BodyPart.SPINE][0];
        BoxCollider col = spine.bone.gameObject.AddComponent<BoxCollider>();
        Vector3 size = col.size;
        size.z = bodyParameters.Get<float>(BodyParameter.SPINE_LENGTH) + 0.1f;
        col.size = size;
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

        float scale = bodyParameters.Get<float>(BodyParameter.SCALE);
        LineStructureBounds bounds = new LineStructureBounds(skeletonLines[BodyPart.ALL], LineStructureType.ANIMAL, (pointMapBoundsModifier * scale + 2.5f), (voxelSize * scale));

        BlockDataMap pointMap = new BlockDataMap(bounds.sizeI.x, bounds.sizeI.y, bounds.sizeI.z);
        List<LineSegment> skeleton = skeletonLines[BodyPart.ALL];
        for (int x = 0; x < pointMap.GetLength(0); x++) {
            for (int y = 0; y < pointMap.GetLength(1); y++) {
                for (int z = 0; z < pointMap.GetLength(2); z++) {
                    int i = pointMap.index1D(x, y, z);
                    Vector3 samplePos = new Vector3(x, y, z) * (voxelSize * scale) + bounds.lowerBounds;
                    pointMap.mapdata[i] = new BlockData(calcBlockType(skeleton, samplePos, scale), BlockData.BlockType.NONE);
                }
            }
        }
        meshData = new MeshData();
        meshData = MeshDataGenerator.GenerateMeshData(pointMap, (voxelSize * scale), -(bounds.lowerBounds / (voxelSize * scale)), MeshDataGenerator.MeshDataType.ANIMAL)[0];
    }

    /// <summary>
    /// Calculates the blocktype of the position
    /// </summary>
    /// <param name="pos">Position to examine</param>
    /// <returns>Blocktype</returns>
    private BlockData.BlockType calcBlockType(List<LineSegment> skeleton, Vector3 pos, float scale) {
        //List<LineSegment> skeleton = skeletonLines[BodyPart.ALL]; This line of code is here for historical reasons
        //This line of code is the most expesive line of code in the history, removing it in favor of passing the list as an argument
        //Increased the performance of animal generation by about 1000% when using 16 cores.
        for (int i = 0; i < skeleton.Count; i++) { 
            float dist = skeleton[i].distance(pos);
            if (dist < (skeleton[i].radius)) {
                return BlockData.BlockType.ANIMAL;
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

        for (int i = 0; i < skinningBones.Count; i++) {
            float dist = skinningBones[i].boneLine.distance(vert);            

            if (dist < bestDist) {
                bestIndex = skinningBones[i].indexOfBone;
                bestDist = dist;
            }            
        }

        BoneWeight boneWeight = new BoneWeight();
        boneWeight.boneIndex0 = bestIndex;
        boneWeight.weight0 = 1f;
        return boneWeight;
    }

    //    _____       _                    _____          _      
    //   |  __ \     | |                  / ____|        | |     
    //   | |  | | ___| |__  _   _  __ _  | |     ___   __| | ___ 
    //   | |  | |/ _ \ '_ \| | | |/ _` | | |    / _ \ / _` |/ _ \
    //   | |__| |  __/ |_) | |_| | (_| | | |___| (_) | (_| |  __/
    //   |_____/ \___|_.__/ \__,_|\__, |  \_____\___/ \__,_|\___|
    //                             __/ |                         
    //                            |___/                          

    /// <summary>
    /// Generates the extreme minimum of maximum of bodyparams for debugging
    /// </summary>
    /// <param name="maxFloat">generate max float values? false for minimums</param>
    /// <param name="maxInt">generate max int values? false for minimums</param>
    private void generateBodyParamsDebug(bool maxFloat, bool maxInt) {
        bodyParameters.Clear();
        bool scaleReady = false;
        foreach (KeyValuePair<BodyParameter, object> pair in bodyParametersRange.getDict()) {
            float scale = 1;
            if (scaleReady) {
                scale = bodyParameters.Get<float>(BodyParameter.SCALE);
            }


            if (pair.Value.GetType().Equals(typeof(Range<float>))) {
                Range<float> range = (Range<float>)pair.Value;
                if (maxFloat) {
                    bodyParameters.Add(pair.Key, rng.randomFloat(range.Max, range.Max) * scale);
                } else {
                    bodyParameters.Add(pair.Key, rng.randomFloat(range.Min, range.Min) * scale);
                }
            }


            if (pair.Value.GetType().Equals(typeof(Range<int>))) {
                Range<int> range = (Range<int>)pair.Value;
                if (maxInt) {
                    bodyParameters.Add(pair.Key, rng.randomInt(range.Max - 1, range.Max - 1));
                } else {
                    bodyParameters.Add(pair.Key, rng.randomInt(range.Min, range.Min));
                }
            }

            if (!scaleReady) { //Scale is the first enum
                scaleReady = true;
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
}

