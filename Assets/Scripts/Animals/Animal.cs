using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SkinnedMeshRenderer))]
[RequireComponent(typeof(Rigidbody))]
public abstract class Animal : MonoBehaviour {
    protected AnimalSkeleton skeleton;
    protected AnimalBrain brain;
    protected AnimalState state = new AnimalState();

    //Coroutine flags
    private bool flagSpineCorrecting = false;
    protected bool flagAnimationTransition = false;

    //Animation stuff
    protected const float ikSpeed = 10f;
    protected const float ikTolerance = 0.05f;
    protected delegate bool ragDollCondition();
    protected AnimalAnimation currentAnimation;
    protected Bone spineBone;

    //Physics stuff
    protected float headingChangeRate = 5f;
    protected float acceleration = 5f;
    private const float levelSpeed = 6f;
    protected Rigidbody rb;
    protected Vector3 gravity;


    virtual protected void Awake() {
        rb = GetComponent<Rigidbody>();
        state.transform = transform;
    }

    virtual protected void Start() {
    }

    protected abstract void Update();

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

        spineBone = skeleton.getBones(BodyPart.SPINE)[0];

        currentAnimation = null;
        state.inWater = false;
        flagSpineCorrecting = false;
        flagAnimationTransition = false;
    }

    /// <summary>
    /// Gets the animalbrain
    /// </summary>
    /// <returns>The brain</returns>
    public AnimalBrain getAnimalBrain() {
        return brain;
    }

    /// <summary>
    /// Sets the animal brain
    /// </summary>
    /// <param name="brain">The brain to use</param>
    virtual public void setAnimalBrain(AnimalBrain brain) {
        this.brain = brain;
        this.brain.state = state;
    }

    /// <summary>
    /// Gets the AnimalState
    /// </summary>
    /// <returns></returns>
    public AnimalState getState() {
        return state;
    }

    /// <summary>
    /// Sets the AnimalState
    /// </summary>
    /// <param name="state"></param>
    public void setState(AnimalState state) {
        this.state = state;
    }

    /// <summary>
    /// Spawns the animal
    /// </summary>
    /// <param name="pos"></param>
    public void Spawn(Vector3 pos) {
        brain.Spawn(pos);
    }

    //    _   _                               _     _ _         __                  _   _                 
    //   | \ | |                             | |   | (_)       / _|                | | (_)                
    //   |  \| | ___  _ __ ______ _ __  _   _| |__ | |_  ___  | |_ _   _ _ __   ___| |_ _  ___  _ __  ___ 
    //   | . ` |/ _ \| '_ \______| '_ \| | | | '_ \| | |/ __| |  _| | | | '_ \ / __| __| |/ _ \| '_ \/ __|
    //   | |\  | (_) | | | |     | |_) | |_| | |_) | | | (__  | | | |_| | | | | (__| |_| | (_) | | | \__ \
    //   |_| \_|\___/|_| |_|     | .__/ \__,_|_.__/|_|_|\___| |_|  \__,_|_| |_|\___|\__|_|\___/|_| |_|___/
    //                           | |                                                                      
    //                           |_|                                                                      

    protected abstract void calculateSpeedAndHeading();
    protected abstract void calcVelocity();
    private bool spineIsCorrect { get { return spineBone.bone.localRotation == Quaternion.identity; } }

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
            referenceTransform = spineBone.bone;
        }

        for (int i = 0; i < currentPositions.Length; i++) {
            currentPositions[i] = limb[i + 1].bone.position;
        }

        while (condition()) {
            for (int i = 0; i < desiredPositions.Length; i++) {
                if(referenceTransform != null && limb[0].bone != null)
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
        if (effector == null)
            return true;
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
        if (!flagAnimationTransition) {
            StartCoroutine(transistionAnimation(next, speedScaling, nextSpeedScaling, transitionTime));
        }
        return !flagAnimationTransition;
    }

    /// <summary>
    /// Transitions between the current animation and the next animation
    /// </summary>
    /// <param name="next">Animation to transition too</param>
    /// <param name="speedScaling">float for scaling the speed of animation</param>
    /// <param name="transitionTime">Time to spend on transition</param>
    /// <returns></returns>
    private IEnumerator transistionAnimation(AnimalAnimation next, float speedScaling, float nextSpeedScaling, float transitionTime) {
        flagAnimationTransition = true;
        for (float t = 0; t <= 1f; t += Time.deltaTime / transitionTime) {
            if (!flagAnimationTransition) {
                break;
            }
            currentAnimation.animateLerp(next, t, state.speed * Mathf.Lerp(speedScaling, nextSpeedScaling, t));
            yield return 0;
        }
        currentAnimation = next;
        flagAnimationTransition = false;
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
    virtual protected void doGravity() {
        int layerMaskWater = 1 << 4;
        RaycastHit hitWater;
        int layerMaskGround = 1 << 8;
        RaycastHit hitGround;

        bool flagHitGround = Physics.Raycast(new Ray(spineBone.bone.position, -spineBone.bone.up), out hitGround, 200f, layerMaskGround);
        bool flagHitWater = Physics.Raycast(new Ray(spineBone.bone.position, -spineBone.bone.up), out hitWater, 200f, layerMaskWater);

        float stanceHeight = skeleton.getBodyParameter<float>(BodyParameter.LEG_LENGTH) / 2;

        bool canStand = false;

        //Calculate water state
        if (flagHitWater) {
            if (hitWater.distance < 2f) {
                state.onWaterSurface = true;
            } else {
                state.onWaterSurface = false;
            }
        } else {
            state.onWaterSurface = false;           
        }       

        if (flagHitGround) {
            canStand = hitGround.distance <= stanceHeight;
        }

        if (canStand || !state.inWater) {
            groundedGravity(hitGround, spineBone, stanceHeight);
        } else if (state.onWaterSurface) {
            waterSurfaceGravity(hitWater);
        } else if (state.inWater) {
            waterGravity();
        } else {
            notGroundedGravity();
        }
    }

    /// <summary>
    /// Gravity calculations for when you are not grounded
    /// </summary>
    virtual protected void notGroundedGravity() {
        state.grounded = false;
        state.inWater = false;
        gravity += Physics.gravity * Time.deltaTime;
    }

    /// <summary>
    /// Gravity calculation for when you are grounded
    /// </summary>
    /// <param name="hit">Point where raycast hit the ground</param>
    /// <param name="spine">Spine of animal</param>
    /// <param name="stanceHeight">The height of the stance</param>
    virtual protected void groundedGravity(RaycastHit hit, Bone spine, float stanceHeight) {
        float dist2ground = Vector3.Distance(hit.point, spine.bone.position);
        float distFromStance = Mathf.Abs(stanceHeight - dist2ground);
        if (distFromStance <= stanceHeight) {
            state.grounded = true;
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
    /// Gravity calculation for when you are in water
    /// </summary>
    /// <param name="hit">Point where raycast hit</param>
    virtual protected void waterGravity() {
        state.grounded = false;
        gravity = -Physics.gravity;
        if (!spineIsCorrect) {            
            tryCorrectSpine();
        }
    }

    /// <summary>
    /// Gravity calculations for water surface
    /// </summary>
    virtual protected void waterSurfaceGravity(RaycastHit hit) {
        state.grounded = false;
        
        if (hit.distance > 0.5f) {
            gravity = Physics.gravity;
        } else {
            gravity = Vector3.zero;
        }
        if (!spineIsCorrect) {
            tryCorrectSpine();
        }
    }

    /// <summary>
    /// Tries to level the spine with the ground
    /// </summary>
    virtual protected void levelSpine() {
        if (state.grounded) {            
            levelSpineWithAxis(transform.forward, spineBone.bone.forward, skeleton.getBodyParameter<float>(BodyParameter.SPINE_LENGTH));
            levelSpineWithAxis(transform.right, spineBone.bone.right, skeleton.getBodyParameter<float>(BodyParameter.LEG_JOINT_LENGTH));
        } else if (!spineIsCorrect) {
            tryCorrectSpine();
        }
        state.spineHeading = spineBone.bone.rotation * Vector3.forward;
    }

    /// <summary>
    /// Levels the spine with terrain along axis
    /// </summary>
    /// <param name="axis">Axis to level along</param>
    /// <param name="currentAxis">Current state of axis</param>
    /// <param name="length">Length to check with</param>
    private void levelSpineWithAxis(Vector3 axis, Vector3 currentAxis, float length) {
        Vector3 point1 = spineBone.bone.position + axis * length / 2f + Vector3.up * 20;
        Vector3 point2 = spineBone.bone.position - axis * length / 2f + Vector3.up * 20;

        int layerMask = 1 << 8;
        RaycastHit hit1;
        RaycastHit hit2;
        Physics.Raycast(new Ray(point1, Vector3.down), out hit1, 100f, layerMask);
        Physics.Raycast(new Ray(point2, Vector3.down), out hit2, 100f, layerMask);
        point1 = spineBone.bone.position + currentAxis * length / 2f;
        point2 = spineBone.bone.position - currentAxis * length / 2f;
        Vector3 a = hit1.point - hit2.point;
        Vector3 b = point1 - point2;

        float angle = Mathf.Acos(Vector3.Dot(a, b) / (a.magnitude * b.magnitude));
        Vector3 normal = Vector3.Cross(a, b);
        if (angle > 0.01f) {
            spineBone.bone.rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg * levelSpeed * Time.deltaTime, -normal) * spineBone.bone.rotation;
            if (!checkConstraints(spineBone)) {
                spineBone.bone.rotation = Quaternion.AngleAxis(-angle * Mathf.Rad2Deg * levelSpeed * Time.deltaTime, -normal) * spineBone.bone.rotation;
            }
        }
    }

    /// <summary>
    /// Tries to correct the spine
    /// </summary>
    /// <returns>Success flag</returns>
    private bool tryCorrectSpine() {
        if (!flagSpineCorrecting) {
            StartCoroutine(correctSpine());
        }
        return !flagSpineCorrecting;
    }

    /// <summary>
    /// Corrects the spine (returning it to zero rotation)
    /// </summary>
    /// <returns></returns>
    private IEnumerator correctSpine() {
        flagSpineCorrecting = true;
        Quaternion originalRot = spineBone.bone.localRotation;
        for (float t = 0; t <= 1f; t += Time.deltaTime) {
            spineBone.bone.localRotation = Quaternion.Lerp(originalRot, Quaternion.identity, t);
            yield return 0;
        }
        spineBone.bone.localRotation = Quaternion.identity;
        flagSpineCorrecting = false;
    }

    /// <summary>
    /// Calling this function removes negative y from headings
    /// </summary>
    protected void preventDownardMovement() {
        if (state.heading.y < 0) {
            state.heading.y = 0;
            state.heading.Normalize();
        }
        if (state.spineHeading.y < 0) {
            state.spineHeading.y = 0;
            state.spineHeading.Normalize();
        }
    }

    virtual protected void OnTriggerEnter(Collider other) {

    }

    virtual protected void OnTriggerExit(Collider other) {
        if (other.name == "waterSubChunk") {
            state.inWater = false;
        }
    }

    virtual protected void OnTriggerStay(Collider other) {
        if (other.name == "waterSubChunk") {
            state.inWater = true;
        }
    }

    virtual protected void OnCollisionEnter(Collision collision) {
        brain.OnCollisionEnter();
    }

   virtual protected void OnDisable() {
        flagSpineCorrecting = false;
        flagAnimationTransition = false;
    }

    //DEBUG FUNCTIONS
    //DEBUG FUNCTIONS
    //DEBUG FUNCTIONS
    //DEBUG FUNCTIONS
    //DEBUG FUNCTIONS
    protected void debug(string message) {
        if (brain != null && brain.GetType().BaseType.Equals(typeof(AnimalBrainPlayer))) {
            Debug.Log(message);
        }
    }

    /// <summary>
    /// Generates a string of debug info
    /// </summary>
    /// <returns></returns>
    public string getDebugString() {
        string s = "";
        s += "Grounded: " + state.grounded.ToString() + "\n";
        s += "InWater: " + state.inWater.ToString() + "\n";
        s += "OnWaterSurface: " + state.onWaterSurface.ToString() + "\n\n";

        s += "Desired speed: " + state.desiredSpeed + "\n";
        s += "Desired heading: " + state.desiredHeading + "\n\n";

        s += "Speed: " + state.speed + "\n";
        s += "Position: " + transform.position + "\n\n";

        s += "Animation in transition: " + flagAnimationTransition.ToString();
        return s;
    }
}
