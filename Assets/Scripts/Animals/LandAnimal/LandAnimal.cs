using UnityEngine;
using System.Collections.Generic;


public abstract class LandAnimal : Animal {
    //Animation stuff
    bool ragDolling = false;
    AnimalAnimation walkingAnimation;
    LandAnimalSkeleton landSkeleton;
    private float speedAnimScaling;

    //Physics stuff
    protected const float walkSpeed = 5f;
    protected const float runSpeed = walkSpeed * 4f;


    // Update is called once per frame
    void Update() {
        if (skeleton != null) {
            calculateSpeedAndHeading();           
            move();
            levelSpine();
            doGravity();
            handleRagdoll();
            handleAnimations();          
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
        landSkeleton = (LandAnimalSkeleton)skeleton;

        generateAnimations();
    }

    override protected abstract void move();

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
            currentAnimation.animate(speed * speedAnimScaling);
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
        if (grounded || inWater) {
            ragDolling = false;
        } else if (!grounded && !ragDolling && !inWater) {
            ragDolling = true;
            for (int i = 0; i < skeleton.getBodyParameter<int>(BodyParameter.LEG_PAIRS); i++) {
                StartCoroutine(ragdollLimb(landSkeleton.getLeg(true, i), skeleton.getLines(BodyPart.RIGHT_LEGS)[i], () => { return !grounded; }, true));
                StartCoroutine(ragdollLimb(landSkeleton.getLeg(false, i), skeleton.getLines(BodyPart.LEFT_LEGS)[i], () => { return !grounded; }, true));
            }
            StartCoroutine(ragdollLimb(skeleton.getBones(BodyPart.NECK), skeleton.getLines(BodyPart.NECK)[0], () => { return !grounded; }, true));
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
        if (Vector3.Angle(heading, desiredHeading) > 0.1f) {
            heading = Vector3.RotateTowards(heading, desiredHeading, Time.deltaTime * headingChangeRate, 1f);
        }
        if (inWater) {
            preventDownardMovement();
        }
        if (Mathf.Abs(desiredSpeed - speed) > 0.2f) {
            if (grounded) {
                speed += Mathf.Sign(desiredSpeed - speed) * Time.deltaTime * acceleration;
            } else if (inWater) {
                speed += Mathf.Sign(desiredSpeed - speed) * Time.deltaTime * acceleration * 0.5f;
            } else {
                speed += Mathf.Sign(desiredSpeed - speed) * Time.deltaTime * acceleration * 0.2f;
            }
        }
    }     
}
