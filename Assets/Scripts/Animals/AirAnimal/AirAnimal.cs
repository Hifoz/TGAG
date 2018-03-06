using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public abstract class AirAnimal : Animal {
    private enum KeyFrameType {
        SPINE,
        WING1,
        WING2
    }

    protected AirAnimalSkeleton airSkeleton;

    protected bool grounded = false;
    protected const float walkSpeed = 5f;
    protected const float flySpeed = 30f;
    
    private float flappingSpeed = 0.03f;
    private float flapTimer = 0;
    private float flapType = 0; //Which dict to get frames from
    bool animInTransition = false;
    int FlyingKeyFramesCount = 3;
    Dictionary<KeyFrameType, Vector3[]> FlyingKeyFrames = new Dictionary<KeyFrameType, Vector3[]>() {
        { KeyFrameType.SPINE, new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 0) } }, //Position for spine
        { KeyFrameType.WING1, new Vector3[] { new Vector3(0, 0, 85), new Vector3(0, 0, -45), new Vector3(0, 0, 85) } }, //Rotation for first bone in wing
        { KeyFrameType.WING2, new Vector3[] { new Vector3(0, 0, -170), new Vector3(0, 0, 40), new Vector3(0, 0, -170) } } //Rotation for second bone in wing
    };

    Dictionary<KeyFrameType, Vector3[]> GlidingKeyFrames = new Dictionary<KeyFrameType, Vector3[]>() {
        { KeyFrameType.SPINE, new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 0.5f, 0), new Vector3(0, 0, 0) } }, //Position for spine
        { KeyFrameType.WING1, new Vector3[] { new Vector3(0, 0, 20), new Vector3(0, 0, 0), new Vector3(0, 0, 20) } }, //Rotation for first bone in wing
        { KeyFrameType.WING2, new Vector3[] { new Vector3(0, 0, -20), new Vector3(0, 0, 0), new Vector3(0, 0, -20) } } //Rotation for second bone in wing
    };


    override protected abstract void move();

    private void Update() {
        if (skeleton != null) { 
            move();
            calculateSpeedAndHeading();
            flapWings();
        }
    }

    /// <summary>
    /// sets the skeleton, and applies the new mesh.
    /// </summary>
    override public void setSkeleton(AnimalSkeleton skeleton) {
        base.setSkeleton(skeleton);
        airSkeleton = (AirAnimalSkeleton)skeleton;

        List<Bone> tail = skeleton.getBones(BodyPart.TAIL);
        LineSegment tailLine = skeleton.getLines(BodyPart.TAIL)[0];
        StartCoroutine(ragdollLimb(tail, tailLine, () => { return true; }, false, 4f, transform));

        List<Bone> rightLegs = skeleton.getBones(BodyPart.RIGHT_LEGS);
        LineSegment rightLegsLine = skeleton.getLines(BodyPart.RIGHT_LEGS)[0];
        StartCoroutine(ragdollLimb(rightLegs, rightLegsLine, () => { return true; }, false, 5f, transform));

        List<Bone> leftLegs = skeleton.getBones(BodyPart.LEFT_LEGS);
        LineSegment leftLegsLine = skeleton.getLines(BodyPart.LEFT_LEGS)[0];
        StartCoroutine(ragdollLimb(leftLegs, leftLegsLine, () => { return true; }, false, 5f, transform));
    }

    /// <summary>
    /// Function for calculating speed and heading
    /// </summary>
    private void calculateSpeedAndHeading() {
        if (Vector3.Angle(heading, desiredHeading) > 0.1f) {
            heading = Vector3.RotateTowards(heading, desiredHeading, Time.deltaTime * headingChangeRate, 1f);
        }
        if (Mathf.Abs(desiredSpeed - speed) > 0.2f) {           
            speed += Mathf.Sign(desiredSpeed - speed) * Time.deltaTime * acceleration;           
        }
    }

    private void flapWings() {
        float frames = FlyingKeyFramesCount - 1;
        float frame = Utils.frac(flapTimer) * frames;
        flapTimer += Time.deltaTime * flappingSpeed * (speed + 0.5f);

        flapWing(airSkeleton.getWing(true), frame, 1);
        flapWing(airSkeleton.getWing(false), frame, -1);

        Bone spine = skeleton.getBones(BodyPart.SPINE)[0];
        spine.bone.localPosition = Vector3.Lerp(
            getKeyFrame(KeyFrameType.SPINE, (int)frame), 
            getKeyFrame(KeyFrameType.SPINE, (int)frame + 1), 
            Utils.frac(frame)
        );


        if (desiredSpeed == 0 && !animInTransition && flapType != 0f) {
            StartCoroutine(transitionAnimation(1f, 0f, 0.5f));
        } else if (desiredSpeed != 0 && !animInTransition && flapType != 1f) {
            StartCoroutine(transitionAnimation(0f, 1f, 0.5f));
        }
    }

    private void flapWing(List<Bone> wing,  float frame, int sign) {
        wing[0].bone.localRotation = Quaternion.Euler(sign * Vector3.Lerp(
                getKeyFrame(KeyFrameType.WING1, (int)frame), 
                getKeyFrame(KeyFrameType.WING1, (int)frame + 1), 
                Utils.frac(frame)
            )
        );
        wing[1].bone.localRotation = Quaternion.Euler(sign * Vector3.Lerp(
               getKeyFrame(KeyFrameType.WING2, (int)frame), 
               getKeyFrame(KeyFrameType.WING2, (int)frame + 1),
               Utils.frac(frame)
            )
       );
    }

    private Vector3 getKeyFrame(KeyFrameType type, int index) {
        return Vector3.Lerp(GlidingKeyFrames[type][index], FlyingKeyFrames[type][index], flapType);
    }

    private IEnumerator transitionAnimation(float from, float to, float time) {
        animInTransition = true;
        for (float t = 0; t <= 1f; t += Time.deltaTime / time) {
            flapType = Mathf.Lerp(from, to, t);
            yield return 0;
        }
        flapType = to;
        animInTransition = false;
    }
}
