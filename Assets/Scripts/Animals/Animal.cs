using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SkinnedMeshRenderer))]
[RequireComponent(typeof(Rigidbody))]
public abstract class Animal : MonoBehaviour {

    //Animation stuff
    protected const float ikSpeed = 10f;
    protected const float ikTolerance = 0.05f;

    protected AnimalSkeleton skeleton;
    protected delegate bool ragDollCondition();

    protected AnimalAnimation currentAnimation;
    protected bool animationInTransition = false;

    //Physics stuff
    protected Vector3 desiredHeading = Vector3.zero;
    protected Vector3 heading = Vector3.zero;
    protected Vector3 spineHeading = Vector3.forward;
    protected float headingChangeRate = 5f;

    protected float desiredSpeed = 0;
    protected float speed = 0;
    protected float acceleration = 5f;

    private const float levelSpeed = 3f;

    protected bool grounded = false;
    protected Vector3 gravity = Physics.gravity;

    protected Rigidbody rb;

    //NPC stuff
    protected Vector3 roamCenter;


    virtual protected void Start() {
        rb = GetComponent<Rigidbody>();
    }

    //    _____       _     _ _         __                  _   _                 
    //   |  __ \     | |   | (_)       / _|                | | (_)                
    //   | |__) |   _| |__ | |_  ___  | |_ _   _ _ __   ___| |_ _  ___  _ __  ___ 
    //   |  ___/ | | | '_ \| | |/ __| |  _| | | | '_ \ / __| __| |/ _ \| '_ \/ __|
    //   | |   | |_| | |_) | | | (__  | | | |_| | | | | (__| |_| | (_) | | | \__ \
    //   |_|    \__,_|_.__/|_|_|\___| |_|  \__,_|_| |_|\___|\__|_|\___/|_| |_|___/
    //                                                                            
    //                                                                            

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

        animationInTransition = false;
        currentAnimation = null;
    }

    /// <summary>
    /// Sets the speed of the animal
    /// </summary>
    /// <param name="speed">Value to use</param>
    public void setSpeed(float speed) {
        this.speed = speed;
    }

    //    _   _                               _     _ _         __                  _   _                 
    //   | \ | |                             | |   | (_)       / _|                | | (_)                
    //   |  \| | ___  _ __ ______ _ __  _   _| |__ | |_  ___  | |_ _   _ _ __   ___| |_ _  ___  _ __  ___ 
    //   | . ` |/ _ \| '_ \______| '_ \| | | | '_ \| | |/ __| |  _| | | | '_ \ / __| __| |/ _ \| '_ \/ __|
    //   | |\  | (_) | | | |     | |_) | |_| | |_) | | | (__  | | | |_| | | | | (__| |_| | (_) | | | \__ \
    //   |_| \_|\___/|_| |_|     | .__/ \__,_|_.__/|_|_|\___| |_|  \__,_|_| |_|\___|\__|_|\___/|_| |_|___/
    //                           | |                                                                      
    //                           |_|                                                                      

    /// <summary>
    /// Gets the speed of the animal
    /// </summary>
    /// <returns>float speed</returns>
    public float getSpeed() {
        return speed;
    }

    protected abstract void move();
    protected abstract void calculateSpeedAndHeading();

    //                   _                 _   _                __                  _   _                 
    //       /\         (_)               | | (_)              / _|                | | (_)                
    //      /  \   _ __  _ _ __ ___   __ _| |_ _  ___  _ __   | |_ _   _ _ __   ___| |_ _  ___  _ __  ___ 
    //     / /\ \ | '_ \| | '_ ` _ \ / _` | __| |/ _ \| '_ \  |  _| | | | '_ \ / __| __| |/ _ \| '_ \/ __|
    //    / ____ \| | | | | | | | | | (_| | |_| | (_) | | | | | | | |_| | | | | (__| |_| | (_) | | | \__ \
    //   /_/    \_\_| |_|_|_| |_| |_|\__,_|\__|_|\___/|_| |_| |_|  \__,_|_| |_|\___|\__|_|\___/|_| |_|___/
    //                                                                                                    
    //                                                                                                    

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
                ccdPartial(limb.GetRange(i, 2), currentPositions[i], ikSpeed);
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
    /// <param name="maxIter">Max number of iterations</param>
    /// <returns>Bool target reached</returns>
    protected bool ccdComplete(List<Bone> limb, Vector3 target, int maxIter = 1) {
        Transform effector = limb[limb.Count - 1].bone;
        float dist = Vector3.Distance(effector.position, target);
        int iter = 0;
        while (dist > ikTolerance && iter <= maxIter) {
            for (int i = limb.Count - 1; i >= 0; i--) {
                Transform bone = limb[i].bone;

                Vector3 a = effector.position - bone.position;
                Vector3 b = target - bone.position;

                float angle = Mathf.Acos(Vector3.Dot(a, b) / (a.magnitude * b.magnitude));
                Vector3 normal = Vector3.Cross(a, b);
                if (angle > 0.01f) {
                    bone.rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, normal) * bone.rotation;
                    if (!checkConstraints(limb[i])) {
                        bone.rotation = Quaternion.AngleAxis(-angle * Mathf.Rad2Deg, normal) * bone.rotation;
                    }
                }
            }
            iter++;
        }
        return dist < ikTolerance;
    }

    /// <summary>
    /// Does one iteration of CCD
    /// </summary>
    /// <param name="limb">Limb to bend</param>
    /// <param name="target">Target to reach</param>
    /// <param name="speed">How quickly to move towards target</param>
    /// <returns>Bool target reached</returns>
    protected bool ccdPartial(List<Bone> limb, Vector3 target, float speed = ikSpeed) {
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

        bool min = rotation.x > bone.minAngles.x && rotation.y > bone.minAngles.y && rotation.z > bone.minAngles.z;
        bool max = rotation.x < bone.maxAngles.x && rotation.y < bone.maxAngles.y && rotation.z < bone.maxAngles.z;
        return min && max;
    }

    /// <summary>
    /// Attempts to ground the limb, moving the effector towards the ground
    /// </summary>
    /// <param name="limb">Limb to ground</param>
    /// <param name="maxRange">If height is more the maxRange, no attempt will be made</param>
    /// <param name="complete">Attempt to do a complete CCD or partial?</param>
    /// <returns></returns>
    protected bool groundLimb(List<Bone> limb, float maxRange = 1f, bool complete = true) {
        RaycastHit hit;
        Vector3 effector = limb[limb.Count - 1].bone.position;
        int layerMask = 1 << 8;
        if (Physics.Raycast(new Ray(effector + Vector3.up * 10, Vector3.down), out hit, 40f, layerMask)) {
            if (effector.y - hit.point.y <= maxRange) {
                if (complete) {
                    return ccdComplete(limb, hit.point, 20);
                } else {
                    return ccdPartial(limb, hit.point, ikSpeed);
                }
            } else {
                if (complete) {
                    return ccdComplete(limb, effector + Vector3.down * maxRange, 20);
                } else {
                    return ccdPartial(limb, effector + Vector3.down * maxRange, ikSpeed);
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Tries to transitions between the current animation and the next animation
    /// </summary>
    /// <param name="next">Animation to transition too</param>
    /// <param name="speedScaling">float for scaling the speed of animation</param>
    /// <param name="nextSpeedScaling">The speed scaling for the next anim</param>
    /// <param name="transitionTime">Time to spend on transition</param>
    /// <returns>Success flag</returns>
    protected bool tryAnimationTransition(AnimalAnimation next, float speedScaling = 1f, float nextSpeedScaling = 1f, float transitionTime = 1f) {
        if (!animationInTransition) {
            StartCoroutine(transistionAnimation(next, speedScaling, nextSpeedScaling, transitionTime));
        }
        return !animationInTransition;
    }

    /// <summary>
    /// Transitions between the current animation and the next animation
    /// </summary>
    /// <param name="next">Animation to transition too</param>
    /// <param name="speedScaling">float for scaling the speed of animation</param>
    /// <param name="transitionTime">Time to spend on transition</param>
    /// <returns></returns>
    private IEnumerator transistionAnimation(AnimalAnimation next, float speedScaling, float nextSpeedScaling, float transitionTime) {
        animationInTransition = true;
        for (float t = 0; t <= 1f; t += Time.deltaTime / transitionTime) {
            if (!animationInTransition) {
                break;
            }
            currentAnimation.animateLerp(next, t, speed * Mathf.Lerp(speedScaling, nextSpeedScaling, t));
            yield return 0;
        }
        currentAnimation = next;
        animationInTransition = false;
    }

    //    _____  _               _             __                  _   _                 
    //   |  __ \| |             (_)           / _|                | | (_)                
    //   | |__) | |__  _   _ ___ _  ___ ___  | |_ _   _ _ __   ___| |_ _  ___  _ __  ___ 
    //   |  ___/| '_ \| | | / __| |/ __/ __| |  _| | | | '_ \ / __| __| |/ _ \| '_ \/ __|
    //   | |    | | | | |_| \__ \ | (__\__ \ | | | |_| | | | | (__| |_| | (_) | | | \__ \
    //   |_|    |_| |_|\__, |___/_|\___|___/ |_|  \__,_|_| |_|\___|\__|_|\___/|_| |_|___/
    //                  __/ |                                                            
    //                 |___/                                                             

    /// <summary>
    /// Does the physics for gravity
    /// </summary>
    protected void doGravity() {
        Bone spine = skeleton.getBones(BodyPart.SPINE)[0];
        RaycastHit hit;
        int layerMask = 1 << 8;
        if (Physics.Raycast(new Ray(spine.bone.position, -spine.bone.up), out hit, 200f, layerMask)) {
            groundedGravity(hit, spine);
        } else {
            notGroundedGravity();
        }
    }

    /// <summary>
    /// Gravity calculations for when you are not grounded
    /// </summary>
    virtual protected void notGroundedGravity() {
        grounded = false;
        gravity += Physics.gravity * Time.deltaTime;
    }

    /// <summary>
    /// Gravity calculation for when you are grounded
    /// </summary>
    /// <param name="hit">Point where raycast hit the ground</param>
    /// <param name="spine">Spine of animal</param>
    virtual protected void groundedGravity(RaycastHit hit, Bone spine) {
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
            notGroundedGravity();
        }
    }

    /// <summary>
    /// Tries to level the spine with the ground
    /// </summary>
    virtual protected void levelSpine() {
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

    //    _   _ _____   _____    __                  _   _                 
    //   | \ | |  __ \ / ____|  / _|                | | (_)                
    //   |  \| | |__) | |      | |_ _   _ _ __   ___| |_ _  ___  _ __  ___ 
    //   | . ` |  ___/| |      |  _| | | | '_ \ / __| __| |/ _ \| '_ \/ __|
    //   | |\  | |    | |____  | | | |_| | | | | (__| |_| | (_) | | | \__ \
    //   |_| \_|_|     \_____| |_|  \__,_|_| |_|\___|\__|_|\___/|_| |_|___/
    //                                                                     
    //                                                                     

    /// <summary>
    /// Spawns the animal at position
    /// </summary>
    /// <param name="pos">Vector3 pos</param>
    public bool Spawn(Vector3 pos) {
        transform.position = pos;
        roamCenter = pos;
        roamCenter.y = 0;

        RaycastHit hit;
        int layerMask = 1 << 8;
        if (Physics.Raycast(new Ray(transform.position, Vector3.down), out hit, ChunkConfig.chunkHeight + 20f, layerMask)) {
            Vector3 groundTarget = hit.point + Vector3.up * 10;
            transform.position = groundTarget;
            desiredHeading = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Function for when this animal used to be a player
    /// </summary>
    public void takeOverPlayer() {
        roamCenter = transform.position + new Vector3(Random.Range(2f, 5f), 100, Random.Range(2f, 5f));
        roamCenter.y = 0;
        RaycastHit hit;
        if (Physics.Raycast(new Ray(roamCenter, Vector3.down), out hit)) {
            roamCenter = hit.point;
        }

        desiredHeading = transform.rotation * Vector3.forward;
        desiredHeading.y = 0;
    }
}
