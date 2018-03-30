using UnityEngine;
using System.Collections.Generic;

public class WaterAnimalSkeleton : AnimalSkeleton {

    /// <summary>
    /// Constructor that does the mainThread skeleton generation, and binds skeleton to the passed transform
    /// </summary>
    /// <param name="root">Transform to bind skeleton to</param>
    public WaterAnimalSkeleton(Transform root, int seed = -1) {
        bodyParametersRange = new MixedDictionary<BodyParameter>(new Dictionary<BodyParameter, object>() {
                { BodyParameter.SCALE, new Range<float>(0.5f, 1.0f) },

                { BodyParameter.HEAD_SIZE, new Range<float>(2f, 4f) },
                { BodyParameter.HEAD_RADIUS, new Range<float>(0.5f, 1.0f) },

                { BodyParameter.SPINE_LENGTH, new Range<float>(7, 17) },
                { BodyParameter.SPINE_JOINTS, new Range<int>(3, 7) },
                { BodyParameter.SPINE_RADIUS, new Range<float>(1f, 2.0f) },
            }
        );

        base.rng = new ThreadSafeRng(seed == -1 ? seedGen.randomInt() : seed);

        generateInMainThread(root);
    }

    /// <summary>
    /// Generates the bodyparamaters for the skeleton,
    /// they are stored in the bodyParameters dictionary
    /// </summary>
    override protected void generateBodyParams() {
        base.generateBodyParams();
        bodyParameters.Add(
            BodyParameter.SPINE_JOINT_LENGTH,
            bodyParameters.Get<float>(BodyParameter.SPINE_LENGTH) / bodyParameters.Get<int>(BodyParameter.SPINE_JOINTS)
        );        
    }

    /// <summary>
    /// Makes the skeletonLines for the AnimalSkeleton
    /// </summary>
    override protected void makeSkeletonLines() {
        generateBodyParams();
        //SPINE
        int spineJoints = bodyParameters.Get<int>(BodyParameter.SPINE_JOINTS);
        float spineJointLen = bodyParameters.Get<float>(BodyParameter.SPINE_JOINT_LENGTH);
        float spineRadius = bodyParameters.Get<float>(BodyParameter.SPINE_RADIUS);
        float spineLen = bodyParameters.Get<float>(BodyParameter.SPINE_LENGTH);

        Vector3 spineDir = Vector3.back;
        Vector3 root = Vector3.zero;
        for (int i = 0; i < spineJoints; i++) {
            LineSegment bone = new LineSegment(root + spineDir * i * spineJointLen, root + spineDir * (i + 1) * spineJointLen, spineRadius);
            float radius = Mathf.Lerp(spineRadius, spineRadius / 2f, ((bone.a - root).magnitude / spineLen));
            bone.radius = radius;
            addSkeletonLine(bone, BodyPart.SPINE);
        }
        
        //HEAD
        List<LineSegment> head = createHead(root);
        addSkeletonLines(head, BodyPart.HEAD);        
    }

    /// <summary>
    /// Creates the head of the animal
    /// </summary>
    /// <param name="headSize">float headSize</param>
    /// <returns>List<LineSegment> head</returns>
    private List<LineSegment> createHead(Vector3 neckEnd) {
        List<LineSegment> head = new List<LineSegment>();
        float headSize = bodyParameters.Get<float>(BodyParameter.HEAD_SIZE);
        for (int i = -1; i <= 1; i += 2) {
            for (int j = -1; j <= 1; j += 2) {
                Vector3 nose = new Vector3(0, 0, headSize) + neckEnd;
                Vector3 midHead = new Vector3(i * 0.5f, j * 1.5f, 1) * headSize / 2f + neckEnd;
                float radius = bodyParameters.Get<float>(BodyParameter.HEAD_RADIUS);
                head.Add(new LineSegment(neckEnd, midHead, radius));
                head.Add(new LineSegment(midHead, nose, radius));
            }
        }
        return head;
    }

    /// <summary>
    /// Populates the skeletonBones dicitonary with bones
    /// </summary>
    override protected void makeAnimBones() {
        //SPINE
        List<LineSegment> spineLines = skeletonLines[BodyPart.SPINE];
        LineSegment rootSkinningBone = new LineSegment(spineLines[0].a, -spineLines[0].b);
        Transform parent = createAndBindBone(spineLines[0].a, rootBone, rootSkinningBone, "Spine Root", BodyPart.SPINE).bone;
        foreach (LineSegment spineLine in spineLines) {           
            parent = createAndBindBone(spineLine.a, parent, spineLine, "Spine", BodyPart.SPINE).bone;
        }
    }

    /// <summary>
    /// Adds colliders to the skeleton
    /// </summary>
    override protected void createColliders() {
        Bone spine = skeletonBones[BodyPart.SPINE][0];
        BoxCollider col = spine.bone.gameObject.AddComponent<BoxCollider>();
        Vector3 size = col.size;
        size.z = bodyParameters.Get<float>(BodyParameter.SPINE_JOINT_LENGTH) + 0.1f;
        col.size = size;
    }
}
