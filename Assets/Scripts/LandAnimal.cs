using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SkinnedMeshRenderer))]
[RequireComponent(typeof(Rigidbody))]
public abstract class LandAnimal : MonoBehaviour {
    protected AnimalSkeleton skeleton;
    private float ikSpeed = 10;
    private const float ikTolerance = 0.1f;

    protected Vector3 desiredHeading = Vector3.zero;
    protected Vector3 heading = Vector3.zero;
    protected Vector3 spineHeading = Vector3.forward;
    private const float headingChangeRate = 5f;

    protected float desiredSpeed = 2f;
    protected float speed = 2f;
    private const float acceleration = 5f;
    protected const float walkSpeed = 5f;
    protected const float runSpeed = walkSpeed * 4f;

    protected bool grounded = false;
    protected Vector3 gravity = Physics.gravity;
    private const float levelSpeed = 3f;

    private float timer = 0;

    // Update is called once per frame
    void FixedUpdate() {
        if (Vector3.Angle(heading, desiredHeading) > 0.1f) {
            heading = Vector3.RotateTowards(heading, desiredHeading, Time.deltaTime * headingChangeRate, 1f);
        }
        if (Mathf.Abs(desiredSpeed - speed) > 0.2f) {
            speed += Mathf.Sign(desiredSpeed - speed) * Time.deltaTime * acceleration;
        }

        if (skeleton != null) {
            move();
            levelSpine();
            walk();
            timer += Time.deltaTime * speed / 2f;
        }
    }

    /// <summary>
    /// sets the skeleton, and applies the new mesh.
    /// </summary>
    public void setSkeleton(AnimalSkeleton skeleton) {
        this.skeleton = skeleton;
        GetComponent<SkinnedMeshRenderer>().sharedMesh = skeleton.getMesh();
        GetComponent<SkinnedMeshRenderer>().rootBone = transform;

        List<Bone> skeletonBones = skeleton.getBones(BodyPart.ALL);
        Transform[] bones = new Transform[skeletonBones.Count];
        for (int i = 0; i < bones.Length; i++) {
            bones[i] = skeletonBones[i].bone;
        }
        GetComponent<SkinnedMeshRenderer>().bones = bones;
    }

    public void resetJoints() {
        foreach (Bone bone in skeleton.getBones(BodyPart.ALL)) {
            bone.bone.transform.rotation = Quaternion.identity;
        }
    }

    protected abstract void move();

    /// <summary>
    /// Tries to level the spine with the ground
    /// </summary>
    private void levelSpine() {
        float spineLength = skeleton.getBodyParameter<float>(BodyParameter.SPINE_LENGTH);
        Bone spine = skeleton.getBones(BodyPart.SPINE)[0];

        RaycastHit hit1;
        RaycastHit hit2;
        Physics.Raycast(new Ray(spine.bone.position + spine.bone.forward * spineLength / 2f, Vector3.down), out hit1);
        Physics.Raycast(new Ray(spine.bone.position - spine.bone.forward * spineLength / 2f, Vector3.down), out hit2);
        Vector3 a = hit1.point - hit2.point;
        Vector3 b = spine.bone.forward * spineLength;

        float angle = Mathf.Acos(Vector3.Dot(a, b) / (a.magnitude * b.magnitude));
        Vector3 normal = Vector3.Cross(a, b);
        if (angle > 0.01f) {
            spine.bone.rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg * levelSpeed * Time.deltaTime, -normal) * spine.bone.rotation;
            if (!checkConstraints(spine)) {
                spine.bone.rotation = Quaternion.AngleAxis(-angle * Mathf.Rad2Deg * levelSpeed * Time.deltaTime, -normal) * spine.bone.rotation;
            }
        }

        RaycastHit hit;
        if (Physics.Raycast(new Ray(spine.bone.position, -spine.bone.up), out hit)) {
            Vector3[] groundLine = new Vector3[2] { spine.bone.position, hit.point };

            float stanceHeight = skeleton.getBodyParameter<float>(BodyParameter.LEG_LENGTH) / 2;
            float dist2ground = Vector3.Distance(hit.point, spine.bone.position);
            if (Mathf.Abs(stanceHeight - dist2ground) <= stanceHeight) {
                gravity += -Physics.gravity * (stanceHeight - dist2ground) / stanceHeight * Time.deltaTime;
            } else {
                gravity += Physics.gravity * Time.deltaTime;
            }

            if (dist2ground <= stanceHeight + 0.2f) {
                grounded = true;
            } else {
                grounded = false;
            }
        }

        spineHeading = spine.bone.rotation * Vector3.back;
    }

    /// <summary>
    /// Makes the animal do a walking animation
    /// </summary>
    private void walk() {
        int legPairs = skeleton.getBodyParameter<int>(BodyParameter.LEG_PAIRS);
        for(int i = 0; i < legPairs; i++) {
            walkLeg(skeleton.getLeg(true, i), -1, Mathf.PI * i);
            walkLeg(skeleton.getLeg(false, i), 1, Mathf.PI * (i + 1));            
        }
    }

    /// <summary>
    /// Uses IK to make the leg walk.
    /// </summary>
    /// <param name="leg">List<Bone> leg</param>
    /// <param name="sign">int sign, used to get a correct offset for IK target</param>
    /// <param name="radOffset">Walk animation offset in radians</param>
    private bool walkLeg(List<Bone> leg, int sign, float radOffset) {
        float legLength = skeleton.getBodyParameter<float>(BodyParameter.LEG_LENGTH);
        float jointLength = skeleton.getBodyParameter<float>(BodyParameter.LEG_JOINT_LENGTH);

        Vector3 target = leg[0].bone.position + sign * transform.right * legLength / 4f; //Offset to the right
        target += heading * Mathf.Cos(timer + radOffset) * legLength / 4f;  //Forward/Backward motion
        float rightOffset = (Mathf.Sin(timer + Mathf.PI + radOffset)) * legLength / 8f; //Right/Left motion
        rightOffset = (rightOffset > 0) ? rightOffset : 0;
        target += sign * transform.right * rightOffset;

        Vector3 subTarget = target;
        subTarget.y -= jointLength / 2f;
        for (int i = 0; i < leg.Count - 1; i++) {
            ccd(leg.GetRange(i, 2), target, ikSpeed / 4f);
        }

        RaycastHit hit;
        if (Physics.Raycast(new Ray(target, Vector3.down), out hit)) {
            float heightOffset = (Mathf.Sin(timer + Mathf.PI + radOffset)) * legLength / 8f; //Up/Down motion
            heightOffset = (heightOffset > 0) ? heightOffset : 0;

            target = hit.point;
            target.y += heightOffset;
            if (ccd(leg, target, ikSpeed)) {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Does one iteration of CCD
    /// </summary>
    /// <param name="limb">Limb to bend</param>
    /// <param name="target">Target to reach</param>
    /// <returns>Bool target reached</returns>
    private bool ccd(List<Bone> limb, Vector3 target, float speed) {
        Debug.DrawLine(target, target + Vector3.up * 10, Color.red);
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
    private bool checkConstraints(Bone bone) {
        Vector3 rotation = bone.bone.localEulerAngles;
        rotation.x = (rotation.x > 180) ? rotation.x - 360 : rotation.x;
        rotation.y = (rotation.y > 180) ? rotation.y - 360 : rotation.y;
        rotation.z = (rotation.z > 180) ? rotation.z - 360 : rotation.z;

        //Debug.Log("Rot: " + rotation + "__Min: " + bone.minAngles + "__Max: " + bone.maxAngles);
        bool min = rotation.x > bone.minAngles.x && rotation.y > bone.minAngles.y && rotation.z > bone.minAngles.z;
        bool max = rotation.x < bone.maxAngles.x && rotation.y < bone.maxAngles.y && rotation.z < bone.maxAngles.z;
        return min && max;
    }

    private void OnCollisionEnter(Collision collision) {
        gravity = Vector3.zero;
    }
}
