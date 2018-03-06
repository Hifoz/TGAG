using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SkinnedMeshRenderer))]
[RequireComponent(typeof(Rigidbody))]
public abstract class Animal : MonoBehaviour {

    protected const float ikSpeed = 10f;
    protected const float ikTolerance = 0.005f;

    protected Vector3 desiredHeading = Vector3.zero;
    protected Vector3 heading = Vector3.zero;
    protected Vector3 spineHeading = Vector3.forward;
    protected float headingChangeRate = 5f;

    protected float desiredSpeed = 0;
    protected float speed = 0;
    protected float acceleration = 5f;

    protected AnimalSkeleton skeleton;
    protected delegate bool ragDollCondition();

    protected Rigidbody rb;

    virtual protected void Start() {
        rb = GetComponent<Rigidbody>();
    }

    protected abstract void move();

    /// <summary>
    /// Gets the underlying skeleton
    /// </summary>
    /// <returns>AnimalSkeleton</returns>
    public AnimalSkeleton getSkeleton() {
        return skeleton;
    }

    /// <summary>
    /// sets the skeleton, and applies the new mesh.
    /// </summary>
    virtual public void setSkeleton(AnimalSkeleton skeleton) {
        this.skeleton = skeleton;
        transform.rotation = Quaternion.identity;
        transform.localRotation = Quaternion.identity;
        if (skeleton != null) {
            foreach (Bone bone in skeleton.getBones(BodyPart.ALL)) {
                bone.bone.rotation = Quaternion.identity;
                bone.bone.localRotation = Quaternion.identity;
            }
        }

        skeleton.applyMeshData();
        GetComponent<SkinnedMeshRenderer>().rootBone = transform;

        List<Bone> skeletonBones = skeleton.getBones(BodyPart.ALL);
        Transform[] bones = new Transform[skeletonBones.Count];
        for (int i = 0; i < bones.Length; i++) {
            bones[i] = skeletonBones[i].bone;
        }
        GetComponent<SkinnedMeshRenderer>().bones = bones;
    }

    /// <summary>
    /// Resets the rotation of all joints, mostly for debugging
    /// </summary>
    public void resetJoints() {
        foreach (Bone bone in skeleton.getBones(BodyPart.ALL)) {
            bone.bone.transform.rotation = Quaternion.identity;
        }
    }

    /// <summary>
    /// Courutine for "ragdolling" a limb while condition is true.
    /// </summary>
    /// <param name="limb">Limb to ragdoll</param>
    /// <param name="model">Rigid position of limb</param>
    /// <param name="condition">condition to ragdoll by</param>
    /// <param name="returnAfter">Should the limb return to zero rotation after ragdoll</param>
    /// <param name="limbResistance">How much the limb resists displacement</param>
    /// <param name="referenceTransform">the reference transform is the transform to use for calculating desired limb positions</param>
    /// <returns></returns>
    protected IEnumerator ragdollLimb(List<Bone> limb, LineSegment model, ragDollCondition condition, bool returnAfter = false, float limbResistance = 1f, Transform referenceTransform = null) {
        Vector3[] desiredPositions = new Vector3[limb.Count - 1];
        Vector3[] currentPositions = new Vector3[limb.Count - 1];

        if (referenceTransform == null) {
            referenceTransform = skeleton.getBones(BodyPart.SPINE)[0].bone;
        }

        for (int i = 0; i < currentPositions.Length; i++) {
            currentPositions[i] = limb[i + 1].bone.position;
        }

        while (condition()) {
            for (int i = 0; i < desiredPositions.Length; i++) {
                desiredPositions[i] = limb[0].bone.position + referenceTransform.rotation * model.direction * model.length * (i + 1) / limb.Count;
            }

            for (int i = 0; i < limb.Count - 1; i++) {
                ccd(limb.GetRange(i, 2), currentPositions[i], ikSpeed);
                float distance = Vector3.Distance(currentPositions[i], desiredPositions[i]);
                currentPositions[i] = Vector3.MoveTowards(currentPositions[i], desiredPositions[i], Time.deltaTime * distance * limbResistance);
            }
            yield return 0;
        }

        if (returnAfter) {
            Quaternion[] rotations = new Quaternion[limb.Count];
            for (int i = 0; i < rotations.Length; i++) {
                rotations[i] = limb[i].bone.localRotation;
            }

            for (float t = 0; t <= 1f; t += Time.deltaTime * 6f) {
                for (int i = 0; i < rotations.Length; i++) {
                    limb[i].bone.localRotation = Quaternion.Lerp(rotations[i], Quaternion.identity, t);
                }
                yield return 0;
            }
            for (int i = 0; i < rotations.Length; i++) {
                limb[i].bone.localRotation = Quaternion.identity;
            }
        }
    }

    /// <summary>
    /// Does one iteration of CCD
    /// </summary>
    /// <param name="limb">Limb to bend</param>
    /// <param name="target">Target to reach</param>
    /// <returns>Bool target reached</returns>
    protected bool ccd(List<Bone> limb, Vector3 target, float speed = ikSpeed) {
        //Debug.DrawLine(target, target + Vector3.up * 10, Color.red);
        Transform effector = limb[limb.Count - 1].bone;
        float dist = Vector3.Distance(effector.position, target);

        if (dist > ikTolerance) {
            for (int i = limb.Count - 1; i >= 0; i--) {
                Transform bone = limb[i].bone;

                Vector3 a = effector.position - bone.position;
                Vector3 b = target - bone.position;


                float angle = Mathf.Acos(Vector3.Dot(a, b) / (a.magnitude * b.magnitude));
                Vector3 normal = Vector3.Cross(a, b);
                if (angle > 0.01f) {
                    bone.rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg * speed * Time.deltaTime, normal) * bone.rotation;
                    if (!checkConstraints(limb[i])) {
                        bone.rotation = Quaternion.AngleAxis(-angle * Mathf.Rad2Deg * speed * Time.deltaTime, normal) * bone.rotation;
                    }
                }
            }
        }
        return dist < ikTolerance;
    }

    /// <summary>
    /// Checks the joint constraints for the bone
    /// </summary>
    /// <param name="bone">Bone bone, the bone to check</param>
    /// <returns>True if bone satisfies bone constraints, false otherwise</returns>
    protected bool checkConstraints(Bone bone) {
        Vector3 rotation = bone.bone.localEulerAngles;
        rotation.x = (rotation.x > 180) ? rotation.x - 360 : rotation.x;
        rotation.y = (rotation.y > 180) ? rotation.y - 360 : rotation.y;
        rotation.z = (rotation.z > 180) ? rotation.z - 360 : rotation.z;

        //Debug.Log("Rot: " + rotation + "__Min: " + bone.minAngles + "__Max: " + bone.maxAngles);
        bool min = rotation.x > bone.minAngles.x && rotation.y > bone.minAngles.y && rotation.z > bone.minAngles.z;
        bool max = rotation.x < bone.maxAngles.x && rotation.y < bone.maxAngles.y && rotation.z < bone.maxAngles.z;
        return min && max;
    }
}
