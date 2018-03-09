using UnityEngine;
using System.Collections.Generic;


public abstract class LandAnimal : Animal {
    protected const float walkSpeed = 5f;
    protected const float runSpeed = walkSpeed * 4f;

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
    override protected void calculateSpeedAndHeading() {
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
            ccdPartial(leg.GetRange(i, 2), target, ikSpeed / 4f);
        }

        RaycastHit hit;
        int layerMask = 1 << 8;
        if (Physics.Raycast(new Ray(target, spine.rotation * Vector3.down), out hit, 50f, layerMask)) {
            float heightOffset = (Mathf.Sin(timer + Mathf.PI + radOffset)) * legLength / 8f; //Up/Down motion
            heightOffset = (heightOffset > 0) ? heightOffset : 0;

            target = hit.point;
            target.y += heightOffset;
            if (ccdPartial(leg, target, ikSpeed)) {
                return true;
            }
        }
        return false;
    }

    

    private void OnCollisionEnter(Collision collision) {
        gravity = Vector3.zero;
    }
}
