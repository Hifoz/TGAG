using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Super class for all air animals
/// </summary>
public abstract class AirAnimal : Animal {
    protected AirAnimalSkeleton airSkeleton;

    protected bool grounded = false;
    protected const float walkSpeed = 5f;
    protected const float flySpeed = 30f;
    protected const float glideDrag = 0.25f;

    private const float animSpeedScaling = 0.09f;
    private bool animationInTransition = false;

    private AnimalAnimation flappingAnimation;
    private AnimalAnimation glidingAnimation;
    private AnimalAnimation currentAnimation;


    override protected abstract void move();

    private void Update() {
        if (skeleton != null) { 
            move();
            calculateSpeedAndHeading();
            if (!animationInTransition) {
                currentAnimation.animate(speed * animSpeedScaling);
            }

            if (desiredSpeed == 0 && !animationInTransition && currentAnimation != glidingAnimation) {
                StartCoroutine(transistionAnimation(glidingAnimation, 0.5f));
            } else if (desiredSpeed != 0 && !animationInTransition && currentAnimation != flappingAnimation) {
                StartCoroutine(transistionAnimation(flappingAnimation, 0.5f));
            }
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
        //Getting relevant bones
        Bone spineBone = skeleton.getBones(BodyPart.SPINE)[0];
        List<Bone> rightWing = airSkeleton.getWing(true);
        List<Bone> leftWing = airSkeleton.getWing(false);

        //Flapping animation
        flappingAnimation = new AnimalAnimation();
        int flappingAnimationFrameCount = 3;
        BoneKeyFrames spine = new BoneKeyFrames(spineBone, flappingAnimationFrameCount);
        BoneKeyFrames wing1_1 = new BoneKeyFrames(rightWing[0], flappingAnimationFrameCount);
        BoneKeyFrames wing1_2 = new BoneKeyFrames(rightWing[1], flappingAnimationFrameCount);
        BoneKeyFrames wing2_1 = new BoneKeyFrames(leftWing[0], flappingAnimationFrameCount);
        BoneKeyFrames wing2_2 = new BoneKeyFrames(leftWing[1], flappingAnimationFrameCount);
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

        //Gliding anmiation
        glidingAnimation = new AnimalAnimation();
        int glidingAnimationFrameCount = 3;
        spine = new BoneKeyFrames(spineBone, glidingAnimationFrameCount);
        wing1_1 = new BoneKeyFrames(rightWing[0], glidingAnimationFrameCount);
        wing1_2 = new BoneKeyFrames(rightWing[1], glidingAnimationFrameCount);
        wing2_1 = new BoneKeyFrames(leftWing[0], glidingAnimationFrameCount);
        wing2_2 = new BoneKeyFrames(leftWing[1], glidingAnimationFrameCount);
        spine.setPositions(new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 0.5f, 0), new Vector3(0, 0, 0) });
        wing1_1.setRotations(new Vector3[] { new Vector3(0, 0, 20), new Vector3(0, 0, 0), new Vector3(0, 0, 20) });
        wing1_2.setRotations(new Vector3[] { new Vector3(0, 0, -20), new Vector3(0, 0, 0), new Vector3(0, 0, -20) });
        wing2_1.setRotations(Utils.multVectorArray(wing1_1.Rotations, -1));
        wing2_2.setRotations(Utils.multVectorArray(wing1_2.Rotations, -1));
        glidingAnimation.add(spine);
        glidingAnimation.add(wing1_1);
        glidingAnimation.add(wing1_2);
        glidingAnimation.add(wing2_1);
        glidingAnimation.add(wing2_2);

        //Init current animation
        currentAnimation = glidingAnimation;
    }

    private IEnumerator transistionAnimation(AnimalAnimation next, float transitionTime = 1f) {
        animationInTransition = true;
        for (float t = 0; t <= 1f; t += Time.deltaTime / transitionTime) {
            currentAnimation.animateLerp(next, t, speed * animSpeedScaling);
            yield return 0;
        }
        currentAnimation = next;
        animationInTransition = false;
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
