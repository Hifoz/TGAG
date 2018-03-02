using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SkinnedMeshRenderer))]
[RequireComponent(typeof(Rigidbody))]
public abstract class LandAnimal : MonoBehaviour {
    protected AnimalSkeleton skeleton;
    private float ikSpeed = 10f;
    private const float ikTolerance = 0.005f;

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

    bool ragDolling = false;
    delegate bool ragDollCondition(); 

    // Update is called once per frame
    void FixedUpdate() {
        if (skeleton != null) {
            calculateSpeedAndHeading();
            move();
            levelSpine();
            doGravity();
            handleRagdoll();
        }
    }

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
    public void setSkeleton(AnimalSkeleton skeleton) {
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

        List<Bone> tail = skeleton.getBones(BodyPart.TAIL);
        LineSegment tailLine = skeleton.getLines(BodyPart.TAIL)[0];
        StartCoroutine(ragdollLimb(tail, tailLine, () => { return true; }, false, 4f, transform));
    }

    /// <summary>
    /// Resets the rotation of all joints, mostly for debugging
    /// </summary>
    public void resetJoints() {
        foreach (Bone bone in skeleton.getBones(BodyPart.ALL)) {
            bone.bone.transform.rotation = Quaternion.identity;
        }
    }

    protected abstract void move();

    /// <summary>
    /// Function for handling ragdoll effects when free falling
    /// </summary>
    private void handleRagdoll() {
        if (grounded) {
            walk();
            timer += (Time.deltaTime * speed / 2f) / skeleton.getBodyParameter<float>(BodyParameter.SCALE);
            ragDolling = false;
        } else if (!grounded && !ragDolling) {
            ragDolling = true;
            for (int i = 0; i < skeleton.getBodyParameter<int>(BodyParameter.LEG_PAIRS); i++) {
                StartCoroutine(ragdollLimb(skeleton.getLeg(true, i), skeleton.getLines(BodyPart.RIGHT_LEGS)[i], () => { return !grounded; }));
                StartCoroutine(ragdollLimb(skeleton.getLeg(false, i), skeleton.getLines(BodyPart.LEFT_LEGS)[i], () => { return !grounded; }));
            }
            StartCoroutine(ragdollLimb(skeleton.getBones(BodyPart.NECK), skeleton.getLines(BodyPart.NECK)[0], () => { return !grounded; }, true));
        }
    }

    /// <summary>
    /// Function for calculating speed and heading
    /// </summary>
    private void calculateSpeedAndHeading() {
        if (Vector3.Angle(heading, desiredHeading) > 0.1f) {
            heading = Vector3.RotateTowards(heading, desiredHeading, Time.deltaTime * headingChangeRate, 1f);
        }
        if (Mathf.Abs(desiredSpeed - speed) > 0.2f) {
            if (grounded) {
                speed += Mathf.Sign(desiredSpeed - speed) * Time.deltaTime * acceleration;
            } else {
                speed += Mathf.Sign(desiredSpeed - speed) * Time.deltaTime * acceleration * 0.2f;
            }
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
    private IEnumerator ragdollLimb(List<Bone> limb, LineSegment model, ragDollCondition condition, bool returnAfter = false, float limbResistance = 1f, Transform referenceTransform = null) {
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
            for(int i = 0; i < rotations.Length; i++) {
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
    /// Tries to level the spine with the ground
    /// </summary>
    private void levelSpine() {
        Bone spine = skeleton.getBones(BodyPart.SPINE)[0];
        levelSpineWithAxis(transform.forward, spine.bone.forward, skeleton.getBodyParameter<float>(BodyParameter.SPINE_LENGTH));
        levelSpineWithAxis(transform.right, spine.bone.right, skeleton.getBodyParameter<float>(BodyParameter.LEG_JOINT_LENGTH));
        spineHeading = spine.bone.rotation * Vector3.forward;
    }

    /// <summary>
    /// Levels the spine with terrain along axis
    /// </summary>
    /// <param name="axis">Axis to level along</param>
    private void levelSpineWithAxis(Vector3 axis, Vector3 currentAxis, float length) {
        Bone spine = skeleton.getBones(BodyPart.SPINE)[0];

        Vector3 point1 = spine.bone.position + axis * length / 2f + Vector3.up * 20;
        Vector3 point2 = spine.bone.position - axis * length / 2f + Vector3.up * 20;

        int layerMask = 1 << 8;
        RaycastHit hit1;
        RaycastHit hit2;
        Physics.Raycast(new Ray(point1, Vector3.down), out hit1, 100f, layerMask);
        Physics.Raycast(new Ray(point2, Vector3.down), out hit2, 100f, layerMask);
        point1 = spine.bone.position + currentAxis * length / 2f;
        point2 = spine.bone.position - currentAxis * length / 2f;
        Vector3 a = hit1.point - hit2.point;
        Vector3 b = point1 - point2;

        float angle = Mathf.Acos(Vector3.Dot(a, b) / (a.magnitude * b.magnitude));
        Vector3 normal = Vector3.Cross(a, b);
        if (angle > 0.01f) {
            spine.bone.rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg * levelSpeed * Time.deltaTime, -normal) * spine.bone.rotation;
            if (!checkConstraints(spine)) {
                spine.bone.rotation = Quaternion.AngleAxis(-angle * Mathf.Rad2Deg * levelSpeed * Time.deltaTime, -normal) * spine.bone.rotation;
            }
        }
    }

    /// <summary>
    /// Does the physics for gravity
    /// </summary>
    private void doGravity() {
        Bone spine = skeleton.getBones(BodyPart.SPINE)[0];
        RaycastHit hit;
        if (Physics.Raycast(new Ray(spine.bone.position, -spine.bone.up), out hit, 200f, (1<<8))) {
            Vector3[] groundLine = new Vector3[2] { spine.bone.position, hit.point };

            float stanceHeight = skeleton.getBodyParameter<float>(BodyParameter.LEG_LENGTH) / 2;
            float dist2ground = Vector3.Distance(hit.point, spine.bone.position);
            float distFromStance = Mathf.Abs(stanceHeight - dist2ground);
            if (distFromStance <= stanceHeight) {
                grounded = true;
                float sign = Mathf.Sign(dist2ground - stanceHeight);
                if (distFromStance > stanceHeight / 16f && gravity.magnitude < Physics.gravity.magnitude * 1.5f) {
                    gravity = sign * Physics.gravity * Mathf.Pow(distFromStance / stanceHeight, 2);
                } else {
                    gravity += sign * Physics.gravity * Mathf.Pow(distFromStance / stanceHeight, 2) * Time.deltaTime;                    
                }
            } else {
                grounded = false;
                gravity += Physics.gravity * Time.deltaTime;
            }
        }
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
        Transform spine = skeleton.getBones(BodyPart.SPINE)[0].bone;

        Vector3 target = leg[0].bone.position + sign * spine.right * legLength / 4f; //Offset to the right
        target += heading * Mathf.Cos(timer + radOffset) * legLength / 4f;  //Forward/Backward motion
        float rightOffset = (Mathf.Sin(timer + Mathf.PI + radOffset)) * legLength / 8f; //Right/Left motion
        rightOffset = (rightOffset > 0) ? rightOffset : 0;
        target += sign * spine.right * rightOffset;

        Vector3 subTarget = target;
        subTarget.y -= jointLength / 2f;
        for (int i = 0; i < leg.Count - 1; i++) {
            ccd(leg.GetRange(i, 2), target, ikSpeed / 4f);
        }

        RaycastHit hit;
        if (Physics.Raycast(new Ray(target, spine.rotation * Vector3.down), out hit)) {
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
