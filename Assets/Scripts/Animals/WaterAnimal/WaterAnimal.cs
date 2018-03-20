using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaterAnimal : Animal {
    //Coroutine flags
    private bool flagFlapBackToWater = false;

    //Animation stuff
    private WaterAnimalSkeleton waterSkeleton;
    private AnimalAnimation swimAnimation;
    private const float speedAnimScaling = 0.2f;

    //Physics stuff
    private Vector3 waterExitPoint = Vector3.zero;

    override protected void Start() {
        base.Start();
    }

    override protected void Update() {
        if (skeleton != null) {
            calculateSpeedAndHeading();
            brain.move();
            calcVelocity();
            doGravity();
            handleAnimations();
        }
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
    /// sets the skeleton, and applies the new mesh.
    /// </summary>
    override public void setSkeleton(AnimalSkeleton skeleton) {
        base.setSkeleton(skeleton);
        waterSkeleton = (WaterAnimalSkeleton)skeleton;

        generateAnimations();
        flagFlapBackToWater = false;
    }

    /// <summary>
    /// Sets the brain of the animal
    /// </summary>
    /// <param name="brain"></param>
    public override void setAnimalBrain(AnimalBrain brain) {
        base.setAnimalBrain(brain);
    }


    //                   _                 _   _                __                  _   _                 
    //       /\         (_)               | | (_)              / _|                | | (_)                
    //      /  \   _ __  _ _ __ ___   __ _| |_ _  ___  _ __   | |_ _   _ _ __   ___| |_ _  ___  _ __  ___ 
    //     / /\ \ | '_ \| | '_ ` _ \ / _` | __| |/ _ \| '_ \  |  _| | | | '_ \ / __| __| |/ _ \| '_ \/ __|
    //    / ____ \| | | | | | | | | | (_| | |_| | (_) | | | | | | | |_| | | | | (__| |_| | (_) | | | \__ \
    //   /_/    \_\_| |_|_|_| |_| |_|\__,_|\__|_|\___/|_| |_| |_|  \__,_|_| |_|\___|\__|_|\___/|_| |_|___/
    //                                                                                                    
    //                                                                                               

    /// <summary>
    /// Handles animations
    /// </summary>
    private void handleAnimations() {
        currentAnimation.animate(speedAnimScaling * state.speed);
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

    //    _____  _               _             __                  _   _                 
    //   |  __ \| |             (_)           / _|                | | (_)                
    //   | |__) | |__  _   _ ___ _  ___ ___  | |_ _   _ _ __   ___| |_ _  ___  _ __  ___ 
    //   |  ___/| '_ \| | | / __| |/ __/ __| |  _| | | | '_ \ / __| __| |/ _ \| '_ \/ __|
    //   | |    | | | | |_| \__ \ | (__\__ \ | | | |_| | | | | (__| |_| | (_) | | | \__ \
    //   |_|    |_| |_|\__, |___/_|\___|___/ |_|  \__,_|_| |_|\___|\__|_|\___/|_| |_|___/
    //                  __/ |                                                            
    //                 |___/                                                

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

            bool flagHitGround = Physics.Raycast(new Ray(spineBone.bone.position, -spineBone.bone.up), out hitGround, 200f, layerMaskGround);

            if (flagHitGround) {
                if (hitGround.distance < 1f) {
                    state.grounded = true;
                } else {
                    state.grounded = false;
                }
            } else {
                state.grounded = false;
            }
        } else {
            int layerMaskWater = 1 << 4;
            RaycastHit hitWater;
            bool flagHitWater = Physics.Raycast(new Ray(spineBone.bone.position, -spineBone.bone.up), out hitWater, 10f, layerMaskWater);
            if (flagHitWater && hitWater.distance > 1f) {
                //state.inWater = false;
                //inWaterInt = 0;
            }            
        }


        if (state.inWater) {
            waterGravity();
        } else if (state.grounded) {
            gravity = Vector3.zero;
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
        gravity = Vector3.zero;
    }   

    /// <summary>
    /// Does velocity calculations
    /// </summary>
    override protected void calcVelocity() {
        Vector3 velocity = state.heading.normalized * state.speed;
        rb.velocity = velocity + gravity;
        transform.LookAt(state.transform.position + state.heading);
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
