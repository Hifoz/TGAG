using UnityEngine;
using System.Collections;

public class WaterAnimalBrainPlayer : AnimalBrainPlayer {
    Vector3 right = Vector3.right;
    Vector3 up = Vector3.up;

    override public float slowSpeed { get { return 10f; } }
    override public float fastSpeed { get { return 40f; } }

    /// <summary>
    /// Function for that lets the player control the animal
    /// </summary>
    override public void move() {
        state.desiredSpeed = 0;

        if (!Input.GetKey(KeyCode.LeftAlt)) {
            right = Camera.main.transform.rotation * Vector3.right;
            up = Camera.main.transform.rotation * Vector3.up;

            state.desiredHeading = Camera.main.transform.forward;
            state.desiredHeading.Normalize();
        }

        Vector3 finalHeading = Vector3.zero;
 
        if (Input.GetKey(KeyCode.W)) {
            finalHeading += state.desiredHeading;
        }
        if (Input.GetKey(KeyCode.S)) {
            finalHeading -= state.desiredHeading;
        }
        if (Input.GetKey(KeyCode.A)) {
            finalHeading += Quaternion.AngleAxis(-90, up) * state.desiredHeading;
        }
        if (Input.GetKey(KeyCode.D)) {
            finalHeading += Quaternion.AngleAxis(90, up) * state.desiredHeading;
        }
        if (Input.GetKey(KeyCode.Space)) {
            if (state.grounded) {
                actions["flap"]();
            } else {
                finalHeading += Quaternion.AngleAxis(-45, right) * state.desiredHeading;
            }
        }
        if (Input.GetKey(KeyCode.C)) {
            finalHeading += Quaternion.AngleAxis(45, right) * state.desiredHeading;
        }

        if (finalHeading != Vector3.zero) {
            state.desiredHeading = finalHeading;
            setSpeed();
        }       
    }

    /// <summary>
    /// Sets speed based on input
    /// </summary>
    private void setSpeed() {
        if (Input.GetKey(KeyCode.LeftShift) && state.inWater) {
            state.desiredSpeed = fastSpeed;
        } else {
            state.desiredSpeed = slowSpeed;
        }
    }
}
