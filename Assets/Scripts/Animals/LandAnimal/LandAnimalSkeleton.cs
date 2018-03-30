using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// The skeleton of a LandAnimal
/// </summary>
public class LandAnimalSkeleton : AnimalSkeleton {

    /// <summary>
    /// Constructor that does the mainThread skeleton generation, and binds skeleton to the passed transform
    /// </summary>
    /// <param name="root">Transform to bind skeleton to</param>
    public LandAnimalSkeleton(Transform root, int seed = -1) {
        bodyParametersRange = new MixedDictionary<BodyParameter>(new Dictionary<BodyParameter, object>() {
                { BodyParameter.SCALE, new Range<float>(0.5f, 1.0f) },

                { BodyParameter.HEAD_SIZE, new Range<float>(2f, 4f) },
                { BodyParameter.HEAD_RADIUS, new Range<float>(0.5f, 1.0f) },

                { BodyParameter.NECK_LENGTH, new Range<float>(4, 5) },
                { BodyParameter.NECK_RADIUS, new Range<float>(0.5f, 1.5f) },

                { BodyParameter.SPINE_LENGTH, new Range<float>(4, 7) },
                { BodyParameter.SPINE_RADIUS, new Range<float>(1.0f, 1.5f) },

                { BodyParameter.LEG_PAIRS, new Range<int>(2, 4) },
                { BodyParameter.LEG_JOINTS, new Range<int>(2, 3) },
                { BodyParameter.LEG_LENGTH, new Range<float>(5, 10) },
                //LEG_JOINT_LENGTH is calculated from LEG_JOINTS and LEG_LENGTH
                { BodyParameter.LEG_RADIUS, new Range<float>(0.5f, 0.7f) },

                { BodyParameter.TAIL_JOINTS, new Range<int>(2, 5) },
                { BodyParameter.TAIL_LENGTH, new Range<float>(3, 12) },
                //TAIL_JOINT_LENGTH is calculated from TAIL_JOINTS and TAIL_LENGTH
                { BodyParameter.TAIL_RADIUS, new Range<float>(0.5f, 1.5f) }
            }
        );

        base.rng = new ThreadSafeRng(seed == -1 ? seedGen.randomInt() : seed);

        generateInMainThread(root);
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
    /// Generates the bodyparamaters for the skeleton,
    /// they are stored in the bodyParameters dictionary
    /// </summary>
    override protected void generateBodyParams() {
        base.generateBodyParams();
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
    override protected void makeSkeletonLines() {
        generateBodyParams();
        //SPINE
        float spineLen = bodyParameters.Get<float>(BodyParameter.SPINE_LENGTH);
        LineSegment spineLine = new LineSegment(
            Vector3.forward * spineLen / 2f,
            -Vector3.forward * spineLen / 2f,
            bodyParameters.Get<float>(BodyParameter.SPINE_RADIUS)
        );
        addSkeletonLine(spineLine, BodyPart.SPINE);
        //NECK
        LineSegment neckLine = new LineSegment(
            spineLine.a,
            spineLine.a + new Vector3(0, 0.5f, 0.5f).normalized * bodyParameters.Get<float>(BodyParameter.NECK_LENGTH),
            bodyParameters.Get<float>(BodyParameter.NECK_RADIUS)
        );
        addSkeletonLine(neckLine, BodyPart.NECK);
        //HEAD
        List<LineSegment> head = createHead(neckLine.b);
        addSkeletonLines(head, BodyPart.HEAD);
        //TAIL
        LineSegment tailLine;
        tailLine = new LineSegment(
            spineLine.b,
            spineLine.b + new Vector3(0, 0.5f, -0.5f).normalized * bodyParameters.Get<float>(BodyParameter.TAIL_LENGTH),
            bodyParameters.Get<float>(BodyParameter.TAIL_RADIUS)
        );
        addSkeletonLine(tailLine, BodyPart.TAIL);
        //LEGS
        int legPairs = bodyParameters.Get<int>(BodyParameter.LEG_PAIRS);
        float legLength = bodyParameters.Get<float>(BodyParameter.LEG_LENGTH);
        float legRadius = bodyParameters.Get<float>(BodyParameter.LEG_RADIUS);
        float spineLength = bodyParameters.Get<float>(BodyParameter.SPINE_LENGTH);
        for (int i = 0; i < legPairs; i++) {
            Vector3 offset = -Vector3.forward * spineLength * ((float)i / (legPairs - 1)) + spineLine.a;
            LineSegment right = new LineSegment(new Vector3(0, 0, 0), new Vector3(0.5f, -0.5f, 0).normalized * legLength, legRadius) + offset;
            LineSegment left = new LineSegment(new Vector3(0, 0, 0), new Vector3(-0.5f, -0.5f, 0).normalized * legLength, legRadius) + offset;
            addSkeletonLine(right, BodyPart.RIGHT_LEGS);
            addSkeletonLine(left, BodyPart.LEFT_LEGS);
        }
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
                Vector3 midHead = new Vector3(i, j, 1) * headSize / 2f + neckEnd;
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
        LineSegment spineLine = skeletonLines[BodyPart.SPINE][0];
        Bone spineBone = createAndBindBone(Vector3.Lerp(spineLine.a, spineLine.b, 0.5f), rootBone, spineLine, "Mid Spine", BodyPart.SPINE);
        spineBone.minAngles = new Vector3(-90, -1, -90);
        spineBone.maxAngles = new Vector3(90, 1, 90);
        //NECK
        Bone neckBoneBase = createAndBindBone(skeletonLines[BodyPart.NECK][0].a, spineBone.bone, skeletonLines[BodyPart.NECK][0], "Neck", BodyPart.NECK);
        neckBoneBase.minAngles = new Vector3(-90, -90, -90);
        neckBoneBase.maxAngles = new Vector3(90, 90, 90);
        createAndBindBone(skeletonLines[BodyPart.NECK][0].b, neckBoneBase.bone, skeletonLines[BodyPart.HEAD], "Neck", BodyPart.NECK);
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
}
