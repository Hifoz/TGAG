using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Animation for use with animals. A container for BoneKeyFrames
/// </summary>
public class AnimalAnimation {
    
    List<BoneKeyFrames> boneAnimations = new List<BoneKeyFrames>();
    
    /// <summary>
    /// Adds the BoneKeyFrames to the animation.
    /// </summary>
    /// <param name="boneKeyFrames">keyframes to add</param>
    public void add(BoneKeyFrames boneKeyFrames) {
        boneAnimations.Add(boneKeyFrames);
    }

    /// <summary>
    /// Animates all the BoneKeyFrames with given speed (1f = normal speed)
    /// </summary>
    /// <param name="speed">float speed of animation</param>
    public void animate(float speed = 1f) {
        foreach(BoneKeyFrames keyFrames in boneAnimations) {
            keyFrames.animate(speed);
        }
    }

    /// <summary>
    /// Animates all the BoneKeyFrames interpolated with other animation with given speed (1f = normal speed)
    /// </summary>
    /// <param name="speed">float speed of animation</param>
    /// <param name="other">Animation to interpolate with</param>
    /// <param name="t">interpolation float</param>
    public void animateLerp(AnimalAnimation other, float t, float speed = 1f) {
        BoneKeyFrames otherKeyFrames = null;
        foreach (BoneKeyFrames keyFrames in boneAnimations) {
            otherKeyFrames = other.getKeyFramesWithBone(keyFrames.Bone);
            if (otherKeyFrames != null) {
                keyFrames.animateLerp(speed, otherKeyFrames, t);
            } else {
                keyFrames.animate(speed);
            }
        }
    }

    /// <summary>
    /// Looks for a BoneKeyFrame with the given bone
    /// </summary>
    /// <param name="bone">Bone to look for</param>
    /// <returns>BoneKeyFrames with the bone or null</returns>
    private BoneKeyFrames getKeyFramesWithBone(Bone bone) {
        foreach(BoneKeyFrames keyFrames in boneAnimations) {
            if (keyFrames.Bone == bone) {
                return keyFrames;
            }
        }
        return null;
    }
}


