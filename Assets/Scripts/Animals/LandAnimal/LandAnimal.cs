﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class LandAnimal : Animal {
    //Coroutine flags
    bool flagJumping = false;
    private bool flagRagDollTail = false;

    //Animation stuff
    bool ragDolling = false;
    AnimalAnimation walkingAnimation;
    LandAnimalSkeleton landSkeleton;
    private float speedAnimScaling;

    protected override void Start() {
        base.Start();
    }

    // Update is called once per frame
    override protected void Update() {
        if (skeleton != null) {
            if (!displayMode) {
                calculateSpeedAndHeading();
                if (brain != null)
                    brain.move();
                calcVelocity();
                levelSpine();
                doGravity();
                handleRagdoll();
                handleAnimations();
            } else {
                rb.velocity = Vector3.zero;
                walkingAnimation.animate(brain.slowSpeed * speedAnimScaling);
            }
        }
    }

    /// <summary>
    /// sets the skeleton, and applies the new mesh.
    /// </summary>
    override public void setSkeleton(AnimalSkeleton skeleton) {
        base.setSkeleton(skeleton);        
        landSkeleton = (LandAnimalSkeleton)skeleton;

        generateAnimations();
    }

    public override void setAnimalBrain(AnimalBrain brain) {
        base.setAnimalBrain(brain);
        brain.addAction("jump", tryJump);
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
    /// Generates the walking animation for land animals
    /// </summary>
    private void generateAnimations() {
        //Getting relevant bones
        int legPairs = skeleton.getBodyParameter<int>(BodyParameter.LEG_PAIRS);
        List<Bone> neckBones = skeleton.getBones(BodyPart.NECK); 

        walkingAnimation = new AnimalAnimation();
        int walkingAnimationFrameCount = 4;

        KeyFrameTrigger[] soundTriggers = null;
        if (!displayMode) {
            soundTriggers = new KeyFrameTrigger[]{
                () => animalAudio.playWalkSound(),
                null,
                () => animalAudio.playWalkSound(),
                null
            };
        }
        Vector3[] legJoint1Frames = new Vector3[] { new Vector3(0, -45, 45), new Vector3(0, 0, 45), new Vector3(0, 45, 45), new Vector3(0, 0, 75) };
        Vector3[] legJoint2Frames = new Vector3[] { new Vector3(0, 0, -90), new Vector3(0, 0, -90), new Vector3(0, 0, -90), new Vector3(0, 0, -45) };
        for (int i = 0; i < legPairs; i++) {
            List<Bone> rightleg = landSkeleton.getLeg(true, i);
            List<Bone> leftleg = landSkeleton.getLeg(false, i);

            BoneKeyFrames leg1_1 = new BoneKeyFrames(rightleg[0], walkingAnimationFrameCount, 2);
            BoneKeyFrames leg1_2 = new BoneKeyFrames(rightleg[1], walkingAnimationFrameCount, 2);
            BoneKeyFrames leg2_1 = new BoneKeyFrames(leftleg[0], walkingAnimationFrameCount, 2);
            BoneKeyFrames leg2_2 = new BoneKeyFrames(leftleg[1], walkingAnimationFrameCount, 2);

            leg1_1.setRotations(legJoint1Frames);
            leg1_2.setRotations(legJoint2Frames);
            leg2_1.setRotations(Utils.shiftArray(Utils.multVectorArray(legJoint1Frames, -1), 2));
            leg2_2.setRotations(Utils.shiftArray(Utils.multVectorArray(legJoint2Frames, -1), 2));

            if (soundTriggers != null) {
                leg1_1.setTriggers(soundTriggers);
            }

            walkingAnimation.add(leg1_1);
            walkingAnimation.add(leg1_2);
            walkingAnimation.add(leg2_1);
            walkingAnimation.add(leg2_2);

            legJoint1Frames = Utils.shiftArray(legJoint1Frames, 2);
            legJoint2Frames = Utils.shiftArray(legJoint2Frames, 2);
        }
        BoneKeyFrames neckBase = new BoneKeyFrames(neckBones[0], walkingAnimationFrameCount, 4);
        BoneKeyFrames neckTop = new BoneKeyFrames(neckBones[1], walkingAnimationFrameCount, 4);

        neckBase.setRotations(new Vector3[] { new Vector3(20, -5, 10), new Vector3(0, 0, 0), new Vector3(20, 5, -10), new Vector3(0, 0, 0) });
        neckTop.setRotations(Utils.multVectorArray(neckBase.Rotations, -1));

        walkingAnimation.add(neckBase);
        walkingAnimation.add(neckTop);

        speedAnimScaling =  7f / (skeleton.getBodyParameter<float>(BodyParameter.LEG_LENGTH) / skeleton.getBodyParameter<float>(BodyParameter.SCALE));
        currentAnimation = walkingAnimation;
    }

    /// <summary>
    /// Handles animation logic for animal
    /// </summary>
    private void handleAnimations() {
        if (!ragDolling) {
            currentAnimation.animate(Time.timeScale * state.speed * speedAnimScaling);
            int legPairs = skeleton.getBodyParameter<int>(BodyParameter.LEG_PAIRS);
            for (int i = 0; i < legPairs; i++) {
                groundLimb(landSkeleton.getLeg(true, i), 0.5f);
                groundLimb(landSkeleton.getLeg(true, i), 0.5f);
            }
        }        
    }

    /// <summary>
    /// Function for handling ragdoll effects when free falling
    /// </summary>
    private void handleRagdoll() {
        if (state.grounded || state.inWater || state.onWaterSurface) {
            ragDolling = false;
        } else if (!state.grounded && !ragDolling && !state.inWater) {
            ragDolling = true;
            ragDollCondition condition = () => { return (!state.grounded && !state.inWater && !state.onWaterSurface); };
            for (int i = 0; i < skeleton.getBodyParameter<int>(BodyParameter.LEG_PAIRS); i++) {
                StartCoroutine(ragdollLimb(landSkeleton.getLeg(true, i), skeleton.getLines(BodyPart.RIGHT_LEGS)[i], condition, true));
                StartCoroutine(ragdollLimb(landSkeleton.getLeg(false, i), skeleton.getLines(BodyPart.LEFT_LEGS)[i], condition, true));
            }
            StartCoroutine(ragdollLimb(skeleton.getBones(BodyPart.NECK), skeleton.getLines(BodyPart.NECK)[0], condition, true));
        }

        if (!flagRagDollTail) {
            flagRagDollTail = true;
            List<Bone> tail = skeleton.getBones(BodyPart.TAIL);
            LineSegment tailLine = skeleton.getLines(BodyPart.TAIL)[0];
            StartCoroutine(ragdollLimb(tail, tailLine, () => { return flagRagDollTail; }, false, 4f, transform));
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
    /// Does velocity calculations
    /// </summary>
    override protected void calcVelocity() {
        Vector3 velocity;
        if (state.grounded || state.inWater) {
            velocity = state.spineHeading.normalized * state.speed;
        } else {
            velocity = state.heading.normalized * state.speed;
        }

        if (state.inWater || state.onWaterSurface)
            velocity *= 0.4f;


        if ((state.inWindArea || transform.position.y > WindController.globalWindHeight) && !state.grounded && !state.inWater && !state.onWaterSurface && transform.position.magnitude > 20f) {
            velocity *= 0.9f;
            velocity += WindController.globalWindDirection * WindController.globalWindSpeed * 0.4f;
        }



        rb.velocity = velocity + gravity;
        transform.LookAt(state.transform.position + state.heading);
    }

    /// <summary>
    /// Function for calculating speed and heading
    /// </summary>
    override protected void calculateSpeedAndHeading() {
        if (Vector3.Angle(state.heading, state.desiredHeading) > 0.1f) {
            state.heading = Vector3.RotateTowards(state.heading, state.desiredHeading, Time.deltaTime * headingChangeRate, 1f);
        } else {
            state.heading = state.desiredHeading;
        }

        if ((state.inWater || state.onWaterSurface) && !state.canStand) {
            enforceWaterdMovement();
        }
        if (Mathf.Abs(state.desiredSpeed - state.speed) > 0.2f) {
            if (state.grounded) {
                state.speed += Mathf.Sign(state.desiredSpeed - state.speed) * Time.deltaTime * acceleration;
            } else if (state.inWater) {
                state.speed += Mathf.Sign(state.desiredSpeed - state.speed) * Time.deltaTime * acceleration * 0.5f;
            } else {
                state.speed += Mathf.Sign(state.desiredSpeed - state.speed) * Time.deltaTime * acceleration * 0.2f;
            }
        } else {
            state.speed = state.desiredSpeed;
        }
    }

    /// <summary>
    /// Gravity calculation for when you are grounded
    /// </summary>
    /// <param name="hit">Point where raycast hit the ground</param>
    /// <param name="spine">Spine of animal</param>
    /// <param name="stanceHeight">The height of the stance</param>
    override protected void groundedGravity(VoxelRayCastHit hit, Bone spine, float stanceHeight) {
        const float stanceTolerance = 16f;
        float distFromStance = Mathf.Abs(stanceHeight - hit.distance);
        if (distFromStance <= stanceHeight) {
            state.grounded = true;
            float sign = Mathf.Sign(hit.distance - stanceHeight);
            if (distFromStance > (stanceHeight / stanceTolerance) && !flagJumping) {
                gravity = sign * Physics.gravity * Mathf.Pow(distFromStance / stanceHeight, 2);
            } else {
                gravity += sign * Physics.gravity * Mathf.Pow(distFromStance / stanceHeight, 2) * Time.deltaTime;
            }
        } else {
            notGroundedGravity();
        }
    }


    /// <summary>
    /// Tries to jump
    /// </summary>
    /// <returns>success flag</returns>
    private bool tryJump() {
        if (!flagJumping && state.grounded) {
            StartCoroutine(jump());
        }
        return !flagJumping;
    }

    /// <summary>
    /// Makes the player jump
    /// </summary>
    /// <returns></returns>
    private IEnumerator jump() {
        flagJumping = true;
        gravity += -Physics.gravity * 2f;
        yield return new WaitForSeconds(0.25f);
        flagJumping = false;
    }

    override public void OnVoxelEnter(BlockData.BlockType type) {
        base.OnVoxelEnter(type);
        flagJumping = false;
    }

    override protected void OnDisable() {
        base.OnDisable();
        flagRagDollTail = false;
        flagJumping = false;
    }
}
