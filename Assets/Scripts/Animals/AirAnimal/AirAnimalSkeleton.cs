using UnityEngine;
using System.Collections.Generic;

public class AirAnimalSkeleton : AnimalSkeleton {

    /// <summary>
    /// Constructor that does the mainThread skeleton generation, and binds skeleton to the passed transform
    /// </summary>
    /// <param name="root">Transform to bind skeleton to</param>
    public AirAnimalSkeleton(Transform root) {
        bodyParametersRange = new MixedDictionary<BodyParameter>(new Dictionary<BodyParameter, object>() {
                { BodyParameter.SCALE, new Range<float>(0.5f, 1.0f) },

                { BodyParameter.HEAD_SIZE, new Range<float>(2f, 4f) },
                { BodyParameter.HEAD_RADIUS, new Range<float>(0.5f, 1.0f) },

                { BodyParameter.NECK_LENGTH, new Range<float>(4, 5) },
                { BodyParameter.NECK_RADIUS, new Range<float>(0.5f, 0.8f) },

                { BodyParameter.SPINE_LENGTH, new Range<float>(4, 7) },
                { BodyParameter.SPINE_RADIUS, new Range<float>(0.5f, 1.0f) },

                { BodyParameter.LEG_PAIRS, new Range<int>(1, 1) },
                { BodyParameter.LEG_JOINTS, new Range<int>(2, 3) },
                { BodyParameter.LEG_LENGTH, new Range<float>(5, 10) },
                //LEG_JOINT_LENGTH is calculated from LEG_JOINTS and LEG_LENGTH
                { BodyParameter.LEG_RADIUS, new Range<float>(0.5f, 0.7f) },

                { BodyParameter.TAIL_JOINTS, new Range<int>(2, 5) },
                { BodyParameter.TAIL_LENGTH, new Range<float>(3, 12) },
                //TAIL_JOINT_LENGTH is calculated from TAIL_JOINTS and TAIL_LENGTH
                { BodyParameter.TAIL_RADIUS, new Range<float>(0.5f, 0.8f) },

                { BodyParameter.WING_LENGTH, new Range<float>(10f, 20f) },
                { BodyParameter.WING_JOINTS, new Range<int>(2, 2) },
                { BodyParameter.WING_RADIUS, new Range<float>(0.5f, 0.75f) }
            }
        );

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
    /// Gets the main bones in a wing
    /// </summary>
    /// <param name="rightWing">bool rightWing</param>
    /// <returns>The bones</returns>
    public List<Bone> getWing(bool rightWing) {
        BodyPart bodyPart = (rightWing) ? BodyPart.RIGHT_WING : BodyPart.LEFT_WING;
        return skeletonBones[bodyPart].GetRange(0, bodyParameters.Get<int>(BodyParameter.WING_JOINTS));
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
        bodyParameters.Add(
            BodyParameter.WING_JOINT_LENGTH,
            bodyParameters.Get<float>(BodyParameter.WING_LENGTH) / bodyParameters.Get<int>(BodyParameter.WING_JOINTS)
        );
    }

    /// <summary>
    /// Makes the skeletonLines for the AnimalSkeleton
    /// </summary>
    override protected void makeSkeletonLines() {
        generateBodyParams();
        //generateBodyParamsDebug(true, true);
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
            spineLine.a + new Vector3(0, 0, 1.0f).normalized * bodyParameters.Get<float>(BodyParameter.NECK_LENGTH),
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
            spineLine.b + new Vector3(0, 0, -1.0f).normalized * bodyParameters.Get<float>(BodyParameter.TAIL_LENGTH),
            bodyParameters.Get<float>(BodyParameter.TAIL_RADIUS)
        );
        addSkeletonLine(tailLine, BodyPart.TAIL);
        //LEGS
        float legLength = bodyParameters.Get<float>(BodyParameter.LEG_LENGTH);
        float legRadius = bodyParameters.Get<float>(BodyParameter.LEG_RADIUS);
        LineSegment right = new LineSegment(spineLine.b, spineLine.b + new Vector3(-0.5f, -0.5f, 0).normalized * legLength, legRadius);
        LineSegment left = new LineSegment(spineLine.b, spineLine.b + new Vector3(0.5f, -0.5f, 0).normalized * legLength, legRadius);
        addSkeletonLine(right, BodyPart.RIGHT_LEGS);
        addSkeletonLine(left, BodyPart.LEFT_LEGS);
        //WINGS
        List<LineSegment> rightWing = createWing(spineLine, true);
        List<LineSegment> leftWing = createWing(spineLine, false);
        addSkeletonLines(rightWing, BodyPart.RIGHT_WING);
        addSkeletonLines(leftWing, BodyPart.LEFT_WING);
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
                Vector3 midHead = new Vector3(i, j * 0.5f, 1) * headSize / 2f + neckEnd;
                float radius = bodyParameters.Get<float>(BodyParameter.HEAD_RADIUS);
                head.Add(new LineSegment(neckEnd, midHead, radius));
                head.Add(new LineSegment(midHead, nose, radius));
            }
        }
        return head;
    }

    private List<LineSegment> createWing(LineSegment spine, bool rightWing) {
        List<LineSegment> wing = new List<LineSegment>();
        float xDir = (rightWing) ? 1 : -1;
        Vector3 spineCenter = Vector3.Lerp(spine.a, spine.b, 0.5f);
        float radius = bodyParameters.Get<float>(BodyParameter.WING_RADIUS);
        float len = bodyParameters.Get<float>(BodyParameter.WING_LENGTH);

        LineSegment centerBone = new LineSegment(spineCenter, spineCenter + new Vector3(xDir, 0, 0) * len, radius);
        wing.Add(centerBone);
        float[] t = new float[] { 0.1f, 0.3f, 0.7f, 0.9f };
        for (int i = 0; i < 4; i++) {
            Vector3 spineCenterOffset = Vector3.Lerp(spine.a, spine.b, t[i]);
            wing.Add(new LineSegment(spineCenterOffset, centerBone.b, radius));
        }
        return wing;
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
        Bone neckBone = createAndBindBone(skeletonLines[BodyPart.NECK][0].b, neckBoneBase.bone, "Neck", BodyPart.NECK);
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
        //WINGS
        createAndBindWing(BodyPart.RIGHT_WING, spineBone, "Right Wing");
        createAndBindWing(BodyPart.LEFT_WING, spineBone, "Left Wing");
    }

    /// <summary>
    /// Creates and binds the bones for a wing
    /// </summary>
    /// <param name="bodyPart">Bodypart for the wing</param>
    /// <param name="spineBone">Spine bone of the skeleton</param>
    /// <param name="name">Name for the wing</param>
    private void createAndBindWing(BodyPart bodyPart, Bone spineBone, string name) {
        if (bodyPart != BodyPart.RIGHT_WING && bodyPart != BodyPart.LEFT_WING) {
            throw new System.Exception("createAndBindWing ERROR! Bodypart is not a wing! you provided: " + bodyPart.ToString());
        }
        int wingJointCount = bodyParameters.Get<int>(BodyParameter.WING_JOINTS);
        List<LineSegment> wing = skeletonLines[bodyPart];
        List<Bone> wingBones = createAndBindBones(wing[0], spineBone.bone, wingJointCount, name, bodyPart);
        for (int i = 1; i < wing.Count; i++) {
            LineSegment boneLine = wing[i];
            LineSegment subBone = new LineSegment(boneLine.a, Vector3.Lerp(boneLine.a, boneLine.b, 0.5f));
            createAndBindBone(subBone.a, wingBones[0].bone, subBone, name, bodyPart);
            subBone = new LineSegment(subBone.b, boneLine.b);
            createAndBindBone(subBone.a, wingBones[1].bone, subBone, name, bodyPart);
        }
    }
}
