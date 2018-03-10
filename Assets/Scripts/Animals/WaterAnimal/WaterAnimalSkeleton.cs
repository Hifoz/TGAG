using UnityEngine;
using System.Collections.Generic;

public class WaterAnimalSkeleton : AnimalSkeleton {

    /// <summary>
    /// Constructor that does the mainThread skeleton generation, and binds skeleton to the passed transform
    /// </summary>
    /// <param name="root">Transform to bind skeleton to</param>
    public WaterAnimalSkeleton(Transform root) {
        bodyParametersRange = new MixedDictionary<BodyParameter>(new Dictionary<BodyParameter, object>() {
                { BodyParameter.SCALE, new Range<float>(0.5f, 1.0f) },

                { BodyParameter.HEAD_SIZE, new Range<float>(2f, 4f) },
                { BodyParameter.HEAD_RADIUS, new Range<float>(0.5f, 1.0f) },

                { BodyParameter.NECK_LENGTH, new Range<float>(3, 4) },
                { BodyParameter.NECK_RADIUS, new Range<float>(0.5f, 0.8f) },

                { BodyParameter.SPINE_LENGTH, new Range<float>(5, 10) },
                { BodyParameter.SPINE_RADIUS, new Range<float>(0.5f, 1.0f) },

                { BodyParameter.TAIL_JOINTS, new Range<int>(2, 4) },
                { BodyParameter.TAIL_LENGTH, new Range<float>(5, 10) },
                //TAIL_JOINT_LENGTH is calculated from TAIL_JOINTS and TAIL_LENGTH
                { BodyParameter.TAIL_RADIUS, new Range<float>(0.5f, 0.8f) },
            }
        );

        generateInMainThread(root);
    }

    override protected void generateBodyParams() {
        base.generateBodyParams();
    }

    override protected void makeAnimBones() {
        throw new System.NotImplementedException();
    }

    override protected  void makeSkeletonLines() {
        throw new System.NotImplementedException();
    }
}
