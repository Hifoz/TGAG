using UnityEngine;
using System.Collections;

public class AirAnimalBrainPlayer : AnimalBrainPlayer {

    Vector3 right = Vector3.right;
    Vector3 up = Vector3.up;

    override public float slowSpeed { get { return 5f; } }
    override public float fastSpeed { get { return 30f; } }

    /// <summary>
    /// Function for that lets the player control the animal
    /// </summary>
    override public void move() {
        state.desiredSpeed = 0;

        if (!Input.GetKey(KeyCode.LeftAlt)) {
            state.desiredHeading = Camera.main.transform.forward;
            if (state.grounded || state.inWater) {
                state.desiredHeading.y = 0;
            }
            state.desiredHeading.Normalize();


            right = Camera.main.transform.rotation * Vector3.right;
            up = Camera.main.transform.rotation * Vector3.up;
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
            if (state.grounded || state.inWater) {
                actions["launch"]();
            } else if (!state.grounded) {
                finalHeading += Quaternion.AngleAxis(-45, right) * state.desiredHeading;
            }
        }

        if (!state.grounded || !state.inWater) {
            if (Input.GetKey(KeyCode.C)) {
                finalHeading += Quaternion.AngleAxis(45, right) * state.desiredHeading;
            }
        }


        if (finalHeading != Vector3.zero) {
            state.desiredHeading = finalHeading;
            setSpeed();
        }

        Vector3 velocity;
        if (state.grounded || state.inWater) {
            velocity = state.spineHeading.normalized * state.speed;
        } else {
            velocity = state.heading.normalized * state.speed;
        }
        state.rb.velocity = velocity + state.gravity;
        state.transform.LookAt(state.transform.position + state.heading);
    }

    /// <summary>
    /// Sets speed based on input
    /// </summary>
    private void setSpeed() {
        if (state.grounded || state.inWater) {
            state.desiredSpeed = slowSpeed;
        } else {
            state.desiredSpeed = fastSpeed;
        }
    }
}
