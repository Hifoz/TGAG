using UnityEngine;
using System.Collections.Generic;


public abstract class LandAnimal : Animal {
    protected const float walkSpeed = 5f;
    protected const float runSpeed = walkSpeed * 4f;

    protected bool grounded = false;
    protected Vector3 gravity = Physics.gravity;
    private const float levelSpeed = 3f;

    private float timer = 0;

    bool ragDolling = false;

    // Update is called once per frame
    void Update() {
        if (skeleton != null) {
            calculateSpeedAndHeading();
            move();
            levelSpine();
            doGravity();
            handleRagdoll();
        }
    }

    /// <summary>
    /// sets the skeleton, and applies the new mesh.
    /// </summary>
    override public void setSkeleton(AnimalSkeleton skeleton) {
        base.setSkeleton(skeleton);
        List<Bone> tail = skeleton.getBones(BodyPart.TAIL);
        LineSegment tailLine = skeleton.getLines(BodyPart.TAIL)[0];
        StartCoroutine(ragdollLimb(tail, tailLine, () => { return true; }, false, 4f, transform));
    }

    override protected abstract void move();

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
                StartCoroutine(ragdollLimb(((LandAnimalSkeleton)skeleton).getLeg(true, i), skeleton.getLines(BodyPart.RIGHT_LEGS)[i], () => { return !grounded; }));
                StartCoroutine(ragdollLimb(((LandAnimalSkeleton)skeleton).getLeg(false, i), skeleton.getLines(BodyPart.LEFT_LEGS)[i], () => { return !grounded; }));
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
        int layerMask = 1 << 8;
        if (Physics.Raycast(new Ray(spine.bone.position, -spine.bone.up), out hit, 200f, layerMask)) {
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
        } else {
            grounded = false;
            gravity += Physics.gravity * Time.deltaTime;
        }
    }

    /// <summary>
    /// Makes the animal do a walking animation
    /// </summary>
    private void walk() {
        int legPairs = skeleton.getBodyParameter<int>(BodyParameter.LEG_PAIRS);
        for(int i = 0; i < legPairs; i++) {
            walkLeg(((LandAnimalSkeleton)skeleton).getLeg(true, i), -1, Mathf.PI * i);
            walkLeg(((LandAnimalSkeleton)skeleton).getLeg(false, i), 1, Mathf.PI * (i + 1));            
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
        int layerMask = 1 << 8;
        if (Physics.Raycast(new Ray(target, spine.rotation * Vector3.down), out hit, 50f, layerMask)) {
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

    

    private void OnCollisionEnter(Collision collision) {
        gravity = Vector3.zero;
    }
}
