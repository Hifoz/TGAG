using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaterAnimal : Animal {
    //Coroutine flags
    private bool flagFlap = false;

    //Animation stuff
    private AnimalAnimation swimAnimation;
    private AnimalAnimation flapAnimation;
    private const float speedAnimScaling = 0.2f;

    //Physics stuff
    private Vector3 waterExitPoint = Vector3.zero;

    override protected void Start() {
        base.Start();
    }

    override protected void Update() {
        if (skeleton != null) {
            calculateSpeedAndHeading();
            if(brain != null)
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

        generateAnimations();
        flagFlap = false;
    }

    /// <summary>
    /// Sets the brain of the animal
    /// </summary>
    /// <param name="brain"></param>
    public override void setAnimalBrain(AnimalBrain brain) {
        base.setAnimalBrain(brain);
        brain.addAction("flap", tryFlap);
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
        if (state.inWater && swimAnimation != currentAnimation) {
            tryAnimationTransition(swimAnimation, speedAnimScaling, speedAnimScaling, 0.5f);
        } else if (!state.inWater && flapAnimation != currentAnimation) {
            tryAnimationTransition(flapAnimation, speedAnimScaling, speedAnimScaling, 0.5f);
        }

        currentAnimation.animate(speedAnimScaling * state.speed);
    }

    /// <summary>
    /// Generatges animations for the fish
    /// </summary>
    private void generateAnimations() {
        //Getting relevant bones
        generateSwimAnimation();
        generateFlapAnimation();
        currentAnimation = swimAnimation;
    }

    /// <summary>
    /// Generates the swimming animation
    /// </summary>
    private void generateSwimAnimation() {
        List<Bone> spine = skeleton.getBones(BodyPart.SPINE);
        Bone head = spine[0];
        Bone firstSpine = spine[1];
        spine = spine.GetRange(2, spine.Count - 2);

        swimAnimation = new AnimalAnimation();
        int swimAnimationFrameCount = 2;

        Vector3[] spineFrames0 = new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 0, 0) };
        Vector3[] spineFrames1 = new Vector3[] { new Vector3(0, -45, 0), new Vector3(0, 45, 0) };
        Vector3[] spineFrames2 = Utils.multVectorArray(spineFrames1, -2);

        BoneKeyFrames boneKeyFrames = new BoneKeyFrames(head, swimAnimationFrameCount);
        boneKeyFrames.setRotations(spineFrames0);
        swimAnimation.add(boneKeyFrames);

        boneKeyFrames = new BoneKeyFrames(firstSpine, swimAnimationFrameCount);
        boneKeyFrames.setRotations(spineFrames1);
        swimAnimation.add(boneKeyFrames);

        foreach (Bone bone in spine) {
            boneKeyFrames = new BoneKeyFrames(bone, swimAnimationFrameCount);
            boneKeyFrames.setRotations(spineFrames2);
            swimAnimation.add(boneKeyFrames);

            spineFrames2 = Utils.multVectorArray(spineFrames2, -1);
        }
    }

    /// <summary>
    /// Generates the flapping animation
    /// </summary>
    private void generateFlapAnimation() {
        List<Bone> spine = skeleton.getBones(BodyPart.SPINE);

        flapAnimation = new AnimalAnimation();
        int flapAnimationFrameCount = 2;

        Vector3[] spineFrames = new Vector3[] { new Vector3(-15, 0, 0), new Vector3(15, 0, 0) };

        foreach (Bone bone in spine) {
            BoneKeyFrames boneKeyFrames = new BoneKeyFrames(bone, flapAnimationFrameCount);
            boneKeyFrames.setRotations(spineFrames);
            flapAnimation.add(boneKeyFrames);
        }
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
        } else {
            state.heading = state.desiredHeading;
        }
        
        if (Mathf.Abs(state.desiredSpeed - state.speed) > 0.2f) {
            state.speed += Mathf.Sign(state.desiredSpeed - state.speed) * Time.deltaTime * acceleration;
        } else {
            state.speed = state.desiredSpeed;
        }
    }

    /// <summary>
    /// Does the physics for gravity
    /// </summary>
    override protected void doGravity() {
        if (!state.inWater) {
            Ray ray = new Ray(spineBone.bone.position, -spineBone.bone.up);
            VoxelRayCastHit hitGround = VoxelPhysics.rayCast(ray, 200f, VoxelRayCastTarget.SOLID);

            bool flagHitGround = VoxelPhysics.isSolid(hitGround.type);

            if (flagHitGround) {
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
        } else if (state.grounded && !flagFlap) {
            gravity = Vector3.zero;
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

        if (state.grounded && !flagFlap) {
            rb.velocity = Vector3.zero;
        }
    }   
   
    /// <summary>
    /// Tries to flap back to water
    /// </summary>
    /// <returns>success flag</returns>
    private bool tryFlap() {
        if (!flagFlap) {
            StartCoroutine(flap());
        }
        return !flagFlap;
    }

    /// <summary>
    /// Makes the fish flap back into water
    /// </summary>
    /// <returns></returns>
    private IEnumerator flap() {
        flagFlap = true;
        state.speed = brain.fastSpeed / 4;
        gravity += -Physics.gravity * 2f;
        yield return new WaitForSeconds(0.25f);
        flagFlap = false;
    }
}
