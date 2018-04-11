using UnityEngine;
using System.Collections;

/// <summary>
/// Helper class, usefull for sharing the state with the brain
/// </summary>
public class AnimalState {
    public bool onWaterSurface = false;
    public bool inWater = false;
    public bool grounded = false;
    public bool inWindArea = false;
    public bool canStand = false;

    public float desiredSpeed = 0;
    public float speed = 0;

    public Vector3 desiredHeading = Vector3.zero;
    public Vector3 heading = Vector3.zero;
    public Vector3 spineHeading = Vector3.forward;

    public Transform transform;

}