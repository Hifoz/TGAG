using UnityEngine;

/// <summary>
/// Class for a bone and keyframes
/// </summary>
public class BoneKeyFrames {
    /// <summary>
    /// Helper struct, encapsulates values related to animation and time
    /// </summary>
    private struct FrameTimeData {
        public float timeFraction;
        public float frame;
        public float frameFraction;
        public int thisFrame;
        public int nextFrame;
        public float timeModifier;
    }

    public string name;

    Bone bone;
    int frameCount;
    float animTime;
    float timer;

    float[] frameTimes;
    Vector3[] rotations;
    Vector3[] positions;
    Vector3[] scales;

    public Bone Bone { get { return bone; } }
    public int FrameCount { get { return frameCount; } }
    public float[] FrameTimes { get { return FrameTimes; } }
    public Vector3[] Rotations { get { return rotations; } }
    public Vector3[] Positions { get { return positions; } }
    public Vector3[] Scales { get { return scales; } }

    /// <summary>
    /// Constructor that initializes the KeyFrames for the bone.
    /// Remember to actually populate the rotations, positions and scales
    ///     arrays after creating the object
    /// </summary>
    /// <param name="bone">Bone to apply the keyframes to</param>
    /// <param name="frameCount">Amount of keyframes</param>
    /// <param name="name">Name of bone animation</param>
    /// <param name="animTime">How much time it should take to play all keyframes</param>
    public BoneKeyFrames(Bone bone, int frameCount, float animTime = 1f, string name = "") {
        this.name = (name == "") ? bone.bone.name : name;
        this.bone = bone;
        this.frameCount = frameCount;
        this.animTime = animTime;

        timer = 0.1f;
        frameTimes = new float[frameCount];
        for (int i = 0; i < frameCount; i++) {
            frameTimes[i] = 1f;
        }

        rotations = null;
        positions = null;
        scales = null;
    }

    /// <summary>
    /// Sets the frameTimes of the BoneKeyFrames
    /// </summary>
    /// <param name="frameTimes">New values</param>
    public void setFrameTimes(float[] frameTimes) {
        if (frameTimes.Length != frameCount) {
            throw new System.Exception("BoneKeyFrames, setFrameTimes error! The provided array is not the correct length");
        }

        this.frameTimes = frameTimes;
    }

    /// <summary>
    /// Sets the rotations of the BoneKeyFrames
    /// </summary>
    /// <param name="rotations">New values</param>
    public void setRotations(Vector3[] rotations) {
        if (rotations.Length != frameCount) {
            throw new System.Exception("BoneKeyFrames, setRotations error! The provided array is not the correct length");
        }

        this.rotations = rotations;
    }

    /// <summary>
    /// Sets the positions of the BoneKeyFrames
    /// </summary>
    /// <param name="positions">New values</param>
    public void setPositions(Vector3[] positions) {
        if (positions.Length != frameCount) {
            throw new System.Exception("BoneKeyFrames, setPositions error! The provided array is not the correct length");
        }

        this.positions = positions;
    }

    /// <summary>
    /// Sets the scales of the BoneKeyFrames
    /// </summary>
    /// <param name="scales">New values</param>
    public void setScales(Vector3[] scales) {
        if (scales.Length != frameCount) {
            throw new System.Exception("BoneKeyFrames, setScales error! The provided array is not the correct length");
        }

        this.scales = scales;
    }

    /// <summary>
    /// Animates the bone with the given keyframes, provied the time as an argument
    /// </summary>
    /// <param name="speed">the animation speed, 1f for normal speed</param>
    public void animate(float speed) {
        FrameTimeData frameTimeData = calculateFrameTimeData(speed);
        timer += Time.deltaTime * frameTimeData.timeModifier;
        Vector3[] values = getValuesAtTime(timer, frameTimeData);

        bone.bone.localRotation = Quaternion.Euler(values[0]);
        bone.bone.localPosition = values[1];
        bone.bone.localScale = values[2];
    }

    /// <summary>
    /// Animates the bone with the given keyframes, provied the time as an argument 
    ///     and interpolates the animation with the provided keyframes 
    /// Interpolates like this: lerp(this, other, t)
    /// </summary>
    /// <param name="speed">the animation speed, 1f for normal speed</param>
    /// <param name="other">The keyframes to interpolate against</param>
    /// <param name="t">Interpolation float</param>
    public void animateLerp(float speed, BoneKeyFrames other, float t) {
        if (bone != other.bone) {
            throw new System.Exception("BoneKeyFrames, animationLerp error! The other BoneKeyFrames is for a different bone!");
        }

        FrameTimeData frameTimeData = calculateFrameTimeData(speed);
        FrameTimeData frameTimeDataOther = other.calculateFrameTimeData(speed);
        timer += Time.deltaTime * Mathf.Lerp(frameTimeData.timeModifier, frameTimeDataOther.timeModifier, t);
        Vector3[] thisValues = getValuesAtTime(timer, frameTimeData);
        Vector3[] otherValues = other.getValuesAtTime(timer, speed);

        bone.bone.localRotation = Quaternion.Euler(Vector3.Lerp(thisValues[0], otherValues[0], t));
        bone.bone.localPosition = Vector3.Lerp(thisValues[1], otherValues[1], t);
        bone.bone.localScale = Vector3.Lerp(thisValues[2], otherValues[2], t);
    }

    /// <summary>
    /// Computes the values at the provided time
    /// This overload is used when this BoneKeyFrames is being interpolated with another BoneKeyFrames
    /// </summary>
    /// <param name="time"></param>
    /// <returns>Vector[3], where: index 0 = rotations, index 1 = positions, index 2 = scales</returns>
    private Vector3[] getValuesAtTime(float time, float speed) {
        timer = time;
        FrameTimeData frameTimeData = calculateFrameTimeData(speed);

        return getValuesAtTime(time, frameTimeData);
    }

    /// <summary>
    /// Computes the values at the provided time
    /// </summary>
    /// <param name="time"></param>
    /// <param name="frameTimeData">The data for the frame</param>
    /// <returns>Vector[3], where: index 0 = rotations, index 1 = positions, index 2 = scales</returns>
    private Vector3[] getValuesAtTime(float time, FrameTimeData frameTimeData) {
        Vector3[] values = new Vector3[3];
        values[0] = (rotations != null) ? Vector3.Lerp(rotations[frameTimeData.thisFrame], rotations[frameTimeData.nextFrame], frameTimeData.frameFraction) : bone.bone.localEulerAngles;
        values[1] = (positions != null) ? Vector3.Lerp(positions[frameTimeData.thisFrame], positions[frameTimeData.nextFrame], frameTimeData.frameFraction) : bone.bone.localPosition;
        values[2] = (scales != null) ? Vector3.Lerp(scales[frameTimeData.thisFrame], scales[frameTimeData.nextFrame], frameTimeData.frameFraction) : bone.bone.localScale;
        return values;
    }

    /// <summary>
    /// calculates the FrameTimeData for the current time
    /// </summary>
    /// <param name="speed">animation speed</param>
    /// <returns></returns>
    private FrameTimeData calculateFrameTimeData(float speed) {
        FrameTimeData frameTimeData = new FrameTimeData();
        frameTimeData.timeFraction = Utils.frac(timer);
        frameTimeData.frame = frameTimeData.timeFraction * frameCount;
        frameTimeData.frameFraction = Utils.frac(frameTimeData.frame);
        frameTimeData.thisFrame = (int)frameTimeData.frame;
        frameTimeData.nextFrame = (frameTimeData.thisFrame + 1) % frameCount;
        frameTimeData.timeModifier = speed / (frameCount * frameTimes[frameTimeData.thisFrame]);
        frameTimeData.timeModifier /= animTime;
        return frameTimeData;
    }
}