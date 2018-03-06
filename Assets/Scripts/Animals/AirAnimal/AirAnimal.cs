using UnityEngine;
using System.Collections.Generic;

public abstract class AirAnimal : Animal {

    protected bool grounded = false;
    protected const float walkSpeed = 5f;
    protected const float flySpeed = 30f;
    protected AirAnimalSkeleton airSkeleton;

    override protected abstract void move();

    private void Update() {
        if (skeleton != null) { 
            move();
            calculateSpeedAndHeading();
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

        List<Bone> rightWing = airSkeleton.getWing(true);
        LineSegment rightWingLine = skeleton.getLines(BodyPart.RIGHT_WING)[0];
        StartCoroutine(ragdollLimb(rightWing, rightWingLine, () => { return true; }, false, 10f, transform));

        List<Bone> leftWing = airSkeleton.getWing(false);
        LineSegment leftWingLine = skeleton.getLines(BodyPart.LEFT_WING)[0];
        StartCoroutine(ragdollLimb(leftWing, leftWingLine, () => { return true; }, false, 10f, transform));
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
}
