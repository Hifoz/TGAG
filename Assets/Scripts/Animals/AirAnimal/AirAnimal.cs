using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Super class for all air animals
/// </summary>
public abstract class AirAnimal : Animal {
    //Animation stuff
    protected AirAnimalSkeleton airSkeleton;

    private const float animSpeedScalingAir = 0.05f;
    private const float animSpeedScalingGround = 0.5f;    

    private bool ragDollLegs = true;

    private AnimalAnimation flappingAnimation;
    private AnimalAnimation glidingAnimation;
    private AnimalAnimation walkingAnimation;

    //Physics stuff
    protected const float walkSpeed = 5f;
    protected const float flySpeed = 30f;
    protected const float glideDrag = 0.25f;

    protected bool isLaunching = false;

    private bool correctingSpine = false;
    private bool spineIsCorrrect = true;

    private void Update() {
        if (skeleton != null) {
            move();
            calculateSpeedAndHeading();
            doGravity();
            levelSpine();
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
        airSkeleton = (AirAnimalSkeleton)skeleton;

        List<Bone> tail = skeleton.getBones(BodyPart.TAIL);
        LineSegment tailLine = skeleton.getLines(BodyPart.TAIL)[0];
        StartCoroutine(ragdollLimb(tail, tailLine, () => { return true; }, false, 1f, transform));

        makeLegsRagDoll();

        generateAnimations();
    }


    //    _   _                               _     _ _         __                  _   _                 
    //   | \ | |                             | |   | (_)       / _|                | | (_)                
    //   |  \| | ___  _ __ ______ _ __  _   _| |__ | |_  ___  | |_ _   _ _ __   ___| |_ _  ___  _ __  ___ 
    //   | . ` |/ _ \| '_ \______| '_ \| | | | '_ \| | |/ __| |  _| | | | '_ \ / __| __| |/ _ \| '_ \/ __|
    //   | |\  | (_) | | | |     | |_) | |_| | |_) | | | (__  | | | |_| | | | | (__| |_| | (_) | | | \__ \
    //   |_| \_|\___/|_| |_|     | .__/ \__,_|_.__/|_|_|\___| |_|  \__,_|_| |_|\___|\__|_|\___/|_| |_|___/
    //                           | |                                                                      
    //                           |_|                                                                      

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
    /// Generates animations for the AirAnimal
    /// </summary>
    private void generateAnimations() {
          //Flapping animation
        flappingAnimation = generateFlyingAnimation(
          new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 1, 0) },
          new Vector3[] { new Vector3(0, 0, 85), new Vector3(0, 0, -45) },
          new Vector3[] { new Vector3(0, 0, -170), new Vector3(0, 0, 40) }
        );

        //Gliding anmiation
        glidingAnimation = generateFlyingAnimation(
            new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 0.5f, 0) },
            new Vector3[] { new Vector3(0, 0, 20), new Vector3(0, 0, 0) },
            new Vector3[] { new Vector3(0, 0, -20), new Vector3(0, 0, 0) }
        );

        //Walking animation
        generateWalkingAnimation();

        //Init current animation
        currentAnimation = glidingAnimation;
    }

    /// <summary>
    /// Function for creating a flying animation, consisting of wing rotations and spine positions
    /// </summary>
    /// <param name="spinePos">Keyframes for spine positions</param>
    /// <param name="wingRot1">Keyframes for wing rotations 1</param>
    /// <param name="wingRot2">Keyframes for wing rotations 2</param>
    /// <returns></returns>
    private AnimalAnimation generateFlyingAnimation(Vector3[] spinePos, Vector3[] wingRot1, Vector3[] wingRot2) {
        Bone spineBone = skeleton.getBones(BodyPart.SPINE)[0];
        Bone neckBone = skeleton.getBones(BodyPart.NECK)[0];
        List<Bone> rightWing = airSkeleton.getWing(true);
        List<Bone> leftWing = airSkeleton.getWing(false);

        AnimalAnimation flyingAnimation = new AnimalAnimation();
        int flyingAnimationFrameCount = 2;

        BoneKeyFrames spine = new BoneKeyFrames(spineBone, flyingAnimationFrameCount);
        BoneKeyFrames wing1_1 = new BoneKeyFrames(rightWing[0], flyingAnimationFrameCount);
        BoneKeyFrames wing1_2 = new BoneKeyFrames(rightWing[1], flyingAnimationFrameCount);
        BoneKeyFrames wing2_1 = new BoneKeyFrames(leftWing[0], flyingAnimationFrameCount);
        BoneKeyFrames wing2_2 = new BoneKeyFrames(leftWing[1], flyingAnimationFrameCount);

        spine.setPositions(spinePos);
        wing1_1.setRotations(wingRot1);
        wing1_2.setRotations(wingRot2);
        wing2_1.setRotations(Utils.multVectorArray(wingRot1, -1));
        wing2_2.setRotations(Utils.multVectorArray(wingRot2, -1));

        flyingAnimation.add(spine);
        flyingAnimation.add(wing1_1);
        flyingAnimation.add(wing1_2);
        flyingAnimation.add(wing2_1);
        flyingAnimation.add(wing2_2);

        return flyingAnimation;
    }

    /// <summary>
    /// Generates the walking animation
    /// </summary>
    private void generateWalkingAnimation() {
        //Getting relevant bones
        List<Bone> rightWing = airSkeleton.getWing(true);
        List<Bone> leftWing = airSkeleton.getWing(false);
        List<Bone> rightleg = skeleton.getBones(BodyPart.RIGHT_LEGS);
        List<Bone> leftleg = skeleton.getBones(BodyPart.LEFT_LEGS);

        walkingAnimation = new AnimalAnimation();
        int walkingAnimationFrameCount = 4;

        BoneKeyFrames wing1_1 = new BoneKeyFrames(rightWing[0], walkingAnimationFrameCount);
        BoneKeyFrames wing1_2 = new BoneKeyFrames(rightWing[1], walkingAnimationFrameCount);
        BoneKeyFrames wing2_1 = new BoneKeyFrames(leftWing[0], walkingAnimationFrameCount);
        BoneKeyFrames wing2_2 = new BoneKeyFrames(leftWing[1], walkingAnimationFrameCount);
        BoneKeyFrames leg1_1 = new BoneKeyFrames(rightleg[0], walkingAnimationFrameCount);
        BoneKeyFrames leg1_2 = new BoneKeyFrames(rightleg[1], walkingAnimationFrameCount);
        BoneKeyFrames leg2_1 = new BoneKeyFrames(leftleg[0], walkingAnimationFrameCount);
        BoneKeyFrames leg2_2 = new BoneKeyFrames(leftleg[1], walkingAnimationFrameCount);

        float frameTime = 2.0f;
        float[] wingFrameTimes = new float[] { frameTime, frameTime, frameTime, frameTime };
        wing1_1.setFrameTimes(wingFrameTimes);
        wing1_2.setFrameTimes(wingFrameTimes);
        wing2_1.setFrameTimes(wingFrameTimes);
        wing2_2.setFrameTimes(wingFrameTimes);

        wing1_1.setRotations(new Vector3[] { new Vector3(0, -30, 0), new Vector3(0, 0, 0), new Vector3(0, 30, 0), new Vector3(0, 0, 30) });
        wing1_2.setRotations(new Vector3[] { new Vector3(0, 0, -45), new Vector3(0, 0, -45), new Vector3(0, 0, -45), new Vector3(0, 0, -20) });
        wing2_1.setRotations(Utils.shiftArray(Utils.multVectorArray(wing1_1.Rotations, -1), 2));
        wing2_2.setRotations(Utils.shiftArray(Utils.multVectorArray(wing1_2.Rotations, -1), 2));
        leg1_1.setRotations(Utils.shiftArray(wing1_1.Rotations, 2));
        leg1_2.setRotations(Utils.shiftArray(wing1_2.Rotations, 2));
        leg2_1.setRotations(Utils.shiftArray(wing2_1.Rotations, 2));
        leg2_2.setRotations(Utils.shiftArray(wing2_2.Rotations, 2));

        walkingAnimation.add(wing1_1);
        walkingAnimation.add(wing1_2);
        walkingAnimation.add(wing2_1);
        walkingAnimation.add(wing2_2);
        walkingAnimation.add(leg1_1);
        walkingAnimation.add(leg1_2);
        walkingAnimation.add(leg2_1);
        walkingAnimation.add(leg2_2);
    }

    /// <summary>
    /// Handles the animation logic
    /// </summary>
    private void handleAnimations() {
        if (!animationInTransition) {
            currentAnimation.animate(speed * ((grounded) ? animSpeedScalingGround : animSpeedScalingAir));
            if (currentAnimation == walkingAnimation) {
                groundLegsAndWings();
            }
        }

        if (grounded) {
            if (currentAnimation != walkingAnimation) {
                tryAnimationTransition(walkingAnimation, animSpeedScalingAir, animSpeedScalingGround, 0.5f);
            }
        } else {
            float transistionTime = (currentAnimation == walkingAnimation) ? 0.5f : 0.5f;
            float nextSpeedScaling = (currentAnimation == walkingAnimation) ? animSpeedScalingGround : animSpeedScalingAir;
            if (desiredSpeed == 0 && currentAnimation != glidingAnimation) {                
                tryAnimationTransition(glidingAnimation, animSpeedScalingAir, nextSpeedScaling, transistionTime);
            } else if (desiredSpeed != 0 && currentAnimation != flappingAnimation) {
                tryAnimationTransition(flappingAnimation, animSpeedScalingAir, nextSpeedScaling, transistionTime);
            }
        }

        if (ragDollLegs && grounded) {
            ragDollLegs = false;
        } else if (!ragDollLegs && !grounded) {
            ragDollLegs = true;
            makeLegsRagDoll();
        }
    }

    /// <summary>
    /// Grounds the legs and wings
    /// </summary>
    private void groundLegsAndWings() {
        List<Bone> rightWing = airSkeleton.getWing(true);
        List<Bone> leftWing = airSkeleton.getWing(false);
        List<Bone> rightleg = skeleton.getBones(BodyPart.RIGHT_LEGS);
        List<Bone> leftleg = skeleton.getBones(BodyPart.LEFT_LEGS);

        groundLimb(rightWing, 1f);
        groundLimb(leftWing, 1f);
        groundLimb(rightleg, 1f);
        groundLimb(leftleg, 1f);
    }


    /// <summary>
    /// Sets the legs to ragdoll
    /// </summary>
    private void makeLegsRagDoll() {
        List<Bone> rightLegs = skeleton.getBones(BodyPart.RIGHT_LEGS);
        LineSegment rightLegsLine = skeleton.getLines(BodyPart.RIGHT_LEGS)[0];
        StartCoroutine(ragdollLimb(rightLegs, rightLegsLine, () => { return ragDollLegs; }, true, 5f, transform));

        List<Bone> leftLegs = skeleton.getBones(BodyPart.LEFT_LEGS);
        LineSegment leftLegsLine = skeleton.getLines(BodyPart.LEFT_LEGS)[0];
        StartCoroutine(ragdollLimb(leftLegs, leftLegsLine, () => { return ragDollLegs; }, true, 5f, transform));
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
        if (desiredSpeed - speed > 0.2f) { //Acceleration           
            speed += Time.deltaTime * acceleration;            
        } else if (speed - desiredSpeed > 0.2f) { //Deceleration
            if (!grounded) {
                speed -= Time.deltaTime * acceleration * glideDrag;
            } else {
                speed -= Time.deltaTime * acceleration;
            }
        }
    }

    /// <summary>
    /// Gravity calculations for when you are not grounded
    /// </summary>
    override protected void notGroundedGravity() {
        grounded = false;
        if (speed <= flySpeed / 2) {
            gravity += Physics.gravity * Time.deltaTime * (1 - speed / (flySpeed / 2f));
        } else {
            gravity = Vector3.zero;
        }
    }

    /// <summary>
    /// Tries to launch the animal for flight
    /// </summary>
    /// <returns>Success flag</returns>
    protected bool tryLaunch() {
        if (!isLaunching) {
            StartCoroutine(launch());
        }
        return !isLaunching;
    }

    /// <summary>
    /// Launches the Air animal for flight
    /// </summary>
    /// <returns></returns>
    private IEnumerator launch() {
        isLaunching = true;
        acceleration = acceleration * 4;
        gravity -= Physics.gravity * 2f;
        for (float t = 0; t <= 1f; t += Time.deltaTime) {
            grounded = false;
            yield return 0;
        }
        acceleration = acceleration / 4;
        isLaunching = false;
    }

    /// <summary>
    /// Levels the spine with the ground
    /// </summary>
    protected override void levelSpine() {
        if (grounded) {
            base.levelSpine();
            spineIsCorrrect = false;
        } else if (!grounded && !spineIsCorrrect) {            
            spineIsCorrrect = tryCorrectSpine();
        }
    }

    /// <summary>
    /// Tries to correct the spine
    /// </summary>
    /// <returns>Success flag</returns>
    private bool tryCorrectSpine() {
        if (!correctingSpine) {
            StartCoroutine(correctSpine());
        }
        return !correctingSpine;
    }

    /// <summary>
    /// Corrects the spine (returning it to zero rotation)
    /// </summary>
    /// <returns></returns>
    private IEnumerator correctSpine() {
        correctingSpine = true;
        Bone spine = skeleton.getBones(BodyPart.SPINE)[0];
        Quaternion originalRot = spine.bone.localRotation;
        for (float t = 0; t <= 1f; t += Time.deltaTime) {
            spine.bone.localRotation = Quaternion.Lerp(originalRot, Quaternion.identity, t);
            yield return 0;
        }
        spine.bone.localRotation = Quaternion.identity;
        correctingSpine = false;
    }

    private void OnCollisionEnter(Collision collision) {
        gravity = Vector3.zero;
        isLaunching = false;
    }

}
