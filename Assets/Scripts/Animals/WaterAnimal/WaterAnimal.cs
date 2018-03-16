using UnityEngine;
using System;
using System.Collections.Generic;

public class WaterAnimal : Animal {

    private WaterAnimalSkeleton waterSkeleton;

    private AnimalAnimation swimAnimation;

    override protected void Start() {
        base.Start();

        WaterAnimalSkeleton s = new WaterAnimalSkeleton(transform);
        s.generateInThread();
        setSkeleton(s);
    }

    override protected void Update() {
        if (skeleton != null) {
            currentAnimation.animate();
        }
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
    /// sets the skeleton, and applies the new mesh.
    /// </summary>
    override public void setSkeleton(AnimalSkeleton skeleton) {
        base.setSkeleton(skeleton);
        waterSkeleton = (WaterAnimalSkeleton)skeleton;

        generateAnimations();
    }
   

    public override void setAnimalBrain(AnimalBrain brain) {
        base.setAnimalBrain(brain);
    }

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
}
