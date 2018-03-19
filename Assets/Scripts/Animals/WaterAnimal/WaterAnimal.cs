using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaterAnimal : Animal {
    private bool flagFlapBackToWater = false;

    private WaterAnimalSkeleton waterSkeleton;

    private AnimalAnimation swimAnimation;

    private const float speedAnimScaling = 0.2f;

    private Vector3 waterExitPoint = Vector3.zero;

    override protected void Start() {
        base.Start();
    }

    override protected void Update() {
        if (skeleton != null) {
            calculateSpeedAndHeading();
            brain.move();
            doGravity();
            handleAnimations();
        }
    }

    private void handleAnimations() {
        currentAnimation.animate(speedAnimScaling * state.speed);
    }

    /// <summary>
    /// Function for calculating speed and heading
    /// </summary>
    override protected void calculateSpeedAndHeading() {
        if (Vector3.Angle(state.heading, state.desiredHeading) > 0.1f) {
            state.heading = Vector3.RotateTowards(state.heading, state.desiredHeading, Time.deltaTime * headingChangeRate, 1f);
        }
        
        if (Mathf.Abs(state.desiredSpeed - state.speed) > 0.2f) {
            state.speed += Mathf.Sign(state.desiredSpeed - state.speed) * Time.deltaTime * acceleration;
        }
    }

    /// <summary>
    /// Does the physics for gravity
    /// </summary>
    override protected void doGravity() {
        if (!state.inWater) {
            int layerMaskGround = 1 << 8;
            RaycastHit hitGround;

            if (Physics.Raycast(new Ray(spineBone.bone.position, -spineBone.bone.up), out hitGround, 200f, layerMaskGround)) {
                if (hitGround.distance < 1f) {
                    state.grounded = true;
                } else {
                    state.grounded = false;
                }
            } else {
                state.grounded = false;
            }
        }


        if (state.inWater) {
            waterGravity();
        } else if (state.grounded) {
            state.gravity = Vector3.zero;
            tryFlapBackIntoWater();
        } else {
            notGroundedGravity();
        }
    }

    /// <summary>
    /// Calulates gravity when in water
    /// </summary>
    protected override void waterGravity() {
        state.grounded = false;
        state.gravity = Vector3.zero;
    }

    /// <summary>
    /// sets the skeleton, and applies the new mesh.
    /// </summary>
    override public void setSkeleton(AnimalSkeleton skeleton) {
        base.setSkeleton(skeleton);
        waterSkeleton = (WaterAnimalSkeleton)skeleton;

        generateAnimations();
        flagFlapBackToWater = false;
    }
   

    public override void setAnimalBrain(AnimalBrain brain) {
        base.setAnimalBrain(brain);
    }

    /// <summary>
    /// Generatges animations for the fish
    /// </summary>
    private void generateAnimations() {
        //Getting relevant bones
        List<Bone> spine = skeleton.getBones(BodyPart.SPINE);
        Bone firstSpine = spine[1];
        spine = spine.GetRange(2, spine.Count - 2);

        swimAnimation = new AnimalAnimation();
        int swimAnimationFrameCount = 2;

        Vector3[] spineFrames1 = new Vector3[] { new Vector3(0, -45, 0), new Vector3(0, 45, 0) };
        Vector3[] spineFrames2 = Utils.multVectorArray(spineFrames1, -2);

        BoneKeyFrames boneKeyFrames = new BoneKeyFrames(firstSpine, swimAnimationFrameCount);
        boneKeyFrames.setRotations(spineFrames1);
        swimAnimation.add(boneKeyFrames);

        foreach (Bone bone in spine) {
            boneKeyFrames = new BoneKeyFrames(bone, swimAnimationFrameCount);
            boneKeyFrames.setRotations(spineFrames2);
            swimAnimation.add(boneKeyFrames);

            spineFrames2 = Utils.multVectorArray(spineFrames2, -1);
        }
        currentAnimation = swimAnimation;
    }
   
    /// <summary>
    /// Tries to flap back to water
    /// </summary>
    /// <returns>success flag</returns>
    private bool tryFlapBackIntoWater() {
        if (!flagFlapBackToWater) {
            StartCoroutine(flapBackToWater());
        }
        return !flagFlapBackToWater;
    }

    /// <summary>
    /// Makes the fish flap back into water
    /// </summary>
    /// <returns></returns>
    private IEnumerator flapBackToWater() {
        flagFlapBackToWater = true;

        const float speed = 100;

        Vector3 currentPos = transform.position;
        Vector3 halfwayControlPoint = Vector3.Lerp(currentPos, waterExitPoint, 0.5f) + Vector3.up * 100f;

        float totalDist = (halfwayControlPoint - currentPos).magnitude + (waterExitPoint - halfwayControlPoint).magnitude;
        float delta = speed / totalDist;

        for (float t = 0; t < 1f; t += Time.deltaTime * delta) { // Spline lerp
            Vector3 first = Vector3.Lerp(currentPos, halfwayControlPoint, t);
            Vector3 second = Vector3.Lerp(halfwayControlPoint, waterExitPoint, t);
            transform.position = Vector3.Lerp(first, second, t);
            yield return 0;
        }

        flagFlapBackToWater = false;
    }

    override protected void OnTriggerExit(Collider other) {
        base.OnTriggerExit(other);
        waterExitPoint = transform.position;
    }
}
