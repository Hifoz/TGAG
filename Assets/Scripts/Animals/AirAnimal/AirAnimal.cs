using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Super class for all air animals
/// </summary>
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
    protected const float glideDrag = 0.25f;

    protected const float animSpeedScaling = 0.09f;
    
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

    private AnimalAnimation flappingAnimation;
    private AnimalAnimation glidingAnimation;


    override protected abstract void move();

    private void Update() {
        if (skeleton != null) { 
            move();
            calculateSpeedAndHeading();
            flappingAnimation.animate(speed * animSpeedScaling);
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

        generateAnimations();
    }

    /// <summary>
    /// Generates animations for the AirAnimal
    /// </summary>
    private void generateAnimations() {
        flappingAnimation = new AnimalAnimation();
        int flappingAnimationFrameCount = 3;
        BoneKeyFrames spine = new BoneKeyFrames(skeleton.getBones(BodyPart.SPINE)[0], flappingAnimationFrameCount);
        BoneKeyFrames wing1_1 = new BoneKeyFrames(airSkeleton.getWing(true)[0], flappingAnimationFrameCount);
        BoneKeyFrames wing1_2 = new BoneKeyFrames(airSkeleton.getWing(true)[1], flappingAnimationFrameCount);
        BoneKeyFrames wing2_1 = new BoneKeyFrames(airSkeleton.getWing(false)[0], flappingAnimationFrameCount);
        BoneKeyFrames wing2_2 = new BoneKeyFrames(airSkeleton.getWing(false)[1], flappingAnimationFrameCount);
        spine.setPositions(new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 0) });
        wing1_1.setRotations(new Vector3[] { new Vector3(0, 0, 85), new Vector3(0, 0, -45), new Vector3(0, 0, 85) }); 
        wing1_2.setRotations(new Vector3[] { new Vector3(0, 0, -170), new Vector3(0, 0, 40), new Vector3(0, 0, -170) });
        wing2_1.setRotations(Utils.multVectorArray(wing1_1.Rotations, -1));
        wing2_2.setRotations(Utils.multVectorArray(wing1_2.Rotations, -1));
        flappingAnimation.add(spine);
        flappingAnimation.add(wing1_1);
        flappingAnimation.add(wing1_2);
        flappingAnimation.add(wing2_1);
        flappingAnimation.add(wing2_2);
    }

    /// <summary>
    /// Function for calculating speed and heading
    /// </summary>
    private void calculateSpeedAndHeading() {
        if (Vector3.Angle(heading, desiredHeading) > 0.1f) {
            heading = Vector3.RotateTowards(heading, desiredHeading, Time.deltaTime * headingChangeRate, 1f);
        }
        if (desiredSpeed - speed > 0.2f) { //Acceleration           
            speed += Time.deltaTime * acceleration;            
        } else if (speed - desiredSpeed > 0.2f) { //Deceleration
            speed -= Time.deltaTime * acceleration * glideDrag;
        }
    }
}
