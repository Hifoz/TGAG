using UnityEngine;
using System.Collections;

/// <summary>
/// Class for the playable air animal
/// </summary>
public class AirAnimalPlayer : AirAnimal {
    Vector3 right = Vector3.right;
    Vector3 up = Vector3.up;

    /// <summary>
    /// Function for that lets the player control the animal
    /// </summary>
    override protected void move() {
        desiredSpeed = 0;
        
        if (!Input.GetKey(KeyCode.LeftAlt)) {
            desiredHeading = Camera.main.transform.forward;
            desiredHeading.Normalize();

            right = Camera.main.transform.rotation * Vector3.right;
            up = Camera.main.transform.rotation * Vector3.up;
        }

        Vector3 finalHeading = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) {
            finalHeading += desiredHeading;
        }
        if (Input.GetKey(KeyCode.S)) {
            finalHeading -= desiredHeading;
        }
        if (Input.GetKey(KeyCode.A)) {
            finalHeading += Quaternion.AngleAxis(-90, up) * desiredHeading;
        }
        if (Input.GetKey(KeyCode.D)) {
            finalHeading += Quaternion.AngleAxis(90, up) * desiredHeading;
        }


        if (Input.GetKey(KeyCode.Space)) {
            if (grounded) {
                tryLaunch();
            } else if (!grounded) {
                finalHeading += Quaternion.AngleAxis(-45, right) * desiredHeading;
            }                
        }

        if (!grounded) {
            if (Input.GetKey(KeyCode.C)) {
                finalHeading += Quaternion.AngleAxis(45, right) * desiredHeading;
            }
        }


        if (finalHeading != Vector3.zero) {
            desiredHeading = finalHeading;
            setSpeed();
        }

        Vector3 velocity;
        if (grounded) {
            velocity = spineHeading.normalized * speed;
        } else {
            velocity = heading.normalized * speed;
        }
        rb.velocity = velocity + gravity;
        transform.LookAt(transform.position + heading);
    }

    /// <summary>
    /// Sets speed based on input
    /// </summary>
    private void setSpeed() {
        if (grounded) {
            desiredSpeed = walkSpeed;
        } else {
            desiredSpeed = flySpeed;
        }           
    }    
}
